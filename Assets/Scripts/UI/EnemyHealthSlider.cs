using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthSlider : MonoBehaviour
{
    // 物理血条和精神血条
    public Image phyHealthSlider;
    public Image menHealthSlider;
    public float phyHealthMax;
    public float menHealthMax;
    public float currentPhyHealth;
    public float currentMenHealth;

    private void Start()
    {
        currentPhyHealth = phyHealthMax;
        currentMenHealth = menHealthMax;
    }
    private void Update()
    {
        UpdateHealthBars(currentPhyHealth / phyHealthMax, currentMenHealth / menHealthMax);
    }

    // 物理血量和精神血量的百分比
    public void UpdateHealthBars(float phyHealthPercentage, float menHealthPercentage)
    {
        // 更新血条的填充比例
        phyHealthSlider.fillAmount = Mathf.Clamp01(phyHealthPercentage);  // 物理血条
        menHealthSlider.fillAmount = Mathf.Clamp01(menHealthPercentage);  // 精神血条
    }
}
