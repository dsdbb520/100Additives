using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUIHandler : MonoBehaviour
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




    // 冻结卡牌
    public void FreezeCard()
    {
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
