using System.Collections;
using UnityEngine;

public class GameInit : MonoBehaviour
{
    [SerializeField] private GameObject managersPrefab;

    private void Awake()
    {
        GameObject managers = Instantiate(managersPrefab);
        DontDestroyOnLoad(managers);
    }

    private void Start()
    {
        SceneController.Instance.LoadSceneAsync(GameConstants.SceneName.MenuScene);
    }
}
