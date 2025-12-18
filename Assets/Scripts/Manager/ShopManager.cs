using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

[System.Serializable]
public class ShopItem
{
    public CardData cardData;
    public int originalPrice;
    public int finalPrice;
    public bool isDiscounted; //是否半价
    public bool isSold;       //是否已售出

    public ShopItem(CardData card, int basePrice, bool discount)
    {
        cardData = card;
        originalPrice = basePrice;
        isDiscounted = discount;
        finalPrice = discount ? basePrice / 2 : basePrice;
        isSold = false;
    }
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("配置")]
    public List<CardData> allCardsDatabase; //卡牌数据库
    public Transform shopPanel;             //商店面板
    public Transform itemsContainer;        //商品槽位父物体
    public GameObject shopSlotPrefab;       //商品槽位预制体
    public Button closeButton;

    [Header("运行时数据")]
    public List<ShopItem> currentInventory = new List<ShopItem>();

    // 引用
    public CurrencyManager currencyManager;
    public DeckManager deckManager;
    public MapManager mapManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        allCardsDatabase = Resources.LoadAll<CardData>("Cards").ToList();
    }

    private void Start()
    {
        if (shopPanel != null) shopPanel.gameObject.SetActive(false);
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseShop);
        }
        else
        {
            Debug.LogWarning("ShopManager: CloseButton 未赋值！");
        }
    }

    //生成商店
    public void GenerateShop()
    {
        currentInventory.Clear();

        //筛选符合条件的卡池
        var validCards = allCardsDatabase.Where(c =>
            c.rarity == CardRarity.Common || c.rarity == CardRarity.Uncommon
        ).ToList();

        //分类
        //主食
        var staples = validCards.Where(c => c.tags.Contains(TagType.Ingredient)).ToList();

        //小吃
        var snacks = validCards.Where(c => c.targetType == CardTargetType.SmallStove && !c.tags.Contains(TagType.Seasoning)).ToList();

        //佐料
        var seasonings = validCards.Where(c => c.tags.Contains(TagType.Seasoning)).ToList();

        //抽取
        List<CardData> selectedCards = new List<CardData>();
        selectedCards.AddRange(GetRandomCards(staples, 4));
        selectedCards.AddRange(GetRandomCards(snacks, 3));
        selectedCards.AddRange(GetRandomCards(seasonings, 3));

        //随机选2个进行打折
        HashSet<int> discountIndices = new HashSet<int>();
        if (selectedCards.Count >= 2)
        {
            while (discountIndices.Count < 2)
            {
                discountIndices.Add(Random.Range(0, selectedCards.Count));
            }
        }

        //生成对象并定价
        for (int i = 0; i < selectedCards.Count; i++)
        {
            CardData card = selectedCards[i];
            bool isDiscount = discountIndices.Contains(i);

            //随机定价策略
            int price = CalculatePrice(card);

            currentInventory.Add(new ShopItem(card, price, isDiscount));
        }

        //刷新UI
        RefreshShopUI();
    }

    //随机抽取x张不重复的卡
    private List<CardData> GetRandomCards(List<CardData> sourcePool, int count)
    {
        List<CardData> result = new List<CardData>();
        List<CardData> poolCopy = new List<CardData>(sourcePool); // 拷贝一份防止修改原列表

        for (int i = 0; i < count; i++)
        {
            if (poolCopy.Count == 0) break;
            int idx = Random.Range(0, poolCopy.Count);
            result.Add(poolCopy[idx]);
            poolCopy.RemoveAt(idx);
        }
        return result;
    }

    //价格计算
    private int CalculatePrice(CardData card)
    {
        int basePrice = 0;
        //基础价格区间
        if (card.rarity == CardRarity.Common) basePrice = Random.Range(40, 60);
        else if (card.rarity == CardRarity.Uncommon) basePrice = Random.Range(70, 100);
        else basePrice = Random.Range(120, 150); //防备万一有更高级的

        return basePrice;
    }

    //UI逻辑
    public void OpenShop()
    {
        GenerateShop();
        shopPanel.gameObject.SetActive(true);
        shopPanel.localScale = Vector3.zero;
        shopPanel.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
    }

    public void CloseShop()
    {
        shopPanel.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            shopPanel.gameObject.SetActive(false);

            //结算节点
            if (mapManager != null) mapManager.FinishCurrentNode();
        });
    }

    private void RefreshShopUI()
    {
        //清空旧槽位
        foreach (Transform child in itemsContainer) Destroy(child.gameObject);

        //生成新槽位
        foreach (var item in currentInventory)
        {
            GameObject slotObj = Instantiate(shopSlotPrefab, itemsContainer);
            slotObj.GetComponent<ShopSlotUI>().Init(item);
        }
    }

    //购买逻辑
    public void BuyItem(ShopItem item, GameObject slotObj)
    {
        if (item.isSold) return;

        if (currencyManager.SpendGold(item.finalPrice))
        {
            //标记为已售出
            item.isSold = true;

            //加入玩家牌库
            deckManager.allCards.Add(item.cardData.Clone());
            deckManager.UpdateCardCountDisplay();

            FloatingHint.Instance.ShowHint($"购买成功！{item.cardData.cardName} 加入了牌组");

            ShopSlotUI slotUI = slotObj.GetComponent<ShopSlotUI>();
            if (slotUI != null)
            {
                slotUI.PlayStampAnimation();
            }
        }
    }
}