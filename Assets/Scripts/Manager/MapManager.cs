using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class MapManager : MonoBehaviour
{
    public GameObject mapPanel;
    public GameObject battlePanel;
    public GameObject nodeButtonPrefab;
    public RectTransform nodesContainer;
    private MapNode currentNode;
    private int unlockedLayerIndex = 0;
    private bool isProcessing = false;
    private Dictionary<MapNode, Image> nodeImageMap = new Dictionary<MapNode, Image>();

    public List<MapNode> currentLayerNodes = new List<MapNode>();

    void Start()
    {
        GenerateSimpleMap();
    }


    void GenerateSimpleMap()
    {
        // 清空旧的 UI
        foreach (Transform child in nodesContainer) Destroy(child.gameObject);
        nodeImageMap.Clear();
        unlockedLayerIndex = 0;
        isProcessing = false;
        //生成 5 层
        for (int i = 0; i < 5; i++)
        {
            NodeType type = NodeType.Battle;
            if (i == 2) type = NodeType.Rest; //第3层休息
            if (i == 4) type = NodeType.Boss; //第5层 Boss

            MapNode node = new MapNode(i, 0, type);
            CreateNodeButton(node, i);
        }
        nodesContainer.anchoredPosition = Vector2.zero;
    }

    void CreateNodeButton(MapNode node, int yPos)
    {
        GameObject buttonObj = Instantiate(nodeButtonPrefab, nodesContainer);

        Image buttonImage = buttonObj.GetComponent<Image>();

        if (buttonImage != null)
        {
            nodeImageMap.Add(node, buttonImage);
        }

        buttonObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos * 200);

        buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = node.type.ToString();

        buttonObj.GetComponent<Button>().onClick.AddListener(() => OnNodeClicked(node));
    }

    void OnNodeClicked(MapNode node)
    {
        if (node.isCleared)
        {
            FloatingHint.Instance.ShowHint("这个区域已经探索过了！");
            return;
        }
        if (node.layerIndex > unlockedLayerIndex)
        {
            FloatingHint.Instance.ShowHint("太远了，还没走到那里！");
            return;
         }
        if (isProcessing)
        {
            //如果正在处理，给一个视觉反馈，但是不执行逻辑
            if (nodeImageMap.ContainsKey(node))
            {
                Transform buttonTrans = nodeImageMap[node].transform;
                buttonTrans.DOComplete();
                buttonTrans.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1);
            }
            return;
        }

        currentNode = node;
        Debug.Log($"进入节点: {node.type}");
        if (node.type == NodeType.Battle || node.type == NodeType.Boss)
        {
            EnterBattle();
        }
        else
        {
            //非战斗节点（休息、商店等）：直接在地图上自动结算
            StartCoroutine(ProcessNonBattleNode(node));
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
        if (currentNode != null)
        {
            currentNode.isCleared = true;
            UpdateNodeVisuals(); // 刷新一下UI显示
        }
        int nextLayer = currentNode.layerIndex + 1;
        unlockedLayerIndex = nextLayer;
        if (nextLayer < 5)
        {
            FocusOnLayer(nextLayer);
        }
        currentNode = null;
    }

    System.Collections.IEnumerator ProcessNonBattleNode(MapNode node)
    {
        FloatingHint.Instance.ShowHint($"正在探索 {node.type}...");

        isProcessing = true;

        yield return new WaitForSeconds(1.0f);

        node.isCleared = true;
        UpdateNodeVisuals();

        int nextLayer = node.layerIndex + 1;
        unlockedLayerIndex = nextLayer;

        if (nextLayer < 5)
        {
            FocusOnLayer(nextLayer);
        }

        currentNode = null; // 清空当前节点
        FloatingHint.Instance.ShowHint($"{node.type} 探索完毕！");
        isProcessing = false;
    }

    void UpdateNodeVisuals()
    {
        foreach (var pair in nodeImageMap)
        {
            MapNode node = pair.Key;
            Image img = pair.Value;

            if (node.isCleared)
            {
                img.color = Color.gray; //打过的变灰

                img.GetComponent<Button>().interactable = false; 
            }
            else if (node.layerIndex == unlockedLayerIndex)
            {
                img.color = Color.white;
                img.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                img.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }
    }

    // 平滑移动地图
    public void FocusOnLayer(int layerIndex)
    {
        float spacing = 200f;

        float targetY = -(layerIndex * spacing);

        nodesContainer.DOAnchorPosY(targetY, 0.5f).SetEase(Ease.InOutQuad);
    }



}