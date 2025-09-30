using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyContoller : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private bool isUI = false;

    private void Awake()
    {
        if (gameObject.CompareTag("UI"))
        {
            isUI = true;
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError($"[DestroyController] CanvasGroup not found");
            }
        }

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        if (isUI)
            ShowCanvas();
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe<OnSceneLoaded>(CheckCurrentScene);
        EventManager.Instance.Subscribe<OnGameFinished>(DestroyGameObjects);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe<OnSceneLoaded>(CheckCurrentScene);
        EventManager.Instance.Unsubscribe<OnGameFinished>(DestroyGameObjects);
    }

    private void CheckCurrentScene(OnSceneLoaded _)
    {
        if (SceneManager.GetActiveScene().name == GameConstants.SceneName.FinalScene && isUI)
            HideCanvas();
    }

    private void DestroyGameObjects(OnGameFinished _) => Destroy(gameObject);

    private void HideCanvas()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ShowCanvas()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
}
