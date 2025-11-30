using UnityEngine;
using System.Collections.Generic;



[System.Serializable]
public class EnemyAction
{
    public string actionName;
    public EnemyIntentType intentType;
    public float value; //攻击力/护盾值/压力值/塞卡数量
    [TextArea] public string description; //鼠标悬停显示的描述

    //如果是塞卡技能，塞哪种卡
    public CardData statusCard;

    //权重 (用于随机选择)
    public int weight = 10;
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float maxPhyHP;
    public float maxMenHP;
    public List<TagType> weaknessTags;
    public List<TagType> resistTags;

    [Header("Actions")]
    public List<EnemyAction> actions = new List<EnemyAction>();
    //是否按顺序行动
    public bool isSequential = false;
}
