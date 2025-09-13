using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Default Volume Settings")]
    [SerializeField] private float bgmFadeDuration = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float defaultBgmVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float defaultSfxVolume = 1.0f;

    [Header("SFX Pool Settings")]
    [SerializeField] private GameObject sfxSourcePrefab;

    private AudioSource bgmSource;
    private bool isFadingBgm = false;
    private float bgmVolume;

    private Transform sfxSourceParent;
    private float sfxVolume;

    protected override void Awake()
    {
        base.Awake();

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        SetBGMVolume(defaultBgmVolume);

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
        var clip = await AddressableManager.Instance.LoadAssetAsync<AudioClip>(key);
        if (clip == null)
        {
            Debug.LogError($"[AudioManager] SFX clip not found for key: {key}");
            return;
        }

        GameObject sfxPlayer = ObjectPoolManager.Instance.SpawnFromPool(sfxSourcePrefab, sfxSourceParent.position, Quaternion.identity);
        sfxPlayer.transform.SetParent(sfxSourceParent);

        AudioSource source = sfxPlayer.GetComponent<AudioSource>();
        source.clip = clip;
        source.volume = volumeOverride == 0? sfxVolume : volumeOverride;
        source.Play();

        StartCoroutine(ReturnSFXToPool(sfxPlayer, clip.length));
    }

    private IEnumerator ReturnSFXToPool(GameObject objectToReturn, float delay)
    {
        yield return new WaitForSeconds(delay);

        AudioSource source = objectToReturn.GetComponent<AudioSource>();
        if (source != null)
        {
            source.clip = null;
        }

        ObjectPoolManager.Instance.ReturnToPool(sfxSourcePrefab, objectToReturn);
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
            await FadeAsync(0f);
            bgmSource.Stop();
        }

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();

        await FadeAsync(bgmVolume);
        isFadingBgm = false;
    }

    public async void StopBGM()
    {
        if (isFadingBgm || !bgmSource.isPlaying) return;

        isFadingBgm = true;
        await FadeAsync(0f);
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
