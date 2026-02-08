using UnityEngine;
using System.Collections.Generic;

namespace Utils
{
    /// <summary>
    /// Manages a pool of objects to optimize performance by reducing instantiation costs.
    /// </summary>
    public class PoolingManager : Singleton<PoolingManager>
    {

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
            [HideInInspector]
            public int spawnCounter = 0;
        }

        [SerializeField] private List<Pool> pools;
        [SerializeField] private Transform poolContainer; // Parent for inactive objects

        private Dictionary<string, Queue<GameObject>> _poolDictionary;

        private void Awake()
        {
            _poolDictionary = new Dictionary<string, Queue<GameObject>>();

            InitializePools();
        }

        /// <summary>
        /// Pre-instantiates objects based on the pool configuration.
        /// </summary>
        private void InitializePools()
        {
            foreach (var pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, poolContainer);
                    pool.spawnCounter++;
                    obj.name = $"{pool.tag} {pool.spawnCounter}";
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                _poolDictionary.Add(pool.tag, objectPool);
            }
        }

        /// <summary>
        /// Spawns an object from the pool.
        /// </summary>
        /// <param name="tag">The tag identifier for the pool.</param>
        /// <param name="position">World position to spawn at.</param>
        /// <param name="rotation">Rotation to apply.</param>
        /// <returns>The spawned GameObject.</returns>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!_poolDictionary.ContainsKey(tag))
            {
                Debug.LogError($"Pool with tag {tag} doesn't exist.");
                return null;
            }

            Pool pool = pools.Find(p => p.tag == tag);
            GameObject objectToSpawn = _poolDictionary[tag].Count > 0
                ? _poolDictionary[tag].Dequeue()
                : Instantiate(pool.prefab, poolContainer);

            objectToSpawn.SetActive(true);
            pool.spawnCounter++;
            objectToSpawn.name = $"{tag} {((_poolDictionary[tag].Count == 0) ? pool.spawnCounter : _poolDictionary[tag].Count)}";
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            // Move to root or active gameplay hierarchy to avoid scaling issues if container is scaled
            objectToSpawn.transform.SetParent(parent);

            return objectToSpawn;
        }

        /// <summary>
        /// Returns an object back to the pool.
        /// </summary>
        /// <param name="tag">The pool tag.</param>
        /// <param name="objectToReturn">The object to recycle.</param>
        public void ReturnToPool(string tag, GameObject objectToReturn)
        {
            if (!_poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Trying to return object to non-existent pool {tag}. Destroying instead.");
                Destroy(objectToReturn);
                return;
            }

            objectToReturn.SetActive(false);
            // Reset parent to container to keep hierarchy clean
            objectToReturn.transform.SetParent(poolContainer);
            _poolDictionary[tag].Enqueue(objectToReturn);
        }
    }
}