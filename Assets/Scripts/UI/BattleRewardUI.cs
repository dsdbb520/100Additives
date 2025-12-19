using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class BattleRewardUI : MonoBehaviour
{
    public static BattleRewardUI Instance { get; private set; }

    [Header("面板")]
    public GameObject rewardPanel;     //奖励列表主面板
    public Transform rewardsContainer; //放置奖励按钮的容器
    public GameObject cardDraftPanel;  //选牌面板
    public Transform cardGrid;         //放卡的网格
    public Button skipButton;          //离开战斗按钮

    [Header("预制体")]
    public GameObject rewardItemPrefab;
    public GameObject cardSelectPrefab; //用于选牌的卡牌预制体

    [Header("选牌状态")]
    public TextMeshProUGUI draftTitle;
    private int cardsPickedCount = 0;
    private int maxPickCount = 2;
    private List<CardData> currentDraftPool;
    private RewardItemUI currentCardRewardItem; //记录当前正在点的那个奖励按钮

    public DeckManager deckManager;
    private MapManager mapManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        mapManager = FindObjectOfType<MapManager>();
        rewardPanel.SetActive(false);
        cardDraftPanel.SetActive(false);

        skipButton.onClick.AddListener(OnLeaveBattle);
    }

    //显示奖励列表
    public void ShowVictoryRewards(BattleType battleType)
    {
        rewardPanel.SetActive(true);
        //动画
        rewardPanel.GetComponent<CanvasGroup>().alpha = 0;
        rewardPanel.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetUpdate(true); //即使暂停也能播放

        //清空旧奖励
        foreach (Transform child in rewardsContainer) Destroy(child.gameObject);

        //生成金币奖励
        int gold = RewardManager.Instance.CalculateGoldReward(battleType);
        GameObject goldObj = Instantiate(rewardItemPrefab, rewardsContainer);
        goldObj.GetComponent<RewardItemUI>().InitGold(gold, this);

        //生成卡牌奖励
        GameObject cardObj = Instantiate(rewardItemPrefab, rewardsContainer);
        cardObj.GetComponent<RewardItemUI>().InitCard(this);

        //以后可以在这里加遗物
    }

    //打开选牌界面
    public void OpenCardDraftPanel(RewardItemUI itemUI)
    {
        currentCardRewardItem = itemUI; //记住是哪个按钮触发的
        cardsPickedCount = 0;

        // 获取当前战斗类型 (需要 BattleManager 记录)
        // 这里简化：为了获取卡池，我们需要让 RewardManager 知道类型
        // 建议在 ShowVictoryRewards 时就生成好数据存起来，或者这里传入类型
        // 简单起见，我们重新获取一次当前战斗类型
        BattleType type = BattleManager.Instance.isBossBattle ? BattleType.Boss :
                          (BattleManager.Instance.isEliteBattle ? BattleType.Elite : BattleType.Normal);

        currentDraftPool = RewardManager.Instance.GenerateCardRewardPool(type);

        //显示面板
        rewardPanel.SetActive(false); //暂时隐藏奖励列表
        cardDraftPanel.SetActive(true);
        UpdateDraftTitle();

        foreach (Transform child in cardGrid) Destroy(child.gameObject);

        foreach (var cardData in currentDraftPool)
        {
            GameObject cardObj = Instantiate(cardSelectPrefab, cardGrid);

            //初始化卡牌显示
            CardUIHandler handler = cardObj.GetComponent<CardUIHandler>();
            cardObj.transform.localPosition = Vector3.zero;
            cardObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            handler.cardData = cardData;
            handler.isInteractive = false; //选牌界面不需要拖拽

            //添加点击事件
            Button btn = cardObj.GetComponent<Button>();
            if (btn == null) btn = cardObj.AddComponent<Button>();

            btn.onClick.AddListener(() => OnCardPicked(cardData, cardObj));
        }
    }

    //选牌逻辑
    void OnCardPicked(CardData card, GameObject cardObj)
    {
        if (cardsPickedCount >= maxPickCount) return;

        //视觉反馈
        FloatingHint.Instance.ShowHint($"选择了：{card.cardName}");

        //加入牌库
        deckManager.ObtainCard(card.Clone());
        deckManager.UpdateCardCountDisplay();


        cardObj.GetComponent<Button>().interactable = false;
        cardObj.transform.DOScale(0, 0.2f);

        cardsPickedCount++;
        UpdateDraftTitle();

        if (cardsPickedCount >= maxPickCount)
        {
            Invoke("FinishDrafting", 0.5f);
        }
    }

    void UpdateDraftTitle()
    {
        draftTitle.text = $"选择卡牌加入牌组 ({cardsPickedCount}/{maxPickCount})";
    }

    void FinishDrafting()
    {
        cardDraftPanel.SetActive(false);
        rewardPanel.SetActive(true); //回到奖励列表

        //强制刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(rewardPanel.GetComponent<RectTransform>());

        //标记奖励已领取
        if (currentCardRewardItem != null)
        {
            currentCardRewardItem.SetClaimed();
        }
    }

    //离开战斗
    void OnLeaveBattle()
    {
        //隐藏UI
        rewardPanel.SetActive(false);
        cardDraftPanel.SetActive(false);

        //结算节点
        if (BattleManager.Instance.isBossBattle)
        {
            //如果是Boss战胜利，直接通关
            VictoryUI.Instance.ShowVictoryScreen();
        }
        else
        {
            //如果是普通/精英战斗，正常结算节点
            FindObjectOfType<MapManager>().FinishCurrentNode();
        }
    }
}