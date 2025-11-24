using UnityEngine;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;  // 卡牌预制体
    public Transform handPanel;    // 手牌容器
    public List<CardData> handCards = new List<CardData>();  // 手牌数据

    public void AddCardToHand(CardData cardData)
    {
        // 实例化卡牌并显示在 UI 上
        GameObject cardObj = Instantiate(cardPrefab, handPanel);
        CardUIHandler cardHandler = cardObj.GetComponent<CardUIHandler>();
        cardHandler.cardData = cardData;  // 绑定卡牌数据

        handCards.Add(cardData);  // 将卡牌添加到手牌列表
    }

    public void RemoveCardFromHand(CardData cardData)
    {
        handCards.Remove(cardData);
    }

    public void DiscardAllCard()
    {
        // 将所有手牌中的卡牌添加到弃牌堆
        foreach (CardData cardData in handCards)
        {
            // 添加卡牌到 discardPile
            FindObjectOfType<DeckManager>().discardPile.Add(cardData);
        }

        // 清空手牌列表
        handCards.Clear();

        // 清理手牌 UI 上的卡牌对象
        foreach (Transform card in handPanel)
        {
            Destroy(card.gameObject);  // 销毁手牌
        }
    }
}
