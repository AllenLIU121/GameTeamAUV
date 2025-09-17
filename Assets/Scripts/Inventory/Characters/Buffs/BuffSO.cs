// BuffSO.cs (放在Scripts/Systems/BuffSystem/目录下)
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Family Survival/Buffs/Buff")]
public class BuffSO : ScriptableObject
{
    public enum DiseaseType
    {
        None,
        Cold,       // 感冒
        Diarrhea,   // 腹泻
        Pneumonia   // 肺炎
    }

    public enum BuffType
    {
        Buff,       // 增益效果
        Debuff,     // 减益效果
        Disease,    // 疾病
        Passive     // 被动效果
    }

    public enum StatType
    {
        Stamina,
        Hunger,
        StaminaDecayRate,      // 体力衰减速率
        HungerDecayRate,       // 饥饿衰减速率
        CarryCapacity,         // 负重能力
        SpecialResourceChance, // 特殊资源概率
        MovementSpeed,         // 移动速度
        HealingEffectiveness   // 治疗效果
    }
    [Header("基础信息")]
    public string buffID;
    public string buffName;
    [TextArea] public string description;
    public Sprite icon;
    public BuffType buffType = BuffType.Buff;
    
    [Header("持续时间设置")]
    public float duration = 10f;
    public bool isPermanent = false;
    public bool canStack = false;
    public int maxStacks = 1;
    
    [Header("状态影响")]
    public StatType[] affectedStats;
    public float[] statModifiers; // 正值增益，负值减益
    public bool isMultiplicative = false; // true为乘算，false为加算
    
    [Header("疾病相关")]
    public DiseaseType diseaseType = DiseaseType.None;
    public bool isCurable = true;
    
    [Header("视觉效果")]
    public GameObject visualEffect;
    public AudioClip soundEffect;
    
    public virtual void OnApply(GameObject target) 
    {
        if (visualEffect != null)
        {
            Instantiate(visualEffect, target.transform);
        }
        
        Debug.Log($"{buffName} 应用于 {target.name}");
    }
    
    public virtual void OnUpdate(GameObject target, float deltaTime) { }
    
    public virtual void OnRemove(GameObject target) 
    {
        Debug.Log($"{buffName} 从 {target.name} 移除");
    }
}

// 具体疾病 Buff
[CreateAssetMenu(fileName = "Disease_Cold", menuName = "")]
public class ColdDiseaseBuff : BuffSO
{
    public ColdDiseaseBuff()
    {
        buffType = BuffType.Disease;
        diseaseType = DiseaseType.Cold;
        duration = 90f; // 疾病持续90秒
        
        affectedStats = new StatType[] { StatType.StaminaDecayRate };
        statModifiers = new float[] { 1.4f }; // 体力流失增加40%
    }
}

[CreateAssetMenu(fileName = "Disease_Diarrhea", menuName = "")]
public class DiarrheaDiseaseBuff : BuffSO
{
    public DiarrheaDiseaseBuff()
    {
        buffType = BuffType.Disease;
        diseaseType = DiseaseType.Diarrhea;
        duration = 90f;
        
        affectedStats = new StatType[] { StatType.StaminaDecayRate, StatType.HungerDecayRate };
        statModifiers = new float[] { 1.3f, 1.2f }; // 体力和饥饿流失增加
    }
}

[CreateAssetMenu(fileName = "Disease_Pneumonia", menuName = "")]
public class PneumoniaDiseaseBuff : BuffSO
{
    public PneumoniaDiseaseBuff()
    {
        buffType = BuffType.Disease;
        diseaseType = DiseaseType.Pneumonia;
        duration = 120f;
        
        affectedStats = new StatType[] { StatType.StaminaDecayRate };
        statModifiers = new float[] { 1.6f }; // 体力流失增加60%
    }
}

// 技能产生的 Buff
[CreateAssetMenu(fileName = "Buff_MoraleBoost", menuName = "")]
public class MoraleBoostBuff : BuffSO
{
    public MoraleBoostBuff()
    {
        buffType = BuffType.Buff;
        duration = 60f;
        
        affectedStats = new StatType[] { StatType.StaminaDecayRate };
        statModifiers = new float[] { -0.2f }; // 体力衰减减少20%
        isMultiplicative = true;
    }
}

[CreateAssetMenu(fileName = "Buff_LuckBoost", menuName = "")]
public class LuckBoostBuff : BuffSO
{
    public LuckBoostBuff()
    {
        buffType = BuffType.Buff;
        duration = 300f; // 5分钟
        
        affectedStats = new StatType[] { StatType.SpecialResourceChance };
        statModifiers = new float[] { 0.15f }; // 特殊资源概率提升15%
    }
}

[CreateAssetMenu(fileName = "Buff_CarryCapacity", menuName = "")]
public class CarryCapacityBuff : BuffSO
{
    public CarryCapacityBuff()
    {
        buffType = BuffType.Passive;
        isPermanent = true;
        
        affectedStats = new StatType[] { StatType.CarryCapacity };
        statModifiers = new float[] { 5f }; // 负重增加5kg
    }
}