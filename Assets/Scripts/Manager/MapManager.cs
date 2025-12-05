using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using System.Linq;

public class MapManager : MonoBehaviour
{
    [Header("References")]
    public GameObject mapPanel;
    public GameObject battlePanel;
    public GameObject nodeButtonPrefab;
    public RectTransform nodesContainer;
    public GameObject globalHUD;
    public RestManager restManager;
    public DeckManager deckManager;
    public CardData rottenLeafCard;

    [Header("Settings")]
    public float hexSize = 60f;
    public int mapRadius = 3;

    private Dictionary<HexCoordinates, MapNode> mapGrid = new Dictionary<HexCoordinates, MapNode>();
    private Dictionary<MapNode, GameObject> nodeObjMap = new Dictionary<MapNode, GameObject>();

    private MapNode playerCurrentNode;

    private bool isProcessing = false; //交互锁

    void Start()
    {
        GenerateHexMap();
    }

    //生成网格结构
    void GenerateHexMap()
    {
        foreach (Transform child in nodesContainer) Destroy(child.gameObject);
        mapGrid.Clear();
        nodeObjMap.Clear();
        isProcessing = false;

        //生成基础 3 环
        for (int x = -mapRadius; x <= mapRadius; x++)
        {
            for (int y = -mapRadius; y <= mapRadius; y++)
            {
                int z = -x - y;
                if (Mathf.Abs(z) <= mapRadius)
                {
                    // 先默认设为 Battle，稍后统一刷内容
                    CreatePlaceholderNode(new HexCoordinates(x, y));
                }
            }
        }

        //生成第 4 环不规则边缘 (保持之前的逻辑)
        List<HexCoordinates> ring3 = mapGrid.Keys.Where(c => HexCoordinates.Distance(c, HexCoordinates.Zero) == 3).ToList();
        int extraNodesCount = Random.Range(6, 12);

        for (int i = 0; i < extraNodesCount; i++)
        {
            if (ring3.Count == 0) break;
            HexCoordinates randomEdge = ring3[Random.Range(0, ring3.Count)];
            HexCoordinates[] neighbors = HexCoordinates.GetNeighbors(randomEdge);

            foreach (var neighbor in neighbors)
            {
                if (!mapGrid.ContainsKey(neighbor))
                {
                    CreatePlaceholderNode(neighbor);
                    break;
                }
            }
        }

        //根据规则填充节点类型
        AssignNodeTypes();

        //居中 & 初始化
        nodesContainer.anchoredPosition = Vector2.zero;
        if (mapGrid.ContainsKey(HexCoordinates.Zero))
        {
            MapNode startNode = mapGrid[HexCoordinates.Zero];
            startNode.isExplored = true;
            playerCurrentNode = startNode;
            UpdateVisibility();
        }
    }

    void CreatePlaceholderNode(HexCoordinates coords)
    {
        if (mapGrid.ContainsKey(coords)) return;

        MapNode node = new MapNode(coords, NodeType.Battle); // 默认类型
        node.worldPosition = coords.ToWorldPosition(hexSize);
        mapGrid.Add(coords, node);

        GameObject btnObj = Instantiate(nodeButtonPrefab, nodesContainer);
        btnObj.GetComponent<RectTransform>().anchoredPosition = node.worldPosition;
        nodeObjMap.Add(node, btnObj);

        btnObj.GetComponent<Button>().onClick.AddListener(() => OnNodeClicked(node));
        UpdateNodeVisual(node);
    }


