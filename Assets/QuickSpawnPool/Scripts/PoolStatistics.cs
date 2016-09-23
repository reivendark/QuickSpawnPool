#if(UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSpawnPool
{
    public class PoolStatistics
    {
        public Dictionary<int, PoolElementData> TransformStorage { get; private set; }
        public Dictionary<int, PoolElementData> PoolableStorage { get; private set; }

        public void Initialize()
        {
            TransformStorage = new Dictionary<int, PoolElementData>();
            PoolableStorage = new Dictionary<int, PoolElementData>();
        }

        internal void CheckSpawnTransform(Transform prefab)
        {
            int id = prefab.GetInstanceID();
            if(TransformStorage.ContainsKey(id))
            {
                PoolElementData ped = TransformStorage[id];
                ped.instances++;
                TransformStorage[id] = ped;
                return;
            }

            string path = AssetDatabase.GetAssetPath(prefab.gameObject);

            TransformStorage.Add(id, new PoolElementData(prefab.name, path, 0, 0, 1));

            if(prefab.GetComponent<IPoolable>() != null)
            {
                Debug.LogError("[Pool] You're trying to spawn IPoolable like Transform. Name: " + prefab.name);
            }
        }

        internal void TrySpawnTransform(Pool.PoolableThingy pooledPrefab)
        {
            int id = pooledPrefab.id;
            if(TransformStorage.ContainsKey(id))
            {
                PoolElementData ped = TransformStorage[id];
                ped.spawnCount++;
                TransformStorage[id] = ped;
            }
            else
            {
                throw new Exception("Yout must not be here");
            }
        }

        internal void CheckSpawnPoolable(Transform pooledPrefab)
        {
            int id = pooledPrefab.GetInstanceID();
            if(PoolableStorage.ContainsKey(id))
            {
                PoolElementData ped = PoolableStorage[id];
                ped.instances++;
                PoolableStorage[id] = ped;
                return;
            }

            string path = AssetDatabase.GetAssetPath(pooledPrefab.gameObject);

            PoolableStorage.Add(id, new PoolElementData(pooledPrefab.name, path, 0, 0, 1));

            IPoolable poolable = pooledPrefab.GetComponent<IPoolable>();
            if(poolable == null)
            {
                Debug.LogError("[Pool] You're trying to spawn Transform like IPoolable. Name: " + pooledPrefab.name);
            }
        }

        internal void TrySpawnIPoolable(Pool.IPoolableThingy poolable)
        {
            if(PoolableStorage.ContainsKey(poolable.id))
            {
                PoolElementData ped = PoolableStorage[poolable.id];
                ped.spawnCount++;
                PoolableStorage[poolable.id] = ped;
            }
            else
            {
                throw new Exception("Yout must not be here");
            }
        }

        internal void CheckDespawnTransform(Pool.PoolableThingy prefab)
        {
            if(prefab.t.GetComponent<IPoolable>() != null)
            {
                Debug.LogError("[Pool] You're trying to despawn IPoolable like Transform. Name: " + prefab.t.name);
            }
        }

        internal void TryDespawnTransform(Pool.PoolableThingy prefab)
        {
            if(TransformStorage.ContainsKey(prefab.id))
            {
                PoolElementData ped = TransformStorage[prefab.id];
                ped.despawnCount++;
                TransformStorage[prefab.id] = ped;
            }
            else
            {
                throw new Exception("Yout must not be here");
            }
        }

        internal void CheckDespawnPoolable(Pool.IPoolableThingy prefab)
        {
            IPoolable poolable = prefab.poolable.transform.GetComponent<IPoolable>();
            if(poolable == null)
            {
                Debug.LogError("[Pool] You're trying to despawn Transform like IPoolable. Name: " + prefab.poolable.transform);
            }
        }

        internal void TryDespawnIPoolable(Pool.IPoolableThingy poolable)
        {
            if(PoolableStorage.ContainsKey(poolable.id))
            {
                PoolElementData ped = PoolableStorage[poolable.id];
                ped.despawnCount++;
                PoolableStorage[poolable.id] = ped;
            }
            else
            {
                throw new Exception("Yout must not be here");
            }
        }

        public struct PoolElementData
        {
            public string name;
            public int spawnCount;
            public int despawnCount;
            public int instances;
            public string path;

            public PoolElementData(string name, string path, int spawnCount, int despawnCount, int instances)
            {
                this.name = name;
                this.path = path;
                this.spawnCount = spawnCount;
                this.despawnCount = despawnCount;
                this.instances = instances;
            }
        }
    }
}
#endif