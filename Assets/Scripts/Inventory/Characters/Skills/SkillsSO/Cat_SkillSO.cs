using UnityEngine;

// 猫恢复弟弟体力值
[CreateAssetMenu(fileName = "Cat_Skill", menuName = "Family Survival/Skills/Cat_Skill")]
public class Cat_SkillSO : SkillSO
{
    // 角色存活时: 增加幸运值
    public override void OnActivate(CharacterSO owner)
    {

    }

    // 角色死亡时: 恢复幸运值
    public override void OnDeactivate(CharacterSO owner)
    {

    }

    // 恢复弟弟体力值
    public override bool ExecuteSkill(CharacterSO caster, BuffManager buffManager)
    {
        var characterManager = GameStateManager.Instance.Character;
        if (characterManager == null) return false;

        var targetGO = characterManager.GetCharacterGameObject("brother");
        if (targetGO == null) return false;

        var targetStatus = targetGO.GetComponent<CharacterStatus>();
        if (targetStatus != null && targetStatus.IsAlive)
        {
            targetStatus.ModifyStamina(targetStatus.characterSO.maxStamina);
            return true;
        }
        return false;
    }

}