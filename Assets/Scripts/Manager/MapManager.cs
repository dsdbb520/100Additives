using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

public class MapManager : MonoBehaviour
{
    [Header("界面")]
    public GameObject mapPanel;
    public GameObject battlePanel;
    public GameObject nodeButtonPrefab;
    public RectTransform nodesContainer;
    public GameObject globalHUD;
    public RestManager restManager;
    public DeckManager deckManager;
    public ShopManager shopManager;
    public CardData rottenLeafCard;

    [Header("UI控制")]
    public TextMeshProUGUI boilingCountdownText; //沸腾倒计时
    public Button challengeBossButton;  //挑战boss
    public Button resetViewButton;    //复原视图

    [Header("地图生成设定")]
    public float hexSize = 60f;
    public int mapRadius = 3;
    public int ring1EventNum = 2;

    [Header("沸腾阶段")]
    public int foundKeyIngredients = 0; //已看到的主菜数量
    public int collectedKeyIngredients = 0; //已拾取的主菜数量
    public bool isBoiling = false; //是否处于沸腾阶段
    public int stepsInBoiling = 0; //沸腾阶段已走的步数
    public int maxBoilingSteps = 8; //倒计时上限

    private Dictionary<HexCoordinates, MapNode> mapGrid = new Dictionary<HexCoordinates, MapNode>();
    private Dictionary<MapNode, GameObject> nodeObjMap = new Dictionary<MapNode, GameObject>();

    private MapNode playerCurrentNode;

    private Vector2 lastMousePosition; //上一帧鼠标位置
    private bool isDragging = false;   //是否正在拖拽
    private bool isProcessing = false; //交互锁

    void Start()
    {
        if (boilingCountdownText != null) boilingCountdownText.gameObject.SetActive(false);
        if (challengeBossButton != null)
        {
            challengeBossButton.gameObject.SetActive(false);
            challengeBossButton.onClick.AddListener(() => ForceStartBossBattle("主动出击！上菜！"));
        }
        if (resetViewButton != null)
        {
            resetViewButton.onClick.AddListener(ResetViewToPlayer);
        }


        GenerateHexMap();
    }

    private void Update()
    {


        //右键拖拽地图
        if (Input.GetMouseButtonDown(1))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
            if (nodesContainer != null) nodesContainer.DOKill();
        }
        if (Input.GetMouseButton(1) && isDragging)
        {
            if (nodesContainer != null)
            {
                Vector2 currentMousePosition = Input.mousePosition;
                Vector2 diff = currentMousePosition - lastMousePosition;

                //移动速度（灵敏度）
                nodesContainer.anchoredPosition += diff;

                lastMousePosition = currentMousePosition;
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }
    }


    #region 生成地图相关
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
            FocusOnNode(startNode, 0f);
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
        if (ringNodes[1].Count > 0)
        {
            List<MapNode> r1 = ringNodes[1];
            int count = r1.Count;
            List<NodeType> typePool = new List<NodeType>();

            //设定逻辑：战斗数量 = 总数 - 事件数量
            int eventCount = ring1EventNum;
            int battleCount = count - eventCount;

            // 填充池子
            for (int i = 0; i < battleCount; i++) typePool.Add(NodeType.Battle);
            for (int i = 0; i < eventCount; i++) typePool.Add(NodeType.Event);

            //打乱池子
            typePool = ShuffleList(typePool);

            //分配给格子
            for (int i = 0; i < count; i++)
            {
                //防止越界（万一地图格子数不对）
                if (i < typePool.Count)
                    r1[i].type = typePool[i];
                else
                    r1[i].type = NodeType.Battle; //默认填充战斗
            }
        }

        FillRingWithWeights(ringNodes[2], 2);
        FillRingWithWeights(ringNodes[3], 3);
        FillRingWithWeights(ringNodes[4], 4);

        //统计当前地图上所有的商店数量
        int shopCount = mapGrid.Values.Count(n => n.type == NodeType.Shop);
        int minShops = 2;

