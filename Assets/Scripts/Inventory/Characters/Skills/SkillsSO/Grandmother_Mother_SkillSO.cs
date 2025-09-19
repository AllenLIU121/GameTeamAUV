using UnityEngine;

// 姥姥保鲜技能
[CreateAssetMenu(fileName = "Grandmother(Mother)_RefreshSkill", menuName = "Family Survival/Skills/Grandmother(Mother) Refresh")]
public class Grandmother_Mother_SkillSO : SkillSO
{
    public override bool ExecuteSkill(int slotIndex)
    {
        return GameStateManager.Instance.Inventory.RestoreItemRefreshness(slotIndex);
    }
}