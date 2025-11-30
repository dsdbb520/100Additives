using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class TooltipUI : MonoBehaviour
{

    public static TooltipUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject tooltipPanel; //整个提示框面板
    public TextMeshProUGUI headerText; //标题
    public TextMeshProUGUI contentText; //详细描述
    public CanvasGroup canvasGroup; //控制透明度

    [Header("Settings")]
    public Vector2 offset = new Vector2(15, -15); //鼠标偏移量

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        //初始隐藏
        Hide(true);
    }

    private void Update()
    {
        //只有显示的时候才跟随鼠标
        if (tooltipPanel.activeSelf)
        {
            FollowMouse();
        }
    }

    private void FollowMouse()
    {
        //获取鼠标位置
        Vector2 mousePos = Input.mousePosition;

        //设置位置 (加上偏移量，防止挡住鼠标)
        transform.position = mousePos + offset;

        //TODO：屏幕边缘检测，防止提示框跑出屏幕外
    }

    public void Show(string header, string content)
    {
        if (string.IsNullOrEmpty(header) && string.IsNullOrEmpty(content)) return;

        headerText.text = header;
        contentText.text = content;

        //激活并播放渐显动画
        tooltipPanel.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 0.2f).SetLink(gameObject);
    }

    public void Hide(bool instant = false)
    {
        if (instant)
        {
            tooltipPanel.SetActive(false);
            canvasGroup.alpha = 0;
        }
        else
        {
            canvasGroup.DOFade(0, 0.2f).SetLink(gameObject).OnComplete(() =>
            {
                tooltipPanel.SetActive(false);
            });
        }
    }
}