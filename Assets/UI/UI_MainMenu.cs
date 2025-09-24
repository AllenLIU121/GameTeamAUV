using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKeyDown)
            GameManager.Instance.NewGame();
    }
}
