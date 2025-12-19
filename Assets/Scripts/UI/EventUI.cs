using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class EventUI : MonoBehaviour
{
    [Header("面板")]
    public GameObject eventPanel;
    public Image eventImage;
    public TextMeshProUGUI titleText;

    [Header("对话滚动区")]
    public ScrollRect scrollRect;
    public Transform dialogueContainer; //Content
    public GameObject textPrefab;
    public Button nextStepArea;         //覆盖在右侧的大透明按钮，点击继续对话

    [Header("选项区")]
    public Transform optionsContainer;  //底部放按钮的容器
    public GameObject optionButtonPrefab;

    private EventManager eventManager;

    public void Init(EventManager manager)
    {
        eventManager = manager;
        nextStepArea.onClick.RemoveAllListeners();
        nextStepArea.onClick.AddListener(() => eventManager.AdvanceDialogue());

        // 初始隐藏
        eventPanel.SetActive(false);
    }

    public void ShowEvent(EventData data)
    {
        eventPanel.SetActive(true);
        titleText.text = data.title;
        eventImage.sprite = data.eventImage;

        //清空旧对话和选项
        foreach (Transform child in dialogueContainer) Destroy(child.gameObject);
        foreach (Transform child in optionsContainer) Destroy(child.gameObject);

        nextStepArea.gameObject.SetActive(true); //启用点击继续
    }

    //添加一条新对话
    public void AddDialogueLine(string text)
    {
        GameObject newText = Instantiate(textPrefab, dialogueContainer);
        TextMeshProUGUI tmp = newText.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alpha = 0;

        //动画：淡入 + 稍微上浮
        tmp.DOFade(1, 0.5f);

        //强制滚动到底部
        StartCoroutine(AutoScrollToBottom());
    }

    IEnumerator AutoScrollToBottom()
    {
        yield return new WaitForEndOfFrame(); //等UI刷新高度
        //Canvas.ForceUpdateCanvases(); 
        scrollRect.verticalNormalizedPosition = 0f;
    }

    //生成选项按钮
    public void ShowOptions(EventData data)
    {
        nextStepArea.gameObject.SetActive(false); //禁用点击继续

        for (int i = 0; i < data.options.Count; i++)
        {
            var optData = data.options[i];
            int index = i; //闭包捕获

            bool isLock = !eventManager.CheckOptionCondition(data.eventID, index);

            GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            btnText.text = optData.description;

            if (!string.IsNullOrEmpty(optData.hoverText))
            {
                EventOptionHover hover = btnObj.AddComponent<EventOptionHover>();
                hover.content = optData.hoverText;
            }

            if (isLock)
            {
                btn.interactable = false;
                btnText.color = Color.gray;
            }
            else
            {
                btn.onClick.AddListener(() => eventManager.SelectOption(index));
            }
        }

        StartCoroutine(AutoScrollToBottom()); //选项出来后也滚一下
    }

    public void Close()
    {
        eventPanel.SetActive(false);
    }
}