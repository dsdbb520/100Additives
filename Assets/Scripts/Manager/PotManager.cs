using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
public class PotManager : MonoBehaviour
{
    public List<CardData> cookingPot = new List<CardData>();
    public Transform potPanel;  // 锅面板容器，用于显示锅中的卡牌

    // 添加卡牌到锅
    public void AddCardToPot(CardData card, GameObject cardObject)
    {
        cookingPot.Add(card);

        cardObject.transform.SetParent(potPanel);  // 移动卡牌到锅面板
        cardObject.transform.DOScale(new Vector3(0.7f, 0.7f, 1), 0.3f); // 将卡牌缩小
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

    // 计算并清空锅
    public void ServeDish()
    {
        float totalPhyDamage = 0;
        float totalMenDamage = 0;

        foreach (var card in cookingPot)
        {
            totalPhyDamage += card.phyDamage;
            totalMenDamage += card.menDamage;
        }

        // 计算伤害并清空锅
        Debug.Log($"Dealing {totalPhyDamage} physical damage and {totalMenDamage} mental damage.");
        cookingPot.Clear();
    }
}
