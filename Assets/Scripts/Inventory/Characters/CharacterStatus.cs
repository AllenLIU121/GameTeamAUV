using System.Collections.Generic;
using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [Header("角色信息和控制组件")]
    public CharacterSO characterSO;

    private BuffManager buffManager;
    private CharacterManager characterManager;

    // Runtime
    public bool IsAlive { get; private set; }
    public float CurrentStamina => currentStamina;
    public float CurrentHunger => currentHunger;
    private float currentStamina;
    private float currentHunger;

    private float randomDiseaseTimer = 5f;
    private float randomDiseaseCheckInterval = 1f; // 每5s跑一次随机患病概率

    public string CharacterID => characterSO.characterID;
    // public float MaxStamina => buffManager.GetStatModifier(characterSO, BuffSO.StatType.MaxStamina, characterSO.maxStamina);
    // public float MaxHunger => buffManager.GetStatModifier(characterSO, BuffSO.StatType.MaxHunger, characterSO.maxHunger);
    // public float StaminaDecayRate => buffManager.GetStatModifier(characterSO, BuffSO.StatType.StaminaDecayRate, characterSO.staminaDecayRate);
    // public float HungerDecayRate => buffManager.GetStatModifier(characterSO, BuffSO.StatType.HungerDecayRate, characterSO.hungerDecayRate);
    public float MaxStamina { get; private set; }
    public float MaxHunger { get; private set; }
    public float StaminaDecayRate { get; private set; }
    public float HungerDecayRate { get; private set; }

    private float directMaxStaminaModifier = 0f;

    private void Awake()
    {
        EventManager.Instance.Subscribe<OnBuffApplied>(HandleBuffApplied);
        EventManager.Instance.Subscribe<OnBuffRemoved>(HandleBuffRemoved);
    }

    private void Start()
    {
        characterManager = GameStateManager.Instance.Character;
        if (characterManager != null)
        {
            characterManager.RegisterCharacter(characterSO, gameObject);
        }

        buffManager = GameStateManager.Instance.Buff;

        // 初始化角色状态, 加载存档时此数据会被覆盖
        InitializeStats();
    }

    private void OnDestroy()
    {
        if (characterManager != null)
        {
            characterManager.UnregisterCharacter(characterSO);
        }
        
        EventManager.Instance.Unsubscribe<OnBuffApplied>(HandleBuffApplied);
        EventManager.Instance.Unsubscribe<OnBuffRemoved>(HandleBuffRemoved);
    }


    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing && IsAlive)
            UpdateContinuousStats(Time.deltaTime);
    }
    
    // 统一更新所有持续性状态变化: Buff衰减, 饥饿/体力惩罚
    private void UpdateContinuousStats(float deltaTime)
    {
        // 基础衰减速率
        float currentFrameStaminaDecay = StaminaDecayRate;
        float currentFrameHungerDecay = HungerDecayRate;

        // 饥饿值过低引起的体力流失
        float hungerPercent = currentHunger / MaxHunger;
        float staminaPercent = currentStamina / MaxStamina;

        if (hungerPercent <= 0f)            // 饥饿值为0时 体力流失(速率0.13/s)
        {
            currentFrameStaminaDecay = 0.13f * 10; 
        }
        else if (hungerPercent < 0.2f && staminaPercent > 0.1f)       // 饥饿值<20% 体力值>10%时 体力流失(速率0.1/s)
        {
            currentFrameStaminaDecay = 0.1f * 10;
        }
        else if (hungerPercent < 0.5f)       // 饥饿值<50%时 体力流失(速率0.08/s) 
        {
            currentFrameStaminaDecay = 0.08f * 10;
        }

        // 最终衰减值
        ModifyStamina(-currentFrameStaminaDecay * deltaTime, true);
        ModifyHunger(-currentFrameHungerDecay * deltaTime, true);

        // 体力值低于30%时 有20%几率获取随机疾病Debuff
        if (staminaPercent < 0.95f)
        {
            randomDiseaseTimer += deltaTime;
            if (randomDiseaseTimer >= randomDiseaseCheckInterval)
            {
                randomDiseaseTimer = 0f;
                if (Random.value < 0.95f)
                {
                    var randomDisease = buffManager.buffCollections.GetRandomDiseaseBuff();
                    buffManager.ApplyBuff(characterSO, randomDisease);
                    Debug.Log($"<color=orange>'{characterSO.characterName}' has caught '{randomDisease}' because of low stamina.</color>");
                }
            }
        }
    }

    private void HandleBuffApplied(OnBuffApplied eventData)
    {
        string eventCharacterID = eventData.target.characterID;
        if (eventCharacterID == characterSO.characterID)
        {
            RecalculateStats();
        }
    }

    private void HandleBuffRemoved(OnBuffRemoved eventData)
    {
        string eventCharacterID = eventData.target.characterID;
        if (eventCharacterID == characterSO.characterID)
        {
            RecalculateStats();
        }
    }

    public void RecalculateStats()
    {
        if (buffManager == null) return;

        float runtimeBaseMaxStamina = characterSO.maxStamina + directMaxStaminaModifier;

        MaxStamina = buffManager.GetStatModifier(characterSO, BuffSO.StatType.MaxStamina, runtimeBaseMaxStamina);
        MaxHunger = buffManager.GetStatModifier(characterSO, BuffSO.StatType.MaxHunger, characterSO.maxHunger);
        StaminaDecayRate = buffManager.GetStatModifier(characterSO, BuffSO.StatType.StaminaDecayRate, characterSO.staminaDecayRate);
        HungerDecayRate = buffManager.GetStatModifier(characterSO, BuffSO.StatType.HungerDecayRate, characterSO.hungerDecayRate);

        currentStamina = Mathf.Min(currentStamina, MaxStamina);
        currentHunger = Mathf.Min(currentHunger, MaxHunger);
    }

    public void ModifyStamina(float amount, bool publishEvent = true)
    {
        if (!IsAlive || amount == 0) return;

        float oldValue = currentStamina;
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, MaxStamina);

        if (publishEvent && Mathf.Abs(currentStamina - oldValue) > 0f)
        {
            Debug.Log($"[CharacterStatus] 发布体力值变化事件 - 角色ID: {characterSO.characterID}, 旧值: {oldValue}, 新值: {currentStamina}, 变化量: {amount}");
            // Debug.Log($"{characterSO.characterID} CharacterStatus: Publish OnCharacterStatChanged Event (Stamina)");
            EventManager.Instance.Publish(new OnCharacterStatChanged
            {
                characterID = characterSO.characterID,
                statType = BuffSO.StatType.Stamina,
                newValue = currentStamina,
                changeAmount = amount
            });
        }

        if (currentStamina <= 0 && IsAlive)
        {
            Die();
        }
    }

    public void ModifyHunger(float amount, bool publishEvent = true)
    {
        if (!IsAlive || amount == 0) return;
        
        float oldValue = currentHunger;
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, MaxHunger);

        if (publishEvent && Mathf.Abs(currentHunger - oldValue) > 0f)
        {
            Debug.Log($"[CharacterStatus] 发布饥饿值变化事件 - 角色ID: {characterSO.characterID}, 旧值: {oldValue}, 新值: {currentHunger}, 变化量: {amount}");
            // Debug.Log($"{characterSO.characterID} CharacterStatus: Publish OnCharacterStatChanged Event (Hunger)");
            EventManager.Instance.Publish(new OnCharacterStatChanged
            {
                characterID = this.CharacterID,
                statType = BuffSO.StatType.Hunger,
                newValue = currentHunger,
                changeAmount = amount
            });
        }
    }

    public void ModifyMaxStamina(float amount)
    {
        if (!IsAlive) return;

        directMaxStaminaModifier += amount;
        RecalculateStats();

        EventManager.Instance.Publish(new OnCharacterStatChanged
        {
            characterID = characterSO.characterID,
            statType = BuffSO.StatType.MaxStamina,
            newValue = MaxStamina,
            changeAmount = amount
        });

        if (currentStamina == (MaxStamina - amount) && amount > 0)
        {
            ModifyStamina(amount);
        }
    }

    public void InitializeStats()
    {
        currentStamina = characterSO.maxStamina;
        currentHunger = characterSO.maxHunger;
        StaminaDecayRate = characterSO.staminaDecayRate;
        HungerDecayRate = characterSO.hungerDecayRate;

        RecalculateStats();
        IsAlive = true;

        Debug.Log($"Character {characterSO.characterID} is alive: {IsAlive}");
        Debug.Log($"CurrentStamina: {currentStamina}, MaxStamina: {MaxStamina}, StaminaDecayRate: {StaminaDecayRate}");
        Debug.Log($"CurrentHunger: {currentHunger}, MaxHunger: {MaxHunger}, HungerDecayRate: {HungerDecayRate}");
    }

    public void LoadState(CharacterRuntimeData data)
    {
        directMaxStaminaModifier = data.directMaxStaminaModifier;
        RecalculateStats();

        currentStamina = data.currentStamina;
        currentHunger = data.currentHunger;
        IsAlive = data.isAlive;        
    }

    public CharacterRuntimeData GetStateForSaving()
    {
        return new CharacterRuntimeData
        {
            characterID = this.CharacterID,
            isAlive = IsAlive,
            currentStamina = this.currentStamina,
            currentHunger = this.currentHunger,
            directMaxStaminaModifier = this.directMaxStaminaModifier
        };
    }

    // 角色死亡逻辑 
    public void Die()
    {
        if (!IsAlive) return;

        IsAlive = false;
        currentHunger = currentStamina = 0;

        // 清除所有Buff
        buffManager.ClearAllBuffs(characterSO);

        EventManager.Instance.Publish(new OnCharacterDied
        {
            characterSO = characterSO
        });
        Debug.Log($"<color=red>Character '{characterSO.characterID}' has died.</color>");

        if(characterSO.skill != null)
            characterSO.skill.OnDeactivate(characterSO);
    }

    // 角色复活逻辑, 体力饥饿恢复一半
    public void Resurrect()
    {
        if (IsAlive) return;

        IsAlive = true;
        ModifyStamina(characterSO.maxStamina / 2, false);
        ModifyHunger(characterSO.maxHunger / 2, false);
    }
}