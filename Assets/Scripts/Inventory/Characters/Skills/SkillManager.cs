using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Dictionary<string, Dictionary<string, SkillRuntime>> characterSkillsData =
        new Dictionary<string, Dictionary<string, SkillRuntime>>();

    private void Awake()
    {
        GameStateManager.Instance.RegisterSkillManager(this);

        // EventManager.Instance.Subscribe<OnCharacterRegistered>(HandleCharacterRegistered);
        // EventManager.Instance.Subscribe<OnCharacterDied>(HandleCharacterDied);
    }

    private void Start()
    {
        StartCoroutine(ActivateSkillsAfterSeconds());
    } 

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing)
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
        
        // EventManager.Instance.Unsubscribe<OnCharacterRegistered>(HandleCharacterRegistered);
        // EventManager.Instance.Unsubscribe<OnCharacterDied>(HandleCharacterDied);
    }

    // private void HandleCharacterRegistered(OnCharacterRegistered eventData)
    // {
    //     Debug.Log($"[SkillManager] Activating passive skills for {eventData.characterSO.characterName}.");
    //     eventData.characterSO.skill.OnActivate(eventData.characterSO);
    // }

    // private void HandleCharacterDied(OnCharacterDied eventData)
    // {
    //     Debug.Log($"[SkillManager] Deactivating passive skills for {eventData.characterSO.characterName}.");
    //     eventData.characterSO.skill.OnDeactivate(eventData.characterSO);
    // }

    private IEnumerator ActivateSkillsAfterSeconds()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log($"[SkillManager] Activating passive skills...");
        foreach (var characterSO in GameStateManager.Instance.Character.GetAllAliveCharacterSOs())
        {
            if (characterSO.skill != null)
                characterSO.skill.OnActivate(characterSO);
        }
    }

    // 只注册主动技能并创建SkillRuntime实例
    public void RegisterCharacterSkill(string characterID, SkillSO skill)
    {
        if (skill.cooldownTime == 0) return;

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

        // 检查角色是否死亡
        var characterObject = GameStateManager.Instance.Character.GetCharacterGameObject(characterID);
        if (characterObject == null) return false;

        var status = characterObject.GetComponent<CharacterStatus>();
        if (status != null && status.IsAlive)
        {
            // 检测技能是否在冷却
            var skillRuntime = characterSkillsData[characterID][skillID];
            if (!skillRuntime.IsReady) return false;

            // 开始冷却
            skillRuntime.StartCooldown();

            // PublishSkillActivatedEvent(characterID, skillID);
            return true;
        }
        else
        {
            Debug.LogWarning($"Character '{characterID}' is dead.");
            return false;
        }
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

        // 检查角色是否死亡
        var characterObject = GameStateManager.Instance.Character.GetCharacterGameObject(characterID);
        if (characterObject == null) return false;

        var status = characterObject.GetComponent<CharacterStatus>();
        if (status != null && status.IsAlive)
        {
            // 检测技能是否在冷却
            var skillRuntime = characterSkillsData[characterID][skillID];
            if (!skillRuntime.IsReady) return false;

            bool success = skillRuntime.SkillData.ExecuteSkill(slotIndex);
            if (success)
            {
                // 开始冷却
                skillRuntime.StartCooldown();
                return true;
            }
            else
            {
                Debug.Log($"Failed to execute the skill: {skillRuntime.SkillData.skillName}.");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"Character '{characterID}' is dead.");
            return false;
        }
    }

    // 重置所有技能冷却
    public void ResetAllCooldowns()
    {
        foreach (var SkillData in characterSkillsData.Values)
        { 
            foreach(var skillRuntime in SkillData.Values)
            {
                skillRuntime.ResetCooldown();
            }
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