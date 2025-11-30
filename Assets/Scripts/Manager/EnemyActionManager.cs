using UnityEngine;
using System.Collections.Generic;

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

    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealthStars>();
        deckManager = FindObjectOfType<DeckManager>();
        potManager = FindObjectOfType<PotManager>();
        battleManager = FindObjectOfType<BattleManager>();
        enemyUI = FindObjectOfType<EnemyHealthSlider>();
    }

    public void InitEnemy(EnemyData data)
    {
        currentEnemyData = data;
        currentShield = 0;
        actionIndex = 0;
        PlanNextAction(); //进场直接规划第一回合
    }

    //规划下回合意图
    public void PlanNextAction()
    {
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

        //TODO: 调用 UI 更新意图图标
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

    //执行当前规划的动作 (在 EnemyTurn 调用)
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
}