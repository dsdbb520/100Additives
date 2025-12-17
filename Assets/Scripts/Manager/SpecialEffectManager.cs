using System.Linq;
using UnityEngine;

//效果触发时机
public enum EffectTriggerPhase
{
    OnDraw,    //抽到时
    OnHandTurnEnd,   //回合结束还在手里时
    OnAdd,   //放入锅中时
    OnServe  //上菜时
}

public class SpecialEffectManager : MonoBehaviour
{
    public static SpecialEffectManager Instance { get; private set; }

    private bool activeGasMask = false;

    public PotManager potManager;
    public BattleManager battleManager;
    public HandManager handManager;
    public DeckManager deckManager;
    public MapManager mapManager;
    public PlayerHealthStars playerHealth;
    public SmallStoveManager smallStoveManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //检测压力增长
    public void OnPressureIncreased()
    {
        if (activeGasMask)
        {
            playerHealth.AddShield(2);
            FloatingHint.Instance.ShowHint("防毒面具：护盾+2");
        }
    }

    /// <summary>
    /// 执行特殊效果
    /// </summary>
    /// <param name="effectID">卡牌的效果名</param>
    /// <param name="cardSource">哪一张牌</param>
    /// <param name="isSmallStove">是否是小灶的效果</param>
    /// <param name="phase">当前触发的阶段</param>
    public void ApplyEffect(string effectID, CardData cardSource, bool isSmallStove, EffectTriggerPhase phase)
    {
        if (string.IsNullOrEmpty(effectID)) return;
        if (isSmallStove && phase == EffectTriggerPhase.OnServe) return;
        var enemyManager = FindObjectOfType<EnemyActionManager>();

        switch (effectID)
        {
            // 螺蛳粉（上菜时：敌方全体5精神伤害，压力+20%）
            case "SnailNoodle":
                if (phase == EffectTriggerPhase.OnServe)
                {
                    battleManager.DealMenDamageFromEffect(5);
                    potManager.AddDirectPressure(20f);
                    FloatingHint.Instance.ShowHint("螺蛳粉：入味伤害5点！");
                }
                break;

            //撒尿牛丸（上菜时：50%双倍伤害，50%自伤5且压力+15%）
            case "BeefBall":
                if (phase == EffectTriggerPhase.OnServe)
                {
                    if (Random.value < 0.5f)
                    {
                        cardSource.phyDamage *= 2; //修改副本，仅本次有效
                        FloatingHint.Instance.ShowHint("撒尿牛丸：爆浆！伤害翻倍");
                    }
                    else
                    {
                        playerHealth.TakeDamage(5);
                        potManager.AddDirectPressure(15f);
                        FloatingHint.Instance.ShowHint("撒尿牛丸：烫嘴了！受到5点伤害");
                    }
                }
                break;

            //奥利给 (上菜时：玩家掉10HP)
            case "Shit":
                if (phase == EffectTriggerPhase.OnServe)
                {
                    playerHealth.TakeDamage(10);
                    FloatingHint.Instance.ShowHint("奥利给：呕...San值狂掉");
                }
                break;

            //干冰刺身（上菜时：压力减少20%）
            case "DryIce":
                if (phase == EffectTriggerPhase.OnServe)
                {
                    potManager.AddDirectPressure(-20f);
                    FloatingHint.Instance.ShowHint("干冰降温：压力-20%");
                }
                break;

            //微波炉鸡蛋（加入时：炸锅概率+20%）
            case "MicrowaveEgg": 
                if (phase == EffectTriggerPhase.OnAdd)
                { 
                    potManager.explosionChanceModifier += 0.2f;
                    FloatingHint.Instance.ShowHint("微波炉鸡蛋：炸锅风险激增！");
                }
                break;

            //惠灵顿拖鞋（上菜时: 获得5盾，压力+5%）
            case "WellingtonSlipper":
                if (phase == EffectTriggerPhase.OnServe)
                {
                    playerHealth.AddShield(5);
                    potManager.AddDirectPressure(5f);
                }
                break;

            //福尔马林凤爪：吸血
            case "FormalinChickenFeet": 
                if (phase == EffectTriggerPhase.OnAdd)
                {
                    battleManager.lifestealActive = true;
                }
                break;

            // 魔鬼辣椒（上菜: 伤害倍率+0.5，压力+25%）
            case "GhostPepper": 
                if (phase == EffectTriggerPhase.OnServe)
                { 
                    potManager.ModifyHeatMultiplier(0.5f);
                    potManager.AddDirectPressure(25f);
                }
                break;

            //板蓝根泡面（下一回合多抽1张）
            case "IsatisRoot": 
                if (phase == EffectTriggerPhase.OnServe)
                {
                    battleManager.extraDrawsNextTurn += 1;
                    FloatingHint.Instance.ShowHint("板蓝根：下回合多抽1张牌");
                }
                break;


            case "PickledCabbage": //老坛酸菜（塞烂菜叶给弃牌堆）
                if (phase == EffectTriggerPhase.OnServe)
                {
                    if (mapManager.rottenLeafCard != null)
                    {
                        deckManager.discardPile.Add(mapManager.rottenLeafCard.Clone());
                        FloatingHint.Instance.ShowHint("老坛酸菜：弃牌堆增加了烂菜叶");
                    }
                }
                break;

            //一滴香（大锅x1.5/小灶翻倍）
            case "OneDropFragrance": 
                if (isSmallStove)
                { 
                    battleManager.doubleStapleDamage = true;
                    FloatingHint.Instance.ShowHint("一滴香(小灶)：增鲜！");
                }
                else if (phase == EffectTriggerPhase.OnAdd)
                {
                    potManager.ModifyHeatMultiplier(0.5f);
                }
                break;

            //增稠剂（锅中已有的流体牌伤害+5，压力+5%）
            case "Thickener": 
                if (phase == EffectTriggerPhase.OnAdd)
                {
                    foreach (var c in potManager.cookingPot)
                    {
                        if (c.tags.Contains(TagType.Fluid)) 
                        { 
                            c.phyDamage += 5;
                            c.pressure += 5;
                        }
                    }
                    FloatingHint.Instance.ShowHint("增稠剂：流体变强了");
                }
                break;

            //苏丹红（所有牌变辣。压力+20%）
            case "SudanRed": 
                if (phase == EffectTriggerPhase.OnAdd)
                {
                    foreach (var c in potManager.cookingPot)
                    {
                        if (!c.tags.Contains(TagType.Spicy)) c.tags.Add(TagType.Spicy);
                    }
                    FloatingHint.Instance.ShowHint("苏丹红：全锅变辣！");
                }
                break;

            //防腐剂：压力不自然增长
            case "Preservative": 
                if (phase == EffectTriggerPhase.OnAdd)
                {
                    potManager.stopPressureGrowth = true;
                }
                break;

            case "Cyclamate": //甜蜜素（大锅:伤-20% / 小灶:回5HP）
                if (isSmallStove)
                {
                    playerHealth.Heal(5);
                }
                else if (phase == EffectTriggerPhase.OnAdd)
                {
                    potManager.ModifyHeatMultiplier(-0.2f);
                }
                break;

            //地沟油（燃气+2）
            case "GutterOil": 
                if (phase == EffectTriggerPhase.OnAdd)
                {
                    battleManager.currentEnergy += 2;
                    battleManager.UpdateEnergyUI();
                }
                break;

            //膨大剂（抽2张主食）
            case "SwellingAgent": 
                if (phase == EffectTriggerPhase.OnAdd)
                {
                    deckManager.DrawStapleCards(2);
                    FloatingHint.Instance.ShowHint("膨大剂：检索主食2张！");
                }
                break;

            //起搏器（小灶:回满费,失10HP）
            case "Pacemaker": 
                if (isSmallStove)
                {
                    battleManager.currentEnergy = battleManager.maxEnergy;
                    battleManager.UpdateEnergyUI();
                    playerHealth.TakeDamage(10);
                }
                break;

            //安全阀（小灶: 免一次炸锅伤害）
            case "SafetyValve": 
                if (isSmallStove)
                {
                    potManager.ignoreExplosionDamage = true;
                    FloatingHint.Instance.ShowHint("安全阀已安装");
                }
                break;


            case "Coke": //冰阔落：回3HP
                if (isSmallStove)
                {
                    playerHealth.Heal(3);
                }
                break;


            case "ExpiredMilk": // 过期牛奶：移除Debuff, 失3HP
                if (isSmallStove)
                {
                    playerHealth.TakeDamage(3);
                    playerHealth.ClearDebuffs();
                    FloatingHint.Instance.ShowHint("过期牛奶：以毒攻毒");
                }
                break;

            //健胃消食片：消耗手牌泔水回血
            case "DigestionTablet": 
                if (isSmallStove)
                {
                    int count = 0;
                    //找出所有要消耗的卡
                    var trashCards = handManager.handCards.Where(c => c.tags.Contains(TagType.Curse)).ToList();

                    if (trashCards.Count == 0)
                    {
                        FloatingHint.Instance.ShowHint("手里没有垃圾可以消化...");
                    }
                    //遍历删除
                    foreach (var c in trashCards)
                    {
                        handManager.ExhaustCard(c);
                        count++;
                    }
                    if (count > 0)
                    {
                        playerHealth.Heal(count * 5);
                        FloatingHint.Instance.ShowHint($"消食片：消化了 {count} 张垃圾");
                    }
                }
                break;

            //偷来的外卖：盾8, 抽1
            case "StolenTakeout": 
                if (isSmallStove)
                {
                    playerHealth.AddShield(8);
                    deckManager.DrawCard(1);
                }
                break;

            //肾上腺素：费+1
            case "Adrenaline": 
                if (isSmallStove)
                {
                    battleManager.currentEnergy += 1;
                    battleManager.UpdateEnergyUI();
                }
                break;


            case "Appetizer": //开胃菜：下张入锅牌-1费
                if (isSmallStove)
                {
                    battleManager.nextCardCostModifier = -1;
                    FloatingHint.Instance.ShowHint("开胃菜：下张牌便宜");
                }
                break;


            case "ActivatedCarbon": //活性炭：移除敌人Buff
                if (isSmallStove)
                {
                    if (enemyManager != null && enemyManager.strengthBuff > 0)
                    {
                        enemyManager.strengthBuff = 0; //暂时写成移除力量
                        FloatingHint.Instance.ShowHint("活性炭：吸走了敌人的力量");
                    }
                }
                break;


            case "Vape": //电子烟：回5HP，锅压+5%
                if (isSmallStove)
                {
                    playerHealth.Heal(5);
                    potManager.AddDirectPressure(5f);
                }
                break;


            case "GasMask": //防毒面具（被动：压力加时加盾）
                if (isSmallStove)
                {
                    activeGasMask = true; //激活本场战斗的被动
                    FloatingHint.Instance.ShowHint("防毒面具装备中...");
                }
                break;


            case "PowerPill": //大力丸：主食翻倍, 失15HP
                if (isSmallStove)
                {
                    battleManager.doubleStapleDamage = true;
                    playerHealth.TakeDamage(15);
                }
                break;


            case "Detergent": //洗洁精：移除锅里最后一张
                if (isSmallStove)
                {
                    potManager.RemoveLastCard(handManager);
                    potManager.UpdateTotalPressure();
                }
                break;


            case "WashFace": //甚至洗了手:回20HP，塞烂菜叶
                if (isSmallStove)
                {
                    playerHealth.Heal(20);
                    if (mapManager.rottenLeafCard != null)
                        deckManager.drawPile.Add(mapManager.rottenLeafCard.Clone());
                    deckManager.Shuffle();   //注意：这里放完回重新洗牌，以后加入控顶类牌需要注意
                    deckManager.UpdateCardCountDisplay();
                }
                break;

            //头发（下张牌+1费）
            case "Hair": 
                if (phase == EffectTriggerPhase.OnAdd)
                { 
                    battleManager.nextCardCostModifier = 1;
                    FloatingHint.Instance.ShowHint("头发：恶心！下张牌变贵！");
                }
                break;

            //烂菜叶：抽到时压力+10
            case "RottenLeaf":
                if (phase == EffectTriggerPhase.OnDraw)
                {
                    potManager.AddDirectPressure(10f);
                    FloatingHint.Instance.ShowHint("抽到烂菜叶！压力+10%");
                }
                break;

            //苍蝇：回合结束在手牌扣3血
            case "Fly":
                if (phase == EffectTriggerPhase.OnHandTurnEnd)
                {
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(3);
                        FloatingHint.Instance.ShowHint("苍蝇叮了你！-3 HP");
                    }
                }
                break;
            //麻辣烫基底：本锅中每有一张 辣 牌,额外造成5点伤害，压力+10%
            case "SpicyBase":
                if (phase == EffectTriggerPhase.OnServe)
                {
                    int spicyCount = potManager.CountTagsInPot(TagType.Spicy);
                    potManager.AddDirectPressure(spicyCount * 10f);
                    int extraDamage = spicyCount * 5;
                    if (extraDamage > 0)
                    {
                        battleManager.DealPhyDamageFromEffect(extraDamage);
                        battleManager.DealMenDamageFromEffect(extraDamage);
                        FloatingHint.Instance.ShowHint($"麻辣烫基底触发！额外 {extraDamage} 伤害");
                    }
                }
                break;

            //压缩饼干：若压力<50%, 伤害翻倍，且压力+5%
            case "CompressedBiscuit":
                if (phase == EffectTriggerPhase.OnServe)
                {
                    if (potManager.GetTotalPressure() < 50f)
                    {
                        cardSource.phyDamage *= 2;
                        potManager.AddDirectPressure(5f);
                        FloatingHint.Instance.ShowHint("压缩饼干伤害翻倍！");
                    }
                }
                break;

            //铀235蛋糕：本锅热度倍率+50%
            case "UraniumCake":
                if (phase == EffectTriggerPhase.OnAdd)
                {
                    potManager.ModifyHeatMultiplier(0.5f);
                    FloatingHint.Instance.ShowHint("反应堆启动！热度倍率+50%");
                }
                break;

            //液氮：大锅当前压力直接归零
            case "LiquidNitrogen":
                if (phase == EffectTriggerPhase.OnAdd)
                {
                    potManager.UpdateTotalPressure();
                    potManager.DisableServing(); // 禁止上菜也是立即生效的限制
                    FloatingHint.Instance.ShowHint("液氮注入！压力清零");
                }
                break;

            //工业明胶：大锅压力不增加/小灶获得护盾
            case "IndustrialGlue":
                if (phase == EffectTriggerPhase.OnAdd)
                {
                    if (isSmallStove) { playerHealth.AddShield(5);FloatingHint.Instance.ShowHint("获得5点护盾"); } 
                    else { potManager.stopPressureGrowth = true; FloatingHint.Instance.ShowHint("加入工业明胶，压力停止增长！"); }
                }
                break;

            //含笑半步癫 (眩晕，压力+30%)
            case "SmileMadness": 
                if (phase == EffectTriggerPhase.OnServe)
                {
                    var enemy = FindObjectOfType<EnemyActionManager>();
                    if (enemy != null) enemy.ApplyStun();
                    potManager.AddDirectPressure(30f);
                }
                break;

            //红伞伞 (中毒，压力+15%)
            case "RedUmbrella":
                if (phase == EffectTriggerPhase.OnServe)
                {
                    var enemy = FindObjectOfType<EnemyActionManager>();
                    if (enemy != null) enemy.ApplyPoison(2);
                    potManager.AddDirectPressure(15f);
                }
                break;

            //九转大肠（虚弱，压力+10%）
            case "Intestine": 
                if (phase == EffectTriggerPhase.OnServe)
                {
                    var enemy = FindObjectOfType<EnemyActionManager>();
                    if (enemy != null) enemy.ApplyAttackDebuff(2);
                    potManager.AddDirectPressure(10f);
                }
                break;
            
            default:
                Debug.LogWarning($"未实现的特殊效果 ID：{effectID}");
                break;
        }
    }
}