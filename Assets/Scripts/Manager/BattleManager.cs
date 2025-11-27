using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        GameStart,
        Start,
        PlayerTurn,
        Resolution,
        EnemyTurn,
        EndTurn,
        Win,
        Lose
    }

    public BattleState currentState;
    public List<EnemyData> enemyList = new List<EnemyData>();  // 战斗可选敌人列表
    public Button RetryButton;
    private float enemyMaxHealth;
    private float enemyCurrentHealth;
    private DeckManager deckManager;
    private PotManager potManager;  // 引用 PotManager用于获取锅中的卡牌
    private HandManager handManager;
    private PlayerHealthStars playerHealthStars;
    private EnemyData currentEnemy;

    void Start()
    {
        currentState = BattleState.Start;
        Debug.Log("Battle Started!");
        deckManager = FindObjectOfType<DeckManager>();
        potManager = FindObjectOfType<PotManager>();
        handManager = FindObjectOfType<HandManager>();
        ChangeState(BattleState.GameStart);
    }

    public void ChangeState(BattleState newState)
    {
        currentState = newState;
        Debug.Log($"Battle state changed to {currentState}");

        // 根据状态做出不同反应
        switch (currentState)
        {
            case BattleState.GameStart:
                GameStart();
                break;
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
                WinTurn();
                break;
            case BattleState.Lose:
                LoseTurn();
                break;
        }
    }



    #region Start
    private void GameStart()
    {
        // 随机选择一个敌人
        if (enemyList.Count > 0)
        {
            int randomIndex = Random.Range(0, enemyList.Count);
            currentEnemy = enemyList[randomIndex];
            Debug.Log($"Selected Enemy: {currentEnemy.name}");
            enemyMaxHealth = currentEnemy.maxPhyHP;
            enemyCurrentHealth = enemyMaxHealth;
        }
        else
        {
            Debug.LogWarning("Enemy list is empty!");
        }
        ChangeState(BattleState.Start);
    }

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
        // 计算压力总和
        float totalPressure = potManager.UpdateTotalPressure();

        // 判断是否超出 100% 压力
        bool isExplosion = false;
        if (totalPressure > 100f)
        {
            //计算超出的压力部分
            float excessPressure = totalPressure - 100f;

            //计算爆炸概率
            float explosionChance = Mathf.Clamp(excessPressure / 20f, 0f, 1f);

            //随机判断是否发生爆炸
            if (UnityEngine.Random.value < explosionChance)
            {
                isExplosion = true;
                int selfDamage = 1 + Mathf.FloorToInt(excessPressure / 10f);

                playerHealthStars.TakeDamage(selfDamage);
                FindObjectOfType<FloatingHint>().ShowHint($"炸锅了！玩家受到 {selfDamage} 点伤害！");
            }
            if(playerHealthStars.currentHealth <= 0)
            {
                ChangeState(BattleState.Lose);
            }
        }
        if (!isExplosion)
        {
            // 计算伤害
            var (phyDamage, menDamage) = CalculateTotalDamage(potManager.cookingPot, currentEnemy);
            if (phyDamage >= enemyCurrentHealth || menDamage >= enemyCurrentHealth)
            {
                ChangeState(BattleState.Win);
            }
            else
            {
                enemyCurrentHealth -= phyDamage;
                FindObjectOfType<EnemyHealthSlider>().UpdateHealthBars(enemyCurrentHealth / enemyMaxHealth, enemyCurrentHealth / enemyMaxHealth);
                FindObjectOfType<FloatingHint>().ShowHint("本次造成了" + phyDamage.ToString() + "点伤害");
            }
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


    #region Win&Lose
    private void WinTurn()
    {
        FindObjectOfType<FloatingHint>().ShowHint("获得胜利！");
        FindObjectOfType<MapManager>().ReturnToMap();
        
    }

    private void LoseTurn()
    {
        FindObjectOfType<FloatingHint>().ShowHint("获得失败！");
        FindObjectOfType<MapManager>().ReturnToMap();
    }

    #endregion

}
