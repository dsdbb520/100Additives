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
    private float enemyMaxHealth;
    private float enemyCurrentHealth;
    private DeckManager deckManager;
    private PotManager potManager;  // 引用 PotManager用于获取锅中的卡牌
    private HandManager handManager;
    public List<EnemyData> enemyList = new List<EnemyData>();  // 战斗可选敌人列表
    private EnemyData currentEnemy;

    void Start()
    {
        currentState = BattleState.Start;
        Debug.Log("Battle Started!");
        deckManager = FindObjectOfType<DeckManager>();
        potManager = FindObjectOfType<PotManager>();
        handManager = FindObjectOfType<HandManager>();
        // 随机选择一个敌人
        if (enemyList.Count > 0)
        {
            int randomIndex = Random.Range(0, enemyList.Count);
            currentEnemy = enemyList[randomIndex];
            Debug.Log($"Selected Enemy: {currentEnemy.name}");
            enemyMaxHealth=currentEnemy.maxPhyHP;
            enemyCurrentHealth=enemyMaxHealth;
        }
        else
        {
            Debug.LogWarning("Enemy list is empty!");
        }
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
                RoundStart();
                break;
            case BattleState.PlayerTurn:
                PlayerTurn();
                break;
            case BattleState.EndTurn:
                RoundEnd();
                break;
            case BattleState.EnemyTurn:
                EnemyAttack(currentEnemy);
                break;
            case BattleState.Resolution:
                ResolveBattle();
                break;
            case BattleState.Win:
                // 胜利
                break;
            case BattleState.Lose:
                // 失败
                break;
        }
    }



    #region RoundStart
    private void RoundStart()
    {
        ChangeState(BattleState.PlayerTurn);
    }

    #endregion

    #region PlayerTurn
    private void PlayerTurn()
    {
        deckManager.PlayerTurnStart();
        Debug.Log("进入玩家回合");
    }

    #endregion

    #region EnemyTurn
    private void EnemyAttack(EnemyData enemy)
    {
        Debug.Log("敌人发起了攻击");
        handManager.DiscardAllCard();
        deckManager.UpdateCardCountDisplay();
        Debug.Log("回合结束，丢弃所有手牌");
        ChangeState(BattleState.Start);
    }


    #endregion

    #region Resolution
    private void ResolveBattle()
    {
        Debug.Log("Resolution Phase Started");
        // 计算伤害
        var (phyDamage, menDamage) = CalculateTotalDamage(potManager.cookingPot, currentEnemy);
        if (phyDamage >= enemyCurrentHealth || menDamage >= enemyCurrentHealth)
        {
            ChangeState(BattleState.Win);
        }
        else
        {
            enemyCurrentHealth -= phyDamage;
            FindObjectOfType<EnemyHealthSlider>().UpdateHealthBars(enemyCurrentHealth, enemyCurrentHealth);
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



    #region EndTurn
    private void RoundEnd()
    {
        ChangeState(BattleState.EnemyTurn);
    }


    #endregion

}