        if (shopCount < minShops)
        {
            Debug.Log($"生成时商店不足 ({shopCount}个)，正在强制补充...");
            List<MapNode> candidates = new List<MapNode>();
            candidates.AddRange(ringNodes[2].Where(n => n.type == NodeType.Battle));
            candidates.AddRange(ringNodes[3].Where(n => n.type == NodeType.Battle));
            candidates = ShuffleList(candidates);

            //补充商店直到满足最小数量
            int needed = minShops - shopCount;
            for (int i = 0; i < needed; i++)
            {
                if (i < candidates.Count)
                {
                    candidates[i].type = NodeType.Shop;
                    Debug.Log($"已将节点 {candidates[i].coordinates} 强制转换为 Shop");
                }
            }
        }

        //特殊放置: 3个主菜食材 (覆盖掉刚才生成的)
        //则: 距离 3-4-5，尽量分散。这里先用简易版：
        //在Ring3或4里随机找3个距离较远的格子变成KeyIngredient
        List<MapNode> outerNodes = new List<MapNode>();
        outerNodes.AddRange(ringNodes[3]);
        outerNodes.AddRange(ringNodes[4]);

        //简单随机选3个
        List<MapNode> keySpots = ShuffleList(outerNodes).Take(3).ToList();
        foreach (var node in keySpots) node.type = NodeType.KeyIngredient;
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

