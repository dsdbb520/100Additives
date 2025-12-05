public enum TagType
{
    None,
    Spicy,
    Sweet,
    Hard,
    Toxic,
    Liquid
}

public enum HideTagType
{
    None,
    Spicy,
    Sweet,
    Hard,
    Toxic,
    Liquid
}

public enum BuffType
{
    DrawPressure, //抽到时增加压力
    Exhaust       //打出后消耗/移除游戏
}

public enum EnemyIntentType
{
    Attack,
    Defend,
    Buff,
    Debuff,
    Special, 
    Unknown 
}