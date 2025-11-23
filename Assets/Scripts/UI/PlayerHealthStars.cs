using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthStars : MonoBehaviour
{
    public Image[] healthStars;  // 存储五颗星星的数组
    public float currentHealth;  // 当前血量（0-100）
    public float maxHealth = 100;  // 最大血量

    void Start()
    {
        currentHealth = maxHealth; //Debug用
        UpdateHealthStars(); // 初始化星星显示
    }

    private void Update()
    {
        UpdateHealthStars();
    }

    // 调用此方法以处理伤害，暂定直接使用加减算法
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 确保血量在 0 到 maxHealth 之间
        UpdateHealthStars(); // 更新星星显示
    }

    // 更新五颗星星的显示
    void UpdateHealthStars()
    {
        // 计算血量对应的星星填充比例
        float healthPercentage = currentHealth / maxHealth;
        int fullStarsCount = Mathf.FloorToInt(healthPercentage * healthStars.Length);  // 满星个数
        float remainingHealthPercentage = healthPercentage * healthStars.Length - fullStarsCount;

        // 更新每颗星星的显示
        for (int i = 0; i < healthStars.Length; i++)
        {
            // 满星
            if (i < fullStarsCount)
            {
                healthStars[i].fillAmount = 1f;  // 完全填充
            }
            // 半星
            else if (i == fullStarsCount && remainingHealthPercentage > 0)
            {
                healthStars[i].fillAmount = remainingHealthPercentage;  // 半星
            }
            // 空星
            else
            {
                healthStars[i].fillAmount = 0f;  // 空星
            }
        }
    }
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthStars();
    }

}
