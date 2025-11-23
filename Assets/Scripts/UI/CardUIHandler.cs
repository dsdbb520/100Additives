using UnityEngine;
using UnityEngine.UI;

public class CardUIHandler : MonoBehaviour
{
    public CardData cardData;
    public Text cardNameText;
    public Text costText;
    //public卡牌图标

    private PotManager potManager; // 用来添加到烹饪锅中

    void Start()
    {
        cardNameText.text = cardData.cardName;
        costText.text = cardData.cost.ToString();

        potManager = FindObjectOfType<PotManager>();
        GetComponent<Button>().onClick.AddListener(OnCardClicked);
    }

    void OnCardClicked()
    {
        // 添加到烹饪锅
        potManager.AddCardToPot(cardData);

        // 移除手牌中的卡牌
        Destroy(gameObject); // 移除当前卡牌UI对象
    }
}