    void AssignNodeTypes()
    {
        //先把所有格子按环分类
        var ringNodes = new Dictionary<int, List<MapNode>>();
        for (int i = 0; i <= 4; i++) ringNodes[i] = new List<MapNode>();

        foreach (var node in mapGrid.Values)
        {
            int dist = HexCoordinates.Distance(node.coordinates, HexCoordinates.Zero);
            if (dist <= 4) ringNodes[dist].Add(node);
        }

        //Ring 0: 起点
        foreach (var node in ringNodes[0]) node.type = NodeType.Start;

        //Ring 1: 安全区
        //规则: 至少1个休息，1个商店/事件
        if (ringNodes[1].Count > 0)
        {
            List<MapNode> r1 = ShuffleList(ringNodes[1]); // 洗牌

            //强制放置
            if (r1.Count > 0) r1[0].type = NodeType.Rest;
            if (r1.Count > 1) r1[1].type = NodeType.Shop;

            //剩下的按权重随机
            for (int i = 2; i < r1.Count; i++)
            {
                r1[i].type = GetRandomTypeForRing(1);
            }
        }

        FillRingWithWeights(ringNodes[2], 2);
        FillRingWithWeights(ringNodes[3], 3);
        FillRingWithWeights(ringNodes[4], 4);

        //特殊放置: 3个主菜食材 (覆盖掉刚才生成的)
        // 则: 距离 3-4-5，尽量分散。这里先用简易版：
        //在Ring3或4里随机找3个距离较远的格子变成KeyIngredient
        List<MapNode> outerNodes = new List<MapNode>();
        outerNodes.AddRange(ringNodes[3]);
        outerNodes.AddRange(ringNodes[4]);

        // 简单随机选 3 个 (后续可以写更复杂的角度判断算法)
        List<MapNode> keySpots = ShuffleList(outerNodes).Take(3).ToList();
        foreach (var node in keySpots) node.type = NodeType.KeyIngredient;

        // 刷新所有 UI 图标
        foreach (var node in mapGrid.Values) UpdateNodeVisual(node);
    }

    void FillRingWithWeights(List<MapNode> nodes, int ringIndex)
    {
        foreach (var node in nodes)
        {
            node.type = GetRandomTypeForRing(ringIndex);
        }
    }

    //配置概率
    NodeType GetRandomTypeForRing(int ring)
    {
        float rnd = Random.value * 100f; // 0-100

        if (ring == 1) // 环1 (安全)
        {
            // Battle 50, Rest 10, Shop 15, Event 25
            if (rnd < 50) return NodeType.Battle;
            if (rnd < 60) return NodeType.Rest;
            if (rnd < 75) return NodeType.Shop;
            return NodeType.Event;
        }
        else if (ring == 2) // 环2 (过渡)
        {
            // Battle 45, Elite 5, Rest 20, Shop 10, Event 20
            if (rnd < 45) return NodeType.Battle;
            if (rnd < 50) return NodeType.Elite;
            if (rnd < 70) return NodeType.Rest;
            if (rnd < 80) return NodeType.Shop;
            return NodeType.Event;
        }
        else if (ring == 3) // 环3 (风险)
        {
            // Battle 45, Elite 20, Rest 10, Shop 10, Event 15
            if (rnd < 45) return NodeType.Battle;
            if (rnd < 65) return NodeType.Elite;
            if (rnd < 75) return NodeType.Rest;
            if (rnd < 85) return NodeType.Shop;
            return NodeType.Event;
        }
        else // 环4 (危险)
        {
            // Battle 40, Elite 25, Rest 5, Shop 5, Event 25
            if (rnd < 40) return NodeType.Battle;
            if (rnd < 65) return NodeType.Elite;
            if (rnd < 70) return NodeType.Rest;
            if (rnd < 75) return NodeType.Shop;
            return NodeType.Event;
        }
    }

    void UpdateVisibility()
    {
        foreach (var node in mapGrid.Values)
        {
            if (node.isExplored) node.isVisible = true;
            else node.isVisible = false;
        }

        List<MapNode> exploredNodes = mapGrid.Values.Where(n => n.isExplored).ToList();
        foreach (var explored in exploredNodes)
        {
            HexCoordinates[] neighbors = HexCoordinates.GetNeighbors(explored.coordinates);
            foreach (var neighborCoord in neighbors)
            {
                if (mapGrid.ContainsKey(neighborCoord)) mapGrid[neighborCoord].isVisible = true;
            }
        }

        foreach (var node in mapGrid.Values) UpdateNodeVisual(node);
    }

    void UpdateNodeVisual(MapNode node)
    {
        if (!nodeObjMap.ContainsKey(node)) return;

        GameObject obj = nodeObjMap[node];
        Image img = obj.GetComponent<Image>();
        TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>();

        if (node.isVisible)
        {
            // 蒸汽状态：根据类型显示不同颜色
            // 金色=主菜, 深红=精英, 浅红=战斗, 蓝色=商店, 绿色=休息, 紫色=事件
            switch (node.type)
            {
                case NodeType.Start: img.color = Color.white; break;
                case NodeType.Battle: img.color = new Color(1f, 0.6f, 0.6f); break; //浅红
                case NodeType.Elite: img.color = new Color(0.8f, 0f, 0f); break;    //深红
                case NodeType.Shop: img.color = new Color(0.4f, 0.6f, 1f); break;   //蓝色
                case NodeType.Rest: img.color = new Color(0.4f, 1f, 0.4f); break;   //绿色
                case NodeType.Event: img.color = new Color(0.8f, 0.4f, 1f); break;  //紫色
                case NodeType.KeyIngredient: img.color = new Color(1f, 0.84f, 0f); break; //金色
            }

            text.text = node.type.ToString();

            if (node.isExplored)
            {
                img.color = Color.gray; //狼藉状态
                text.text += "\n(Done)";
            }
        }
        else
        {
            img.color = Color.black;
            text.text = "???";
        }

        if (playerCurrentNode == node)
        {
            img.color = Color.yellow;
            text.text = "YOU";
        }
    }

