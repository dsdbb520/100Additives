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
        if (transform.parent == handManager.handPanel)
        {
            // 如果卡牌在手牌面板上，点击后将其加入到锅中
            potManager.AddCardToPot(cardData, gameObject);
            handManager.RemoveCardFromHand(cardData);  // 从手牌中移除
        }
        else if (transform.parent == potManager.potPanel)
        {
            // 如果卡牌在锅面板上，点击后将其移回手牌
            potManager.RemoveCardFromPot(cardData, gameObject, handManager);

        }
    }
}
