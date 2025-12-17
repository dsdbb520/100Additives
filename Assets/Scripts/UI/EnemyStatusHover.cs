using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

public class EnemyStatusHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{


    public EnemyActionManager enemyActionManager;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (enemyActionManager == null || TooltipUI.Instance == null) return;

        string statusDesc = GetStatusDescription();
        if (!string.IsNullOrEmpty(statusDesc))
        {
            TooltipUI.Instance.Show("当前状态", statusDesc);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }
    }

    //拼接所有状态描述
    private string GetStatusDescription()
    {
        StringBuilder stringBuilder = new StringBuilder();
        bool hasStatus = false;

        //护盾
        if (enemyActionManager.currentShield > 0)
        {
            stringBuilder.AppendLine($"<color=grey><b>护盾</b></color>: {enemyActionManager.currentShield}");
            hasStatus = true;
        }

        //力量
        if (enemyActionManager.strengthBuff > 0)
        {
            stringBuilder.AppendLine($"<color=red><b>力量</b></color>: 造成伤害 +{enemyActionManager.strengthBuff}");
            hasStatus = true;
        }

        //虚弱
        if (enemyActionManager.tempAttackDebuff > 0)
        {
            stringBuilder.AppendLine($"<color=green><b>虚弱</b></color>: 本回合攻击力 -{enemyActionManager.tempAttackDebuff}");
            hasStatus = true;
        }

        //中毒
        if (enemyActionManager.poisonStacks > 0)
        {
            stringBuilder.AppendLine($"<color=#800080><b>中毒</b></color>: 回合结束受到 {enemyActionManager.poisonStacks} 点伤害");
            hasStatus = true;
        }

        //眩晕
        if (enemyActionManager.isStunned)
        {
            stringBuilder.AppendLine($"<color=yellow><b>眩晕</b></color>: 无法行动");
            hasStatus = true;
        }

        if (!hasStatus) return "当前无特殊状态";
        return stringBuilder.ToString();
    }
}