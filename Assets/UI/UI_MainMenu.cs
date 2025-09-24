using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button exitBtn;

    private void Awake()
    {
        newGameBtn.AddButtonListener(StartNewGame, 1f);
        // continueBtn.AddButtonListener(ContinueGame, 1f);
        exitBtn.AddButtonListener(ExitGame);
    }

    private void StartNewGame()
    {
        GameManager.Instance.NewGame();
    }

    // private void ContinueGame()
    // {
    //     _ = GameManager.Instance.ContinueGame();
    // }

    private void ExitGame()
    {
        Application.Quit();
    }
}
