using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button exitBtn;

    private void Awake()
    {
        AudioManager.Instance.PlaySFX("Assets/Audio/BGM/1.菜单界面0919-1.mp3");
        newGameBtn.AddButtonListener(StartNewGame, 1f);
        continueBtn.AddButtonListener(ContinueGame, 1f);
        exitBtn.AddButtonListener(ExitGame);
    }

    private void StartNewGame()
    {
        GameManager.Instance.NewGame();
    }

    private void ContinueGame()
    {
        _ = GameManager.Instance.ContinueGame();
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}
