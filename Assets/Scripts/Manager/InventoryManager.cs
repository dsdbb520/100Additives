using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI面板")]
    public GameObject inventoryPanel;
    public Button closeButton;

    [Header("按钮")]
    public Button deckTabButton;
    public Button potionTabButton;
    public Button relicTabButton;
    public Button inventoryButton;

    //选中时的颜色 / 未选中的颜色
    public Color selectedTabColor = Color.white;
    public Color normalTabColor = Color.gray;

    [Header("内容区域")]
    public GameObject deckContent;   //卡组ScrollView
    public GameObject potionContent; //药水ScrollView
    public GameObject relicContent;  //遗物ScrollView

    [Header("卡组显示")]
    public Transform cardGridParent; //卡牌生成的父节点 (Content)
    public GameObject cardPrefab;    //卡牌预制体
    public TextMeshProUGUI deckCountText; //显示卡牌数量

    [Header("其他显示")]
    public Transform potionGridParent;
    public Transform relicGridParent;
    //以后可以在这里加PotionPrefab, RelicPrefab

    public DeckManager deckManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        closeButton.onClick.AddListener(CloseInventory);
        deckTabButton.onClick.AddListener(ShowDeckTab);
        potionTabButton.onClick.AddListener(ShowPotionTab);
        relicTabButton.onClick.AddListener(ShowRelicTab);
        inventoryButton.onClick.AddListener(OpenInventory);

        inventoryPanel.SetActive(false);
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        //默认打开卡组页
        ShowDeckTab();
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    //标签页切换
    void ResetTabs()
    {
        //隐藏所有内容
        deckContent.SetActive(false);
        potionContent.SetActive(false);
        relicContent.SetActive(false);

        //恢复按钮颜色
        deckTabButton.image.color = normalTabColor;
        potionTabButton.image.color = normalTabColor;
        relicTabButton.image.color = normalTabColor;
    }

    public void ShowDeckTab()
    {
        ResetTabs();
        deckContent.SetActive(true);
        deckTabButton.image.color = selectedTabColor;

        RefreshDeckDisplay();
    }

    public void ShowPotionTab()
    {
        ResetTabs();
        potionContent.SetActive(true);
        potionTabButton.image.color = selectedTabColor;

        //刷新药水
    }

    public void ShowRelicTab()
    {
        ResetTabs();
        relicContent.SetActive(true);
        relicTabButton.image.color = selectedTabColor;

        //刷新遗物
    }

    //内容刷新逻辑
    void RefreshDeckDisplay()
    {
        //清空现有显示
        foreach (Transform child in cardGridParent)
        {
            Destroy(child.gameObject);
        }

        if (deckManager == null) return;

        //获取allCards并排序
        List<CardData> masterDeck = deckManager.allCards;
        masterDeck.Sort((a, b) => a.cost.CompareTo(b.cost)); //按费用排序

        deckCountText.text = $"卡牌总数: {masterDeck.Count}";

        //生成卡牌
        foreach (CardData cardData in masterDeck)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardGridParent);

            //初始化卡牌
            CardUIHandler handler = cardObj.GetComponent<CardUIHandler>();
            if (handler != null)
            {
                handler.cardData = cardData;
                handler.isInteractive = false; //背包里的卡不能拖拽
            }

            //稍微缩小一点，方便排列
            cardObj.transform.localScale = Vector3.one * 0.8f;
        }
    }
}