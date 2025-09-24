using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyContoller : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe<OnSceneLoaded>(CheckCurrentScene);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe<OnSceneLoaded>(CheckCurrentScene);
    }

    private void CheckCurrentScene(OnSceneLoaded _)
    {
        if (SceneManager.GetActiveScene().name == GameConstants.SceneName.MenuScene)
            Destroy(gameObject);
    }
}
