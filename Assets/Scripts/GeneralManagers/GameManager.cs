using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
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

        if (sceneName == "MenuScene")
            ChangeGameState(GameState.MainMenu);
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
            case GameState.Playing:
                HandlePlayingState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
        }

        EventManager.Instance.Publish(new OnGameStateChanged { newState = newState });
    }

    private void HandleMainMenuState()
    {
        Debug.Log("Entering Main Menu State");
        Time.timeScale = 1.0f;
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

}