        if (ring == 1) // 环1（暂时弃用，使用固定生成逻辑）
        {
            if (rnd < 50) return NodeType.Battle;
            if (rnd < 60) return NodeType.Rest;
            if (rnd < 75) return NodeType.Shop;
            return NodeType.Event;
        }
        else if (ring == 2) //环2 (过渡)
        {
            //Battle 45, Elite 5, Rest 20, Shop 10, Event 20
            if (rnd < 45) return NodeType.Battle;
            if (rnd < 50) return NodeType.Elite;
            if (rnd < 70) return NodeType.Rest;
            if (rnd < 80) return NodeType.Shop;
            return NodeType.Event;
        }
        else if (ring == 3) //环3 (风险)
        {
            //Battle 45, Elite 20, Rest 10, Shop 10, Event 15
            if (rnd < 45) return NodeType.Battle;
            if (rnd < 65) return NodeType.Elite;
            if (rnd < 75) return NodeType.Rest;
            if (rnd < 85) return NodeType.Shop;
            return NodeType.Event;
        }
        else //环4 (危险)
        {
            //Battle 40, Elite 25, Rest 5, Shop 5, Event 25
            if (rnd < 40) return NodeType.Battle;
            if (rnd < 65) return NodeType.Elite;
            if (rnd < 70) return NodeType.Rest;
            if (rnd < 75) return NodeType.Shop;
            return NodeType.Event;
        }
    }
    #endregion

    //更新整个地图的显示
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
                if (mapGrid.ContainsKey(neighborCoord))
                {
                    MapNode neighborNode = mapGrid[neighborCoord];

                    //如果这是新发现的格子
                    if (!neighborNode.isVisible)
                    {
                        neighborNode.isVisible = true;
                        if (neighborNode.type == NodeType.KeyIngredient)
                        {
                            //只有当它还没被计入过（避免反复看同一个加计数）
                            //每次重新统计一遍视野内的主菜数量
                        }
                    }
                }
            }
        }

        //重新统计视野内的主菜数量
        int currentVisibleKeys = mapGrid.Values.Count(n => n.isVisible && n.type == NodeType.KeyIngredient);

        //如果看到了3个，且还没沸腾则触发沸腾
        if (!isBoiling && currentVisibleKeys >= 3)
        {
            TriggerBoilingPhase();
        }

        foreach (var node in mapGrid.Values) UpdateNodeVisual(node);

    }

    //聚焦当前视角
    public void FocusOnNode(MapNode node, float duration = 0.5f)
    {
        if (nodesContainer == null) return;
        Vector2 targetPos = -node.worldPosition;
        //平滑移动
        nodesContainer.DOKill(); //打断之前的移动
        nodesContainer.DOAnchorPos(targetPos, duration).SetEase(Ease.OutCubic);
    }

    //重置视角
    public void ResetViewToPlayer()
    {
        if (playerCurrentNode != null)
        {
            //打断当前的拖拽状态
            isDragging = false;
            FocusOnNode(playerCurrentNode, 0.5f);
            FloatingHint.Instance.ShowHint("视角已重置");
        }
    }

    void TriggerBoilingPhase()
    {
        isBoiling = true;
        stepsInBoiling = 0;

        FloatingHint.Instance.ShowHint("BOSS已嗅到食物香气，开始躁动！！！");
        FloatingHint.Instance.ShowHint("【沸腾阶段】敌人变强！8步后强制BOSS战！");

        //强化敌人
        BattleManager battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null) battleManager.enemyStatMultiplier *= 1.15f; // 提升 15%

        //视觉反馈
        if (mapPanel.GetComponent<Image>() != null)
            mapPanel.GetComponent<Image>().color = new Color(1f, 0.8f, 0.8f);

        //显示倒计时
        if (boilingCountdownText != null)
        {
            boilingCountdownText.gameObject.SetActive(true);
            UpdateBoilingUI();
        }
    }

    void UpdateBoilingUI()
    {
        if (boilingCountdownText != null)
        {
            int remaining = maxBoilingSteps - stepsInBoiling;
            boilingCountdownText.text = $"沸腾倒计时：{remaining} 步";

            if (remaining <= 3) boilingCountdownText.color = Color.red;
            else boilingCountdownText.color = Color.white;
        }
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

        playerCurrentNode = targetNode;
        UpdateVisibility();
        FocusOnNode(targetNode);

        if (isBoiling)
        {
            stepsInBoiling++;
            int remaining = maxBoilingSteps - stepsInBoiling;
            FloatingHint.Instance.ShowHint($"BOSS 正在逼近...");
            UpdateBoilingUI();
            if (remaining <= 0)
            {
                ForceStartBossBattle("你逃不掉了！强制进入 BOSS 战！");
                return; //强制打断后续逻辑
            }
        }
        if (targetNode.isExplored)
        {
            //已经是回头路了，无事发生
            return;
        }
        //如果是新探索的格子，进入处理流程
        StartCoroutine(HandleNodeInteraction(targetNode));
    }

    IEnumerator HandleNodeInteraction(MapNode node)
    {
        if (node.type == NodeType.Battle || node.type == NodeType.Elite)
        {
            // 普通战斗
            EnterBattle();
            FindObjectOfType<BattleManager>().StartNormalBattle(); // 确保重置 Boss 标记
            yield break;
        }
        else if (node.type == NodeType.KeyIngredient)
        {
            //拾取主菜
            collectedKeyIngredients++;
            FloatingHint.Instance.ShowHint($"获得主菜食材！({collectedKeyIngredients}/3)");
            FloatingHint.Instance.ShowHint($"捡到一个主菜，Boss 属性应削弱 (当前已收集: {collectedKeyIngredients})");
            yield return new WaitForSeconds(1.0f);
            FinishCurrentNode();
            if (collectedKeyIngredients >= 3)
            {
                FloatingHint.Instance.ShowHint("食材已集齐！随时可以点击按钮挑战 Boss！");
                if (challengeBossButton != null)
                {
                    challengeBossButton.gameObject.SetActive(true);
                    challengeBossButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
                }
            }
        }
        else if (node.type == NodeType.Rest)
        {
            //休息逻辑保持不变
            mapPanel.SetActive(false);
            restManager.OpenRestSite();
            FinishCurrentNode();
        }
        else if (node.type == NodeType.Shop)
        {
            mapPanel.SetActive(false);

            shopManager.OpenShop();
        }
        else if (node.type == NodeType.Event)
        {
            mapPanel.SetActive(false);

            EventManager.Instance.StartRandomEvent();

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
        BattleManager battleManager = FindObjectOfType<BattleManager>();
    }

    public void FinishCurrentNode()
    {
        //结算通用逻辑
        if (playerCurrentNode != null)
        {
            playerCurrentNode.isExplored = true;
        }

        battlePanel.SetActive(false);
        mapPanel.SetActive(true);
        UpdateVisibility();
    }

    void ForceStartBossBattle(string msg)
    {
        FloatingHint.Instance.ShowHint(msg);

        mapPanel.SetActive(false);
        battlePanel.SetActive(true);
        globalHUD.SetActive(true);

        BattleManager battleManager = FindObjectOfType<BattleManager>();
        EnterBattle();
        battleManager.StartBossBattle();
    }
}