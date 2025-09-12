using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterData", menuName = "Game Data/Character Data")]
public class CharacterSO : ScriptableObject
{
    [Header("基础设置")]
    public string characterID;
    public string characterName;
    public Sprite characterPortrait;

    [Header("角色属性")]
    public float maxStamina = 100f;
    public float maxHunger = 100f;
    public float staminaDecayRate = 1f;
    public float hungerDecayRate = 1f;

    [Header("角色技能")]
    public SkillSO skill;
}

public enum StatType
{
    Stamina,
    Hunger
}