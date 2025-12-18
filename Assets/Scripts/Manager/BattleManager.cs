using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
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

    [Header("敌人状态")]
    public float enemyMaxPhyHealth;
    public float enemyCurrentPhyHealth;
    public float enemyMaxMenHealth;
    public float enemyCurrentMenHealth;

    [Header("临时状态")]
    public int nextCardCostModifier = 0; //下一张牌费用修正
    public bool doubleStapleDamage = false; //主食牌伤害翻倍
    public bool lifestealActive = false; //吸血模式
    public int extraDrawsNextTurn = 0; //增加下回合抽牌数

    [Header("能量")]
    public int maxEnergy = 3;     //每回合最大费用
    public int currentEnergy;     //当前费用
    public TextMeshProUGUI energyText;

    [Header("BOSS设置")]
    public List<EnemyData> bossList = new List<EnemyData>(); //Boss列表
    public bool isBossBattle = false; //当前是否是 Boss 战

    [Header("难度设置")]
    public float enemyStatMultiplier = 1.0f;  //敌人属性倍率（为难度系统做铺垫）

    [Header("玩家基础数值")]
    public float basePlayerStrength;

    [Header("组件赋值")]
    public Button RetryButton;
    public DeckManager deckManager;
    public PotManager potManager;
    public HandManager handManager;
    public EnemyActionManager enemyActionManager;
    public SmallStoveManager smallStoveManager;
    public PlayerHealthStars playerHealthStars;
    public EnemyData currentEnemy;



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
        Debug.Log("Battle Started!");

        basePlayerStrength = 0;
        //触发战斗开始遗物
        RelicManager.Instance.TriggerAllRelics(RelicTriggerType.BattleStart);

        if (playerHealthStars != null) playerHealthStars.ClearShield();
        if (handManager != null) handManager.DiscardAllCard(true);
        if (potManager != null) { potManager.ClearPot(); potManager.UpdateTotalPressure(); potManager.ClearPot(); }
        if (deckManager != null) deckManager.ResetDeck();

        currentEnemy = null;
        FloatingHint.Instance.ClearAllHints();
        // 随机选择一个敌人
        if (isBossBattle)
        {
            // Boss 战：从 Boss 列表选
            if (bossList.Count > 0)
            {
                int randomIndex = Random.Range(0, bossList.Count);
                currentEnemy = bossList[randomIndex];
                Debug.Log($"BOSS BATTLE! Selected Boss:{currentEnemy.enemyName}");
            }
            else
            {
                Debug.LogError("Boss list is empty!");
                currentEnemy = enemyList[Random.Range(0, enemyList.Count)];
            }
        }
        else
        {
            int randomIndex = Random.Range(0, enemyList.Count);
            currentEnemy = enemyList[randomIndex];
            Debug.Log($"Selected Enemy: {currentEnemy.name}");
        }

        float hpMult = RelicManager.Instance.GetEnemyMaxHpMultiplier();

        //应用难度倍率
        if (currentEnemy != null)
        {
            //初始化血条、应用血量修正
            enemyMaxPhyHealth = currentEnemy.maxPhyHP * enemyStatMultiplier * hpMult;
            enemyCurrentPhyHealth = enemyMaxPhyHealth;
            enemyMaxMenHealth = currentEnemy.maxMenHP * enemyStatMultiplier * hpMult;
            enemyCurrentMenHealth = enemyMaxMenHealth;
            FindObjectOfType<EnemyHealthSlider>().UpdateHealthBars(1, 1);

            //初始化敌人行动AI
            if (isBossBattle)
            {
                enemyActionManager.InitEnemy(currentEnemy, true, FindObjectOfType<MapManager>().collectedKeyIngredients);
            }
            else
            {
                enemyActionManager.InitEnemy(currentEnemy, false, 0);
            }
        }
        ChangeState(BattleState.Start);
    }

    private void RoundStart()
    {
        deckManager.PlayerTurnStart();

        //应用最大燃气修正
        int extraEnergy = RelicManager.Instance.GetMaxEnergyModifier();
        currentEnergy = maxEnergy + extraEnergy;

        RelicManager.Instance.TriggerAllRelics(RelicTriggerType.TurnStart);
        if (smallStoveManager != null)smallStoveManager.ResetStove();
        UpdateEnergyUI();
        ChangeState(BattleState.PlayerTurn);
    }

    #endregion

    #region PlayerTurn
    private void PlayerTurn()
    {
        Debug.Log("进入玩家回合 (等待操作)");
    }

    #endregion

    #region EnemyTurn
    private void EnemyAttack(EnemyData enemy)
    {
        StartCoroutine(EnemyTurnCoroutine(enemy));
    }
    private IEnumerator EnemyTurnCoroutine(EnemyData enemy)
    {
        //回合开始
        enemyActionManager.OnTurnStart();

        //执行行动（如果没被晕
        enemyActionManager.ExecuteAction();

        yield return new WaitForSeconds(1.0f);

        //回合结束
        enemyActionManager.OnTurnEnd();

        // 检查是否被毒死
        if (enemyCurrentMenHealth <= 0)
        {
            ChangeState(BattleState.Win);
            yield break;
        }
        handManager.DiscardAllCard();
        deckManager.UpdateCardCountDisplay();
        FloatingHint.Instance.ShowHint("回合结束，丢弃所有手牌！");
        yield return new WaitForSeconds(0.5f);
        enemyActionManager.PlanNextAction();
        ChangeState(BattleState.Start);
    }


    #endregion

    #region Resolution
    private void ResolveBattle()
    {
        StartCoroutine(ResolveBattleCoroutine());
    }
    private IEnumerator ResolveBattleCoroutine()
    {
        Debug.Log("Resolution Phase Started");

        FloatingHint.Instance.ShowHint("开始煮菜！");
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(potManager.PlayCookingAnimation(2.5f));
        yield return new WaitForSeconds(2.5f);

        //触发所有卡牌“上菜时”的效果
        foreach (var card in potManager.cookingPot)
        {
            if (!string.IsNullOrEmpty(card.specialEffectID))
            {
                SpecialEffectManager.Instance.ApplyEffect(card.specialEffectID, card, false, EffectTriggerPhase.OnServe);
            }
        }
        yield return new WaitForSeconds(0.5f);

        // 计算压力总和
        float totalPressure = potManager.UpdateTotalPressure();

        // 判断是否超出 100% 压力
        bool isExplosion = false;
        if (totalPressure > 100f)
        {
            //计算超出的压力部分
            float excess = totalPressure - 100f;

            //计算爆炸概率
            float chance = potManager.GetExplosionChance(excess);

            //随机判断是否发生爆炸
            if (UnityEngine.Random.value < chance)
            {
                isExplosion = true;
                if (potManager.ignoreExplosionDamage)
                {
                    FloatingHint.Instance.ShowHint("安全阀生效！免疫炸锅伤害！");
                    potManager.ignoreExplosionDamage = false; //已生效
                }
                else
                {
                    int selfDamage = 1 + Mathf.FloorToInt(excess / 10f);
                    playerHealthStars.TakeDamage(selfDamage);
                    FloatingHint.Instance.ShowHint($"炸锅了！玩家受到{selfDamage}点伤害！");
                }
            }
            if(playerHealthStars.currentHealth <= 0)
            {
                yield return new WaitForSeconds(1.0f);
                ChangeState(BattleState.Lose);
                yield break;
            }
        }

        if (!isExplosion)
        {
            

            // 计算伤害
            var (finalPhy, finalMen) = CalculateTotalDamage(potManager.cookingPot, currentEnemy);

            //执行吸血逻辑
            if (lifestealActive)
            {
                float totalDmg = finalPhy + finalMen;
                int healAmount = Mathf.FloorToInt(totalDmg * 0.5f);
                if (healAmount > 0)
                {
                    playerHealthStars.Heal(healAmount);
                    FloatingHint.Instance.ShowHint($"吸血：恢复{healAmount}点HP");
                }
                lifestealActive = false; //已生效
            }
            enemyCurrentPhyHealth -= finalPhy;
            enemyCurrentMenHealth -= finalMen;

            if (finalPhy > 0 || finalMen > 0)
            {
                RelicManager.Instance.TriggerAllRelics(RelicTriggerType.OnAttack);
            }

            // 限制血量不低于 0 (可选，但这有利于 UI 显示)
            if (enemyCurrentPhyHealth < 0) enemyCurrentPhyHealth = 0;
            if (enemyCurrentMenHealth < 0) enemyCurrentMenHealth = 0;

            // 刷新 UI
            FindObjectOfType<EnemyHealthSlider>().UpdateHealthBars(
                enemyCurrentPhyHealth / enemyMaxPhyHealth,
                enemyCurrentMenHealth / enemyMaxMenHealth
            );

            FloatingHint.Instance.ShowHint($"造成了：物理伤害 {finalPhy} | 精神伤害 {finalMen}");

            // 判断胜利条件：任意一条血归零
            if (enemyCurrentPhyHealth <= 0 || enemyCurrentMenHealth <= 0)
            {
                yield return new WaitForSeconds(1.0f);
                ChangeState(BattleState.Win);
                yield break;
            }
        }

        yield return new WaitForSeconds(1.0f);
        foreach (var card in potManager.cookingPot)
        {
            if (card.exhaustOnPlay)
            {
                //如果是消耗牌，不进弃牌堆
                Debug.Log($"卡牌 {card.cardName} 被消耗了");
            }
            else
            {
                //正常牌进入弃牌堆
                deckManager.discardPile.Add(card);
            }
        }
        potManager.ClearPot();
        doubleStapleDamage = false;  //已生效
        ChangeState(BattleState.PlayerTurn);
    }

    // 计算总伤害
    private (float totalPhyDamage, float totalMenDamage) CalculateTotalDamage(List<CardData> potCards, EnemyData enemy)
    {
        float totalPhyDamage = 0;
        float totalMenDamage = 0;
        //遍历锅中的每张卡牌，计算物理伤害和精神伤害
        foreach (var card in potCards)
        {
            //计算物理伤害
            float phyDamage = CalculateDamage(card.phyDamage, card.tags, enemy);
            //计算精神伤害
            float menDamage = CalculateDamage(card.menDamage, card.tags, enemy);

            //应用力量加成
            phyDamage += basePlayerStrength;
            menDamage += basePlayerStrength;

            //执行双倍伤害效果
            if (doubleStapleDamage && !card.tags.Contains(TagType.Seasoning))
            {
                phyDamage *= 2;
                menDamage *= 2;
            }
            totalPhyDamage += phyDamage;
            totalMenDamage += menDamage;
        }

        //应用伤害倍率
        if (potManager != null && potManager.heatMultiplier != 1.0f)
        {
            totalPhyDamage *= potManager.heatMultiplier;
            totalMenDamage *= potManager.heatMultiplier;
            Debug.Log($"热度倍率生效 x{potManager.heatMultiplier}");
        }

        //物理伤害：会被护盾抵消
        float finalPhy = enemyActionManager.TakePhysicalDamage(totalPhyDamage);

        //精神伤害：直接穿透护盾
        float finalMen = enemyActionManager.TakeMentalDamage(totalMenDamage);

        Debug.Log($"结算伤害 物理： {finalPhy}(原{totalPhyDamage})，精神:{finalMen}(原{totalMenDamage})");
        return (finalPhy, finalMen);

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

    //处理卡牌特效造成的直接伤害
    public void DealMenDamageFromEffect(int amount)
    {
        if (currentEnemy != null)
        {
            //直接扣血
            float actualDamage = enemyActionManager.TakeMentalDamage(amount);
            enemyCurrentMenHealth -= actualDamage;

            FindObjectOfType<EnemyHealthSlider>().UpdateHealthBars(enemyCurrentMenHealth / enemyMaxMenHealth, enemyCurrentMenHealth / enemyMaxMenHealth);

            //检查是否打死
            if (enemyCurrentMenHealth <= 0)
            {
                ChangeState(BattleState.Win);
            }
        }
    }

    public void DealPhyDamageFromEffect(int amount)
    {
        if (currentEnemy != null)
        {
            //直接扣血
            float actualDamage = enemyActionManager.TakePhysicalDamage(amount);
            enemyCurrentPhyHealth -= actualDamage;

            FindObjectOfType<EnemyHealthSlider>().UpdateHealthBars(enemyCurrentPhyHealth / enemyMaxPhyHealth, enemyCurrentPhyHealth / enemyMaxPhyHealth);

            //检查是否打死
            if (enemyCurrentPhyHealth <= 0)
            {
                ChangeState(BattleState.Win);
            }
        }
    }
    #endregion

    #region EndTurn
    private void RoundEnd()
    {
        if (potManager.cookingPot.Count > 0)
        {
            potManager.AddDirectPressure(10f); //增加 10% 压力
            FloatingHint.Instance.ShowHint("锅内余热：压力+10%");
        }
        ChangeState(BattleState.EnemyTurn);
    }


    #endregion

    #region Win&Lose
    private void WinTurn()
    {
        RelicManager.Instance.TriggerAllRelics(RelicTriggerType.PostBattle);
        playerHealthStars.ClearShield();
        FindObjectOfType<MapManager>().FinishCurrentNode();
        
    }

    private void LoseTurn()
    {
        FloatingHint.Instance.ShowHint("获得失败！");
        playerHealthStars.ClearShield();
        FindObjectOfType<MapManager>().FinishCurrentNode();
    }

    #endregion

    public void StartBossBattle()
    {
        isBossBattle = true;
        ChangeState(BattleState.GameStart);
    }

    //外部调用：开启普通/精英战斗
    public void StartNormalBattle(bool isElite = false)
    {
        isBossBattle = false;
        ChangeState(BattleState.GameStart);
    }

    public void UpdateEnergyUI()
    {
        if (energyText != null)
        {
            energyText.text = $"{currentEnergy}/{maxEnergy}";
        }
    }


    //尝试消耗费用，用于检测当前费用是否足够
    public bool TryUseEnergy(int amount)
    {
        //应用修正
        int finalCost = Mathf.Max(0, amount + nextCardCostModifier);
        if (currentEnergy >= finalCost)
        {
            currentEnergy -= finalCost;
            UpdateEnergyUI();

            //消耗掉修正
            if (nextCardCostModifier != 0)
            {
                nextCardCostModifier = 0;
            }
            return true;
        }
        return false;
    }



    private void Update()
    {
        //Debug按键：秒杀敌人
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (currentEnemy != null)
            {
                ChangeState(BattleState.Win);
                FloatingHint.Instance.ShowHint("【DEBUG】获得胜利！");
            }
        }
    }

}
