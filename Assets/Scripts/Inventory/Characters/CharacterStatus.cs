using UnityEngine;
public class CharacterStatus : MonoBehaviour
{
    public CharacterSO Target;//buff收益者
    private BuffManager buffManager;
    private void Start()
    {
        buffManager = GameStateManager.Instance.Buff;
    }
    
    // private void Update()
    // {
    //     UpdateStatsWithBuffs();
    // }
    
    /// <summary>
    /// 计算目标对象指定属性的总修正值
    /// 处理加法和乘法修正的叠加
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="statType">要计算的属性类型</param>
    /// <returns>总修正值（加法修正 + 乘法修正）</returns>
    public float GetStatModifier(CharacterSO target, BuffSO.StatType statType)
    {
        float additiveModifier = 0f;      // 加法修正总和
        float multiplicativeModifier = 1f; // 乘法修正乘积（初始为1）
        
        if (target != null && buffManager.activeBuffs.ContainsKey(target))
        {
            // 遍历所有Buff，计算属性修正
            foreach (var activeBuff in buffManager.activeBuffs[target])
            {
                // 遍历Buff影响的所有属性
                for (int i = 0; i < activeBuff.buff.affectedStats.Count; i++)
                {
                    if (activeBuff.buff.affectedStats[i] == statType)
                    {
                        if (activeBuff.buff.isMultiplicative)
                        {
                            // 乘法修正：各个修正值相乘
                            multiplicativeModifier *= activeBuff.buff.statModifiers[i];
                        }
                        else
                        {
                            // 加法修正：考虑叠加层数
                            additiveModifier += activeBuff.buff.statModifiers[i];
                        }
                    }
                }
            }
        }
        
        // 返回总修正值：加法修正 + (乘法修正 - 1)
        // 例如：乘法修正为1.2时，返回0.2（表示增加20%）
        return additiveModifier + (multiplicativeModifier - 1f);
    }
    private void UpdateStatsWithBuffs()
    {
        // 通过BuffManager获取状态修正
        float staminaDecayModifier = GetStatModifier(Target, BuffSO.StatType.StaminaDecayRate);
        float hungerDecayModifier = GetStatModifier(Target, BuffSO.StatType.HungerDecayRate);
        
        // 应用修正后的衰减率
        // currentStamina -= baseDecay * staminaDecayModifier * Time.deltaTime;
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
}