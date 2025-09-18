using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterData", menuName = "Game Data/Character Data")]
public class CharacterSO : ScriptableObject
{
    [Header("基础设置")]
    public string characterID;
    public string characterName;
    public Sprite characterPortrait;
    public string characterTag; // 角色专属标签 (除了婴儿角色外留空)

    [Header("角色属性")]
    public float maxStamina = 100f;
    public float maxHunger = 100f;
    public float staminaDecayRate = 0;
    public float hungerDecayRate = 0.08333f; // 5/min

    [Header("角色技能")]
    public SkillSO skill;
    
}

// public class Player : CharacterSO
// {
//     public void SetName(string name,string id)
//     {
//         characterName = name;
//         characterID = "01";
//     }
// }
//
// public class Family : CharacterSO
// {
//     public void SetName(string name,string id)
//     {
//         characterName = name;
//         characterID = id;
//     }
// }

// public enum StatType
// {
//     Stamina,
//     Hunger
// }