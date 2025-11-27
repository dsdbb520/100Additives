using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public GameObject mapPanel;
    public GameObject battlePanel;
    public GameObject nodeButtonPrefab;
    public Transform nodesContainer;

    public List<MapNode> currentLayerNodes = new List<MapNode>();

    void Start()
    {
        GenerateSimpleMap();
    }


    void GenerateSimpleMap()
    {
        // 清空旧的 UI
        foreach (Transform child in nodesContainer) Destroy(child.gameObject);

        //生成 5 层
        for (int i = 0; i < 5; i++)
        {
            NodeType type = NodeType.Battle;
            if (i == 2) type = NodeType.Rest; //第3层休息
            if (i == 4) type = NodeType.Boss; //第5层 Boss

            MapNode node = new MapNode(i, 0, type);
            CreateNodeButton(node, i);
        }
    }

    void CreateNodeButton(MapNode node, int yPos)
    {
        GameObject buttonObj = Instantiate(nodeButtonPrefab, nodesContainer);

        buttonObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos * 200);

        buttonObj.GetComponentInChildren<Text>().text = node.type.ToString();

        buttonObj.GetComponent<Button>().onClick.AddListener(() => OnNodeClicked(node));
    }

    void OnNodeClicked(MapNode node)
    {
        Debug.Log($"进入节点: {node.type}");

        if (node.type == NodeType.Battle || node.type == NodeType.Boss)
        {
            EnterBattle();
        }
        else
        {
            FindObjectOfType<FloatingHint>().ShowHint("这里是 " + node.type);
        }
    }

    public void EnterBattle()
    {
        mapPanel.SetActive(false);
        battlePanel.SetActive(true);
        FindObjectOfType<BattleManager>().ChangeState(BattleManager.BattleState.GameStart);
    }

    public void ReturnToMap() //战斗胜利后调用
    {
        battlePanel.SetActive(false);
        mapPanel.SetActive(true);
    }
}