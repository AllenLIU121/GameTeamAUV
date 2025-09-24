using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Dictionary<string, SkillRuntime> characterSkillsData =
        new Dictionary<string, SkillRuntime>();

    private void Awake()
    {
        GameStateManager.Instance.RegisterSkillManager(this);

        EventManager.Instance.Subscribe<OnSkillActivated>(AttemptToActivateSkill);
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
        foreach (var entry in characterSkillsData)
        {
            var skillRuntime = entry.Value;
            if (skillRuntime.IsReady) continue;

            float oldCooldown = skillRuntime.CurrentCooldown;
            skillRuntime.UpdateCooldown(deltaTime);

            if (oldCooldown > 0 && skillRuntime.IsReady)
            {
                EventManager.Instance.Publish(new OnSkillCooldownEnded { characterID = entry.Key });
            }
        }
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.UnregisterSkillManager();
        EventManager.Instance.Unsubscribe<OnSkillActivated>(AttemptToActivateSkill);
    }

    private IEnumerator ActivateSkillsAfterSeconds()
    {
        yield return new WaitForSeconds(0.5f);
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

        characterSkillsData[characterID] = new SkillRuntime(skill);
    }

    // // 激活技能
    // public bool ActivateSkill(string characterID, string skillID)
    // {
    //     if (!characterSkillsData.ContainsKey(characterID) ||
    //         !characterSkillsData[characterID].ContainsKey(skillID))
    //     {
    //         Debug.LogError("[SkillManager] Character or Skill not found");
    //         return false;
    //     }

    //     // 检查角色是否死亡
    //     var characterObject = GameStateManager.Instance.Character.GetCharacterGameObject(characterID);
    //     if (characterObject == null) return false;

    //     var status = characterObject.GetComponent<CharacterStatus>();
    //     if (status != null && status.IsAlive)
    //     {
    //         // 检测技能是否在冷却
    //         var skillRuntime = characterSkillsData[characterID][skillID];
    //         if (!skillRuntime.IsReady) return false;

    //         // 开始冷却
    //         skillRuntime.StartCooldown();

    //         // PublishSkillActivatedEvent(characterID, skillID);
    //         return true;
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"Character '{characterID}' is dead.");
    //         return false;
    //     }
    // }

    // 激活主动技能
    public void AttemptToActivateSkill(OnSkillActivated eventData)
    {
        string characterID = eventData.characterID;
        if (!characterSkillsData.TryGetValue(characterID, out SkillRuntime skillRuntime))
        {
            Debug.Log($"[SkillManager] Character '{characterID}' doesn't have a active skill");
            return;
        }

        var characterStatus = GameStateManager.Instance.Character.GetCharacterStatus(characterID);
        if (characterStatus == null || !characterStatus.IsAlive)
        {
            Debug.LogWarning($"Character '{characterID}' is dead.");
            return;
        }

        // 检测技能是否在冷却
        if (!skillRuntime.IsReady)
        {
            Debug.Log($"[SkillManager] Skill '{skillRuntime.SkillData.skillName}' is on cooldown.");
            return;
        }

        if (skillRuntime.SkillData.targetItem)
        {
            TargetingManager.Instance.BeginTargeting(characterID, skillRuntime.SkillData);  // 需要玩家选择物品栏位
        }
        else
        {
            if (skillRuntime.SkillData.ExecuteSkill())  // 直接执行
            {
                // 开始冷却
                skillRuntime.StartCooldown();
                EventManager.Instance.Publish(new OnSkillCooldownStarted
                {
                    characterID = characterID,
                    maxCooldown = skillRuntime.SkillData.cooldownTime
                });
                Debug.Log($"[SkillManager] Skill '{skillRuntime.SkillData.skillID}' activated.");
            }
        }
    }

    // TargetingManager选择物品栏位上的物品后执行技能
    public void ExecuteSkillWithTarget(string characterID, SkillSO skillSO, int slotIndex)
    {
        if (!characterSkillsData.TryGetValue(characterID, out SkillRuntime skillRuntime)
            || skillRuntime.SkillData != skillSO)
        {
            Debug.LogError($"[SkillManager] Data not found: Caster={characterID}, Skill={skillSO.skillName}");
            return;
        }

        if (skillRuntime.SkillData.ExecuteSkill(slotIndex))
        {
            skillRuntime.StartCooldown();
            EventManager.Instance.Publish(new OnSkillCooldownStarted
            {
                characterID = characterID,
                maxCooldown = skillRuntime.SkillData.cooldownTime
            });
            Debug.Log($"[SkillManager] 技能 '{skillRuntime.SkillData.skillName}' 已成功对槽位 {slotIndex} 释放。");
        }
        else
        {
            Debug.LogWarning($"[SkillManager] 技能 '{skillRuntime.SkillData.skillName}' 对槽位 {slotIndex} 释放失败。");
        }
    }

    // public void ActivateSkill(OnSkillActivated eventData)
    // {
    //     string characterID = eventData.characterID;
    //     if (!characterSkillsData.ContainsKey(characterID) ||
    //         characterSkillsData[characterID] == null)
    //     {
    //         Debug.LogError("[SkillManager] Character or Skill not found");
    //     }

    //     // 检查角色是否死亡
    //     var characterObject = GameStateManager.Instance.Character.GetCharacterGameObject(characterID);
    //     if (characterObject == null) return;

    //     var status = characterObject.GetComponent<CharacterStatus>();
    //     if (status != null && status.IsAlive)
    //     {
    //         // 检测技能是否在冷却
    //         var skillRuntime = characterSkillsData[characterID];
    //         if (!skillRuntime.IsReady) return;

    //         bool success = false;
    //         if (skillRuntime.targetItem)
    //             success = WaitForItemIndex();
    //         else
    //             success = skillRuntime.SkillData.ExecuteSkill();

    //         if (success)
    //         {
    //             // 开始冷却
    //             skillRuntime.StartCooldown();
    //         }
    //         else
    //         {
    //             Debug.Log($"Failed to execute the skill: {skillRuntime.SkillData.skillName}.");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"Character '{characterID}' is dead.");
    //     }
    // }

    // 重置所有技能冷却
    public void ResetAllCooldowns()
    {
        foreach (var skillRuntime in characterSkillsData.Values)
        {
            skillRuntime.ResetCooldown();
        }
    }
}