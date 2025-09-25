using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreManager : Singleton<StoreManager>
{
    [Header("商店配置")]
    public StoreConfigSO storeSO;
    public float essentialItemRate { get; private set; }
    public bool IsOpen { get; private set; } = false;

    private void Start()
    {
        if (storeSO == null)
        {
            Debug.LogError("[StoreManager] StoreSO is null]");
            return;
        }
        essentialItemRate = storeSO.essentialItemRate;
    }

    public void OpenStore()
    {
        AudioManager.Instance.PlaySFX("便利店0919-1.mp3");
        StoreUI storeUI = FindAnyObjectByType<StoreUI>();
        if (storeUI != null)
        {
            storeUI.ActivateStoreUI();
            IsOpen = true;
        }
    }

    public void CloseStore()
    {
        StoreUI storeUI = FindAnyObjectByType<StoreUI>();
        if (storeUI != null)
        {
            storeUI.DeactivateStoreUI();
            IsOpen = false;
        }
    }

    public void CloseAndContinueToNextScene()
    {
        AudioManager.Instance.StopBGM();
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError($"[StoreManager] Player not found]");
            return;
        }

        playerGO.GetComponent<CharacterMove>().BeforeSceneChanged();
    }

    // 预计算总权重
    private float totalSpecialWeight = -1f;

    // 生成商店物品列表
    public List<ItemSO> GenerateStoreItems()
    {
        if (totalSpecialWeight < 0f)
        {
            totalSpecialWeight = storeSO.specialItemsPool.Sum(item => item.weight);
        }

        List<ItemSO> generatedItems = new List<ItemSO>();
        for (int i = 0; i < storeSO.numberOfSlots; i++)
        {
            ItemSO selectedItem = null;
            float categoryRoll = Random.Range(0f, 1f);
            if (categoryRoll < essentialItemRate)
            {
                // 刷新必需品
                if (storeSO.essentialItemsPool.Count > 0)
                {
                    int randomIndex = Random.Range(0, storeSO.essentialItemsPool.Count);
                    selectedItem = storeSO.essentialItemsPool[randomIndex];
                }
            }
            else
            {
                // 刷新特殊物品
                if (storeSO.specialItemsPool.Count > 0 && totalSpecialWeight > 0f)
                {
                    selectedItem = GetRandomItemFromPool(storeSO.specialItemsPool, totalSpecialWeight);
                }
            }

            if (selectedItem != null)
            {
                generatedItems.Add(selectedItem);
            }
        }
        return generatedItems;
    }

    // 从指定物品池中根据权重随机抽取一个物品
    private ItemSO GetRandomItemFromPool(List<WeightedItem> pool, float totalWeight)
    {
        float weightRoll = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        foreach (var weightedItem in pool)
        {
            currentWeight += weightedItem.weight;
            if (weightRoll < currentWeight)
            {
                return weightedItem.item;
            }
        }

        return pool.LastOrDefault().item;
    }

    public void ModifyLuck(float deltaRate)
    {
        essentialItemRate -= deltaRate;
        Debug.Log($"[StoreManager] Special item refreshing rate is {1 - essentialItemRate} now.");
    }

}
