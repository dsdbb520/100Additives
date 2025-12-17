using UnityEngine;

public enum TagType
{
    None,
    Ingredient, //现有标签
    Seasoning,  //佐料
    Spicy,
    Sweet,
    Hard,
    Toxic,
    Fluid,
    Curse
}

public enum CardRarity
{
    Common,    //普通（白）
    Uncommon,  //上等（蓝）
    Rare,      //极品（金）
    Special,   //特殊（紫）
    Curse      //诅咒（灰）
}

public enum CardTargetType
{
    BigPot,      //只能放大锅
    SmallStove,  //只能放小灶
    Dual,        //双用
    None         //无目标
}

public enum EnemyIntentType
{
    Attack,
    Defend,
    Buff,
    Debuff,
    Special, 
    Stun,
    Unknown 
}

public static class RarityUtils
{
    public static Color GetColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return Color.white; //白
            case CardRarity.Uncommon:
                return new Color(0.2f, 0.6f, 1f); //亮蓝
            case CardRarity.Rare:
                return new Color(1f, 0.84f, 0f); //金
            case CardRarity.Special:
                return new Color(0.8f, 0.4f, 1f); //紫
            case CardRarity.Curse:
                return new Color(0.7f, 0.2f, 0.2f); //黑灰色
            default:
                return Color.white;
        }
    }

    //获取富文本用的颜色代码
    public static string GetColorHex(CardRarity rarity)
    {
        Color col = GetColor(rarity);
        return $"#{ColorUtility.ToHtmlStringRGB(col)}";
    }
}