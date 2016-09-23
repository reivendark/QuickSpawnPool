using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuickSpawnPool
{
    public static partial class Pool
    {
        public static bool IsInitialized { get; private set; }
    
        private static Transform _container;
        private static PoolEntity _poolEntity;
    
        #if(POOL_STATISTICS && UNITY_EDITOR)
        public static PoolStatistics PoolStatistics {get; private set; }
        #endif

        /// <summary>
        /// Reset and init Spawn Pool. Call it somewhere on aplication start
        /// </summary>
        public static void Reset()
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics = new PoolStatistics();
            #endif

            _poolEntity = Object.FindObjectOfType<PoolEntity>();
            if(_poolEntity != null)
            {
                _container = _poolEntity.transform;
                for(int i = 0; i < _container.childCount; i++)
                {
                    Object.Destroy(_container.GetChild(i).gameObject);
                }
            }
            else
            {
                _poolEntity = new GameObject("PoolEntity").AddComponent<PoolEntity>();
                _container = _poolEntity.transform;
            }
    
            PoolWithPooledTransforms = new Dictionary<int, Queue<Transform>>();
            PoolWithPooledIPoolable = new Dictionary<int, Queue<IPoolable>>();

            TransformNamesCollection = new Dictionary<string, int>();
            IPoolableNamesCollection = new Dictionary<string, int>();

            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.Initialize();
            #endif

            _poolEntity.LoadAll();
    
            IsInitialized = true;
        }
    
        /// <summary>
        /// Destroys Spawn Pool
        /// </summary>
        public static void Destroy()
        {
            if(!IsInitialized)
                return;

            _poolEntity.Reset();
    
            PoolWithPooledTransforms = null;
            _poolEntity = null;
    
            IsInitialized = false;
        }
    
        /// <summary>
        /// Pre-spawn certain amount of prefab instances and push them into Spawn Pool
        /// </summary>
        /// <param name="prefab">Prefab reference</param>
        /// <param name="amount">Amount of instances to pre-spawn</param>
        public static void PreSpawn(Transform prefab, int amount)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(prefab == null)
            {
                Debug.LogError("Pool.PreSpawn(Transform prefab, int amount) prefab == null");
                return;
            }
            #endif

            bool isIPoolable = prefab.GetComponent<IPoolable>() != null;
            if(isIPoolable)
            {
                for(int i = 0; i < amount; i++)
                {
                    #if(POOL_STATISTICS && UNITY_EDITOR)
                    PoolStatistics.CheckSpawnPoolable(prefab);
                    #endif
                    IPoolableThingy instance = InstantiateIThingy(prefab, Vector3.zero, Quaternion.identity);
                    DespawnIThingy(instance);
                }
            }
            else
            {
                for(int i = 0; i < amount; i++)
                {
                    #if(POOL_STATISTICS && UNITY_EDITOR)
                    PoolStatistics.CheckSpawnTransform(prefab);
                    #endif
                    var instance = InstantiateThingy(prefab, Vector3.zero, Quaternion.identity);
                    DespawnThingy(instance);
                }
            }
        }
    
        /// <summary>
        /// Pre-spawn certain amount of prefab instances from 'Resources' and push them into Spawn Pool
        /// </summary>
        /// /// <param name="path">Full path to prefab without 'Resources/' forlder
        /// <code>Prefabs/Buildings/House/HousePrefabName</code>
        /// </param>
        /// <param name="amount">Amount of instances to pre-spawn</param>
        public static void PreSpawn(string path, int amount)
        {
            PoolResourceLoader.StartResourceLoadAsync<Transform>(path, (t) =>
            {
                PreSpawn(t, amount);
            });
        }

        public struct PoolableThingy
        {
            public Transform t;
            public int id;
        }

        public struct IPoolableThingy
        {
            public IPoolable poolable;
            public int id;
        }
    }
}
