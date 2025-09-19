using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Buff Database", menuName = "Database/Buff Database")]
public class BuffDatabase : ScriptableObject
{
    [SerializeField] private List<BuffSO> allBuffs;

    private Dictionary<string, BuffSO> buffsById;
    private Dictionary<BuffSO.DiseaseType, BuffSO> buffsByDisease;

    public void Initialize()
    {
        buffsById = new Dictionary<string, BuffSO>();
        foreach (var buff in allBuffs)
        {
            if (buff != null && !buffsById.ContainsKey(buff.buffID))
            {
                buffsById.Add(buff.buffID, buff);
            }
        }

        buffsByDisease = new Dictionary<BuffSO.DiseaseType, BuffSO>();
        foreach (var buff in allBuffs.Where(b => b != null && b.buffType == BuffSO.BuffType.Disease))
        {
            buffsByDisease.Add(buff.diseaseType, buff);
        }
    }

    public BuffSO GetBuff(string id)
    {
        buffsById.TryGetValue(id, out BuffSO buff);
        if (buff == null) Debug.LogWarning($"[BuffDatabase] Buff with ID '{id}' not found.");
        return buff;
    }

    public BuffSO GetBuff(BuffSO.DiseaseType diseaseType)
    {
        buffsByDisease.TryGetValue(diseaseType, out BuffSO buff);
        if (buff == null) Debug.LogWarning($"[BuffDatabase] Disease buff '{diseaseType}' not found.");
        return buff;
    }
}
