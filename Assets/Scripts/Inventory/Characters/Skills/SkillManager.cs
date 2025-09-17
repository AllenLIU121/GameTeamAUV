using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Dictionary<string, Dictionary<string, SkillRuntime>> characterSkillsData =
        new Dictionary<string, Dictionary<string, SkillRuntime>>();

    private void Awake()
    {
        GameStateManager.Instance.RegisterSkillManager(this);
    }

    private void Update()
    {
        UpdateAllCooldowns(Time.deltaTime);
    }

    private void UpdateAllCooldowns(float deltaTime)
    {
        foreach (var SkillData in characterSkillsData.Values)
        {
            foreach (var skillRuntime in SkillData.Values)
            {
                skillRuntime.UpdateCooldown(deltaTime);
            }
        }
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.UnregisterSkillManager();
    }

    // 注册角色技能并创建SkillRuntime实例
    public void RegisterCharacterSkill(string characterID, SkillSO skill)
    {
        if (!characterSkillsData.ContainsKey(characterID))
        {
            characterSkillsData[characterID] = new Dictionary<string, SkillRuntime>();
        }
        characterSkillsData[characterID][skill.skillID] = new SkillRuntime(skill);
    }

    // 激活技能
    public bool ActivateSkill(string characterID, string skillID)
    {
        if (!characterSkillsData.ContainsKey(characterID) ||
            !characterSkillsData[characterID].ContainsKey(skillID))
        {
            Debug.LogError("[SkillManager] Character or Skill not found");
            return false;
        }

        var skillRuntime = characterSkillsData[characterID][skillID];
        if (!skillRuntime.IsReady) return false;

        // 开始冷却
        if (!skillRuntime.IsPassive)
        {
            skillRuntime.StartCooldown();
        }

        PublishSkillActivatedEvent(characterID, skillID);
        return true;
    }

    // 激活作用于物品的角色技能
    public bool ActivateSkill(string characterID, string skillID, int slotIndex)
    {
        if (!characterSkillsData.ContainsKey(characterID) ||
            !characterSkillsData[characterID].ContainsKey(skillID))
        {
            Debug.LogError("[SkillManager] Character or Skill not found");
            return false;
        }

        var skillRuntime = characterSkillsData[characterID][skillID];
        if (!skillRuntime.IsReady) return false;

        bool success = skillRuntime.SkillData.ExecuteSkill(slotIndex);
        if (success)
        {
            if (!skillRuntime.IsPassive)
            {
                skillRuntime.StartCooldown();
            }
            PublishSkillActivatedEvent(characterID, skillID);
            return true;
        }
        else
        {
            Debug.Log($"Failed to execute the skill: {skillRuntime.SkillData.skillName}.");
            return false;
        }
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