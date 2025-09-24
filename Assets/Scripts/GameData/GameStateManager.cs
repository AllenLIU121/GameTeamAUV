using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : Singleton<GameStateManager>
{
    // 游戏数据
    public GameData currentData { get; private set; }
    private Stack<GameData> historyStack = new Stack<GameData>();
    private const string SAVE_FILE_NAME = "savegame.json";

    // 注册的服务属性
    public InventoryManager Inventory { get; private set; }
    public CharacterManager Character { get; private set; }
    public SkillManager Skill { get; private set; }
    public BuffManager Buff { get; private set; }

    // 注册/注销服务接口
    public void RegisterInventoryManager(InventoryManager manager) => Inventory = manager;
    public void RegisterCharacterManager(CharacterManager manager) => Character = manager;
    public void RegisterSkillManager(SkillManager manager) => Skill = manager;
    public void UnregisterSkillManager() => Skill = null;
    public void RegisterBuffManager(BuffManager manager) => Buff = manager;
    public void UnregisterBuffManager() => Buff = null;
    public void UnregisterInventoryManager() => Inventory = null;
    public void UnregisterCharacterManager() => Character = null;

    protected override void Awake()
    {
        base.Awake();
        currentData = new GameData();

        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe<OnGameRollback>(SnapshotRollback);
        }
        else
        {
            Debug.LogError("[GameStateManager] EventManager is not initialized.");
        }
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<OnGameRollback>(SnapshotRollback);
        }
    }

    // --------------- 游戏 存/读/回档 ---------------

    // 游戏内数据快照
    public void GenerateSnapshot()
    {
        SyncDataFromModules();

        string json = JsonUtility.ToJson(currentData);
        GameData snapshot = JsonUtility.FromJson<GameData>(json);

        historyStack.Push(snapshot);
        Debug.Log($"<color=green>[GameStateManager] Snapshot created. History depth: {historyStack.Count}</color>");
    }

    // 游戏内数据回滚
    public void SnapshotRollback(OnGameRollback _)
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.SceneRollbackAsync();
        }
        else
        {
            Debug.LogWarning("[GameStateManager] SceneController instance not found during rollback.");
        }

        if (historyStack.Count > 0)
        {
            currentData = historyStack.Peek();

            SyncDataFromSnapShot();

            // ClearAllModifiers();

            // PublishCharacterDataForSync();

            Debug.Log($"<color=green>[GameStateManager] Snapshot rollbacked. History depth: {historyStack.Count}</color>");
        }
        else
        {
            Debug.LogWarning("[GameStateManager] No snapshot to rollback.");
        }
    }

    // #if UNITY_EDITOR || UNITY_STANDALONE_WIN
    //     // 保存游戏数据到本地(Windows), 在玩家退出游戏到菜单时调用
    //     public async Task SaveGameAsync()
    //     {
    //         currentData.lastSceneName = SceneManager.GetActiveScene().name;

    //         SyncDataFromModules();

    //         string json = JsonUtility.ToJson(currentData, true);
    //         string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    //         await Task.Run(() => File.WriteAllText(path, json));
    //         Debug.Log("[GameStateManager] Game saved successfully.");
    //     }

    //     // 从玩家本地(Windows)读取数据, 在玩家进入菜单时并且点击继续游戏时调用
    //     public async Task<bool> LoadGameAsync()
    //     {
    //         string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    //         if (!File.Exists(path))
    //         {
    //             Debug.LogWarning("[GameStateManager] No save file found.");
    //             return false;
    //         }

    //         string json = await Task.Run(() => File.ReadAllText(path));
    //         if (string.IsNullOrEmpty(json))
    //         {
    //             Debug.LogError("[GameStateManager] Save file is empty or corrupted.");
    //             return false;
    //         }

    //         currentData = JsonUtility.FromJson<GameData>(json);
    //         SyncDataFromSnapShot();

    //         // PublishCharacterDataForSync();
    //         EventManager.Instance.Publish(new OnGameDataLoaded());
    //         Debug.Log("[GameStateManager] Game loaded successfully.");
    //         return true;
    //     }

    // #endif

    // 从各个模块中拉取最新数据,更新到CurrentData
    private void SyncDataFromModules()
    {
        // 角色模块数据
        if (Character != null)
        {
            currentData.characters.Clear();
            var allCharacterStatus = Character.GetAllCharacterStatus();
            foreach (var characterStatus in allCharacterStatus)
            {
                if (characterStatus != null)
                {
                    currentData.characters[characterStatus.CharacterID] = characterStatus.GetStateForSaving();
                }
            }
        }

        // 物品栏模块数据
        // 暂时不需要 因为InventoryManager直接操作的此数据currentData

        // 技能模块数据
        // 暂时不需要 当前没有保留技能冷却数据

        // 第二章地图数据
        if (SceneManager.GetActiveScene().name == GameConstants.SceneName.ChapterTwoScene)
        {
            currentData.mapNodes = MapController.Instance.GetMapDataToSave();
        }
    }

    // 将currentData中的数据, 推送到各个模块中
    private void SyncDataFromSnapShot()
    {
        // 角色模块数据
        if (Character != null && currentData.characters != null)
        {
            foreach (var loadedData in currentData.characters.Values)
            {
                var characterStatus = Character.GetCharacterStatus(loadedData.characterID);
                if (characterStatus != null)
                {
                    characterStatus.LoadState(loadedData);
                }
            }
        }

        //物品栏模块数据
        if (Inventory != null)
        {
            Inventory.SyncFromGameData();
        }

        // 技能模块数据 (暂时是直接重置冷却时间)
        if (Skill != null)
        {
            Skill.ResetAllCooldowns();
        }

        // 第二章地图数据
        Debug.Log($"[GameStateManager] MapController exists? {MapController.Instance != null}; MapNodes count: {currentData.mapNodes.Count}");
        if (MapController.Instance != null && currentData.mapNodes.Count > 0)
        {
            MapController.Instance.RestoreMapData(currentData.mapNodes);
        }
    }

}