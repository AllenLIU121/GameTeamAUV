using UnityEngine;

#if UNITY_EDITOR
public class DebugController : Singleton<DebugController>
{
    [Header("场景切换测试")]
    public string sceneToLoad;
    public string assetLabel;

    [Header("向物品栏中加物品")]
    public string itemID;
    public int quantity;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    [ContextMenu("执行场景切换")]
    public void TriggerSceneLoad()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning($"Scene with name '{sceneToLoad}' not found!", this.gameObject);
            return;
        }

        Debug.Log($"--- [DebugController] Start loading scene: '{sceneToLoad}' | Asset label: '{assetLabel}' ---");

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadSceneAsync(sceneToLoad, assetLabel);
        }
        else
        {
            Debug.LogError("SceneController not found! Please make sure all managers are initialized correctly.");
        }
    }

    [ContextMenu("加特定物品")]
    public void AddItemToInventory()
    {
        GameStateManager.Instance.Inventory.AddItem(itemID, quantity);
    }
}
#endif