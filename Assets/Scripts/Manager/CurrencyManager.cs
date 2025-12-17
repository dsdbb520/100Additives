using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("金钱")]
    public int currentGold = 0; //当前金币数量
    public int startGold = 50;  //初始金币

    [Header("UI")]
    public TextMeshProUGUI goldText; //显示金币的文本

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentGold = startGold;
        UpdateUI();
    }


    //加钱
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
        //TODO：加一个金币跳动的动画特效
    }

    //尝试花钱
    public bool SpendGold(int amount)
    {
        if (HasEnoughGold(amount))
        {
            currentGold -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            FloatingHint.Instance.ShowHint("金币不足！");
            return false;
        }
    }

    //检查钱够不够
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    //更新 UI
    private void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = $"当前金币：{currentGold}";
        }
    }

    // --- Debug / 作弊系统 ---
    private void Update()
    {
        //按G键加100块
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(100);
            FloatingHint.Instance.ShowHint("【DEBUG】获得 100 金币");
        }

        //按Alt + G扣100块
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (SpendGold(100))
                FloatingHint.Instance.ShowHint("【DEBUG】扣除 100 金币");
        }
    }
}