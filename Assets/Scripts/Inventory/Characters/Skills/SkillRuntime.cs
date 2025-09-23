using UnityEngine;

[System.Serializable]
public class SkillRuntime
{
    public SkillSO SkillData { get; private set; }
    public float CurrentCooldown { get; private set; }
    public bool IsReady => CurrentCooldown <= 0;
    
    public SkillRuntime(SkillSO skillData)
    {
        SkillData = skillData;
        //如果这里只是初始化的化，这里就写成0f
        CurrentCooldown =0f;
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
