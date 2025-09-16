using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Dictionary<string, Dictionary<string, SkillSO>> characterSkills = 
        new Dictionary<string, Dictionary<string, SkillSO>>();
    
    private GameStateManager gameStateManager;
    
    private void Awake()
    {
        gameStateManager = GameStateManager.Instance;
        InitializeAllSkills();
    }
    
    private void Update()
    {
        UpdateAllCooldowns();
    }

    private void UpdateAllCooldowns()
    {
        
    }
    // 初始化所有技能
    private void InitializeAllSkills()
    {
        foreach (var character in characterSkills)
        {
            foreach (var skill in character.Value.Values)
            {
                skill.Initialize();
            }
        }
    }
    
    // 注册角色技能
    public void RegisterCharacterSkills(string characterID, List<SkillSO> skills)
    {
        if (!characterSkills.ContainsKey(characterID))
        {
            characterSkills[characterID] = new Dictionary<string, SkillSO>();
        }
        
        foreach (var skill in skills)
        {
            skill.Initialize();
            characterSkills[characterID][skill.skillID] = skill;
            
        }
    }
    
    // 激活技能
    public bool ActivateSkill(string characterID, string skillID)
    {
        if (!characterSkills.ContainsKey(characterID) || 
            !characterSkills[characterID].ContainsKey(skillID))
        {
            return false;
        }
        
        var skill = characterSkills[characterID][skillID];
        
        if (!skill.IsReady)
        {
            PublishSkillActivatedEvent(characterID, skillID);
            return false;
        }
        
        // 开始冷却
        if (!skill.isPassive)
        {
            skill.StartCooldown();
        }
        
        
        PublishSkillActivatedEvent(characterID, skillID);
        return true;
    }
    
    
    private void PublishSkillActivatedEvent(string characterID, string skillID)
    {
        EventManager.Instance.Publish(new OnSkillActivated
        {
            characterID = characterID,
            skillID = skillID,
        });
    }
    
}