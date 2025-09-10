public struct OnGameStateChanged   // 游戏状态改变时发布
{
    public GameState newState;
}

public struct OnSceneLoaded { }    // 场景加载完毕时发布

public struct OnGameDataLoaded { }    // 游戏回滚时发布