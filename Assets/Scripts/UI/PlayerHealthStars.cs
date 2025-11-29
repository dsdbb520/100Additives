using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerHealthStars : MonoBehaviour
{
    public Image[] healthStars;
    public float currentHealth;
    public float maxHealth = 100;

    public float currentShield = 0;
    public GameObject shieldGroup;
    public TextMeshProUGUI shieldText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthStars();
        ClearShield();
    }


    public void TakeDamage(float damage)
    {
        if (currentShield > 0)
        {
            if (currentShield >= damage)
            {
                currentShield -= damage;
                damage = 0; //护盾完全抵消伤害
            }
            else
            {
                damage -= currentShield; //伤害溢出
                currentShield = 0;
            }
            UpdateShieldUI();
        }
        //护盾不够再扣血
        if (damage > 0)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthStars();
        }
    }

    public void AddShield(float amount)
    {
        currentShield += amount;
        UpdateShieldUI();
    }

    public void ClearShield()
    {
        currentShield = 0;
        UpdateShieldUI();
    }

    private void UpdateShieldUI()
    {
        if (currentShield > 0)
        {
            shieldGroup.SetActive(true); // 有盾就显示
            shieldText.text = currentShield.ToString();
        }
        else
        {
            shieldGroup.SetActive(false); // 没盾就隐藏
        }
    }

    //更新血量显示
    void UpdateHealthStars()
    {
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
