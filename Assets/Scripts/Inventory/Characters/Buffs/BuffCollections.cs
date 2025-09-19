using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Buff Collections", menuName = "Database/Buff Collections")]
public class BuffCollections : ScriptableObject
{
    // [SerializeField] private List<BuffSO> allBuffs;
    [SerializeField] private List<BuffSO> diseaseBuffs;

    public BuffSO GetDiseaseBuff(BuffSO.DiseaseType diseaseType)
    {
        return diseaseBuffs.Find(buff => buff.diseaseType == diseaseType);
    }

    public BuffSO GetRandomDiseaseBuff()
    {
        return diseaseBuffs[Random.Range(0, diseaseBuffs.Count)];
    }
}
