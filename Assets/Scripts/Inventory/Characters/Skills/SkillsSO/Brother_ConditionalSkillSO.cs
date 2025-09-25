using UnityEngine;

// 弟弟条件被动技能
[CreateAssetMenu(fileName = "Brother_ConditionalSkill", menuName = "Family Survival/Skills/Brother_ConditionalSkills")]
public class BrotherConditionalSkillSO : SkillSO
{
    public override void OnActivate(CharacterSO owner)
    {
        var aliveCharacters = GameStateManager.Instance.Character.GetAllCharacterStatus();
        foreach (var characterStatus in aliveCharacters)
        {
            characterStatus.ModifyMaxStamina(5f);
        }
        Debug.Log($"<color=green> Brother passive skill activated! All characters' MaxStamina +5.</color>");
    }

    public override void OnDeactivate(CharacterSO owner)
    {
        var aliveCharacters = GameStateManager.Instance.Character.GetAllCharacterStatus();
        foreach (var characterStatus in aliveCharacters)
        {
            characterStatus.ModifyMaxStamina(-15f);
        }
        Debug.Log($"<color=green> Brother passive skill activated! All characters' MaxStamina -10.</color>");
    }
}