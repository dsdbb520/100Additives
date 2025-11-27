using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardData cardData;  // 绑定卡牌数据
    public TextMeshProUGUI cardNameText;   // 显示卡牌名称
    public TextMeshProUGUI costText;       // 显示卡牌费用
    public Image cardIcon;                 // 显示卡牌图标
    private PotManager potManager;
    private HandManager handManager;

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

        // 添加点击事件监听
        GetComponent<Button>().onClick.AddListener(OnCardClicked);
    }

    void OnCardClicked()
    {
        if (HandManager.isFrozenMode && transform.parent == handManager.handPanel) 
        {
            if (cardData.isFrozen)
            {
                // 如果卡牌被冻结，解除冻结
                UnfreezeCard();
            }
            else
            {
                FreezeCard();
            }
        }
        else if(!HandManager.isFrozenMode)
        {
            if (transform.parent == handManager.handPanel)
            {
                potManager.AddCardToPot(cardData, gameObject);
                handManager.RemoveCardFromHand(cardData);  // 从手牌中移除
            }
            else if(transform.parent == potManager.potPanel)
            {
                // 如果卡牌在锅面板上，点击后将其移回手牌
                potManager.RemoveCardFromPot(cardData, gameObject, handManager);

            }
        }
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

    // 冻结卡牌
    public void FreezeCard()
    {
        if (handManager.GetFrozenCardCount() >= 2) 
        {
            FindObjectOfType<FloatingHint>().ShowHint("最多只能冻结两张牌！");
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
