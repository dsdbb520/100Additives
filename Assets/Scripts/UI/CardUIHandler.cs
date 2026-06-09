using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardUIHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CardData cardData;  // 绑定卡牌数据
    public RelicData relicData; //绑定遗物数据
    public TextMeshProUGUI cardNameText;   // 显示卡牌名称
    public TextMeshProUGUI costText;       // 显示卡牌费用
    public Image cardIcon;                 // 显示卡牌图标
    public bool isInteractive = true;
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
        if (cardData != null && relicData == null)
        {
            InitCard(cardData);
        }

        potManager = FindObjectOfType<PotManager>();
        handManager = FindObjectOfType<HandManager>();
        battleManager = FindObjectOfType<BattleManager>();
        smallStoveManager = FindObjectOfType<SmallStoveManager>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void InitCard(CardData data)
    {
        cardData = data;
        relicData = null;

        if (cardNameText != null)
        {
            cardNameText.text = data.cardName;
            cardNameText.color = RarityUtils.GetColor(data.rarity);
        }
        if (costText != null)
        {
            costText.text = data.cost.ToString();
            if (costText.transform.parent != null) costText.transform.parent.gameObject.SetActive(true);
            else costText.gameObject.SetActive(true);
        }
        if (cardIcon != null) cardIcon.sprite = data.icon;
    }

    public void InitRelic(RelicData data)
    {
        relicData = data;
        cardData = null;

        //设置基础信息
        if (cardNameText != null)
        {
            cardNameText.text = data.relicName;
            cardNameText.color = RarityUtils.GetColor(data.rarity);
        }

        if (cardIcon != null) cardIcon.sprite = data.icon;

        if (costText != null)
        {
            if (costText.transform.parent != null && costText.transform.parent != transform)
                costText.transform.parent.gameObject.SetActive(false);
            else
                costText.gameObject.SetActive(false);
        }

        isInteractive = false;
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 targetScale = GetBaseScale() * 1.1f; //相对放大10%
        transform.DOScale(targetScale, 0.2f);
        if (TooltipUI.Instance != null)
        {
            if (cardData != null && !cardData.isFrozen)
            {
                string hexColor = RarityUtils.GetColorHex(cardData.rarity);
                string header = $"<color={hexColor}>{cardData.cardName}</color>";
                TooltipUI.Instance.Show(header, cardData.description);
            }
            else if (relicData != null)
            {
                // 遗物 Tooltip
                // 这里简单处理颜色，或者你也给 RarityUtils 加个 GetRelicColorHex
                string header = relicData.relicName;
                TooltipUI.Instance.Show(header, relicData.description);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Vector3 targetScale = GetBaseScale();
        transform.DOScale(targetScale, 0.2f);
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractive)
        {
            return;
        }
        if (HandManager.isFrozenMode && transform.parent == handManager.handPanel)
        {
            if (cardData.isFrozen) UnfreezeCard();
            else FreezeCard();
            return;
        }
        else if (transform.parent == potManager.potPanel)
        {
            FloatingHint.Instance.ShowHint("已下锅的食材不能拿出来了！");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isInteractive)
        {
            return;
        }
        if (cardData.isUnplayable)
        {
            FloatingHint.Instance.ShowHint("这张牌无法打出！");
            return;
        }
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
            //必须是大锅牌或双用牌
            if (cardData.targetType == CardTargetType.BigPot || cardData.targetType == CardTargetType.Dual)
            {
                if (battleManager.TryUseEnergy(cardData.cost))
                {
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
            else
            {
                FloatingHint.Instance.ShowHint("这张牌不能放入大锅！");
            }
        }
        else if (smallStoveManager != null && smallStoveManager.stovePanel != null &&
                 RectTransformUtility.RectangleContainsScreenPoint(smallStoveManager.stovePanel, Input.mousePosition))
        {
            //必须是小灶牌或双用牌
            if (cardData.targetType == CardTargetType.SmallStove || cardData.targetType == CardTargetType.Dual)
            {
                if (battleManager.TryUseEnergy(cardData.cost))
                {
                    bool isSuccess = smallStoveManager.AddCardToStove(cardData, gameObject);
                    if (isSuccess)
                    {
                        isDragging = false;
                        return;
                    }
                }
                    
            }
            else
            {
                FloatingHint.Instance.ShowHint("这张牌不能放入小灶！");
            }
        }

        ReturnToHand();
        isDragging = false;
    }

    private void OnDisable()
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide(true);
        }
        transform.localScale = Vector3.one;
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

    private Vector3 GetBaseScale()
    {
        //如果在大锅里，基准大小是0.7
        if (potManager != null && transform.parent == potManager.potPanel)
        {
            return new Vector3(0.7f, 0.7f, 1f);
        }
        //如果在小灶里
        if (smallStoveManager != null && transform.parent == smallStoveManager.stovePanel)
        {
            return Vector3.zero; //或者其他大小
        }
        //如果在商店里
        if (GetComponentInParent<ShopSlotUI>() != null)
        {
            if (transform.parent == GetComponentInParent<ShopSlotUI>().cardSpawnPoint)
            {
                return new Vector3(0.8f, 0.8f, 1f);
            }
        }
        //如果是奖励页面
        if(transform.parent.name== "CardDraftSelect")
        {
            return new Vector3(0.8f, 0.8f, 1f);
        }
        //如果是背包
        if (transform.parent.name == "Content")
        {
            return new Vector3(0.8f, 0.8f, 1f);
        }
        //默认情况，基准大小是 1.0
        return Vector3.one;
    }
}
