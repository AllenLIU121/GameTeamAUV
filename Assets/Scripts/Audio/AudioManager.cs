using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Default Volume Settings")]
    [SerializeField] private float bgmFadeDuration = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float defaultBgmVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float defaultSfxVolume = 1.0f;

    [Header("SFX Pool Settings")]
    [SerializeField] private GameObject sfxSourcePrefab;

    // BGM
    private AudioSource bgmSource;
    private bool isFadingBgm = false;
    private float bgmVolume;

    // SFX
    private Transform sfxSourceParent;
    private float sfxVolume;
    private Dictionary<SFXCategory, List<PooledAudioSource>> activeSourcesByCategory = new Dictionary<SFXCategory, List<PooledAudioSource>>();
    private HashSet<SfxSO> activeExclusiveSfx = new HashSet<SfxSO>();
    
    protected override void Awake()
    {
        base.Awake();

        // BGM初始化
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        SetBGMVolume(defaultBgmVolume);

        // 设置SFX父物体
        sfxSourceParent = new GameObject("SFXSources").transform;
        sfxSourceParent.SetParent(transform);
        SetSFXVolume(defaultSfxVolume);
    }

    #region SFX
    private void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public async void PlaySFX(string key, float volumeOverride = 0f)
    {
        // 异步加载SfxSO
        var sfxSO = await AddressableManager.Instance.LoadAssetAsync<SfxSO>(key);;
        if (sfxSO == null)
        {
            Debug.LogError($"[AudioManager] SfxSO not found for key: {key}");
            return;
        }

        // 执行播放规则检查
        if (sfxSO.isExclusive && activeExclusiveSfx.Contains(sfxSO))
        {
            Debug.Log($"[AudioManager] Exclusive SFX '{sfxSO.name} is already playing. Request Ignored.");
            return;
        }
        if (sfxSO.interruptSameCategory)
        {
            StopSourcesInCategory(sfxSO.category);
        }

        // 异步加载AudioClip
        if (!sfxSO.clipReference.RuntimeKeyIsValid())
        {
            Debug.LogError($"[AudioManager] AudioClip reference is not valid for SfxData: {sfxSO.name}");
            return;
        }
        AudioClip clip = await AddressableManager.Instance.LoadAssetAsync<AudioClip>(sfxSO.clipReference.RuntimeKey.ToString());
        if (clip == null)
        {
            Debug.LogError($"[AudioManager] AudioClip not loaded for SfxData: {sfxSO.name}");
            return;
        }

        // 从对象池获取音源并播放
        GameObject sfxPlayer = ObjectPoolManager.Instance.SpawnFromPool(sfxSourcePrefab, sfxSourceParent.position, Quaternion.identity);
        sfxPlayer.transform.SetParent(sfxSourceParent);

        PooledAudioSource pooledSource = sfxPlayer.GetComponent<PooledAudioSource>();
        if (pooledSource == null)
        {
            Debug.LogError($"[AudioManager] sfxSourcePrefab is missing the PooledAudioSource component!");
            ObjectPoolManager.Instance.ReturnToPool(sfxSourcePrefab, sfxPlayer);
            return;
        }

        // 更新状态跟踪
        TrackSfxStart(sfxSO, pooledSource);

        // 播放音效, 并传入完成时的回调
        pooledSource.PlaySFX(sfxSO, clip, OnSfxPlaybackComplete);
    }

    public void StopAllSFX()
    {
        foreach(var categoryList in activeSourcesByCategory.Values)
        {
            for (int i = categoryList.Count - 1; i >= 0; i--)
            {
                var source = categoryList[i];
                source.StopAllCoroutines(); // 停止自身的返还协程
                ObjectPoolManager.Instance.ReturnToPool(sfxSourcePrefab, source.gameObject);
            }
        }

        activeSourcesByCategory.Clear();
        activeExclusiveSfx.Clear();
    }

    private void StopSourcesInCategory(SFXCategory category)
    {
        if (!activeSourcesByCategory.TryGetValue(category, out List<PooledAudioSource> categorySources)) return;

        for (int i = categorySources.Count - 1; i >= 0; i--)
        {
            PooledAudioSource source = categorySources[i];
            SfxSO oldSfxSO = source.GetCurrentSfxData();

            // 当新音效的优先级更高时, 立即停止旧音效
            if (oldSfxSO != null)
            {
                source.StopPlaybackAndReturn();
            }
        }
    }

    private void OnSfxPlaybackComplete(PooledAudioSource pooledSource)
    {
        TrackSfxEnd(pooledSource);

        ObjectPoolManager.Instance.ReturnToPool(sfxSourcePrefab, pooledSource.gameObject);
    }

    private void TrackSfxStart(SfxSO sfxSO, PooledAudioSource source)
    {
        if (sfxSO.isExclusive)
        {
            activeExclusiveSfx.Add(sfxSO);
        }

        if (!activeSourcesByCategory.ContainsKey(sfxSO.category))
        {
            activeSourcesByCategory[sfxSO.category] = new List<PooledAudioSource>();
        }
        activeSourcesByCategory[sfxSO.category].Add(source);
    }

    private void TrackSfxEnd(PooledAudioSource source)
    {
        SfxSO sfxSO = source.GetCurrentSfxData();
        if (sfxSO == null) return;

        if (sfxSO.isExclusive)
        {
            activeExclusiveSfx.Remove(sfxSO);
        }

        if (activeSourcesByCategory.ContainsKey(sfxSO.category))
        {
            activeSourcesByCategory[sfxSO.category].Remove(source);
        }
    }

    #endregion

    #region BGM
    private void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (!isFadingBgm)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    public async void PlayBGM(string key)
    {
        if (isFadingBgm) return;

        var clip = await AddressableManager.Instance.LoadAssetAsync<AudioClip>(key);
        if (clip == null)
        {
            Debug.LogError($"[AudioManager] BGM clip not found for key: {key}");
            return;
        }

        if (bgmSource.clip == clip) return;

        isFadingBgm = true;
        if (bgmSource.isPlaying)
        {
            // await FadeAsync(0f);
            bgmSource.Stop();
        }

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();

        // await FadeAsync(bgmVolume);
        isFadingBgm = false;
    }

    public void StopBGM()
    {
        if (isFadingBgm || !bgmSource.isPlaying)
        {
            Debug.LogWarning($"[AudioManager] isFadingBgm: {isFadingBgm}; BGM is playing: {bgmSource.isPlaying}");
            return;
        }

        isFadingBgm = true;
        // await FadeAsync(0f);
        bgmSource.Stop();
        bgmSource.clip = null;
        isFadingBgm = false;
    }

    private async Task FadeAsync(float targetVolume)
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;
        while (timer < bgmFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / bgmFadeDuration);
            await Task.Yield();
        }

        bgmSource.volume = targetVolume;
    }
    #endregion
}
