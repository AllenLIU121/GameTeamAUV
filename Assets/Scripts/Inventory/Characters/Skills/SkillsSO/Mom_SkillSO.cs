using UnityEngine;

[CreateAssetMenu(fileName = "Mom_MedicineSkill", menuName = "Family Survival/Skills/Mom_MedicineSkill")]
public class Mom_SkillSO : SkillSO
{
    public override void OnActivate(CharacterSO owner)
    {
        GameStateManager.Instance.Inventory?.AddEachMedicine();
    }
}
