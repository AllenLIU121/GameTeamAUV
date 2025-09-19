using UnityEngine;

// 奶奶烹饪技能
[CreateAssetMenu(fileName = "Grandmother(Father)_CookFoodSkill", menuName = "Family Survival/Skills/Grandmother(Father) Cook Food")]
public class Grandmother_Father_SkillSO : SkillSO
{
    public override bool ExecuteSkill(int slotIndex)
    { 
        return GameStateManager.Instance.Inventory.CookItem(slotIndex);
    }
}