using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class TooltipUI : MonoBehaviour
{

    public static TooltipUI Instance { get; private set; }

    [Header("UI引用")]
    public GameObject tooltipPanel;
    public RectTransform rectTransform;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI contentText;
    public CanvasGroup canvasGroup;
    public LayoutGroup layoutGroup;

    [Header("Settings")]
    public Vector2 offset = new Vector2(10, -5); //鼠标偏移量
    public float padding = 50f; //屏幕边缘安全距离

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        if (rectTransform == null) rectTransform = tooltipPanel.GetComponent<RectTransform>();
        if (layoutGroup == null) layoutGroup = tooltipPanel.GetComponent<LayoutGroup>();

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
        Vector2 mousePos = Input.mousePosition;

        float pivotX = mousePos.x / Screen.width;
        float pivotY = mousePos.y / Screen.height;

        float finalPivotX = (pivotX > 0.8f) ? 1f : 0f;
        float finalPivotY = (pivotY < 0.2f) ? 0f : 1f;

        rectTransform.pivot = new Vector2(finalPivotX, finalPivotY);

        //设置位置
        transform.position = mousePos + CalculateOffset(finalPivotX, finalPivotY);
    }

    //根据轴心动态计算偏移方向
    private Vector2 CalculateOffset(float pivotX, float pivotY)
    {
        float x = (pivotX == 0) ? offset.x : -offset.x;
        float y = (pivotY == 1) ? offset.y : -offset.y;

        return new Vector2(x, y);
    }

    public void Show(string header, string content)
    {
        //赋值文本
        if (string.IsNullOrEmpty(header) && string.IsNullOrEmpty(content)) return;
        headerText.text = header;
        contentText.text = content;

        canvasGroup.DOKill();

        //激活并播放渐显动画
        tooltipPanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false; //抖动一下组件促使刷新
            layoutGroup.enabled = true;
        }
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 0.15f).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void Hide(bool instant = false)
    {
        canvasGroup.DOKill();
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