using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private Text specialItemRateText;
    [SerializeField] private Button nextSceneBtn;

    private List<StoreSlotUI> uiSlots;

    private void Awake()
    {
        uiSlots = canvas.GetComponentsInChildren<StoreSlotUI>().ToList();
        if (uiSlots.Count != StoreManager.Instance.storeSO.numberOfSlots)
        {
            Debug.LogWarning($"[StoreManager] The number of UI Slots is different from SO.numberOfSlots: '{uiSlots.Count}' != '{StoreManager.Instance.storeSO.numberOfSlots}'");
        }
        DeactivateStoreUI();

        if (nextSceneBtn != null)
            nextSceneBtn.AddButtonListener(() => StoreManager.Instance.CloseAndContinueToNextScene());
    }

    // 激活商店UI
    public void ActivateStoreUI()
    {
        canvas.SetActive(true);
        
        RefreshStore();
        SetSpecialItemRateText(StoreManager.Instance.essentialItemRate);
    }

    // 隐藏商店UI
    public void DeactivateStoreUI()
    {
        canvas.SetActive(false);
    }

    // 刷新商店并更新UI
    private void RefreshStore()
    {
        List<ItemSO> itemsToDisplay = StoreManager.Instance.GenerateStoreItems();
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < itemsToDisplay.Count)
            {
                uiSlots[i].DisplayItem(itemsToDisplay[i]);
                Debug.Log($"Slot {i + 1}: Refreshed item: {itemsToDisplay[i].name}");
            }
            else
            {
                uiSlots[i].ClearSlot();
                Debug.Log($"Slot {i + 1}: Cleared item");
            }
        }
    }

    private void SetSpecialItemRateText(float rate)
    {
        specialItemRateText.text = $"special item refreshing rate: {(1 - rate) * 100}%";
    }
}
