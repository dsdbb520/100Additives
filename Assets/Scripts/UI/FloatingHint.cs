using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FloatingHint : MonoBehaviour
{
    public static FloatingHint Instance { get; private set; }
    public GameObject hintPrefab;  // 用来实例化提示的预制体
    public Transform hintContainer;  // 存放所有提示的容器
    public float displayDuration = 2f;  // 提示显示的持续时间
    public float fadeDuration = 0.5f;  // 渐显渐隐的时长
    public float moveDistance = -50f;  // 每个提示上移的距离

    private List<GameObject> activeHints = new List<GameObject>();  // 存储所有当前显示的提示

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // 显示提示的函数
    public void ShowHint(string text)
    {
        // 实例化新的提示
        GameObject newHint = Instantiate(hintPrefab, hintContainer);
        TextMeshProUGUI hintText = newHint.GetComponentInChildren<TextMeshProUGUI>();
        hintText.text = text;

        // 获取提示框的背景 CanvasGroup，控制其渐变效果
        CanvasGroup canvasGroup = newHint.GetComponentInChildren<CanvasGroup>();

        // 设置新提示的初始位置，位于最底部
        RectTransform hintRect = newHint.GetComponent<RectTransform>();

        hintRect.anchoredPosition = new Vector2(0, -moveDistance);

        // 将新提示加入提示列表
        activeHints.Add(newHint);

        // 启动协程，控制渐显、持续和渐隐
        StartCoroutine(DisplayHintCoroutine(newHint, canvasGroup, hintRect));

        // 启动另一个协程，控制旧提示上移
        StartCoroutine(MoveUpHintsCoroutine());
    }

    // 控制提示显示、渐显、隐藏的协程
    private IEnumerator DisplayHintCoroutine(GameObject hint, CanvasGroup canvasGroup, RectTransform hintRect)
    {
        // --- 渐显阶段 ---
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            if (hint == null) yield break;

            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (hint == null) yield break; // 再检查一次
        canvasGroup.alpha = 1;

        yield return new WaitForSeconds(displayDuration);

        // --- 渐隐阶段 ---
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            if (hint == null) yield break;

            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (hint == null) yield break;
        canvasGroup.alpha = 0;

        if (activeHints.Contains(hint))
        {
            activeHints.Remove(hint);
        }
        Destroy(hint);
    }

    // 控制所有当前提示向上移动
    private IEnumerator MoveUpHintsCoroutine()
    {
        // 将所有已存在的提示向上平移
        foreach (var hint in activeHints)
        {
            RectTransform hintRect = hint.GetComponent<RectTransform>();
            Vector2 currentPos = hintRect.anchoredPosition;

            // 向上平移每个提示
            hintRect.anchoredPosition = new Vector2(currentPos.x, currentPos.y + moveDistance);
        }

        yield return null;
    }

    // 清空所有提示
    public void ClearAllHints()
    {
        foreach (GameObject hint in activeHints)
        {
            Destroy(hint);
        }
        activeHints.Clear();
    }
    private void OnDisable()
    {
        ClearAllHints();
    }
}