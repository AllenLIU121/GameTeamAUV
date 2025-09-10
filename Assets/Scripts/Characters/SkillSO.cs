using UnityEngine;

public abstract class SkillSO : ScriptableObject
{
    [Header("技能信息")]
    public string skillName;
    [TextArea] public string skillDescription;
    public float cooldown;

    public abstract void Execute(string userCharacterID);
}
