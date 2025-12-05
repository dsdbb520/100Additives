using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardUIHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public CardData cardData;  // 绑定卡牌数据
    public TextMeshProUGUI cardNameText;   // 显示卡牌名称
    public TextMeshProUGUI costText;       // 显示卡牌费用
    public Image cardIcon;                 // 显示卡牌图标
    private bool isDragging = false;
    private PotManager potManager;
    private HandManager handManager;
    private BattleManager battleManager;
    private SmallStoveManager smallStoveManager;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    void Start()
    {
        // 初始化卡牌UI数据
        if (cardData != null)
        {
            cardNameText.text = cardData.cardName;
            costText.text = cardData.cost.ToString();
            cardIcon.sprite = cardData.icon;
        }

        potManager = FindObjectOfType<PotManager>();
        handManager = FindObjectOfType<HandManager>();
        battleManager = FindObjectOfType<BattleManager>();
        smallStoveManager = FindObjectOfType<SmallStoveManager>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (transform.parent == handManager.handPanel) //只有在手牌区才放大
        {
            transform.DOScale(1.2f, 0.2f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (transform.parent == handManager.handPanel)
        {
            transform.DOScale(1.0f, 0.2f);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (HandManager.isFrozenMode && transform.parent == handManager.handPanel)
        {
            if (cardData.isFrozen) UnfreezeCard();
            else FreezeCard();
            return;
        }

        if (!HandManager.isFrozenMode && transform.parent == handManager.handPanel)
        {
            Debug.Log("显示卡牌详情");
        }
        //else if (transform.parent == potManager.potPanel)
        //{
        //    potManager.RemoveCardFromPot(cardData, gameObject, handManager);
        //}
        else if (transform.parent == potManager.potPanel)
        {
            FloatingHint.Instance.ShowHint("已下锅的食材不能拿出来了！");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (transform.parent == handManager.handPanel && !cardData.isFrozen && !HandManager.isFrozenMode)
        {
            isDragging = true;
            originalParent = transform.parent;
            transform.SetParent(transform.parent.parent);
            canvasGroup.blocksRaycasts = false;
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        transform.position = eventData.position;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        canvasGroup.blocksRaycasts = true;

        //检测是否拖到了大锅
        if (potManager != null && potManager.potPanel != null &&
            RectTransformUtility.RectangleContainsScreenPoint(potManager.potPanel.GetComponent<RectTransform>(), Input.mousePosition))
        {
            if (battleManager.TryUseEnergy(cardData.cost))
            {
                //检测卡牌是否为“消耗”
                if (cardData.buffs.Contains(BuffType.Exhaust))
                {
                    handManager.RemoveCardFromHand(cardData);
                    Destroy(gameObject); //销毁卡牌物体

                    FloatingHint.Instance.ShowHint("卡牌已消耗！");

                    isDragging = false;
                    return;
                }
                potManager.AddCardToPot(cardData, gameObject);
                handManager.RemoveCardFromHand(cardData);
                isDragging = false;
                return;
            }
            else
            {
                FloatingHint.Instance.ShowHint("费用不足！");
            }
        }
        else if (smallStoveManager != null && smallStoveManager.stovePanel != null &&
                 RectTransformUtility.RectangleContainsScreenPoint(smallStoveManager.stovePanel, Input.mousePosition))
        {
            bool isSuccess = smallStoveManager.AddCardToStove(cardData, gameObject);
            if (isSuccess)
            {
                isDragging = false;
                return;
            }
        }

        ReturnToHand();
        isDragging = false;
    }


    void ReturnToHand()
    {
        transform.SetParent(handManager.handPanel);
        transform.localScale = Vector3.one;
    }

    // 冻结卡牌
    public void FreezeCard()
    {
        if (handManager.GetFrozenCardCount() >= 2) 
        {
            FloatingHint.Instance.ShowHint("最多只能冻结两张牌！");
            return;
        }
        cardData.isFrozen = true; // 设置卡牌为冻结状态
        GetComponent<Image>().color = Color.cyan;
        Debug.Log($"Card {cardData.cardName} is frozen.");
    }
    // 解冻卡牌
    public void UnfreezeCard()
    {
        cardData.isFrozen = false; // 解除冻结状态
        GetComponent<Image>().color = new Color(101f / 255f, 97f / 255f, 97f / 255f);
        Debug.Log($"Card {cardData.cardName} is unfrozen.");
    }
}
