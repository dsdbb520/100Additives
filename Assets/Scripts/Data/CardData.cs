using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardData", menuName = "CardData")]
public class CardData : ScriptableObject
{
    [Header("基本信息")]
    public string cardName;
    public Sprite icon;
    public CardRarity rarity;   //品质
    [TextArea] public string description;

    [Header("参数")]
    public int cost;
    public List<TagType> tags;
    public CardTargetType targetType;
    public bool isUnplayable; //是否无法打出
    public bool exhaustOnPlay; //打出后消耗

    [Header("数值")]
    // 区分伤害类型 
    public int phyDamage;
    public int menDamage;
    public int selfDamage;   //对玩家造成的伤害
    public int shieldValue;
    public int healValue;
    public int pressure;

    [Header("特殊效果")]
    //用于处理各种效果
    public string specialEffectID;


    public bool isFrozen;

    //克隆方法，返回一个新的副本
    public CardData Clone()
    {
        CardData clone = Instantiate(this);  //克隆ScriptableObject
        clone.cardName = this.cardName;
        clone.cost = this.cost;
        clone.rarity = this.rarity;
        clone.pressure = this.pressure;
        clone.phyDamage = this.phyDamage;
        clone.menDamage = this.menDamage;
        clone.selfDamage = this.selfDamage;
        clone.healValue = this.healValue;
        clone.shieldValue = this.shieldValue;
        clone.targetType = this.targetType;
        clone.isUnplayable = this.isUnplayable;
        clone.exhaustOnPlay = this.exhaustOnPlay;
        clone.tags = new List<TagType>(this.tags);
        clone.description = this.description;
        clone.icon = this.icon;
        clone.specialEffectID = this.specialEffectID;
        clone.isFrozen = this.isFrozen;
        return clone;
    }
}
