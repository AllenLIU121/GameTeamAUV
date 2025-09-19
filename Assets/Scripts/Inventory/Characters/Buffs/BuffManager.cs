using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff管理器 - 负责管理游戏中所有的Buff/Debuff效果
/// 包括Buff的施加、移除、更新和状态查询
/// </summary>
public class BuffManager : MonoBehaviour
{
    // 存储所有活动Buff的字典
    // Key: 目标游戏对象，Value: 该对象身上的所有ActiveBuff列表
    public Dictionary<CharacterSO, List<ActiveBuff>> activeBuffs = new Dictionary<CharacterSO, List<ActiveBuff>>();
    public BuffDatabase BuffDatabase { get; private set; }
    
    private GameStateManager gameStateManager;
    
    /// <summary>
    /// 活动Buff类 - 表示一个正在生效的Buff实例
    /// </summary>
    public class ActiveBuff
    {
        public BuffSO buff;           // Buff的数据脚本able object
        public float remainingTime;   // Buff剩余持续时间
        public CharacterSO source;     // Buff来源对象（谁施加的这个Buff）

        /// <summary>
        /// ActiveBuff构造函数
        /// </summary>
        /// <param name="buff">Buff数据</param>
        /// <param name="source">施加者</param>
        public ActiveBuff(BuffSO buff, CharacterSO source)
        {
            this.buff = buff;
            this.source = source;
            // 如果是永久Buff，设置时间为最大值，否则使用配置的持续时间
            this.remainingTime = buff.isPermanent ? float.MaxValue : buff.duration;
        }
    }

    private void Awake()
    {
        // 获取游戏状态管理器的单例实例
        gameStateManager = GameStateManager.Instance;
        // 注册到游戏状态管理器
        gameStateManager?.RegisterBuffManager(this);
        BuffDatabase.Initialize();
    }
    
    private void OnDestroy()
    {
        // 从游戏状态管理器取消注册
        gameStateManager?.UnregisterBuffManager();
    }
    
    private void Update()
    {
        // 每帧更新所有Buff的持续时间
        UpdateBuffs(Time.deltaTime);
    }
    
    /// <summary>
    /// 更新所有Buff的持续时间并处理过期Buff
    /// </summary>
    /// <param name="deltaTime">时间增量（秒）</param>
    private void UpdateBuffs(float deltaTime)
    {
        // 遍历字典中的所有目标对象
        foreach (var targetEntry in activeBuffs)
        {
            var target = targetEntry.Key;         // 当前目标对象
            var buffsToRemove = new List<ActiveBuff>(); // 待移除的Buff列表
            
            // 遍历该目标对象身上的所有Buff
            foreach (var activeBuff in targetEntry.Value)
            {
                // 如果不是永久Buff，减少剩余时间
                if (!activeBuff.buff.isPermanent)
                {
                    activeBuff.remainingTime -= deltaTime;
                }
                
                // 调用Buff的每帧更新逻辑
                activeBuff.buff.OnUpdate(target, deltaTime);
                
                // 检查Buff是否到期（非永久Buff且时间用完）
                if (activeBuff.remainingTime <= 0 && !activeBuff.buff.isPermanent)
                {
                    buffsToRemove.Add(activeBuff); // 添加到待移除列表
                }
            }
            
            // 移除所有到期的Buff
            foreach (var buffToRemove in buffsToRemove)
            {
                RemoveBuff(target, buffToRemove.buff);
            }
        }
    }
    
    /// <summary>
    /// 对目标对象施加一个Buff
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="buff">要施加的Buff数据</param>
    /// <param name="source">Buff来源对象（可选）</param>
    /// <returns>是否成功施加Buff</returns>
    public bool ApplyBuff(CharacterSO target, BuffSO buff, CharacterSO source = null)
    {
        // 安全检查：确保目标和Buff不为空
        if (target == null || buff == null) return false;
        
        // 如果目标对象还没有Buff列表，创建一个新的
        if (!activeBuffs.ContainsKey(target))
        {
            activeBuffs[target] = new List<ActiveBuff>();
        }
        
        // 查找目标对象是否已经有同类型的Buff
        var existingBuff = activeBuffs[target].Find(b => b.buff.buffID == buff.buffID);
        
        // 如果已存在同类型Buff
        if (existingBuff != null)
        {
            //只刷新持续时间
            existingBuff.remainingTime = buff.duration;
            // 发布Buff更新事件
            EventManager.Instance.Publish(new OnBuffUpdated
            {
                target = target,
                buff = buff,
                remainingTime = existingBuff.remainingTime,
            });
        }
        else
        {
            // 创建新的ActiveBuff实例
            var newActiveBuff = new ActiveBuff(buff, source);
            activeBuffs[target].Add(newActiveBuff);
            
            // 调用Buff的施加逻辑
            buff.OnApply(target);
            
            // 如果是疾病类型的Buff，发布疾病感染事件
            if (buff.buffType == BuffSO.BuffType.Disease && buff.diseaseType != BuffSO.DiseaseType.None)
            {
                EventManager.Instance.Publish(new OnDiseaseContracted
                {
                    target = target,
                    diseaseType = buff.diseaseType
                });
            }
            
            // 发布Buff施加事件
            EventManager.Instance.Publish(new OnBuffApplied
            {
                target = target,
                buff = buff,
                source = source
            });
        }
        
        return true; // 施加成功
    }
    
