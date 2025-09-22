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
    public float cooldownTime = 120f;
    [Tooltip("一次性技能数值")]
    public float value;
    // public bool isPassive = false;
    // public bool consumeResources = false;
    
    [Header("Buff效果")]
    public BuffSO appliedBuff; // 技能应用的Buff
    
    public virtual void OnActivate(CharacterSO owner) { }
    public virtual void OnDeactivate(CharacterSO owner) { }

    // 作用于物品的角色技能
    public virtual bool ExecuteSkill(int slotIndex) { return false; }

    // 作用于角色的角色技能--只对主角生效
    public virtual bool ExecuteSkill(CharacterSO caster, BuffManager buffManager)
    {
        buffManager.ApplyBuff(caster, appliedBuff);

        return true;
    }
    
    //作用于角色的角色技能--只对所有人都生效
    public virtual bool ExecuteSkill(CharacterSO[] caster, BuffManager buffManager)
    {
        //挨个施加buff
        foreach (var singlecaster in caster)
        {
            buffManager.ApplyBuff(singlecaster, appliedBuff);
        }
        return true;
    }
}