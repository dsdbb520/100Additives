using UnityEngine;
using UnityEngine.EventSystems;

public class EventOptionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string content; //ÐüÍ£ÏÔÊ¾µÄÎÄ×Ö

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(content))
        {
            if (TooltipUI.Instance != null)
            {
                TooltipUI.Instance.Show("", content);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
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