using UnityEngine;
using TMPro;
using DG.Tweening;

public class SmallStoveManager : MonoBehaviour
{
    public RectTransform stovePanel;
    public TextMeshProUGUI usageText;

    public int maxUsagePerTurn = 3;
    private int currentUsage = 0;

    private PlayerHealthStars playerHealth;
    public DeckManager deckManager;
    public HandManager handManager;

    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealthStars>();
        UpdateUsageUI();
    }

    //回合开始时调用，重置次数
    public void ResetStove()
    {
        currentUsage = 0;
        UpdateUsageUI();
    }

    //检查是否还能用
    public bool CanUseStove()
    {
        return currentUsage < maxUsagePerTurn;
    }

    //处理卡牌放入小灶
    public bool AddCardToStove(CardData card, GameObject cardObj)
    {
        if (!CanUseStove())
        {
            FloatingHint.Instance.ShowHint("小灶每回合只能用 3 次！");
            return false;
        }
        if (deckManager == null || handManager == null)
        {
            Debug.LogError("Manager 丢失！");
            return false;
        }
        if (card.healValue <= 0 && card.shieldValue <= 0)
        {
            FloatingHint.Instance.ShowHint("这食材在小灶里没啥用...");
            return false;
        }

        currentUsage++;
        UpdateUsageUI();
        ApplyCardEffect(card);

        //视觉效果
        cardObj.transform.SetParent(stovePanel);
        cardObj.transform.DOScale(0f, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                Destroy(cardObj);
            })
            .SetLink(cardObj);

        // 6. 数据处理
        handManager.RemoveCardFromHand(card);
        deckManager.discardPile.Add(card);

        Debug.Log($"卡牌 {card.cardName} 放入了小灶");

        return true;
    }

    private void ApplyCardEffect(CardData card)
    {
        bool hasEffect = false;

        if (card.healValue > 0)
        {
            playerHealth.Heal(card.healValue);
            FloatingHint.Instance.ShowHint($"恢复 {card.healValue} 点生命");
            hasEffect = true;
        }

        if (card.shieldValue > 0)
        {
            playerHealth.AddShield(card.shieldValue);
            FloatingHint.Instance.ShowHint($"获得 {card.shieldValue} 点护盾");
            hasEffect = true;
        }

        // TODO：在这里加 Buff/Debuff 逻辑

        if (!hasEffect)
        {
            FloatingHint.Instance.ShowHint("这食材在小灶里没啥用...");
        }
    }

    private void UpdateUsageUI()
    {
        if (usageText != null)
        {
            usageText.text = $"{currentUsage}/{maxUsagePerTurn}";
        }
    }
}