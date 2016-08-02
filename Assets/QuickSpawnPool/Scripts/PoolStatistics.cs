#if(POOL_STATISTICS && UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSpawnPool
{
    public class PoolStatistics
    {
        public Dictionary<string, PoolElementData> TransformStorage { get; private set; }
        public Dictionary<string, PoolElementData> PoolableStorage { get; private set; }

        public void Initialize()
        {
            TransformStorage = new Dictionary<string, PoolElementData>();
            PoolableStorage = new Dictionary<string, PoolElementData>();
        }

        internal void CheckSpawnTransform(Transform prefab)
        {
            string name = prefab.name;
            if(TransformStorage.ContainsKey(name))
            {
                PoolElementData ped = TransformStorage[name];
                ped.instances++;
                TransformStorage[name] = ped;
                return;
            }

            string path = AssetDatabase.GetAssetPath(prefab.gameObject);

            TransformStorage.Add(name, new PoolElementData(path, 0, 0, 1));

            if(prefab.GetComponent<IPoolable>() != null)
            {
                Debug.LogError("[Pool] You're trying to spawn IPoolable like Transform. Name: " + prefab.name);
            }
        }

        internal void TrySpawnTransform(Transform prefab)
        {
            string name = prefab.name;
            if(TransformStorage.ContainsKey(name))
            {
                PoolElementData ped = TransformStorage[name];
                ped.spawnCount++;
                TransformStorage[name] = ped;
            }
            else
            {
                throw new Exception("Yout must not be here");
            }
        }

        internal void CheckSpawnPoolable(Transform prefab)
        {
            string name = prefab.name;
            if(PoolableStorage.ContainsKey(name))
            {
                PoolElementData ped = PoolableStorage[name];
                ped.instances++;
                PoolableStorage[name] = ped;
                return;
            }

            string path = AssetDatabase.GetAssetPath(prefab.gameObject);

            PoolableStorage.Add(name, new PoolElementData(path, 0, 0, 1));

            IPoolable poolable = prefab.GetComponent<IPoolable>();
            if(poolable == null)
            {
                Debug.LogError("[Pool] You're trying to spawn Transform like IPoolable. Name: " + prefab.name);
            }
        }

        internal void TrySpawnIPoolable(IPoolable poolable)
        {
            string name = poolable.transform.name;
            if(PoolableStorage.ContainsKey(name))
            {
                PoolElementData ped = PoolableStorage[name];
                ped.spawnCount++;
                PoolableStorage[name] = ped;
            }
            else
            {
                throw new Exception("Yout must not be here");
            }
        }

        internal void CheckDespawnTransform(Transform prefab)
        {
            if(prefab.GetComponent<IPoolable>() != null)
            {
                Debug.LogError("[Pool] You're trying to despawn IPoolable like Transform. Name: " + prefab.name);
            }
        }

        internal void TryDespawnTransform(Transform prefab)
        {
            string name = prefab.name;
            if(TransformStorage.ContainsKey(name))
            {
                PoolElementData ped = TransformStorage[name];
                ped.despawnCount++;
                TransformStorage[name] = ped;
            }
            else
            {
                throw new Exception("Yout must not be here");
            }
        }

        internal void CheckDespawnPoolable(Transform prefab)
        {
            IPoolable poolable = prefab.GetComponent<IPoolable>();
            if(poolable == null)
            {
                Debug.LogError("[Pool] You're trying to despawn Transform like IPoolable. Name: " + prefab.name);
            }
        }

        internal void TryDespawnIPoolable(IPoolable poolable)
        {
            string name = poolable.transform.name;
            if(PoolableStorage.ContainsKey(name))
            {
                PoolElementData ped = PoolableStorage[name];
                ped.despawnCount++;
                PoolableStorage[name] = ped;
            }
            else
            {
                throw new Exception("Yout must not be here");
            }
        }

        public struct PoolElementData
        {
            public int spawnCount;
            public int despawnCount;
            public int instances;
            public string path;

            public PoolElementData(string path, int spawnCount, int despawnCount, int instances)
            {
                this.path = path;
                this.spawnCount = spawnCount;
                this.despawnCount = despawnCount;
                this.instances = instances;
            }
        }
    }
}
#endif