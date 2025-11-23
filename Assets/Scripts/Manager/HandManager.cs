using UnityEngine;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;  // 卡牌预制体
    public Transform handPanel;    // 手牌容器

    public void AddCardToHand(CardData cardData)
    {
        GameObject cardObj = Instantiate(cardPrefab, handPanel);
        CardUIHandler cardHandler = cardObj.GetComponent<CardUIHandler>();
        cardHandler.cardData = cardData; // 绑定数据
    }
}
