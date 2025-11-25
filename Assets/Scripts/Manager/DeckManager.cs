using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public List<CardData> drawPile = new List<CardData>();    // 牌库
    public List<CardData> discardPile = new List<CardData>(); // 弃牌堆

    public HandManager handManager;  // 引用手牌管理器，用于将卡牌添加到手牌
    public TextMeshProUGUI drawPileText;    // 牌库数量文本
    public TextMeshProUGUI discardPileText; // 弃牌堆数量文本
    public Button DrawButton;
    public Button RoundEndButton;

    public List<CardData> allCards = new List<CardData>();    // 所有的卡牌（手动填充的卡牌列表）
    public BattleManager battleManager;

    // 更新卡牌数量的显示
    public void UpdateCardCountDisplay()
    {
        drawPileText.text = "牌堆剩余: " + drawPile.Count;
        discardPileText.text = "弃牌堆: " + discardPile.Count;
    }

    // 向牌库添加卡牌
    public void AddCardsToDeck(List<CardData> cards)
    {
        drawPile.AddRange(cards);  // 将卡牌添加到牌库
        UpdateCardCountDisplay();
    }

    // 洗牌
    public void Shuffle()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            CardData temp = drawPile[i];
            int randomIndex = Random.Range(i, drawPile.Count);
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
        UpdateCardCountDisplay();
    }

    // 抽取卡牌
    public void DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count == 0)
            {
                // 如果抽牌堆为空，将弃牌堆重新洗入抽牌堆，并清空弃牌堆
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                UpdateCardCountDisplay();
                Debug.Log("牌库为空，开始洗牌");
                Shuffle();
                if (drawPile.Count == 0)
                {
                    FindObjectOfType<FloatingHint>().ShowHint("牌堆为空，请先弃牌！");
                }
            }

            if (drawPile.Count > 0)
            {
                // 抽取卡牌
                CardData card = drawPile[0];
                drawPile.RemoveAt(0);
                // 把卡牌添加到手牌并实例化
                handManager.AddCardToHand(card);
            }
        }

        UpdateCardCountDisplay();
    }

    // 游戏开始时初始化
    void Start()
    {
        AddCardsToDeck(allCards);  // 添加卡牌到牌库
        battleManager = FindObjectOfType<BattleManager>();
        DrawButton.GetComponent<Button>().onClick.AddListener(OnDrawButtonClicked);
        RoundEndButton.GetComponent<Button>().onClick.AddListener(PlayerTurnEnd);
    }

    void OnDrawButtonClicked()
    {
        DrawCard(1);
        UpdateCardCountDisplay();
        Debug.Log("抽了一张牌");
    }

    public void PlayerTurnStart()
    {
        DrawCard(3);
        UpdateCardCountDisplay();
    }

    public void PlayerTurnEnd()
    {
        if ( battleManager.currentState == BattleManager.BattleState.PlayerTurn )
        {
            battleManager.ChangeState(BattleManager.BattleState.Resolution);
        }
    }

}
