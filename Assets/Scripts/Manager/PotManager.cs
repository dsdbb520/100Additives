using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
public class PotManager : MonoBehaviour
{
    public List<CardData> cookingPot = new List<CardData>();
    public Transform potPanel;

    // 添加卡牌到锅
    public void AddCardToPot(CardData card, GameObject cardObject)
    {
        if (card.isFrozen)
        {
            Debug.Log($"Card {card.cardName} is frozen and cannot be added to the pot.");
            return; //如果卡牌被冻结，不进入锅
        }
        cookingPot.Add(card);
        cardObject.transform.SetParent(potPanel);  //移动卡牌到锅面板
        cardObject.transform.DOScale(new Vector3(0.7f, 0.7f, 1), 0.3f); //将卡牌缩小
        Debug.Log($"Card {card.cardName} added to the pot.");
    }

    // 移除卡牌并添加回手牌
    public void RemoveCardFromPot(CardData card, GameObject cardObject, HandManager handManager)
    {
        cookingPot.Remove(card);
        handManager.handCards.Add(card);
        cardObject.transform.SetParent(handManager.handPanel);  // 移动卡牌回手牌面板
        cardObject.transform.DOScale(new Vector3(1, 1, 1), 0.3f); // 将卡牌放大
        Debug.Log($"Card {card.cardName} returned to the hand.");
    }

    // 清空锅中的所有卡牌
    public void ClearPot()
    {
        //遍历锅中的每张卡牌，销毁GameObject
        foreach (var card in cookingPot)
        {
            GameObject cardObject = GetCardObject(card);
            if (cardObject != null)
            {
                Destroy(cardObject);  //销毁GameObject
            }
        }
        // 清空锅的卡牌列表
        cookingPot.Clear();
        Debug.Log("All cards cleared from the pot.");
    }

    // 获取卡牌的GameObject
    private GameObject GetCardObject(CardData card)
    {
        foreach (Transform child in potPanel)
        {
            CardUIHandler cardUIHandler = child.GetComponent<CardUIHandler>();
            if (cardUIHandler != null && cardUIHandler.cardData == card)
            {
                return child.gameObject;  //返回找到的卡牌GameObject(当锅中有多张重复牌的时候不一定会销毁指定的那一张，以后写多卡联动要注意)
            }
        }
        return null;
    }
}
