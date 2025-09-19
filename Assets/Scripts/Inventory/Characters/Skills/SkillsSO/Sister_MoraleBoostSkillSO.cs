using UnityEngine;

[CreateAssetMenu(fileName = "Sister_MoraleBoostSkill", menuName = "Family Survival/Skills/Sister_MoraleBoostSkill")]
public class SisterMoraleBoostSkillSO : SkillSO 
{
    public override void OnActivate(CharacterSO owner)
    {
        if (appliedBuff == null) return;

        var buffManager = GameStateManager.Instance.Buff;
        var allCharacters = GameStateManager.Instance.Character.GetAllAliveCharacterSOs();
        foreach (var character in allCharacters)
        {
            buffManager.ApplyBuff(character, appliedBuff);
        }
    }

    public override void OnDeactivate(CharacterSO owner)
    {
        if (appliedBuff == null) return;

        var buffManager = GameStateManager.Instance.Buff;
        var allCharacters = GameStateManager.Instance.Character.GetAllAliveCharacterSOs();
        foreach (var character in allCharacters)
        {
            buffManager.RemoveBuff(character, appliedBuff);
        }
    }
}