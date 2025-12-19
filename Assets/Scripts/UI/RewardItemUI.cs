using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum RewardType
{
    Gold,
    Card,
    Relic
}

public class RewardItemUI : MonoBehaviour
{
    public RewardType type;
    public int goldAmount; //如果是金币，存数量
    public Button button;
    public TextMeshProUGUI text;
    public Image icon;

    // 图标资源
    public Sprite goldIcon;
    public Sprite cardIcon;
    public Sprite relicIcon;

    private BattleRewardUI parentUI;

    public void InitGold(int amount, BattleRewardUI ui)
    {
        type = RewardType.Gold;
        goldAmount = amount;
        parentUI = ui;
        text.text = $"{amount} 金币";
        icon.sprite = goldIcon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
        button.interactable = true;
    }

    public void InitCard(BattleRewardUI ui)
    {
        type = RewardType.Card;
        parentUI = ui;
        text.text = "选择卡牌";
        icon.sprite = cardIcon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
        button.interactable = true;
    }

    void OnClick()
    {
        if (type == RewardType.Gold)
        {
            CurrencyManager.Instance.AddGold(goldAmount);
            FloatingHint.Instance.ShowHint($"获得 {goldAmount} 金币");
            SetClaimed(); //标记为已领取
        }
        else if (type == RewardType.Card)
        {
            parentUI.OpenCardDraftPanel(this); //打开选牌界面
        }
    }

    public void SetClaimed()
    {
        button.interactable = false;
        text.color = Color.gray;
        //如果是金币，领取后直接变成灰色不可点
        //如果是卡牌，等选完牌后再调用这个
    }
}