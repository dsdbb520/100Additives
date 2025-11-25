using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        Start,
        PlayerTurn,
        Resolution,
        EnemyTurn,
        EndTurn,
        Win,
        Lose
    }

    public BattleState currentState;
    private DeckManager deckManager;
    private PotManager potManager;  // 引用 PotManager 用于获取锅中的卡牌
    private EnemyData enemyData;    // 引用敌人的数据

    void Start()
    {
        currentState = BattleState.Start;
        Debug.Log("Battle Started!");
        deckManager = FindObjectOfType<DeckManager>();
        potManager = FindObjectOfType<PotManager>();
        enemyData = FindObjectOfType<EnemyData>();   //TODO：改为战斗开始时获取敌人数据
        ChangeState(BattleState.PlayerTurn);
    }

    public void ChangeState(BattleState newState)
    {
        currentState = newState;
        Debug.Log($"Battle state changed to {currentState}");

        // 根据状态做出不同反应
        switch (currentState)
        {
            case BattleState.Start:
                // 初始化战斗
                break;
            case BattleState.PlayerTurn:
                // 玩家回合
                break;
            case BattleState.Resolution:
                ResolveBattle();
                break;
            case BattleState.EnemyTurn:
                EnemyAttack(enemyData);
                break;
            case BattleState.EndTurn:
                break;
            case BattleState.Win:
                // 胜利
                break;
            case BattleState.Lose:
                // 失败
                break;
        }
    }


    #region Resolution
    private void ResolveBattle()
    {
        Debug.Log("Resolution Phase Started");
        // 计算伤害
        var (phyDamage, menDamage) = CalculateTotalDamage(potManager.cookingPot, enemyData);

        if (phyDamage >= enemyData.maxPhyHP || menDamage >= enemyData.maxMenHP)     //TODO：改为CurrentHealth，加入血条更新
        {
            ChangeState(BattleState.Win);
        }
        deckManager.discardPile.AddRange(potManager.cookingPot);
        potManager.ClearPot();

        ChangeState(BattleState.EnemyTurn);
    }

    // 计算总伤害
    private (float totalPhyDamage, float totalMenDamage) CalculateTotalDamage(List<CardData> potCards, EnemyData enemy)
    {
        float totalPhyDamage = 0;
        float totalMenDamage = 0;
        // 遍历锅中的每张卡牌，计算物理伤害和精神伤害
        foreach (var card in potCards)
        {
            // 计算物理伤害
            float phyDamage = CalculateDamage(card.phyDamage, card.tags, enemy);
            totalPhyDamage += phyDamage;
            // 计算精神伤害
            float menDamage = CalculateDamage(card.menDamage, card.tags, enemy);
            totalMenDamage += menDamage;
        }

        // 输出总伤害
        Debug.Log($"Total Physical Damage: {totalPhyDamage}");
        Debug.Log($"Total Mental Damage: {totalMenDamage}");

        return (totalPhyDamage, totalMenDamage);
    }

    // 计算单个卡牌的伤害
    private float CalculateDamage(float baseDamage, List<TagType> ingredientTags, EnemyData enemy)
    {
        float totalDamage = baseDamage;

        // 计算弱点标签
        foreach (var ingredientTag in ingredientTags)
        {
            if (enemy.weaknessTags.Contains(ingredientTag))
            {
                totalDamage *= 1.10f;  // 每匹配到一个弱点标签，伤害增加10%
            }
        }

        // 计算抗性标签
        foreach (var ingredientTag in ingredientTags)
        {
            if (enemy.resistTags.Contains(ingredientTag))
            {
                totalDamage *= 0.95f;  // 每匹配到一个抗性标签，伤害减少5%
            }
        }

        return totalDamage;
    }
    #endregion

    #region EnemyTurn
    private void EnemyAttack(EnemyData enemy)
    {
        Debug.Log("敌人发起了攻击");
        ChangeState(BattleState.EndTurn);
    }


    #endregion

    #region EndTurn


    #endregion

}
