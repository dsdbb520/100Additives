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
    public int DrawNumber = 7;   // 玩家摸牌数量
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

                //执行抽到时效果
                if (!string.IsNullOrEmpty(card.specialEffectID))
                {
                    SpecialEffectManager.Instance.ApplyEffect(card.specialEffectID, card, false, EffectTriggerPhase.OnDraw);
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
        if (!potManager.canServe)
        {
            FloatingHint.Instance.ShowHint("锅里有液氮，现在上菜太危险了！(本回合禁止上菜)");
            return;
        }
        if (battleManager.TryUseEnergy(3))
        {
            battleManager.ChangeState(BattleManager.BattleState.Resolution);
        }
        else
        {
            FloatingHint.Instance.ShowHint("费用不足！需要 3 点费用来开锅！");
        }
    }


    public void ObtainCard(CardData card)
    {

        allCards.Add(card);

        if (BattleManager.Instance != null && BattleManager.Instance.currentState != BattleManager.BattleState.GameStart)
        {
            drawPile.Add(card.Clone());
            Shuffle();
            UpdateCardCountDisplay();
            FloatingHint.Instance.ShowHint($"{card.cardName} 已加入牌组！");
        }
        RelicManager.Instance.TriggerAllRelics(RelicTriggerType.OnObtainCard, card);
    }


    public void UpgradeRandomCards(int count)
    {
        //从主牌库中筛选出可以升级的牌
        //TODO：替换为加强版CardData
        List<CardData> candidates = allCards.FindAll(c => c.rarity != CardRarity.Curse);

        if (candidates.Count == 0) return;

        int upgradeCount = 0;
        while (upgradeCount < count && candidates.Count > 0)
        {
            int idx = Random.Range(0, candidates.Count);
            CardData card = candidates[idx];

            Debug.Log($"升级了卡牌：{card.cardName}");
            FloatingHint.Instance.ShowHint($"卡牌 {card.cardName} 获得了强化！");

            candidates.RemoveAt(idx);
            upgradeCount++;
        }
    }

    public void DrawStapleCards(int count)
    {
        StartCoroutine(DrawStapleCardsCoroutine(count));
    }

    private IEnumerator DrawStapleCardsCoroutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            //在抽牌堆找主食
            CardData targetCard = drawPile.Find(c => c.tags.Contains(TagType.Ingredient));


            //找到了就抽上来
            if (targetCard != null)
            {
                drawPile.Remove(targetCard);
                handManager.AddCardToHand(targetCard);
                UpdateCardCountDisplay();
            }
            else
            {
                FloatingHint.Instance.ShowHint("抽牌堆里没有主食了！");
                break;
            }

            yield return new WaitForSeconds(0.2f); //抽牌间隔
        }
    }

    public void PlayerTurnStart()
    {
        //基础抽牌数+额外抽牌数
        int totalDraw = DrawNumber + battleManager.extraDrawsNextTurn;
        if (battleManager.extraDrawsNextTurn > 0)
        {
            FloatingHint.Instance.ShowHint($"板蓝根生效！额外抽取 {battleManager.extraDrawsNextTurn} 张牌");
            battleManager.extraDrawsNextTurn = 0;
        }
        DrawCard(totalDraw);
        UpdateCardCountDisplay();
    }

    public void PlayerTurnEnd()
    {
        if ( battleManager.currentState == BattleManager.BattleState.PlayerTurn )
        {
            battleManager.ChangeState(BattleManager.BattleState.EndTurn);
        }
    }

}
