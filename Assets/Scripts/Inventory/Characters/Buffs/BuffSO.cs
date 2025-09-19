// BuffSO.cs (放在Scripts/Systems/BuffSystem/目录下)
using UnityEngine;
using System.Collections.Generic;
using System;

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

    public enum BuffType { Buff, Disease }

    public enum StatType
    {
        Stamina,
        Hunger,
        MaxStamina,
        MaxHunger,
        StaminaDecayRate,      // 体力衰减速率
        HungerDecayRate,       // 饥饿衰减速率
        // CarryCapacity,         // 负重能力
        // SpecialResourceChance, // 特殊资源概率
    }

    [Header("基础信息")]
    public string buffID;
    public string buffName;
    [TextArea] public string description;
    public Sprite icon;
    public BuffType buffType = BuffType.Buff;

    [Header("持续时间设置")]
    public float duration;
    public bool isPermanent = false;

    [Header("状态影响")]
    public List<StatModifier> statModifiers = new List<StatModifier>();

    [Header("疾病相关")]
    public DiseaseType diseaseType = DiseaseType.None;

    [Header("视觉效果")]
    public GameObject visualEffect;
    public AudioClip soundEffect;

    public virtual void OnApply(CharacterSO target)
    {
        Debug.Log($"{buffName} 应用于 {target.name}");
    }

    public virtual void OnUpdate(CharacterSO target, float deltaTime) { }

    public virtual void OnRemove(CharacterSO target)
    {
        Debug.Log($"{buffName} 从 {target.name} 移除");
    }
}

[Serializable]
public struct StatModifier
{
    public BuffSO.StatType statType;
    public float value;
    public bool isMultiplicative;

    public StatModifier(BuffSO.StatType statType, float value, bool isMultiplicative)
    {
        this.statType = statType;
        this.value = value;
        this.isMultiplicative = isMultiplicative;
    }
}