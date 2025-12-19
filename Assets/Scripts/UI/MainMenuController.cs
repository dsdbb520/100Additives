using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("引用")]
    public Button startButton;
    public Button quitButton;
    public Button tutorialButton;
    public Button closeButton;
    public GameObject tutorialPanel;

    [Header("配置")]
    public string gameSceneName = "GameScene";

    private void Start()
    {
        tutorialPanel.SetActive(false);
        //绑定事件
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
        tutorialButton.onClick.AddListener(OpenTutorialPanel);
        closeButton.onClick.AddListener(CloseTutorialPanel);
    }

    public void OpenTutorialPanel()
    {
        tutorialPanel.SetActive(true);
        tutorialPanel.transform.localScale = Vector3.zero;
        tutorialPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
    }

    public void CloseTutorialPanel()
    {
        tutorialPanel.transform.localScale = Vector3.one;
        tutorialPanel.transform.DOScale(0f, 0.5f).SetEase(Ease.OutBack);
        tutorialPanel.SetActive(false);
    }

    public void StartGame()
    {
        //加载游戏场景
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("退出游戏！");
        Application.Quit();
    }
}