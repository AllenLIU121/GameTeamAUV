using UnityEngine;
public class CharacterStatus : MonoBehaviour
{
    private BuffManager buffManager;
    
    private void Start()
    {
        buffManager = GameStateManager.Instance.Buff;
    }
    
    private void Update()
    {
        UpdateStatsWithBuffs();
    }
    
    private void UpdateStatsWithBuffs()
    {
        // 通过BuffManager获取状态修正
        float staminaDecayModifier = buffManager.GetStatModifier(gameObject, BuffSO.StatType.StaminaDecayRate);
        float hungerDecayModifier = buffManager.GetStatModifier(gameObject, BuffSO.StatType.HungerDecayRate);
        
        // 应用修正后的衰减率
        // currentStamina -= baseDecay * staminaDecayModifier * Time.deltaTime;
    }
    
    public void ContractDisease(BuffSO.DiseaseType diseaseType)
    {
        // 通过Buff系统应用疾病
        BuffSO diseaseBuff = GetDiseaseBuff(diseaseType);
        if (diseaseBuff != null)
        {
            buffManager.ApplyBuff(gameObject, diseaseBuff);
        }
    }
    
    private BuffSO GetDiseaseBuff(BuffSO.DiseaseType diseaseType)
    {
        // 根据疾病类型返回对应的BuffSO
        return null;
    }
}