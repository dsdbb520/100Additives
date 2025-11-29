using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RestManager : MonoBehaviour
{
    public GameObject restPanel;
    public Button sleepButton;
    public Button leaveButton;

    private PlayerHealthStars playerHealthStars;
    private MapManager mapManager;

    void Start()
    {
        playerHealthStars = FindObjectOfType<PlayerHealthStars>();
        mapManager = FindObjectOfType<MapManager>();

        sleepButton.onClick.AddListener(OnSleepClicked);
        leaveButton.onClick.AddListener(OnLeaveClicked);

        restPanel.SetActive(false);
    }

    public void OpenRestSite()
    {
        restPanel.SetActive(true);
        restPanel.transform.localScale = Vector3.zero;
        restPanel.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack).SetLink(restPanel);

        sleepButton.interactable = true;
    }

    void OnSleepClicked()
    {

        playerHealthStars.Heal(30f);
        FloatingHint.Instance.ShowHint("生命值已恢复！");

        sleepButton.interactable = false;

        sleepButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f).SetLink(sleepButton.gameObject);
    }

    void OnLeaveClicked()
    {
        //关闭界面动画
        restPanel.transform.DOScale(0, 0.2f)
            .SetEase(Ease.InBack)
            .SetLink(restPanel)
            .OnComplete(() => {
                restPanel.SetActive(false);
                mapManager.FinishCurrentNode();
            });
    }
}