// SkillSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Family Survival/Skills/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("基础信息")]
    public string skillID;
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    
    [Header("技能设置")]
    public float cooldownTime = 10f;
    public bool isPassive = false;
    public bool consumeResources = false;
    
    [Header("Buff效果")]
    public BuffSO appliedBuff; // 技能应用的Buff
    public float buffDuration = 10f;
    public bool buffAffectsCaster = false;
    public bool buffAffectsAllies = false;
    
    // 作用于物品的角色技能
    public virtual bool ExecuteSkill(int slotIndex) { return false; }

    // 作用于角色的角色技能
    public virtual bool ExecuteSkill(GameObject caster, BuffManager buffManager)
    {
        if (appliedBuff != null)
        {
            if (buffAffectsCaster)
            {
                buffManager.ApplyBuff(caster, appliedBuff, caster);
            }

            if (buffAffectsAllies)
            {
                // 找到所有盟友并应用Buff
                var allies = FindObjectsOfType<CharacterStatus>();
                foreach (var ally in allies)
                {
                    if (ally.gameObject != caster)
                    {
                        buffManager.ApplyBuff(ally.gameObject, appliedBuff, caster);
                    }
                }
            }
        }

        return true;
    }
}

