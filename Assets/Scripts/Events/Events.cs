public struct OnGameStateChanged   // 游戏状态改变时发布
{
    public GameState newState;
}

public struct OnSceneLoaded { }    // 场景加载完毕时发布

public struct OnGameDataLoaded { }    // 游戏回滚时发布

public struct OnCharacterStatChanged     // 角色属性改变时发布
{
    public string characterID;
    public StatType statType;    // 角色具体变化的属性
    public float newValue;
    public float changeAmount;
}
