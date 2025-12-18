using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum BattleType
{
    Normal,
    Elite,
    Boss
}

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("配置")]
    public List<CardData> allCardsDatabase;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        allCardsDatabase = Resources.LoadAll<CardData>("Cards").ToList();
    }

    //生成卡牌奖励池
    public List<CardData> GenerateCardRewardPool(BattleType battleType)
    {
        List<CardData> rewardPool = new List<CardData>();
        List<CardData> validCards = new List<CardData>();

        //排除诅咒牌和不可打出的牌
        var sourcePool = allCardsDatabase.Where(c => c.rarity != CardRarity.Curse && !c.isUnplayable).ToList();

        for (int i = 0; i < 6; i++) //生成6张------------------------如果需要修改战斗胜利的卡牌数量，从这里调
        {
            CardRarity targetRarity = GetRarityByBattleType(battleType);

            //筛选对应稀有度的牌
            var candidates = sourcePool.Where(c => c.rarity == targetRarity).ToList();

            //如果该稀有度没牌（防止空池），就降级找
            if (candidates.Count == 0) candidates = sourcePool;

            if (candidates.Count > 0)
            {
                CardData picked = candidates[Random.Range(0, candidates.Count)];
                rewardPool.Add(picked);
            }
        }
        return rewardPool;
    }

    //计算金币
    public int CalculateGoldReward(BattleType battleType)
    {
        switch (battleType)
        {
            case BattleType.Normal: return Random.Range(10, 21); //10-20
            case BattleType.Elite: return Random.Range(25, 36);  //25-35
            case BattleType.Boss: return Random.Range(90, 111);  //90-110
            default: return 10;
        }
    }

    //概率算法
    private CardRarity GetRarityByBattleType(BattleType type)
    {
        float rnd = Random.value * 100f;

        if (type == BattleType.Boss)
        {
            return CardRarity.Rare; //Boss  100%极品
        }
        else if (type == BattleType.Elite)
        {
            // 普通:上等:极品 50%:35%:15%
            if (rnd < 50) return CardRarity.Common;
            if (rnd < 85) return CardRarity.Uncommon;
            return CardRarity.Rare;
        }
        else
        {
            // 普通:上等:极品  70%:25%:5%
            if (rnd < 70) return CardRarity.Common;
            if (rnd < 95) return CardRarity.Uncommon; 
            return CardRarity.Rare;
        }
    }
}