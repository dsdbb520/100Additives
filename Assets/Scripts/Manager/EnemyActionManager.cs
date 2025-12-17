using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyActionManager : MonoBehaviour
{
    public EnemyData currentEnemyData;
    public EnemyAction nextAction; //下回合打算干的事

    private int actionIndex = 0;

    private PlayerHealthStars playerHealth;
    private DeckManager deckManager;
    private PotManager potManager;
    private BattleManager battleManager;
    private EnemyHealthSlider enemyUI; //用来显示护盾

    //敌人的临时状态
    private float currentShield = 0;
    private float strengthBuff = 0; //力量加成

    [Header("Boss数值")]
    public float heavyAttackDamage = 25f;

    //Boss强力技阈值
    private List<float> hpThresholds;

    //记录哪些阈值已经被禁用
    private HashSet<float> disabledThresholds = new HashSet<float>();

    //记录哪些阈值已经触发过
    private HashSet<float> triggeredThresholds = new HashSet<float>();

    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealthStars>();
        deckManager = FindObjectOfType<DeckManager>();
        potManager = FindObjectOfType<PotManager>();
        battleManager = FindObjectOfType<BattleManager>();
        enemyUI = FindObjectOfType<EnemyHealthSlider>();
    }

    public void InitEnemy(EnemyData data, bool isBoss, int weaknessStacks)
    {
        currentEnemyData = data;
        disabledThresholds.Clear();
        triggeredThresholds.Clear();
        hpThresholds.Clear();

        currentShield = 0;
        actionIndex = 0;

        //插入boss强力技
        if (isBoss)
        {
            int attacksRemaining = Mathf.Max(0, 3 - weaknessStacks);
            switch (attacksRemaining)
            {
                case 3:
                    //0主菜：25%, 50%, 75%
                    hpThresholds.Add(0.75f);
                    hpThresholds.Add(0.50f);
                    hpThresholds.Add(0.25f);
                    break;

                case 2:
                    //1主菜：33%, 66%
                    hpThresholds.Add(0.66f);
                    hpThresholds.Add(0.33f);
                    break;

                case 1:
                    //2主菜：50%
                    hpThresholds.Add(0.50f);
                    break;

                case 0:
                    //3主菜：没有强力攻击
                    Debug.Log("BOSS 极度虚弱！强力攻击已完全移除！");
                    break;
            }
        }

        //DEBUG用
        if (hpThresholds.Count > 0)
        {
            string thresholdsStr = string.Join(", ", hpThresholds.Select(v => $"{v * 100}%"));
            Debug.Log($"Boss 初始化完毕。当前强力攻击阈值: [{thresholdsStr}]");
        }
        PlanNextAction(); //进场直接规划第一回合
    }

    //规划下回合意图
    public void PlanNextAction()
    {

        if (CheckForThresholdTrigger())
        {
            return;
        }
        if (currentEnemyData.actions.Count == 0) return;

        if (currentEnemyData.isSequential)
        {
            //按顺序循环
            nextAction = currentEnemyData.actions[actionIndex];
            actionIndex = (actionIndex + 1) % currentEnemyData.actions.Count;
        }
        else
        {
            //按权重随机
            nextAction = GetRandomAction();
        }

        Debug.Log($"敌人意图: {nextAction.actionName} ({nextAction.intentType})");

        //TODO: 调用UI更新意图图标
        FindObjectOfType<EnemyIntentUI>().UpdateIntent(nextAction);
    }

    private EnemyAction GetRandomAction()
    {
        int totalWeight = 0;
        foreach (var act in currentEnemyData.actions) totalWeight += act.weight;

        int rnd = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var act in currentEnemyData.actions)
        {
            currentWeight += act.weight;
            if (rnd < currentWeight) return act;
        }
        return currentEnemyData.actions[0];
    }

    //执行当前规划的动作
    public void ExecuteAction()
    {
        if (nextAction == null) return;

        FloatingHint.Instance.ShowHint($"敌人使用了 {nextAction.actionName}!");

        switch (nextAction.intentType)
        {
            case EnemyIntentType.Attack:
                // 伤害 = 基础值 + 力量Buff
                float damage = nextAction.value + strengthBuff;
                playerHealth.TakeDamage(damage);

                FloatingHint.Instance.ShowHint($"受到 {damage} 点伤害！");
                break;

            case EnemyIntentType.Defend:
                currentShield += nextAction.value;
                //TODO: 需要在 EnemyHealthSlider 里加护盾显示
                FloatingHint.Instance.ShowHint($"敌人获得 {nextAction.value} 护盾");
                break;

            case EnemyIntentType.Buff:
                strengthBuff += nextAction.value;
                FloatingHint.Instance.ShowHint($"敌人力量增加 {nextAction.value}");
                break;

            case EnemyIntentType.Debuff:
                HandleDebuff(nextAction);
                break;

            case EnemyIntentType.Special:
                HandleSpecial(nextAction);
                break;
        }
    }

    void HandleDebuff(EnemyAction action)
    {
        if (action.actionName.Contains("加压"))
        {
            // potManager.AddPressure(action.value); 
            FloatingHint.Instance.ShowHint("锅里压力增大了！");
        }
    }

    void HandleSpecial(EnemyAction action)
    {
        if (action.statusCard != null)
        {
            for (int i = 0; i < (int)action.value; i++)
            {
                deckManager.discardPile.Add(action.statusCard);
            }
            FloatingHint.Instance.ShowHint("牌库被污染了！");
        }
    }

    //敌人受伤逻辑 (含护盾抵消)
    public float TakeDamage(float incomingDamage)
    {
        if (currentShield > 0)
        {
            if (currentShield >= incomingDamage)
            {
                currentShield -= incomingDamage;
                return 0; //全挡住了
            }
            else
            {
                incomingDamage -= currentShield;
                currentShield = 0;
                return incomingDamage; //剩下的伤害
            }
        }
        return incomingDamage;
    }


    //检查血量是否触发大招
    private bool CheckForThresholdTrigger()
    {
        //获取当前Boss血量百分比
        float currentHpPercent = battleManager.enemyCurrentHealth / battleManager.enemyMaxHealth;

        foreach (float threshold in hpThresholds)
        {
            //条件1: 血量低于阈值
            //条件2: 这个阈值没有被禁用
            //条件3: 这个阈值还没触发过
            if (currentHpPercent <= threshold &&
                !disabledThresholds.Contains(threshold) &&
                !triggeredThresholds.Contains(threshold))
            {
                triggeredThresholds.Add(threshold); //标记为已触发

                //临时生成一个强力攻击动作
                nextAction = new EnemyAction();
                nextAction.actionName = "暴怒重击";
                nextAction.intentType = EnemyIntentType.Attack; //显示攻击意图
                nextAction.value = heavyAttackDamage; //设置高额伤害
                nextAction.description = $"生命值低于 {threshold * 100}% 触发的强力攻击！";

                Debug.LogWarning($"BOSS 触发阈值 {threshold * 100}%！释放强力攻击！");

                //更新UI并返回true
                FindObjectOfType<EnemyIntentUI>().UpdateIntent(nextAction);
                return true;
            }
        }
        return false;
    }
}