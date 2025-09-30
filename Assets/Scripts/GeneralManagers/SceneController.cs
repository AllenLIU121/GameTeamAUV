using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : Singleton<SceneController>
{
    private float fadeDuration = 1f;
    private CanvasGroup canvasGroup;
    private Slider progressBar;
    private bool isLoading = false;
    private string currentAssetLabel;

    protected override void Awake()
    {
        base.Awake();

        canvasGroup = GetComponentInChildren<CanvasGroup>();
        progressBar = GetComponentInChildren<Slider>();

        canvasGroup.alpha = 1f;
        progressBar.value = 0f;
        canvasGroup.gameObject.SetActive(false);
    }

    public async void SceneRollbackAsync()
    {
        if (isLoading) return;

        isLoading = true;
        canvasGroup.gameObject.SetActive(true);

        await Fade(1f);
        await Task.Yield();
        await Fade(0f);

        isLoading = false;
        canvasGroup.gameObject.SetActive(false);
    }

    // 异步加载资源和场景
    public async void LoadSceneAsync(string sceneName, string assetLabel = null, bool showLoadingUI = false, Action onComplete = null)
    {
        if (isLoading) return;

        isLoading = true;
        canvasGroup.gameObject.SetActive(true);

        await Fade(1f);

        // 释放当前场景资源, 加载新场景资源
        await ReleaseAndLoadAssets(assetLabel);

        // 停止当前场景音乐
        StopAllAudio();

        // 加载新场景
        await LoadingProgressAsync(sceneName, showLoadingUI);

        onComplete?.Invoke();

        // 发布场景加载完毕事件
        EventManager.Instance.Publish(new OnSceneLoaded());

        await Fade(0f);

        isLoading = false;
        canvasGroup.gameObject.SetActive(false);
    }

    private async Task ReleaseAndLoadAssets(string assetLabel)
    {
        if (!string.IsNullOrEmpty(currentAssetLabel))
        {
            AddressableManager.Instance.ReleaseAssetsByLabel(currentAssetLabel);
            Debug.Log($"[AddressableManager] {currentAssetLabel} assets have been released!");
        }

        if (!string.IsNullOrEmpty(assetLabel))
        {
            currentAssetLabel = assetLabel;
            await AddressableManager.Instance.LoadAssetsByLabelAsync(assetLabel);
            Debug.Log($"[AddressableManager] {assetLabel} assets have been loaded!");
        }
    }

    private async Task LoadingProgressAsync(string sceneName, bool showLoadingUI)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        progressBar.gameObject.SetActive(showLoadingUI);

        while (!operation.isDone)
        {
            if (showLoadingUI)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                if (progressBar != null)
                    progressBar.value = progress;
            }
            await Task.Yield();
        }
    }

    private async Task Fade(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            await Task.Yield();
        }
        canvasGroup.alpha = targetAlpha;
    }

    private void StopAllAudio()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.StopAllSFX();
        }
    }
}
