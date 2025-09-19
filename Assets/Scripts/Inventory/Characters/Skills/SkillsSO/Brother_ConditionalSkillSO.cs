using UnityEngine;

// 弟弟条件被动技能
[CreateAssetMenu(fileName = "Brother_ConditionalSkill", menuName = "Family Survival/Skills/Brother_ConditionalSkills")]
public class BrotherConditionalSkillSO : SkillSO
{
    [Header("条件Buff")]
    public BuffSO aliveBuff; // 存活时给全队施加的Buff
    public BuffSO deadBuff;  // 死亡时给全队施加的Debuff

    public override void OnActivate(CharacterSO owner)
    {
        var buffManager = GameStateManager.Instance.Buff;
        var aliveCharacters = GameStateManager.Instance.Character.GetAllAliveCharacterSOs();
        foreach (var character in aliveCharacters)
        {
            if (character.characterID != owner.characterID)
            {
                buffManager.RemoveBuff(character, deadBuff);
                buffManager.ApplyBuff(character, aliveBuff);
            }
        }
    }

    public override void OnDeactivate(CharacterSO owner)
    {
        var buffManager = GameStateManager.Instance.Buff;
        var aliveCharacters = GameStateManager.Instance.Character.GetAllAliveCharacterSOs();
        foreach (var character in aliveCharacters)
        {
            buffManager.RemoveBuff(character, aliveBuff);
            buffManager.ApplyBuff(character, deadBuff);
        }
    }
}