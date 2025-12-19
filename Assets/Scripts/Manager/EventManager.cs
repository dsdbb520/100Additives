using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("数据")]
    public List<EventData> allEvents;

    [Header("UI引用")]
    public EventUI eventUI;

    //运行时状态
    private EventData currentEvent;
    private int currentDialogueIndex = 0;

    //引用Manager
    public CurrencyManager currencyManager;
    public PlayerHealthStars playerHealth;
    public DeckManager deckManager;
    public RelicManager relicManager;
    public MapManager mapManager;
    public BattleManager battleManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        allEvents = Resources.LoadAll<EventData>("Events").ToList();
        eventUI.Init(this);
    }

    //地图调用
    public void StartRandomEvent()
    {
        if (allEvents.Count == 0) return;

        //随机选一个事件
        currentEvent = allEvents[Random.Range(0, allEvents.Count)];

        //初始化状态
        currentDialogueIndex = 0;

        //UI显示
        eventUI.ShowEvent(currentEvent);

        //自动播放第一句
        AdvanceDialogue();
    }

    //对话
    public void AdvanceDialogue()
    {
        if (currentEvent == null) return;

        if (currentDialogueIndex < currentEvent.dialogueLines.Count)
        {
            //显示下一句
            string line = currentEvent.dialogueLines[currentDialogueIndex];
            eventUI.AddDialogueLine(line);
            currentDialogueIndex++;
        }
        else
        {
            //对话结束，显示选项
            eventUI.ShowOptions(currentEvent);
        }
    }

    //选项条件检查
    public bool CheckOptionCondition(string eventID, int optionIndex)
    {
        //返回false表示按钮不可点击
        switch (eventID)
        {
            case "Salesman": //推销员
                if (optionIndex == 0) return currencyManager.currentGold >= 30; //选项1需30金币
                return true;

            case "GutterOil": //地沟油
                if (optionIndex == 2) return relicManager.HasRelic("GarbageDisposal"); //选项3需垃圾粉碎机
                return true;

            default: return true;
        }
    }

    //选项执行
    public void SelectOption(int index)
    {
        string eventID = currentEvent.eventID;

        //记录选择结果，用于显示提示
        string resultHint = "";

        switch (eventID)
        {
            //事件A: 可疑推销员
            case "Salesman":
                if (index == 0) //买点科技
                {
                    currencyManager.SpendGold(30);
                    //TODO：有一个方法获取随机普通遗物
                    resultHint = "失去了30金币，获得了奇怪的遗物";
                }
                else if (index == 1) //以身试药
                {
                    playerHealth.TakeDamage(15); //扣血
                    deckManager.UpgradeRandomCards(2); //升级牌
                    resultHint = "精神恍惚，但感觉牌组变强了";
                }
                else //离开
                {
                    resultHint = "你转身离开";
                }
                break;

            //事件B: 地沟油
            case "GutterOil":
                if (index == 0) //捞一把
                {
                    currencyManager.AddGold(50);
                    //获得诅咒牌
                    CardData curse = Resources.Load<CardData>("Cards/地沟油");
                    if (curse) deckManager.ObtainCard(curse);
                    resultHint = "获得了金币，但也沾了一身油...";
                }
                else if (index == 1) //爬出
                {
                    int dmg = Mathf.CeilToInt(playerHealth.currentHealth * 0.1f);
                    playerHealth.TakeDamage(dmg);
                    resultHint = "好狼狈...";
                }
                else if (index == 2) //现场提炼
                {
                    CardData spice = Resources.Load<CardData>("Cards/一滴香"); //一滴香
                    if (spice) deckManager.ObtainCard(spice);
                    resultHint = "化腐朽为神奇！获得【一滴香】";
                }
                break;

            //事件C: 网红探店
            case "Influencer":
                if (index == 0) //展示刀工
                {
                    if (Random.value > 0.5f)
                    {
                        currencyManager.AddGold(100);
                        resultHint = "展示成功！获得打赏 100 金币";
                    }
                    else
                    {
                        //CardData badReview = Resources.Load<CardData>("Cards/差评"); //差评
                        //if (badReview) deckManager.ObtainCard(badReview);
                        playerHealth.TakeDamage(5); // 稍微扣点血表示羞愧
                        resultHint = "演砸了... 获得诅咒【差评】";
                    }
                }
                else if (index == 1) //战斗
                {
                    eventUI.Close();
                    battleManager.initialPotPressure = 50f; //设置初始压力
                    battleManager.StartNormalBattle(true);
                    return; //直接返回，不走下面的 LeaveEvent
                }
                else //赶出去
                {
                    resultHint = "这里不欢迎你！";
                }
                break;
        }

        //显示结果并离开
        if (!string.IsNullOrEmpty(resultHint))
            FloatingHint.Instance.ShowHint(resultHint);

        LeaveEvent();
    }

    private void LeaveEvent()
    {
        eventUI.Close();
        mapManager.FinishCurrentNode();
    }
}