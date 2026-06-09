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
    PostBattle,      //战斗结算后
    OnPutIntoPot,    //放入大锅时
    OnPutIntoStove,  //放入小灶时
    OnServeSuccess,  //上菜成功时
    OnObtainCard,    //获得卡牌时
    OnExhaust,       //消耗卡牌时
    OnHeal
}

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance { get; private set; }

    [Header("玩家拥有的遗物")]
    public List<RelicData> ownedRelics = new List<RelicData>();

    [Header("UI引用")]
    public Transform relicBarContainer; //存放遗物图标的父物体
    public GameObject relicIconPrefab;  //遗物图标预制体

    public bool returnNow = false;

    //引用其他管理器
    private BattleManager battleManager;
    private DeckManager deckManager;
    private PlayerHealthStars playerHealth;
    private EnemyActionManager enemyManager;
    public PotManager potManager;
    public SmallStoveManager stoveManager;

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
            //太奶的老花镜: 战斗开始基础压力降低10
            case "GrandmaGlasses":
                if (type == RelicTriggerType.BattleStart)
                {
                    potManager.AddDirectPressure(-10f);
                    FloatingHint.Instance.ShowHint("老花镜：大锅压力降低！");
                }
                break;

            //不锈钢铁碗: 每次放入小灶，护盾+3
            case "SteelBowl":
                if (type == RelicTriggerType.OnPutIntoStove)
                {
                    playerHealth.AddShield(3);
                    FloatingHint.Instance.ShowHint("铁碗：护盾+3");
                }
                break;

            //备用打火机: 每场战斗第一回合，燃气+1
            case "SpareLighter":
                if (type == RelicTriggerType.BattleStart)
                {
                    battleManager.currentEnergy += 1;
                    battleManager.UpdateEnergyUI();
                    FloatingHint.Instance.ShowHint("打火机：燃气+1");
                }
                break;

            //陈年包浆: 成功上菜，回2HP
            case "AgedPatina":
                if (type == RelicTriggerType.OnServeSuccess)
                {
                    playerHealth.Heal(2);
                    FloatingHint.Instance.ShowHint("陈年包浆：回血+2");
                }
                break;

            //止吐手环: 获得烂菜叶时33%概率销毁
            case "AntiVomit":
                if (type == RelicTriggerType.OnObtainCard)
                {
                    CardData card = context as CardData;
                    if (card != null && (card.cardName.Contains("烂菜叶") || card.cardName.Contains("泔水")))
                    {
                        if (Random.value < 0.33f)
                        {
                            deckManager.allCards.Remove(card);
                            deckManager.UpdateCardCountDisplay();
                            FloatingHint.Instance.ShowHint("止吐手环生效：销毁了垃圾牌！");
                        }
                    }
                }
                break;
            //变态辣油: 放入【辣】牌，压力+10%，全场真伤5
            case "ChiliOil":
                if (type == RelicTriggerType.OnPutIntoPot)
                {
                    CardData card = context as CardData;
                    if (card != null && card.tags.Contains(TagType.Spicy))
                    {
                        potManager.AddDirectPressure(10f);
                        if(card.phyDamage>0) battleManager.DealPhyDamageFromEffect(5);
                        if(card.menDamage>0) battleManager.DealMenDamageFromEffect(5);
                        playerHealth.TakeDamage(5);
                        FloatingHint.Instance.ShowHint("辣油触发：真伤5！");
                    }
                }
                break;

            //金刚假牙: 放入【硬】牌，伤害增加50%当前护盾
            case "DiamondDentures":
                if (type == RelicTriggerType.OnPutIntoPot)
                {
                    CardData card = context as CardData;
                    if (card != null && card.tags.Contains(TagType.Hard))
                    {
                        int bonus = Mathf.FloorToInt(playerHealth.currentShield * 0.5f);
                        card.phyDamage += bonus; //修改当前卡牌的伤害
                        FloatingHint.Instance.ShowHint($"金刚假牙：伤害+{bonus}");
                    }
                }
                break;

            //陈年老卤: 放入【毒】牌，触发【入锅时】两次
            case "AgedBrine":
                if (type == RelicTriggerType.OnPutIntoPot)
                {
                    CardData card = context as CardData;
                    if (card != null && card.tags.Contains(TagType.Toxic))
                    {
                        SpecialEffectManager.Instance.ApplyEffect(card.specialEffectID, card, false, EffectTriggerPhase.OnAdd);
                        FloatingHint.Instance.ShowHint("老卤：双倍入味！");
                    }
                }
                break;

            //工业搅拌机: 放入【液体】牌，抽2张
            case "IndustrialMixer":
                if (type == RelicTriggerType.OnPutIntoPot)
                {
                    CardData card = context as CardData;
                    if (card != null && card.tags.Contains(TagType.Fluid))
                    {
                        deckManager.DrawCard(2);
                        FloatingHint.Instance.ShowHint("搅拌机：抽2张");
                    }
                }
                break;

            //胰岛素泵: 治疗溢出转护盾
            case "InsulinPump":
                if (type == RelicTriggerType.OnHeal)
                {
                    // context 传入 {overflowAmount}
                    if (context is float overflow && overflow > 0)
                    {
                        playerHealth.AddShield(overflow * 2);
                        FloatingHint.Instance.ShowHint($"胰岛素泵：转化护盾 {overflow * 2}");
                    }
                }
                break;

            //沼气转化器: 消耗垃圾牌时，回费
            case "BiogasConverter":
                if (type == RelicTriggerType.OnExhaust)
                {
                    CardData card = context as CardData;
                    if (card != null && (card.cardName.Contains("烂菜叶") || card.cardName.Contains("泔水")))
                    {
                        battleManager.currentEnergy += 1;
                        battleManager.UpdateEnergyUI();
                        FloatingHint.Instance.ShowHint("沼气转化：燃气+1");
                    }
                }
                break;

            //双槽鸳鸯锅: 小灶上限+1，首张0费
            //这是一个被动效果，需要修改 SmallStoveManager
            case "DualHotpot":
                // 逻辑主要在 SmallStoveManager 里查询 HasRelic
                break;
        }
    }

    public int GetMaxEnergyModifier()
    {
        int bonus = 0;
        if (HasRelic("Battery")) bonus += 1; //电池：燃气+1
        return bonus;
    }

    public float GetEnemyMaxHpMultiplier()
    {
        float multiplier = 1.0f;
        if (HasRelic("ShrinkRay")) multiplier *= 0.8f; //缩小射线：敌人血量上限变为 80%
        return multiplier;
    }

    public bool HasRelic(string id)
    {
        return ownedRelics.Exists(r => r.relicID == id);
    }
}