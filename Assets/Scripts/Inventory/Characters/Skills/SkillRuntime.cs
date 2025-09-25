using UnityEngine;

[System.Serializable]
public class SkillRuntime
{
    public SkillSO SkillData { get; private set; }
    public float CurrentCooldown { get; private set; }
    public bool targetItem;
    public bool IsReady => CurrentCooldown <= 0;
    
    public SkillRuntime(SkillSO skillData)
    {
        SkillData = skillData;
        CurrentCooldown = 0f;
        targetItem = skillData.targetItem;
    }

    public void UpdateCooldown(float deltaTime)
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown = Mathf.Max(0, CurrentCooldown - deltaTime);
        }
    }

    public void StartCooldown() => CurrentCooldown = SkillData.cooldownTime;
    public void ResetCooldown() => CurrentCooldown = 0;

    public float GetCooldown()
    {
        return CurrentCooldown;
    }
}
