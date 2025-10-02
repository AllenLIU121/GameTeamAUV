using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New SFX Data", menuName = "Audio/SFX Data")]
public class SfxSO : ScriptableObject
{
    [Header("AudioClip设置")]
    public AssetReferenceT<AudioClip> clipReference;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Header("AudioClip规定")]
    public SFXCategory category;
    // public int priority = 128;  // 0-256 值越小优先级越高
    public bool isExclusive = false;  // 是否是唯一性音效
    public bool interruptSameCategory = false;  // 是否打断同类别正在播放的低优先级音效
}

public enum SFXCategory
{
    Default,
    UI,
    Ambient
}