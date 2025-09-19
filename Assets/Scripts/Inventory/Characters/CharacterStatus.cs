using System.Collections.Generic;
using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [Header("角色信息CharacterSO")]
    public CharacterSO characterSO;

    private BuffManager buffManager;
    private BuffDatabase buffDatabase;
    private CharacterManager characterManager;

    // Runtime
    public bool IsAlive { get; private set; }
    public float CurrentStamina => currentStamina;
    private float currentStamina;
    private float currentHunger;

    private float randomDiseaseTimer = 0f;
    private float randomDiseaseCheckInterval = 5f; // 每5s跑一次随机患病概率

    public string CharacterID => characterSO.characterID;
    // public float MaxStamina => buffManager.GetStatModifier(characterSO, BuffSO.StatType.MaxStamina, characterSO.maxStamina);
    // public float MaxHunger => buffManager.GetStatModifier(characterSO, BuffSO.StatType.MaxHunger, characterSO.maxHunger);
    // public float StaminaDecayRate => buffManager.GetStatModifier(characterSO, BuffSO.StatType.StaminaDecayRate, characterSO.staminaDecayRate);
    // public float HungerDecayRate => buffManager.GetStatModifier(characterSO, BuffSO.StatType.HungerDecayRate, characterSO.hungerDecayRate);
    public float MaxStamina { get; private set; }
    public float MaxHunger { get; private set; }
    public float StaminaDecayRate { get; private set; }
    public float HungerDecayRate { get; private set; }


    private void Awake()
    {
        characterManager = GameStateManager.Instance.Character;
        buffManager = GameStateManager.Instance.Buff;
        if (buffManager != null)
        {
            buffDatabase = buffManager.BuffDatabase;
        }

        EventManager.Instance.Subscribe<OnBuffApplied>(HandleBuffApplied);
        EventManager.Instance.Subscribe<OnBuffRemoved>(HandleBuffRemoved);
    }

    private void Start()
    {
        if (characterManager != null)
        {
            characterManager.RegisterCharacter(characterSO, gameObject);
        }

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
        if (IsAlive)
            UpdateContinuousStats(Time.deltaTime);
    }
    
    // 统一更新所有持续性状态变化: Buff衰减, 饥饿/体力惩罚
    private void UpdateContinuousStats(float deltaTime)
    {
        ModifyStamina(-StaminaDecayRate * deltaTime, false);
        ModifyHunger(-HungerDecayRate * deltaTime, false);

        // --- 2.饥饿值过低引起的体力流失 ---
        float hungerPercent = currentHunger / MaxHunger;
        float staminaPercent = currentStamina / MaxStamina;

        if (hungerPercent <= 0f)            // 饥饿值为0时 体力流失(速率0.13/s)
        {
            ModifyStamina(-0.13f * deltaTime, false); 
        }
        else if (hungerPercent < 0.2f && staminaPercent > 0.1f)       // 饥饿值<20% 体力值>10%时 体力流失(速率0.1/s)
        {
            ModifyStamina(-0.1f * deltaTime, false);
        }
        else if (hungerPercent < 0.5f)       // 饥饿值<50%时 体力流失(速率0.08/s) 
        {
            ModifyStamina(-0.08f * deltaTime, false);
        }

        // --- 3.体力值低于30%时 有20%几率获取随机疾病Debuff ---
        if (staminaPercent < 0.3f)
        {
            randomDiseaseTimer += deltaTime;
            if (randomDiseaseTimer >= randomDiseaseCheckInterval)
            {
                randomDiseaseTimer = 0f;
                if (Random.value < 0.2f)
                {
                    var diseases = new List<BuffSO.DiseaseType> { BuffSO.DiseaseType.Cold, BuffSO.DiseaseType.Diarrhea, BuffSO.DiseaseType.Pneumonia };
                    var randomDisease = diseases[Random.Range(0, diseases.Count)];
                    characterManager.ContractDisease(characterSO, randomDisease);
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
        if (buffManager != null) return;
        MaxStamina = buffManager.GetStatModifier(characterSO, BuffSO.StatType.MaxStamina, characterSO.maxStamina);
        MaxHunger = buffManager.GetStatModifier(characterSO, BuffSO.StatType.MaxHunger, characterSO.maxHunger);
        StaminaDecayRate = buffManager.GetStatModifier(characterSO, BuffSO.StatType.StaminaDecayRate, characterSO.staminaDecayRate);
        HungerDecayRate = buffManager.GetStatModifier(characterSO, BuffSO.StatType.HungerDecayRate, characterSO.hungerDecayRate);

        currentStamina = Mathf.Min(currentStamina, MaxStamina);
        currentHunger = Mathf.Min(currentHunger, MaxHunger);
    }

    public void ModifyStamina(float amount, bool publishEvent = true)
    {
        if (!IsAlive) return;

        float oldValue = currentStamina;
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, MaxStamina);

        if (publishEvent && Mathf.Abs(currentStamina - oldValue) > 0.01f)
        {
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
        float oldValue = currentHunger;
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, MaxHunger);

        if (publishEvent && Mathf.Abs(currentHunger - oldValue) > 0.01f)
        {
            EventManager.Instance.Publish(new OnCharacterStatChanged
            {
                characterID = this.CharacterID,
                statType = BuffSO.StatType.Hunger,
                newValue = currentHunger,
                changeAmount = amount
            });
        }
    }

    public void InitializeStats()
    {
        IsAlive = true;
        RecalculateStats();
        currentStamina = characterSO.maxStamina;
        currentHunger = characterSO.maxHunger;
    }

    public void LoadState(CharacterRuntimeData data)
    {
        IsAlive = data.isAlive;
        currentStamina = data.currentStamina;
        currentHunger = data.currentHunger;
    }

    public CharacterRuntimeData GetStateForSaving()
    {
        return new CharacterRuntimeData
        {
            characterID = this.CharacterID,
            isAlive = IsAlive,
            maxStamina = characterSO.maxStamina,
            maxHunger = characterSO.maxHunger,
            currentStamina = this.currentStamina,
            currentHunger = this.currentHunger
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