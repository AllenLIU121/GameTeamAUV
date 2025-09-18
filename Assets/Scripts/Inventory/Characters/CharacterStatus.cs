using System.Collections.Generic;
using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [Header("角色信息CharacterSO")]
    public CharacterSO characterSO;

    private BuffManager buffManager;
    private CharacterManager characterManager;

    // Runtime
    public bool IsAlive { get; private set; }
    private float currentStamina;
    private float currentHunger;

    private float randomDiseaseTimer = 0f;
    private float randomDiseaseCheckInterval = 5f; // 每5s跑一次随机患病概率

    public string CharacterID => characterSO.characterID;

    private void Awake()
    {
        characterManager = GameStateManager.Instance.Character;
        buffManager = GameStateManager.Instance.Buff;
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
    }

    private void Update()
    {
        if (IsAlive)
            UpdateContinuousStats(Time.deltaTime);
    }

    // 统一更新所有持续性状态变化: Buff衰减, 饥饿/体力惩罚
    private void UpdateContinuousStats(float deltaTime)
    {
        // --- 1.由于Buff引起的属性衰减 ---
        float staminaDecayModifier = buffManager.GetStatModifier(gameObject, BuffSO.StatType.StaminaDecayRate);  // 0f
        float hungerDecayModifier = buffManager.GetStatModifier(gameObject, BuffSO.StatType.HungerDecayRate);

        float finalStaminaDecay = characterSO.staminaDecayRate * (1 + staminaDecayModifier) * deltaTime; // 0f
        float finalHungerDecay = characterSO.hungerDecayRate * (1 + hungerDecayModifier) * deltaTime;

        // 应用修正后的衰减率
        // currentStamina -= baseDecay * staminaDecayModifier * Time.deltaTime;

        ModifyStamina(-finalStaminaDecay, false);
        ModifyHunger(-finalHungerDecay, false);

        // --- 2.饥饿值过低引起的体力流失 ---
        float hungerPercent = currentHunger / characterSO.maxHunger;
        float staminaPercent = currentStamina / characterSO.maxStamina;

        if (hungerPercent < 0.2f && staminaPercent > 0.1f)       // 饥饿值<20% 体力值>10%时 体力流失(速率6/min)
        {
            ModifyStamina(-(6f / 60f) * deltaTime, false);
        }
        else if (hungerPercent < 0.5f)       // 饥饿值<50%时 体力流失(速率5/min)
        {
            ModifyStamina(-(5f / 60f) * deltaTime, false);
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
                    ContractDisease(randomDisease);
                    Debug.Log($"<color=orange>'{characterSO.characterName}' has caught '{randomDisease}' because of low stamina.</color>");
                }
            }
        }
    }

    public void ModifyStamina(float amount, bool publishEvent = true)
    {
        if (!IsAlive) return;

        float oldValue = currentStamina;
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, characterSO.maxStamina);

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
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, characterSO.maxHunger);

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

    public void ContractDisease(BuffSO.DiseaseType diseaseType)
    {
        // 通过Buff系统应用疾病
        BuffSO diseaseBuff = GetDiseaseBuff(diseaseType);
        if (diseaseBuff != null)
        {
            buffManager.ApplyBuff(Target, diseaseBuff);
        }
    }

    private BuffSO GetDiseaseBuff(BuffSO.DiseaseType diseaseType)
    {
        // 根据疾病类型返回对应的BuffSO
        return null;
    }

    public void InitializeStats()
    {
        IsAlive = true;
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
        IsAlive = false;
        currentHunger = currentStamina = 0;

        // 清除所有Buff
        buffManager.ClearAllBuffs(gameObject);
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