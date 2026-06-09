using UnityEngine;



[CreateAssetMenu(fileName = "NewRelic", menuName = "Relic Data")]
public class RelicData : ScriptableObject
{
    [Header("基础信息")]
    public string relicID;      //用于逻辑判断的ID
    public string relicName;    //显示名称
    public Sprite icon;         //图标
    [TextArea] public string description; //描述
    public CardRarity rarity;  //稀有度

    [Header("数值参数 (可选)")]
    //一些简单的数值可以直接配在这里
    public int value1;
}