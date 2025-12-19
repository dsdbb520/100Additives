using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using TMPro;

public class VictoryUI : MonoBehaviour
{
    public static VictoryUI Instance { get; private set; }

    [Header("UI 引用")]
    public GameObject victoryPanel;
    public Button mainMenuButton;   //回到主菜单
    public Button quitButton;       //退出游戏
    public TextMeshProUGUI titleText;
    public GameObject victoryImage; 

    [Header("配置")]
    public string mainMenuSceneName = "MainMenuScene";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        victoryPanel.SetActive(false);
    }

    private void Start()
    {
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    public void ShowVictoryScreen()
    {
        victoryPanel.SetActive(true);

        if (titleText != null) titleText.text = "黑暗料理界制霸！";
        if (victoryImage != null) victoryImage.SetActive(true);

        victoryPanel.transform.localScale = Vector3.zero;
        victoryPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        //if (InventoryManager.Instance != null)
        //{
        //    InventoryManager.Instance.OpenInventory();
        //    if (victoryPanel.transform.parent != null)
        //        victoryPanel.transform.SetAsLastSibling();
        //}

        FloatingHint.Instance.ShowHint("恭喜通关！你是真正的黑暗料理之王！");
    }

    public void ShowDefeatScreen()
    {
        victoryPanel.SetActive(true);

        if (titleText != null) titleText.text = "菜做糊了！";
        if (victoryImage != null) victoryImage.SetActive(false);

        victoryPanel.transform.localScale = Vector3.zero;
        victoryPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        FloatingHint.Instance.ShowHint("胜败乃兵家常事，请大侠重新来过！");
    }

    void OnMainMenuClicked()
    {
        StartCoroutine(TransitionToMainMenu());
    }

    IEnumerator TransitionToMainMenu()
    {
        //禁用按钮，防止重复点击
        mainMenuButton.interactable = false;
        quitButton.interactable = false;

        FloatingHint.Instance.ShowHint("即将回到主菜单...");

        yield return new WaitForSeconds(1f);

        GameObject overlayObj = new GameObject("TransitionOverlay");
        overlayObj.transform.SetParent(victoryPanel.transform.parent);
        overlayObj.transform.localScale = Vector3.one;

        RectTransform rt = overlayObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        overlayObj.transform.SetAsLastSibling();

        Image img = overlayObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = true;

        Tween fadeTween = img.DOFade(1f, 1.0f).SetEase(Ease.Linear);
        yield return fadeTween.WaitForCompletion();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnQuitClicked()
    {
        mainMenuButton.interactable = false;
        quitButton.interactable = false;
        FloatingHint.Instance.ShowHint("正在退出游戏...");

        DOVirtual.DelayedCall(1.0f, () => {
            Debug.Log("退出游戏");
            Application.Quit();
        });
    }
}