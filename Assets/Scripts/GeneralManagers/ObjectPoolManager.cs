using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class PoolConfig
{
    public GameObject prefab;
    public int preloadSize;
}

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [Header("Preload Pool Size")]
    [SerializeField] private List<PoolConfig> poolConfigs;
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();
    private Dictionary<int, Transform> poolContainers = new Dictionary<int, Transform>();

    protected override void Awake()
    {
        base.Awake();
        PreloadPools();
    }

    private void PreloadPools()
    {
        foreach (PoolConfig config in poolConfigs)
        {
            CreatePool(config.prefab);
            for (int i = 0; i < config.preloadSize; i++)
            {
                GameObject obj = Instantiate(config.prefab);
                obj.transform.SetParent(poolContainers[config.prefab.GetInstanceID()]);
                obj.SetActive(false);
                poolDictionary[config.prefab.GetInstanceID()].Enqueue(obj);
            }
        }
    }

    private void CreatePool(GameObject prefab)
    {
        int poolKey = prefab.GetInstanceID();
        if (!poolDictionary.ContainsKey(poolKey))
        {
            Transform newContainer = new GameObject(prefab.name + " Pool").transform;
            newContainer.SetParent(transform);
            poolContainers.Add(poolKey, newContainer);
            poolDictionary.Add(poolKey, new Queue<GameObject>());
        }
    }

    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if (!poolDictionary.ContainsKey(poolKey))
        {
            CreatePool(prefab);
        }

        GameObject objectToSpawn;
        if (poolDictionary[poolKey].Count > 0)
        {
            objectToSpawn = poolDictionary[poolKey].Dequeue();
        }
        else
        {
            objectToSpawn = Instantiate(prefab);
            objectToSpawn.transform.SetParent(poolContainers[poolKey]);
        }

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void ReturnToPool(GameObject prefab, GameObject objectToReturn)
    {
        int poolKey = prefab.GetInstanceID();
        if (!poolDictionary.ContainsKey(poolKey))
        {
            Destroy(objectToReturn);
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[poolKey].Enqueue(objectToReturn);
    }
}
