using UnityEngine;

#if UNITY_EDITOR
public class DebugController : Singleton<DebugController>
{
    [Header("场景切换测试")]
    public string sceneToLoad;
    public string assetLabel;

    [Header("游戏状态切换测试")]
    public GameState gameState;

    [Header("播放音效测试")]
    public string sfxKey;

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

    [ContextMenu("执行游戏状态切换")]
    public void TriggerGameStateChange()
    {
        if (GameManager.Instance != null && EventManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(gameState);
        }
        else
        {
            Debug.LogError("GameManager or EventManager not found! Please make sure all managers are initialized correctly.");
        }
    }

    [ContextMenu("执行播放音效")]
    public void TriggerPlaySFX()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(sfxKey);
        }
        else
        {
            Debug.LogError("AudioManager not found! Please make sure all managers are initialized correctly.");
        }
    }
}
#endif