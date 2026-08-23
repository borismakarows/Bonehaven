using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoneHaven
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int initialSize = 10;
        }

        [Header("Pool Definitions")]
        [SerializeField] private List<Pool> pools = new List<Pool>();

        private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var pool in pools)
            {
                Queue<GameObject> objectQueue = new Queue<GameObject>();
                prefabLookup[pool.tag] = pool.prefab;

                for (int i = 0; i < pool.initialSize; i++)
                {
                    GameObject obj = CreateNewObject(pool.tag, pool.prefab);
                    objectQueue.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectQueue);
            }
        }

        private GameObject CreateNewObject(string tag, GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);

            if (!obj.TryGetComponent(out PooledObject pooledComp))
            {
                pooledComp = obj.AddComponent<PooledObject>();
            }
            pooledComp.SetPoolTag(tag);

            return obj;
        }

        /// <summary>
        /// Retrieves an object from the pool, positioning and activating it.
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' does not exist.");
                return null;
            }

            Queue<GameObject> queue = poolDictionary[tag];
            GameObject objectToSpawn;

            if (queue.Count > 0)
            {
                objectToSpawn = queue.Dequeue();
            }
            else
            {
                // Dynamic growth if pool runs out during intense combat
                objectToSpawn = CreateNewObject(tag, prefabLookup[tag]);
            }

            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            return objectToSpawn;
        }

        /// <summary>
        /// Returns an active instance back to its queue.
        /// </summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            poolDictionary[tag].Enqueue(obj);
        }
    }
}