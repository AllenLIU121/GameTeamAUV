using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public Dictionary<string, Dictionary<string, SkillRuntime>> characterSkillsData =
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
        // Debug.Log(GameManager.Instance.CurrentState);
        // if (GameManager.Instance.CurrentState == GameState.Playing)
        // {
        //     Debug.Log("触发更新");
        //     UpdateAllCooldowns(Time.deltaTime);
        // }
        // UpdateAllCooldowns(Time.deltaTime);
    }

    private void UpdateAllCooldowns(float deltaTime)
    {
        foreach (var characterId in characterSkillsData.Keys)
        {
            var skills = characterSkillsData[characterId];
            foreach (var skillId in skills.Keys)
            {
                Debug.Log(characterId+"的cd还剩下:"+skills[skillId].GetCooldown());
                skills[skillId].UpdateCooldown(deltaTime);
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
        //被动技能不注册
        if (skill.cooldownTime == 0) return;

        if (!characterSkillsData.ContainsKey(characterID))
        {
            Debug.Log("注册人的id：" + characterID);
            characterSkillsData[characterID] = new Dictionary<string, SkillRuntime>();
        }
        characterSkillsData[characterID][skill.skillID] = new SkillRuntime(skill);
        Debug.Log("注册后的技能cd：" + characterSkillsData[characterID][skill.skillID].GetCooldown());
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
        if (characterObject == null)
        {
            Debug.Log("characterObject为null");
            return false;
        }

        var status = characterObject.GetComponent<CharacterStatus>();
        if (status != null && status.IsAlive)
        {
            // 检测技能是否在冷却
            var skillRuntime = characterSkillsData[characterID][skillID];
            if (!skillRuntime.IsReady)
            {
                Debug.Log("技能在冷却");
                return false;
            }

            bool success = skillRuntime.SkillData.ExecuteSkill(slotIndex);
            if (success)
            {
                // 开始冷却
                skillRuntime.StartCooldown();
                Debug.Log("技能激活后，冷却时间为"+skillRuntime.GetCooldown());
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
        Debug.Log("触发技能冷却重置");
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
    //带物品的发布
    private void PublishSkillActivatedEvent(string characterID, string skillID,int slotIndex)
    {
        var skillRuntime = characterSkillsData[characterID][skillID];
        EventManager.Instance.Publish(new OnSkillActivated
        {
            characterID = characterID,
            skillID = skillID,
            cooldownTime = skillRuntime.SkillData.cooldownTime,
            currentCooldown = skillRuntime.CurrentCooldown
        });
    }
    public float GetRemainingCooldown(string characterID, string skillID)
    {
        if (characterSkillsData.ContainsKey(characterID) && 
            characterSkillsData[characterID].ContainsKey(skillID))
        {
            return characterSkillsData[characterID][skillID].GetCooldown();
        }
        else
        {
            return 1f;
        }
        
    }

    public float GetCooldownPercent(string characterID, string skillID)
    {
        if (characterSkillsData.ContainsKey(characterID) && 
            characterSkillsData[characterID].ContainsKey(skillID))
        {
            var skillRuntime = characterSkillsData[characterID][skillID];
            return skillRuntime.CurrentCooldown / skillRuntime.SkillData.cooldownTime;
        }
        return 0f;
    }
    
}