    /// <summary>
    /// 从目标对象移除指定的Buff
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="buff">要移除的Buff数据</param>
    /// <returns>是否成功移除Buff</returns>
    public bool RemoveBuff(CharacterSO target, BuffSO buff)
    {
        // 安全检查
        if (target == null || buff == null || !activeBuffs.ContainsKey(target)) return false;
        
        // 查找要移除的Buff
        var buffToRemove = activeBuffs[target].Find(b => b.buff.buffID == buff.buffID);
        if (buffToRemove != null)
        {
            // 调用Buff的移除逻辑
            buff.OnRemove(target);
            
            // 从列表中移除Buff
            activeBuffs[target].Remove(buffToRemove);
            
            // 如果是疾病类型的Buff，发布疾病治愈事件
            if (buff.buffType == BuffSO.BuffType.Disease && buff.diseaseType != BuffSO.DiseaseType.None)
            {
                EventManager.Instance.Publish(new OnDiseaseCured
                {
                    target = target,
                    diseaseType = buff.diseaseType
                });
            }
            
            // 发布Buff移除事件
            EventManager.Instance.Publish(new OnBuffRemoved
            {
                target = target,
                buff = buff
            });
            
            return true; // 移除成功
        }
        
        return false; // 未找到要移除的Buff
    }
    
    /// <summary>
    /// 计算目标对象指定属性的总修正值
    /// 处理加法和乘法修正的叠加
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="statType">要计算的属性类型</param>
    /// <returns>总修正值（加法修正 + 乘法修正）</returns>
    public float GetStatModifier(CharacterSO target, BuffSO.StatType statType, float baseValue)
    {
        float additiveModifier = 0f;      // 加法修正总和
        float multiplicativeModifier = 1f; // 乘法修正乘积（初始为1）
        
        if (target != null && activeBuffs.ContainsKey(target))
        {
            // 遍历所有Buff，计算属性修正
            foreach (var activeBuff in activeBuffs[target])
            {
                // 遍历Buff影响的所有属性
                foreach (var modifier in activeBuff.buff.statModifiers)
                {
                    if (modifier.statType == statType)
                    {
                        if (modifier.isMultiplicative)
                        {
                            // 乘法修正：各个修正值相乘
                            multiplicativeModifier *= 1 + modifier.value;
                        }
                        else
                        {
                            // 加法修正：考虑叠加层数
                            additiveModifier += modifier.value;
                        }
                    }
                }
            }
        }
        
        // 返回总修正值：加法修正 + (乘法修正 - 1)
        // 例如：乘法修正为1.2时，返回0.2（表示增加20%）
        return (baseValue + additiveModifier) * multiplicativeModifier;
    }

    /// <summary>
    /// 移除目标对象身上的特定类型疾病
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="diseaseType">要移除的疾病类型</param>
    /// <returns>是否成功移除疾病</returns>
    public bool RemoveDisease(CharacterSO target, BuffSO.DiseaseType diseaseType)
    {
        // 安全检查
        if (target == null || !activeBuffs.ContainsKey(target)) return false;

        // 查找指定类型的疾病Buff
        var diseaseBuff = activeBuffs[target].Find(b =>
            b.buff.buffType == BuffSO.BuffType.Disease && b.buff.diseaseType == diseaseType);

        // 如果找到疾病Buff，移除它
        if (diseaseBuff != null)
        {
            return RemoveBuff(target, diseaseBuff.buff);
        }

        return false; // 未找到指定类型的疾病
    }
    
    /// <summary>
    /// 检查目标对象是否拥有指定ID的Buff
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="buffID">要检查的Buff ID</param>
    /// <returns>是否拥有该Buff</returns>
    public bool HasBuff(CharacterSO target, string buffID)
    {
        return target != null && 
               activeBuffs.ContainsKey(target) && 
               activeBuffs[target].Exists(b => b.buff.buffID == buffID);
    }
    
    /// <summary>
    /// 检查目标对象是否患有指定类型的疾病
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="diseaseType">要检查的疾病类型</param>
    /// <returns>是否患有该疾病</returns>
    public bool HasDisease(CharacterSO target, BuffSO.DiseaseType diseaseType)
    {
        return target != null && 
               activeBuffs.ContainsKey(target) && 
               activeBuffs[target].Exists(b => 
                   b.buff.buffType == BuffSO.BuffType.Disease && 
                   b.buff.diseaseType == diseaseType);
    }
    
    /// <summary>
    /// 获取目标对象身上的所有Buff列表
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <returns>Buff列表（如果没有则返回空列表）</returns>
    public List<ActiveBuff> GetBuffs(CharacterSO target)
    {
        // 返回副本以避免外部修改影响内部数据
        if (target != null && activeBuffs.ContainsKey(target))
        {
            return new List<ActiveBuff>(activeBuffs[target]);
        }
        return new List<ActiveBuff>(); // 返回空列表而不是null
    }
    
    /// <summary>
    /// 获取目标对象身上的所有疾病Buff
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <returns>疾病Buff列表</returns>
    public List<ActiveBuff> GetDiseases(CharacterSO target)
    {
        var diseases = new List<ActiveBuff>();
        if (target != null && activeBuffs.ContainsKey(target))
        {
            // 筛选出所有疾病类型的Buff
            foreach (var buff in activeBuffs[target])
            {
                if (buff.buff.buffType == BuffSO.BuffType.Disease)
                {
                    diseases.Add(buff);
                }
            }
        }
        return diseases;
    }
    public void ClearAllBuffs(CharacterSO target)
    {
        if (target != null && activeBuffs.ContainsKey(target))
        {
            // 调用每个Buff的移除逻辑
            foreach (var buff in activeBuffs[target])
            {
                buff.buff.OnRemove(target);
            }
            
            // 清空Buff列表
            activeBuffs[target].Clear();
        }
    }
}