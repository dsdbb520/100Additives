using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyHealthSlider : MonoBehaviour
{
    [Header("物理血条（红）")]
    public Image phyHealthSlider;
    public Image phyHealthSliderEase;

    [Header("精神血条（黄）")]
    public Image menHealthSlider;
    public Image menHealthSliderEase;

    public void UpdateHealthBars(float phyTarget, float menTarget)
    {
        // 限制数值在 0-1 之间
        phyTarget = Mathf.Clamp01(phyTarget);
        menTarget = Mathf.Clamp01(menTarget);

        // 分别处理物理和精神血条
        HandleHealthBarChange(phyHealthSlider, phyHealthSliderEase, phyTarget);
        HandleHealthBarChange(menHealthSlider, menHealthSliderEase, menTarget);
    }

    private void HandleHealthBarChange(Image frontSlider, Image backSlider, float targetAmount)
    {
        float currentAmount = frontSlider.fillAmount;

        if (targetAmount > currentAmount)
        {
            if (backSlider != null)
            {
                backSlider.DOFillAmount(targetAmount, 0.1f)
                    .SetEase(Ease.OutQuad)
                    .SetLink(backSlider.gameObject);
            }

            frontSlider.DOFillAmount(targetAmount, 0.5f)
                .SetEase(Ease.OutQuad)
                .SetLink(frontSlider.gameObject);
        }
        else
        {
            frontSlider.DOFillAmount(targetAmount, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetLink(frontSlider.gameObject);

            if (backSlider != null)
            {
                backSlider.DOFillAmount(targetAmount, 0.6f)
                    .SetDelay(0.1f)
                    .SetEase(Ease.OutQuad)
                    .SetLink(backSlider.gameObject);
            }
        }
    }
}