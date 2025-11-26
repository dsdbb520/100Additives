using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;  // 卡牌预制体
    public Transform handPanel;    // 手牌容器
    public List<CardData> handCards = new List<CardData>();  // 手牌数据
    public static bool isFrozenMode = false;  //控制冻结模式
    public Button freezeButton;  //按钮用于切换冻结模式


    private void Start()
    {
        freezeButton.onClick.AddListener(ToggleFreezeMode);
    }

    private void ToggleFreezeMode()
    {
        isFrozenMode = !isFrozenMode; //切换状态
        if (isFrozenMode) freezeButton.image.color = Color.cyan;
        if (!isFrozenMode) freezeButton.image.color = Color.white;
        Debug.Log($"Freeze Mode: {isFrozenMode}");
    }

    public void AddCardToHand(CardData cardData)
    {
        //克隆cardData生成一个新的副本
        CardData cardClone = cardData.Clone();
        //实例化卡牌并显示在 UI 上
        GameObject cardObj = Instantiate(cardPrefab, handPanel);
        CardUIHandler cardHandler = cardObj.GetComponent<CardUIHandler>();
        cardHandler.cardData = cardClone;  //绑定新的卡牌副本数据
        handCards.Add(cardClone);  //将克隆后的卡牌添加到手牌列表
        cardHandler.UnfreezeCard(); //Debug用
        Debug.Log($"Card {cardClone.cardName} added to hand.");
    }

    public void RemoveCardFromHand(CardData cardData)
    {
        handCards.Remove(cardData);
    }

    public void DiscardAllCard()
    {
        //临时保存将要丢弃的卡牌
        List<CardData> cardsToDiscard = new List<CardData>();
        foreach (CardData cardData in handCards)
        {
            if (!cardData.isFrozen)  //检查卡片是否被冻结
            {
                cardsToDiscard.Add(cardData);  //添加到临时列表
            }
        }
        // 将所有没有冻结的卡牌添加到弃牌堆
        FindObjectOfType<DeckManager>().discardPile.AddRange(cardsToDiscard);

        // 清空手牌列表
        handCards.RemoveAll(card => !card.isFrozen);

        foreach (Transform card in handPanel)
        {
            CardUIHandler cardHandler = card.GetComponent<CardUIHandler>();
            if (cardHandler != null && !cardHandler.cardData.isFrozen)
            {
                Destroy(card.gameObject);  // 销毁没有冻结的卡
            }
        }
    }

    public int GetFrozenCardCount()
    {
        int frozenCount = 0;
        //遍历手牌中的每一张卡牌，检查是否被冻结
        foreach (CardData card in handCards)
        {
            if (card.isFrozen)
            {
                frozenCount++;
            }
        }
        return frozenCount;
    }



}
