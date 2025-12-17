using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public float enemyMaxHealth;
    public float enemyCurrentHealth;

    [Header("能量")]
    public int maxEnergy = 3;     //每回合最大费用
    public int currentEnergy;     //当前费用
    public TextMeshProUGUI energyText;

    [Header("BOSS设置")]
    public List<EnemyData> bossList = new List<EnemyData>(); //Boss列表
    public bool isBossBattle = false; //当前是否是 Boss 战

    [Header("难度设置")]
    public float enemyStatMultiplier = 1.0f;  //敌人属性倍率（为难度系统做铺垫）

    [Header("奖励设置")]
    public int minGoldReward = 15;
    public int maxGoldReward = 25;

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

        //应用难度倍率
        if (currentEnemy != null)
        {
            enemyMaxHealth = currentEnemy.maxPhyHP * enemyStatMultiplier;
            enemyCurrentHealth = enemyMaxHealth;
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
        currentEnergy = maxEnergy;
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
        enemyActionManager.ExecuteAction();
        yield return new WaitForSeconds(1.0f);
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
                FloatingHint.Instance.ShowHint($"炸锅了！玩家受到 {selfDamage} 点伤害！");
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
            var (phyDamage, menDamage) = CalculateTotalDamage(potManager.cookingPot, currentEnemy);
            if (phyDamage >= enemyCurrentHealth || menDamage >= enemyCurrentHealth)
            {
                FindObjectOfType<EnemyHealthSlider>().UpdateHealthBars(0, 0);
                FloatingHint.Instance.ShowHint("本次造成了" + phyDamage.ToString() + "点伤害");
                yield return new WaitForSeconds(1.0f);
                ChangeState(BattleState.Win);
                yield break;
            }
            else
            {
                enemyCurrentHealth -= phyDamage;
                FindObjectOfType<EnemyHealthSlider>().UpdateHealthBars(enemyCurrentHealth / enemyMaxHealth, enemyCurrentHealth / enemyMaxHealth);
                FloatingHint.Instance.ShowHint("本次造成了" + phyDamage.ToString() + "点伤害");
            }
            int shieldGain = potManager.cookingPot.Count;
            if (shieldGain > 0)
            {
                playerHealthStars.AddShield(shieldGain);
                FloatingHint.Instance.ShowHint($"获得 {shieldGain} 点护盾！");
            }
        }
        yield return new WaitForSeconds(1.0f);
        deckManager.discardPile.AddRange(potManager.cookingPot);
        potManager.ClearPot();
        ChangeState(BattleState.PlayerTurn);
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
        float finalPhy = enemyActionManager.TakeDamage(totalPhyDamage);
        float finalMen = enemyActionManager.TakeDamage(totalMenDamage); //护盾通用
        Debug.Log($"Total Physical Damage: {totalPhyDamage}");
        Debug.Log($"Total Mental Damage: {totalMenDamage}");
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
        float baseReward = Random.Range(minGoldReward, maxGoldReward + 1);
        int finalReward = Mathf.RoundToInt(baseReward * enemyStatMultiplier);
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGold(finalReward);
        }

        FloatingHint.Instance.ShowHint($"战斗胜利！获得 {finalReward} 金币");
        Debug.Log($"Battle Won. Reward: {baseReward} * {enemyStatMultiplier} = {finalReward}");
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
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            UpdateEnergyUI();
            return true;
        }
        else
        {
            return false;
        }
    }



    private void Update()
    {

        //Debug按键：秒杀敌人
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (currentEnemy != null && enemyCurrentHealth > 1)
            {
                ChangeState(BattleState.Win);
                FloatingHint.Instance.ShowHint("【DEBUG】获得胜利！");
            }
        }
    }

}
