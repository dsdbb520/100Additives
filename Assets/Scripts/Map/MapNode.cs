using UnityEngine;
using System.Collections.Generic;

public enum NodeType
{
    Battle,
    Elite,
    Rest,
    Event,
    Shop,
    Boss
}

[System.Serializable]
public class MapNode
{
    public int layerIndex; //第几层
    public int nodeIndex;  //这一层的第几个
    public NodeType type;  //节点类型
    public bool isCleared = false;
    [System.NonSerialized]
    public List<MapNode> nextNodes = new List<MapNode>(); //下一层节点

    public MapNode(int layer, int index, NodeType type)
    {
        this.layerIndex = layer;
        this.nodeIndex = index;
        this.type = type;
    }
}