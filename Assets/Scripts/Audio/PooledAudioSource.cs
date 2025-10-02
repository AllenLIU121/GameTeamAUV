using System.Collections;
using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PooledAudioSource : MonoBehaviour
{
    private AudioSource audioSource;
    private SfxSO currentSfxData;
    private Coroutine returnCoroutine;
    private Action<PooledAudioSource> onPlaybackComplete;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PlaySFX(SfxSO sfxData, AudioClip clip, Action<PooledAudioSource> onComplete)
    {
        currentSfxData = sfxData;
        onComplete = onPlaybackComplete;

        audioSource.clip = clip;
        audioSource.volume = sfxData.volume;
        audioSource.pitch = sfxData.pitch;

        audioSource.Play();

        if (returnCoroutine != null)
            StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(ReturnToPool(clip.length / sfxData.pitch));
    }

    public SfxSO GetCurrentSfxData() => currentSfxData;

    public void StopPlaybackAndReturn()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        audioSource.Stop();
        onPlaybackComplete?.Invoke(this);
    }

    private IEnumerator ReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);

        onPlaybackComplete?.Invoke(this);
    }
}
