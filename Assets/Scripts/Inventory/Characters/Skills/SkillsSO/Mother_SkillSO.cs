using UnityEngine;

// 妈妈技能: 存活时提供buff:特殊资源恢复额外数值
[CreateAssetMenu(fileName = "Mother_BuffSkill", menuName = "Family Survival/Skills/Mother Buff")]
public class Mother_SkillSO : SkillSO
{
    public override bool ExecuteSkill(int slotIndex)
    { 
        return GameStateManager.Instance.Inventory.CookItem(slotIndex);
    }
}