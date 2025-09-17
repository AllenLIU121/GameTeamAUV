using UnityEngine;

#if UNITY_EDITOR

// 只在Unity编辑器中 开发测试时运行
public static class SceneAutoLoader
{
    private const string MENU_NAME = "Tools/Scene AutoLoader/";
    private static bool IsEnabled
    {
        get => UnityEditor.EditorPrefs.GetBool(MENU_NAME + "Enabled", true);
        set => UnityEditor.EditorPrefs.SetBool(MENU_NAME + "Enabled", value);
    }

    [UnityEditor.MenuItem(MENU_NAME + "Enable")]
    private static void Enable() => IsEnabled = true;

    [UnityEditor.MenuItem(MENU_NAME + "Disable")]
    private static void Disable() => IsEnabled = false;

    [UnityEditor.MenuItem(MENU_NAME + "Enable", true)]
    private static bool EnableValidate() => !IsEnabled;

    [UnityEditor.MenuItem(MENU_NAME + "Disable", true)]
    private static bool DisableValidate() => IsEnabled;

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    // private static void Initialize()
    // {
    //     if (!IsEnabled) return;

    //     if (Object.FindObjectOfType<GameManager>() != null) return; // 正常Boot场景启动

    //     var managersPrefab = Resources.Load<GameObject>("ManagersPrefab/Managers");
    //     if (managersPrefab != null)
    //     {
    //         GameObject manageres = Object.Instantiate(managersPrefab);
    //         MonoBehaviour.DontDestroyOnLoad(manageres);
    //     }
    //     else
    //     {
    //         Debug.LogError("[SceneAutoLoader] Could not find 'Managers' prefab in Resources folder");
    //     }
    // }
}
#endif