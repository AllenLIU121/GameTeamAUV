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
    
    // 运行时数据
    [System.NonSerialized] public float currentCooldown;
    
    public bool IsReady => currentCooldown <= 0;
    
    public virtual void Initialize()
    {
        currentCooldown = 0f;
    }
    
    public virtual void UpdateCooldown(float deltaTime)
    {
        if (currentCooldown > 0) currentCooldown = Mathf.Max(0, currentCooldown - deltaTime);
    }
    
    public virtual void StartCooldown() => currentCooldown = cooldownTime;
    
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

// 具体技能实现
[CreateAssetMenu(fileName = "Grandmother_RefreshSkill", menuName = "Family Survival/Skills/Grandmother Refresh")]
public class GrandmotherSkillSO : SkillSO
{
    [Header("姥姥技能特有设置")]
    public float foodRefreshAmount = 30f;
    
    public override bool ExecuteSkill(GameObject caster, BuffManager buffManager)
    {
        // 食物刷新逻辑
        return base.ExecuteSkill(caster, buffManager);
    }
}

[CreateAssetMenu(fileName = "Sister_MoraleSkill", menuName = "Family Survival/Skills/Sister Morale")]
public class SisterSkillSO : SkillSO
{
    public override bool ExecuteSkill(GameObject caster, BuffManager buffManager)
    {
        // 应用士气Buff
        return base.ExecuteSkill(caster, buffManager);
    }
}