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

    // 注册/注销服务接口
    public void RegisterInventoryManager(InventoryManager manager) => Inventory = manager;
    public void RegisterCharacterManager(CharacterManager manager) => Character = manager;
    public void UnregisterInventoryManager() => Inventory = null;
    public void UnregisterCharacterManager() => Character = null;

    // 角色数据更新请求列表
    private Dictionary<string, List<float>> staminaModifiers = new Dictionary<string, List<float>>();
    private Dictionary<string, List<float>> hungerModifiers = new Dictionary<string, List<float>>();

    protected override void Awake()
    {
        base.Awake();
        currentData = new GameData();
    }

    // 帧末更新本帧变化的数据
    private void LateUpdate()
    {
        ProcessModifiers();
    }

    private void ProcessModifiers()
    {
        if (staminaModifiers.Count == 0 && hungerModifiers.Count == 0) return;
        
        // 记录属性变化的角色
        HashSet<string> modifiedCharacters = new HashSet<string>();
        foreach (var key in staminaModifiers.Keys) modifiedCharacters.Add(key);
        foreach (var key in hungerModifiers.Keys) modifiedCharacters.Add(key);

        foreach (string characterID in modifiedCharacters)
        {
            if (!currentData.characters.ContainsKey(characterID)) continue;

            var data = currentData.characters[characterID];

            // 结算体力值
            if (staminaModifiers.TryGetValue(characterID, out var staminaChanges))
            {
                float totalChange = 0;
                foreach (var modifier in staminaChanges) totalChange += modifier;

                if (totalChange != 0)
                {
                    float oldValue = data.currentStamina;
                    data.currentStamina = Mathf.Clamp(oldValue + totalChange, 0, data.maxStamina);

                    EventManager.Instance.Publish(new OnCharacterStatChanged
                    {
                        characterID = characterID,
                        statType = StatType.Stamina,
                        newValue = data.currentStamina,
                        changeAmount = data.currentStamina - oldValue
                    });
                    Debug.Log($"Character {characterID}'s stamina has changed, newValue: {data.currentStamina}, changeAmount: {data.currentStamina - oldValue}");
                }
            }

            // 结算饥饿值
            if (hungerModifiers.TryGetValue(characterID, out var hungerChanges))
            {
                float totalChange = 0;
                foreach (var modifier in hungerChanges) totalChange += modifier;

                if (totalChange != 0)
                {
                    float oldValue = data.currentHunger;
                    data.currentHunger = Mathf.Clamp(oldValue + totalChange, 0, data.maxHunger);

                    EventManager.Instance.Publish(new OnCharacterStatChanged
                    {
                        characterID = characterID,
                        statType = StatType.Hunger,
                        newValue = data.currentHunger,
                        changeAmount = data.currentHunger - oldValue
                    });
                    Debug.Log($"Character {characterID}'s hunger has changed, newValue: {data.currentHunger}, changeAmount: {data.currentHunger - oldValue}");
                }
            }
        }

        ClearAllModifiers();
    }

    private void ClearAllModifiers()
    {
        staminaModifiers.Clear();
        hungerModifiers.Clear();
    }

    // --------------- 新游戏接口 ---------------
    public void NewGame()
    {
        currentData = new GameData();
        historyStack.Clear();
        ClearAllModifiers();
    }

    // --------------- 初始化/获取 角色数据 ---------------

        // 初始化角色
    public void RegisterNewCharacter(CharacterRuntimeData newCharacterData)
    {
        if (currentData.characters.ContainsKey(newCharacterData.characterID)) return;
        currentData.characters[newCharacterData.characterID] = newCharacterData;
    }

    // 获取单个角色运行时数据
    public CharacterRuntimeData GetCharacterData(string characterID)
    {
        currentData.characters.TryGetValue(characterID, out CharacterRuntimeData characterData);
        return characterData;
    }


    // --------------- 更新角色某一属性数据(在帧末统一结算) ---------------

    // 体力值更新
    public void UpdateStamina(string characterID, float amount)
    {
        if (!staminaModifiers.ContainsKey(characterID))
        {
            staminaModifiers[characterID] = new List<float>();
        }
        staminaModifiers[characterID].Add(amount);
    }

    // 饥饿值更新
    public void UpdateHunger(string characterID, float amount)
    {
        if (!hungerModifiers.ContainsKey(characterID))
        {
            hungerModifiers[characterID] = new List<float>();
        }
        hungerModifiers[characterID].Add(amount);
    }


    // --------------- 游戏 存/读/回档 ---------------

    // 游戏内数据快照
    public void GenerateSnapshot()
    {
        string json = JsonUtility.ToJson(currentData, true);
        GameData snapshot = JsonUtility.FromJson<GameData>(json);
        historyStack.Push(snapshot);
        Debug.Log($"<color=green>[GameStateManager] Snapshot created. History depth: {historyStack.Count}</color>");
    }

    // 游戏内数据回滚
    public void SnapshotRollback()
    {
        if (historyStack.Count > 0)
        {
            currentData = historyStack.Peek();

            ClearAllModifiers();

            PublishCharacterDataForSync();

            EventManager.Instance.Publish(new OnGameDataLoaded());
        }
    }

    public void PublishCharacterDataForSync()
    {
        foreach (var character in currentData.characters.Values)
        {
            // 体力值更新
            EventManager.Instance.Publish(new OnCharacterStatChanged
            {
                characterID = character.characterID,
                statType = StatType.Stamina,
                newValue = character.currentStamina,
                changeAmount = 0
            });

            // 饥饿值更新
            EventManager.Instance.Publish(new OnCharacterStatChanged
            {
                characterID = character.characterID,
                statType = StatType.Hunger,
                newValue = character.currentHunger,
                changeAmount = 0
            });
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
        Debug.Log("[GameStateManager] Game saved successfully.");
    }

    // 从玩家本地(Windows)读取数据, 在玩家进入菜单时并且点击继续游戏时调用
    public async Task<bool> LoadGameAsync()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        if (!File.Exists(path))
        {
            Debug.LogWarning("[GameStateManager] No save file found.");
            return false;
        }

        string json = await Task.Run(() => File.ReadAllText(path));
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("[GameStateManager] Save file is empty or corrupted.");
            return false;
        }

        currentData = JsonUtility.FromJson<GameData>(json);

        PublishCharacterDataForSync();
        EventManager.Instance.Publish(new OnGameDataLoaded());

        Debug.Log("[GameStateManager] Game loaded successfully.");
        return true;
    }
#endif
}