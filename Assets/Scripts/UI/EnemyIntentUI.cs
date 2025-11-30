using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class EnemyIntentUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image intentIcon;
    public TextMeshProUGUI intentValueText;

    [Header("Icons")]
    public Sprite attackSprite;
    public Sprite defendSprite;
    public Sprite buffSprite;
    public Sprite debuffSprite;
    public Sprite unknownSprite;

    private EnemyAction currentAction;

    public void UpdateIntent(EnemyAction action)
    {
        intentValueText.text = "";
        intentIcon.transform.DOKill();
        intentIcon.transform.localScale = Vector3.one;
        currentAction = action;

        switch (action.intentType)
        {
            case EnemyIntentType.Attack:
                intentIcon.sprite = attackSprite;
                intentValueText.text = action.value.ToString();
                break;
            case EnemyIntentType.Defend:
                intentIcon.sprite = defendSprite;
                intentValueText.text = action.value.ToString();
                break;
            case EnemyIntentType.Buff:
                intentIcon.sprite = buffSprite;
                break;
            case EnemyIntentType.Debuff:
                intentIcon.sprite = debuffSprite;
                break;
            default:
                intentIcon.sprite = unknownSprite;
                break;
        }

        //出现动画
        intentIcon.transform.localScale = Vector3.zero;
        intentIcon.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentAction != null)
        {
            //拼接描述文本
            //TODO：把数值也拼进去
            string detail = currentAction.description;

            // 调用 TooltipUI 显示
            if (TooltipUI.Instance != null)
            {
                TooltipUI.Instance.Show(currentAction.actionName, detail);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //鼠标移开时隐藏
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }
    }

    private void OnDisable()
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide(true);
        }
    }
}