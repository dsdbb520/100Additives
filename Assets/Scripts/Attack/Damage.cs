using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{





    // 计算敌人受到的伤害
    public float CalculateDamage(float baseDamage, List<TagType> ingredientTags, EnemyData enemy)
    {
        float totalDamage = baseDamage;

        //计算弱点标签
        foreach (var ingredientTag in ingredientTags)
        {
            if (enemy.weaknessTags.Contains(ingredientTag))
            {
                // 每匹配到一个弱点标签，伤害增加 10%
                totalDamage *= 1.10f;
            }
        }

        //计算抗性标签
        foreach (var ingredientTag in ingredientTags)
        {
            if (enemy.resistTags.Contains(ingredientTag))
            {
                // 每匹配到一个抗性标签，伤害减少 5%
                totalDamage *= 0.95f;
            }
        }

        // 返回计算后的伤害值
        return totalDamage;
    }
}
