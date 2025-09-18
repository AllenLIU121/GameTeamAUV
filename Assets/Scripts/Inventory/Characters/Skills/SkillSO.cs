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
    
    public CharacterSO caster;
    // 作用于物品的角色技能
    public virtual bool ExecuteSkill(int slotIndex) { return false; }

    // 作用于角色的角色技能--只对主角生效
    public virtual bool ExecuteSkill(CharacterSO caster, BuffManager buffManager)
    {
        buffManager.ApplyBuff(caster, appliedBuff, caster);

        return true;
    }
    
    //作用于角色的角色技能--只对所有人都生效
    public virtual bool ExecuteSkill(CharacterSO[] caster, BuffManager buffManager)
    {
        //挨个施加buff
        foreach (var singlecaster in caster)
        {
            buffManager.ApplyBuff(singlecaster, appliedBuff, null);
        }
        return true;
    }
}

//爸爸技能--增加负重buff
[CreateAssetMenu(fileName = "Father_CarryCapacitySkill", menuName = "Family Survival/Skills/Father_CarryCapacity")]
public class Father_SkillSO : SkillSO
{
    public override bool ExecuteSkill(CharacterSO caster, BuffManager buffManager)
    {
        //直接调用BuffSO中提高负重的子类
        CarryCapacityBuff buff = new CarryCapacityBuff();
        buffManager.ApplyBuff(caster, buff, null);
        return true;
    }
}

//妹妹技能--提高士气，减少体力消耗（对所有人生效）
[CreateAssetMenu(fileName = "Sister_MoraleBoostSkill", menuName = "Family Survival/Skills/Sister_MoraleBoost")]
public class Sister_SkillSO : SkillSO
{
    public override bool ExecuteSkill(CharacterSO[] caster, BuffManager buffManager)
    {
        //直接调用BuffSO中提升士气的子类
        MoraleBoostBuff buff = new MoraleBoostBuff();
        foreach (CharacterSO singlecaster in caster)
        {
            buffManager.ApplyBuff(singlecaster, buff, null);
        }
        
        return true;
    }
}

//弟弟技能--活着增加体力上限，死了扣上限（对所有人生效）
[CreateAssetMenu(fileName = "Brother_MoraleBoostSkill", menuName = "Family Survival/Skills/Brother_MoraleBoost")]
public class Brother_SkillSO : SkillSO
{
    public override bool ExecuteSkill(CharacterSO[] caster, BuffManager buffManager)
    {
        //直接调用BuffSO中LiveBuff
        LiveBuff buff = new LiveBuff();
        foreach (CharacterSO singlecaster in caster)
        {
            buffManager.ApplyBuff(singlecaster, buff, null);
        }
        
        return true;
    }
}

//哈基米技能 -- 增加特殊概率（对所有人生效），照顾小朋友（对个别人生效）
[CreateAssetMenu(fileName = "Hajimi_MoraleBoostSkill", menuName = "Family Survival/Skills/Hajimi_MoraleBoost")]
public class Hajimir_SkillSO : SkillSO
{
    //TODO：寻找特殊资源的buff是怎么生效的?给主角还是给猫
    public override bool ExecuteSkill(CharacterSO[] caster, BuffManager buffManager)
    {
        //直接调用BuffSO中LiveBuff
        LuckBoostBuff buff = new LuckBoostBuff();
        //TODO:想一下给谁加
        LookafterChildBuff buff2 = new LookafterChildBuff();
        foreach (CharacterSO singlecaster in caster)
        {
            //这里的判断也需要处理一下，也给Allen了
            if (singlecaster.name == "brother" || singlecaster.name == "sister")
            {
                buffManager.ApplyBuff(singlecaster, buff2, null);
            }
        }
        
        return true;
    }
}
