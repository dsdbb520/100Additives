using UnityEngine;
using TMPro;
using System.Collections;   
using System.Collections.Generic;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public List<CardData> drawPile = new List<CardData>();    // 牌库
    public List<CardData> discardPile = new List<CardData>(); // 弃牌堆

    public HandManager handManager;  // 引用手牌管理器，用于将卡牌添加到手牌
    public TextMeshProUGUI drawPileText;    // 牌库数量文本
    public TextMeshProUGUI discardPileText; // 弃牌堆数量文本
    public int DrawNumber = 3;   // 玩家摸牌数量
    public Button openFireButton;
    public Button RoundEndButton;

    public List<CardData> allCards = new List<CardData>();    // 所有的卡牌（手动填充的卡牌列表）
    public BattleManager battleManager;
    public PotManager potManager;

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
        StartCoroutine(DrawCardsWithDelay(count));
    }
    IEnumerator DrawCardsWithDelay(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count == 0)
            {
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                UpdateCardCountDisplay();
                Debug.Log("牌库为空，开始洗牌");
                Shuffle();
                if (drawPile.Count == 0)
                {
                    FindObjectOfType<FloatingHint>().ShowHint("牌堆为空！");
                    yield break;
                }
            }
            if (drawPile.Count > 0)
            {
                CardData card = drawPile[0];
                drawPile.RemoveAt(0);

                // 发一张牌
                handManager.AddCardToHand(card);

                //触发DrawPressure Buff
                if (card.buffs.Contains(BuffType.DrawPressure))
                {
                    //使用卡牌自身的pressure值作为加压数值
                    if (potManager != null)
                    {
                        potManager.AddDirectPressure(card.pressure);
                    }
                }
                //更新文字显示
                UpdateCardCountDisplay();

                //暂停0.4秒再发下一张
                yield return new WaitForSeconds(0.4f);
            }
        }

        UpdateCardCountDisplay();
    }

    // 游戏开始时初始化
    void Awake()
    {
        battleManager = FindObjectOfType<BattleManager>();
        RoundEndButton.GetComponent<Button>().onClick.AddListener(PlayerTurnEnd);
        openFireButton.GetComponent<Button>().onClick.AddListener(openFire);
    }

    public void ResetDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        //重新把所有牌加进去
        AddCardsToDeck(allCards);
        Shuffle();
        UpdateCardCountDisplay();
        Debug.Log("牌库已重置");
    }

    void openFire()
    {
        if (battleManager.TryUseEnergy(3))
        {
            battleManager.ChangeState(BattleManager.BattleState.Resolution);
        }
        else
        {
            FloatingHint.Instance.ShowHint("费用不足！需要 3 点费用来开锅！");
        }
    }

    public void PlayerTurnStart()
    {
        DrawCard(DrawNumber);
        UpdateCardCountDisplay();
    }

    public void PlayerTurnEnd()
    {
        if ( battleManager.currentState == BattleManager.BattleState.PlayerTurn )
        {
            battleManager.ChangeState(BattleManager.BattleState.EnemyTurn);
        }
    }

}
