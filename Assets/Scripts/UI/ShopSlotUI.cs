using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopSlotUI : MonoBehaviour
{
    [Header("UI引用")]
    public Transform cardSpawnPoint;
    public TextMeshProUGUI priceText;
    public GameObject discountBadge; //打折图标
    public GameObject cardPrefab;
    public Button buyButton;
    public Image soldOutOverlay;     //售罄遮罩

    private ShopItem currentItem;
    private GameObject cardObj; //生成的卡牌实例

    public void Init(ShopItem item)
    {
        currentItem = item;

        //生成展示对象
        foreach (Transform child in cardSpawnPoint) Destroy(child.gameObject);
        if (cardPrefab != null && cardSpawnPoint != null)
        {
            cardObj = Instantiate(cardPrefab, cardSpawnPoint);

            //重置位置
            cardObj.transform.localPosition = Vector3.zero;
            cardObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.anchoredPosition = Vector2.zero;
                cardRect.anchorMin = new Vector2(0.5f, 0.5f);
                cardRect.anchorMax = new Vector2(0.5f, 0.5f);
                cardRect.pivot = new Vector2(0.5f, 0.5f);
            }

            //初始化数据
            CardUIHandler handler = cardObj.GetComponent<CardUIHandler>();
            if (handler != null)
            {
                if (item.type == ItemType.Card)
                {
                    //如果是卡牌
                    handler.InitCard(item.cardData);
                }
                else if (item.type == ItemType.Relic)
                {
                    //如果是遗物
                    handler.InitRelic(item.relicData);
                }
                handler.isInteractive = false;
            }
        }

        //显示价格
        priceText.text = item.finalPrice.ToString();
        if (item.isDiscounted)
        {
            priceText.color = Color.green;
            discountBadge.SetActive(true);
        }
        else
        {
            priceText.color = Color.yellow;
            discountBadge.SetActive(false);
        }

        //状态更新
        UpdateSoldState();

        //绑定按钮
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClick);
    }

    private void OnBuyClick()
    {
        ShopManager.Instance.BuyItem(currentItem, gameObject);
    }

    public void UpdateSoldState()
    {
        if (currentItem.isSold)
        {
            if (soldOutOverlay != null)
            {
                soldOutOverlay.gameObject.SetActive(true);
                soldOutOverlay.transform.localScale = Vector3.one; //静态显示时正常大小

                CanvasGroup cg = soldOutOverlay.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
            buyButton.interactable = false;
            priceText.text = "已售";
            if (cardObj != null) cardObj.GetComponent<CanvasGroup>().alpha = 0.7f;
        }
        else
        {
            if (soldOutOverlay != null) soldOutOverlay.gameObject.SetActive(false);
            buyButton.interactable = true;
        }
    }

    public void PlayStampAnimation()
    {
        //禁用按钮，更新文字
        buyButton.interactable = false;
        priceText.text = "已售";
        if (cardObj != null) cardObj.GetComponent<CanvasGroup>().alpha = 0.7f;

        //准备动画状态
        if (soldOutOverlay != null)
        {
            soldOutOverlay.gameObject.SetActive(true);

            soldOutOverlay.transform.localScale = Vector3.one * 2.5f;

            CanvasGroup cg = soldOutOverlay.GetComponent<CanvasGroup>();
            if (cg == null) cg = soldOutOverlay.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            //执行动画序列
            Sequence seq = DOTween.Sequence();

            //同时进行：缩放撞击 + 透明度渐显
            seq.Join(soldOutOverlay.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBounce)); //弹性撞击
            seq.Join(cg.DOFade(1f, 0.2f)); //快速显现

        }
    }
}