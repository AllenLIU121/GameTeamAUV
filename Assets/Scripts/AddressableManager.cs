using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressableManager : Singleton<AddressableManager>
{
    private Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();
    private Dictionary<GameObject, AsyncOperationHandle> _instanceHandles = new Dictionary<GameObject, AsyncOperationHandle>();
    private Dictionary<string, AsyncOperationHandle<IList<IResourceLocation>>> _labelLocationHandles = new Dictionary<string, AsyncOperationHandle<IList<IResourceLocation>>>();
    private Dictionary<string, List<AsyncOperationHandle>> _labelAssetHandles = new Dictionary<string, List<AsyncOperationHandle>>();

    // 异步加载单个资源
    public async Task<T> LoadAssetAsync<T>(string key)
    {
        if (_assetHandles.TryGetValue(key, out var handle))
        {
            return (T)handle.Result;
        }

        AsyncOperationHandle<T> newHandle = Addressables.LoadAssetAsync<T>(key);
        await newHandle.Task;

        if (newHandle.Status == AsyncOperationStatus.Succeeded)
        {
            _assetHandles[key] = newHandle;
            return newHandle.Result;
        }
        else
        {
            Debug.LogError($"[AddressablesManager] Failed to load asset with key: {key}. Error: {newHandle.OperationException}");
            Addressables.Release(newHandle);
            return default;
        }
    }

    // 根据标签异步加载多个资源
    public async Task LoadAssetsByLabelAsync(string label)
    {
        if (_labelLocationHandles.ContainsKey(label))
        {
            Debug.LogWarning($"[AddressablesManager] Assets with label '{label}' are already loaded or being loaded.");
            return;
        }

        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        _labelLocationHandles[label] = locationsHandle;
        await locationsHandle.Task;

        if (locationsHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AddressablesManager] Failed to get resource locations for label: '{label}'.");
            return;
        }

        var loadTasks = new List<Task>();
        var assetHandles = new List<AsyncOperationHandle>();
        _labelAssetHandles[label] = assetHandles;
        foreach (var location in locationsHandle.Result)
        {
            var loadHandle = Addressables.LoadAssetAsync<Object>(location);
            loadTasks.Add(loadHandle.Task);
            assetHandles.Add(loadHandle);
        }

        await Task.WhenAll(loadTasks);
        Debug.Log($"[AddressablesManager] Successfully loaded all assets for label: '{label}'.");
    }

    // 释放单个已加载资源
    public void ReleaseAsset(string key)
    {
        if (_assetHandles.TryGetValue(key, out var handle))
        {
            _assetHandles.Remove(key);
            Addressables.Release(handle);
        }
    }

    // 释放某个标签下所有已加载资源
    public void ReleaseAssetsByLabel(string label)
    {
        if (!_labelAssetHandles.TryGetValue(label, out var assetHandles))
        {
            foreach (var handle in assetHandles)
            {
                Addressables.Release(handle);
            }
            _labelAssetHandles.Remove(label);
        }

        if (_labelLocationHandles.TryGetValue(label, out var locationHandle))
        {
            Addressables.Release(locationHandle);
            _labelLocationHandles.Remove(label);
        }

        Debug.Log($"[AddressablesManager] Released all assets for label: '{label}'.");
    }

    // 实例化一个预制体到指定Parent下的位置
    public async Task<GameObject> InstantiatePrefabAsync(string key, Transform parent = null)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, parent);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _instanceHandles[handle.Result] = handle;
            return handle.Result;
        }
        else
        {
            Debug.LogError($"[AddressablesManager] Failed to instantiate prefab with key: {key}. Error: {handle.OperationException}");
            return null;
        }
    }

    // 释放一个已实例化的GameObject, 如果该物体不是本管理器创建的，则直接销毁.
    public void ReleaseInstance(GameObject instance)
    {
        if (instance != null && _instanceHandles.TryGetValue(instance, out var handle))
        {
            _instanceHandles.Remove(instance);
            Addressables.Release(handle);
        }
        else
        {
            if (instance != null)
            {
                Debug.LogWarning($"[AddressablesManager] Trying to release an instance that was not created by AddressablesManager: {instance.name}. Destroying it directly.");
                Destroy(instance);
            }
        }
    }

    void OnDestroy()
    {
        foreach (var handle in _assetHandles.Values)
        {
            Addressables.Release(handle);
        }
        _assetHandles.Clear();
        
        foreach (var label in new List<string>(_labelAssetHandles.Keys))
        {
            ReleaseAssetsByLabel(label);
        }
        
        foreach (var handle in _instanceHandles.Values)
        {
            Addressables.Release(handle);
        }
        _instanceHandles.Clear();
    }
}

