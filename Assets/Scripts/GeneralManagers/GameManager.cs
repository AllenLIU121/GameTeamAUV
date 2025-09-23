using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Initializing,
    Playing,
    Paused
}

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; }

    private void Start()
    {
        CurrentState = GameState.MainMenu;
        ChangeGameState(CurrentState);
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe<OnSceneLoaded>(HandleSceneLoaded);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe<OnSceneLoaded>(HandleSceneLoaded);
    }

    private void HandleSceneLoaded(OnSceneLoaded _)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == GameConstants.SceneName.MenuScene)
            ChangeGameState(GameState.Initializing);
        else
            ChangeGameState(GameState.Playing);
    }

    public void ChangeGameState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenuState();
                break;
            case GameState.Initializing:
                HandleInitializingState();
                break;
            case GameState.Playing:
                HandlePlayingState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
        }

        EventManager.Instance.Publish(new OnGameStateChanged { newState = newState });
    }

    // --------------- 新游戏/继续游戏 方法接口 ---------------

    // 新游戏
    public void NewGame()
    {
        // GameStateManager.Instance.NewGame();
        SceneController.Instance.LoadSceneAsync(GameConstants.SceneName.ChapterOneScene);
    }

    // 继续游戏
    public async Task ContinueGame()
    {
        bool loadSuccess = await GameStateManager.Instance.LoadGameAsync();

        if (loadSuccess)
        {
            string lastScene = GameStateManager.Instance.currentData.lastSceneName;
            if (!string.IsNullOrEmpty(lastScene))
            {
                SceneController.Instance.LoadSceneAsync(lastScene);
            }
            else
            {
                Debug.LogError("Save data has no scene name");
            }
        }
        else
        {
            Debug.Log("No save file found, starting a new game.");
            NewGame();
        }
    }

    private void HandleMainMenuState()
    {
        Debug.Log("Entering Main Menu State");
        Time.timeScale = 1.0f;
    }

    // 1s后进入Playing状态
    private void HandleInitializingState()
    {
        Debug.Log("Entering Initializing State");
        StartCoroutine(StartPlaying());
    }

    private void HandlePlayingState()
    {
        Debug.Log("Entering Playing State");
        Time.timeScale = 1.0f;
    }

    private void HandlePausedState()
    {
        Debug.Log("Entering Paused State");
        Time.timeScale = 0.0f;
    }

    private IEnumerator StartPlaying()
    {
        yield return new WaitForSeconds(5f);
        ChangeGameState(GameState.Playing);
    }
}
