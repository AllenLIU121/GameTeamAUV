using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingManager : Singleton<TargetingManager>
{
    // 角色技能目标
    private string characterID;
    private SkillSO skillSO;
    private bool isTargeting = false;
    public bool IsTargeting => isTargeting;

    public void BeginTargeting(string characterID, SkillSO skillSO)
    {
        isTargeting = true;
        this.characterID = characterID;
        this.skillSO = skillSO;
        Debug.Log($"[TargetingManager] Enter targeting mode, waiting for item selection...");
    }

    public void SelectTarget(int slotIndex)
    {
        if (!isTargeting) return;

        GameStateManager.Instance.Skill.ExecuteSkillWithTarget(characterID, skillSO, slotIndex);
        EndTargeting();
    }

    public void CancelTargeting()
    {
        if (!isTargeting) return;
        EndTargeting();
    }

    public void EndTargeting()
    {
        isTargeting = false;
        characterID = null;
        Debug.Log($"[TargetingManager] Targeting ended.");
    }
}
