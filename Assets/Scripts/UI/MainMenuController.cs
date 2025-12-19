using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("按钮引用")]
    public Button startButton;
    public Button quitButton;

    [Header("配置")]
    public string gameSceneName = "GameScene";

    private void Start()
    {
        //绑定事件
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
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