using UnityEngine;
using System.Collections.Generic;

public enum NodeType
{
    Start,          // 起点 (Ring 0)
    Battle,         // 普通战斗
    Elite,          // 精英战斗
    Rest,           // 休息
    Event,          // 随机事件
    Shop,           // 商店
    KeyIngredient,  // 主菜食材 (金色蒸汽)
    Boss            // Boss战 (集齐食材后开启，或者作为特殊的占位)
}

[System.Serializable]
public class MapNode
{
    public HexCoordinates coordinates;
    public Vector3 worldPosition;

    public NodeType type;
    public bool isExplored; //是否玩家已经踩过 (踩过后变脏)
    public bool isVisible;  //是否在视野内 (相邻一圈)
    public bool isMessy;    //是否处于狼藉状态

    public MapNode(HexCoordinates coords, NodeType type)
    {
        this.coordinates = coords;
        this.type = type;
        this.isExplored = false;
        this.isVisible = false;
        this.isMessy = false;
    }
}