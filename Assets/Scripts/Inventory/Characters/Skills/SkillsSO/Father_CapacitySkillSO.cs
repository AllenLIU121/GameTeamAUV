using UnityEngine;

[CreateAssetMenu(fileName = "Father_CapacitySkill", menuName = "Family Survival/Skills/Father_CapacitySkill")]
public class Father_CapacitySkillSO : SkillSO
{
    public override void OnActivate(CharacterSO owner)
    {
        GameStateManager.Instance.Inventory?.ModifyMaxWeightCapacity(value);
    }

    public override void OnDeactivate(CharacterSO owner)
    {
        GameStateManager.Instance.Inventory?.ModifyMaxWeightCapacity(-value);
    }
}
