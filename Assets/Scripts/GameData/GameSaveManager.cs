using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveManager : Singleton<GameSaveManager>
{
    private GameData currentData;
    private Stack<GameData> historyStack = new Stack<GameData>();

    // 角色状态
    private List<float> hungerModifiers = new List<float>();
    private List<float> energyModifiers = new List<float>();

    private const string SAVE_FILE_NAME = "savegame.json";

    protected override void Awake()
    {
        base.Awake();
        currentData = new GameData();
    }

    // 更新Runtime数据
    private void LateUpdate()
    {
        bool hasChanges = hungerModifiers.Count > 0 || energyModifiers.Count > 0;

        if (!hasChanges) return;

        // TODO: Update GameData
    }

    // 游戏内数据回滚
    public void SnapshotRollback()
    {
        if (historyStack.Count > 0)
        {
            currentData = historyStack.Pop();

            hungerModifiers.Clear();
            energyModifiers.Clear();

            // TODO: Update GameData

            EventManager.Instance.Publish(new OnGameDataLoaded());
        }
    }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    // 保存游戏数据到本地(Windows), 在玩家退出游戏到菜单时调用
    public async Task SaveGameAsync()
    {
        currentData.lastSceneName = SceneManager.GetActiveScene().name;

        string json = JsonUtility.ToJson(currentData, true);
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        await Task.Run(() => File.WriteAllText(path, json));
        Debug.Log("[GameSaveManager] Game saved successfully.");
    }

    // 从玩家本地(Windows)读取数据, 在玩家进入菜单时并且点击继续游戏时调用
    public async Task<bool> LoadGameAsync()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        if (!File.Exists(path))
        {
            Debug.LogWarning("[GameSaveManager] No save file found.");
            return false;
        }

        string json = await Task.Run(() => File.ReadAllText(path));
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("[GameSaveManager] Save file is empty or corrupted.");
            return false;
        }

        currentData = JsonUtility.FromJson<GameData>(json);
        EventManager.Instance.Publish(new OnGameDataLoaded());

        Debug.Log("[GameSaveManager] Game loaded successfully.");
        return true;
    }
#endif
}