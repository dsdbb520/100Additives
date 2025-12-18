using System.Collections.Generic;
using UnityEngine;


//遗物触发的时机枚举
public enum RelicTriggerType
{
    OnObtain,        //获得时
    BattleStart,     //战斗开始时
    TurnStart,       //回合开始时
    OnAttack,        //造成伤害时
    OnPlayerHurt,    //玩家受伤时
    OnDraw,          //抽牌时
    PostBattle       //战斗结算后
}

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance { get; private set; }

    [Header("玩家拥有的遗物")]
    public List<RelicData> ownedRelics = new List<RelicData>();

    [Header("UI引用")]
    public Transform relicBarContainer; //存放遗物图标的父物体
    public GameObject relicIconPrefab;  //遗物图标预制体

    //引用其他管理器
    private BattleManager battleManager;
    private DeckManager deckManager;
    private PlayerHealthStars playerHealth;
    private EnemyActionManager enemyManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();
        deckManager = FindObjectOfType<DeckManager>();
        playerHealth = FindObjectOfType<PlayerHealthStars>();
    }

    //获得遗物
    public void ObtainRelic(RelicData relic)
    {
        if (ownedRelics.Contains(relic)) return; //避免重复获得

        ownedRelics.Add(relic);

        //生成UI图标
        if (relicBarContainer != null && relicIconPrefab != null)
        {
            GameObject iconObj = Instantiate(relicIconPrefab, relicBarContainer);
            iconObj.GetComponent<UnityEngine.UI.Image>().sprite = relic.icon;
        }

        FloatingHint.Instance.ShowHint($"获得遗物：{relic.relicName}！");

        //立即触发“获得时”效果
        TriggerRelicEffect(RelicTriggerType.OnObtain, relic);
    }

    //触发逻辑
    public void TriggerAllRelics(RelicTriggerType type, object context = null)
    {
        foreach (var relic in ownedRelics)
        {
            TriggerRelicEffect(type, relic, context);
        }
    }

    private void TriggerRelicEffect(RelicTriggerType type, RelicData relic, object context = null)
    {
        switch (relic.relicID)
        {
            //示例：草莓 (获得时最大生命+5)
            case "Strawberry":
                if (type == RelicTriggerType.OnObtain)
                {
                    playerHealth.maxHealth += 5;
                    playerHealth.Heal(5);
                    FloatingHint.Instance.ShowHint("草莓：最大生命 +5");
                }
                break;

            //示例：金刚杵 (战斗开始时，力量+1)
            case "Vajra":
                if (type == RelicTriggerType.BattleStart)
                {
                    // 我们需要在 BattleManager 加一个 baseDamageBonus 变量
                    battleManager.basePlayerStrength += 1;
                    FloatingHint.Instance.ShowHint("金刚杵：力量 +1");
                }
                break;

            //示例：招财猫 (战斗胜利金币+10)
            case "LuckyCat":
                if (type == RelicTriggerType.PostBattle)
                {
                    CurrencyManager.Instance.AddGold(10);
                    FloatingHint.Instance.ShowHint("招财猫：额外金币 +10");
                }
                break;

            //示例：双截棍 (造成伤害时，10%概率额外造成10点伤害)
            case "Nunchaku":
                if (type == RelicTriggerType.OnAttack)
                {
                    if (Random.value < 0.1f) //10% 概率
                    {
                        // 再次造成伤害
                        battleManager.DealPhyDamageFromEffect(10);
                        FloatingHint.Instance.ShowHint("双截棍触发！额外10伤！");
                    }
                }
                break;

            //示例：咖啡 (回合开始多抽1张)
            case "Coffee":
                if (type == RelicTriggerType.TurnStart)
                {
                    deckManager.DrawCard(1);
                    FloatingHint.Instance.ShowHint("咖啡：精力充沛！");
                }
                break;
        }
    }

    //有些遗物是被动生效的（比如最大燃气+1），不适合用Trigger，适合用查询

    public int GetMaxEnergyModifier()
    {
        int bonus = 0;
        if (HasRelic("Battery")) bonus += 1; // 电池：燃气+1
        return bonus;
    }

    public float GetEnemyMaxHpMultiplier()
    {
        float multiplier = 1.0f;
        if (HasRelic("ShrinkRay")) multiplier *= 0.8f; // 缩小射线：敌人血量上限变为 80%
        return multiplier;
    }

    public bool HasRelic(string id)
    {
        return ownedRelics.Exists(r => r.relicID == id);
    }
}