    void OnNodeClicked(MapNode targetNode)
    {
        if (!targetNode.isVisible)
        {
            FloatingHint.Instance.ShowHint("迷雾太重，无法通行！");
            return;
        }
        if (isProcessing)
        {
            if (nodeObjMap.ContainsKey(targetNode))
                nodeObjMap[targetNode].transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
            return;
        }

        if (targetNode == playerCurrentNode) return;

        int dist = HexCoordinates.Distance(playerCurrentNode.coordinates, targetNode.coordinates);
        if (targetNode.isExplored || dist > 1)
        {
            FloatingHint.Instance.ShowHint($"瞬移！(烂菜叶 x{CalculateRottenLeafCount(dist)})");
            int count = CalculateRottenLeafCount(dist);

            if (rottenLeafCard != null && deckManager != null)
            {
                for (int i = 0; i < count; i++)
                {
                    deckManager.allCards.Add(rottenLeafCard.Clone());
                }
                FloatingHint.Instance.ShowHint($"瞬移代价：牌库被塞入了 {count} 张烂菜叶！");
            }
            else
            {
                Debug.LogError("烂菜叶卡牌数据未配置，或 DeckManager 丢失！");
            }
        }
        else
        {
            FloatingHint.Instance.ShowHint("正常移动");
        }

        playerCurrentNode = targetNode;
        UpdateVisibility();

        if (targetNode.isExplored)
        {
            //已经是回头路了，无事发生
            return;
        }

        //如果是新探索的格子，进入处理流程
        StartCoroutine(HandleNodeInteraction(targetNode));
    }

    System.Collections.IEnumerator HandleNodeInteraction(MapNode node)
    {
        isProcessing = true;

        if (node.type == NodeType.Battle || node.type == NodeType.Elite || node.type == NodeType.Boss)
        {
            //战斗节点：进战斗 (暂时 Elite 也进普通战斗，之后再改 BattleManager 支持精英)
            if (node.type == NodeType.Elite) FloatingHint.Instance.ShowHint("遭遇精英敌人！(暂时作为普通战斗)");

            EnterBattle();
            // 注意：EnterBattle 会切场景，isProcessing 锁不锁都行，回来会重置
        }
        else if (node.type == NodeType.Rest)
        {
            //休息节点：进休息室
            mapPanel.SetActive(false);
            globalHUD.SetActive(true);
            restManager.OpenRestSite();
        }
        else
        {
            //模拟一个“正在探索”的过程
            FloatingHint.Instance.ShowHint($"正在探索 {node.type}...");
            yield return new WaitForSeconds(1.0f);

            //直接结算
            FloatingHint.Instance.ShowHint($"{node.type} 探索完成！(功能开发中)");
            FinishCurrentNode();
        }

        isProcessing = false;
    }

    //烂菜叶计算公式
    int CalculateRottenLeafCount(int dist)
    {
        if (dist <= 3) return 1;
        if (dist <= 6) return 2;
        return 3;
    }

    //辅助：洗牌列表
    List<T> ShuffleList<T>(List<T> list)
    {
        return list.OrderBy(x => Random.value).ToList();
    }

    public void EnterBattle()
    {
        mapPanel.SetActive(false);
        battlePanel.SetActive(true);
        globalHUD.SetActive(true);
        BattleManager bm = FindObjectOfType<BattleManager>();
        bm.ChangeState(BattleManager.BattleState.GameStart);
    }

    public void FinishCurrentNode()
    {
        //点结算通用逻辑
        if (playerCurrentNode != null)
        {
            playerCurrentNode.isExplored = true;
        }

        battlePanel.SetActive(false);
        mapPanel.SetActive(true);

        UpdateVisibility();
    }
}