using System;
using System.Collections.Generic;
using Tanktastic.ObjectScripts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuickSpawnPool
{
    public static class Pool
    {
        public static Dictionary<string, Queue<Transform>> PoolWithTransforms { get; private set; }
        public static Dictionary<string, Queue<IPoolable>> PoolWithScripts { get; private set; }
        public static Dictionary<int, Queue<Transform>> PoolWithThingies { get; private set; } 
    
        public static bool IsInitialized { get; private set; }
    
        #if(POOL_STATISTICS && UNITY_EDITOR)
        public static PoolStatistics PoolStatistics {get; private set; }
        #endif
    
        private static Transform _container;
        private static PoolEntity _poolEntity;
    
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
    
            PoolWithTransforms = new Dictionary<string, Queue<Transform>>();
            PoolWithScripts = new Dictionary<string, Queue<IPoolable>>();
            PoolWithThingies = new Dictionary<int, Queue<Transform>>();
    
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
    
            PoolWithTransforms = null;
            PoolWithScripts = null;
            PoolWithThingies = null;
            _poolEntity = null;
    
            IsInitialized = false;
        }

        /// <summary>
        /// Get object of type Transform from Spawn Pool or Instantiate from prefab
        /// </summary>
        /// <param name="prefab">Reference to prefab</param>
        /// <returns></returns>
        public static Transform SpawnTransform(Transform prefab)
        {
            return SpawnTransform(prefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Get object of type Transform from Spawn Pool or Instantiate from prefab
        /// </summary>
        /// <param name="prefab">Reference to prefab</param>
        /// <param name="position">Instance position on spawn</param>
        /// <param name="rotation">Instance rotation on spawn</param>
        /// <returns>Returns reference to spawned Transform</returns>
        public static Transform SpawnTransform(
            Transform prefab, 
            Vector3 position, 
            Quaternion rotation)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(prefab == null)
            {
                Debug.LogError("[Pool.SpawnTransform(Transform prefab, Vector3 position, Quaternion rotation)] prefab == null");
                return null;
            }
            #endif

            string prefabName = prefab.name;
            if(PoolWithTransforms.ContainsKey(prefabName) && PoolWithTransforms[prefabName].Count > 0)
            {
                Transform poolable = DequeueTransform(prefabName, position, rotation);
                poolable.gameObject.SetActive(true);
                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.TrySpawnTransform(poolable);
                #endif
                return poolable;
            }

            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.CheckSpawnTransform(prefab);
            #endif

            return InstantiateTransform(prefab, position, rotation);
        }

        /// <summary>
        /// Get object of type IPoolable from Spawn Pool or Instantiate from prefab
        /// </summary>
        /// <param name="prefab">Reference to prefab</param>
        /// <returns></returns>
        public static IPoolable SpawnPoolable(Transform prefab)
        {
            return SpawnPoolable(prefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Get object of type IPoolable from Spawn Pool or Instantiate from prefab
        /// </summary>
        /// <param name="prefab">Reference to prefab</param>
        /// <param name="position">Instance position on spawn</param>
        /// <param name="rotation">Instance rotation on spawn</param>
        /// <returns>Returns reference to spawned IPoolable</returns>
        public static IPoolable SpawnPoolable(
            Transform prefab, 
            Vector3 position, 
            Quaternion rotation)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(prefab == null)
            {
                Debug.LogError("Pool.SpawnPoolable(Transform prefab, Vector3 position, Quaternion rotation) prefab == null");
                return null;
            }
            #endif

            string prefabName = prefab.name;
            if(PoolWithScripts.ContainsKey(prefabName) && PoolWithScripts[prefabName].Count > 0)
            {
                IPoolable poolable = PoolWithScripts[prefabName].Dequeue();
                
                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.TrySpawnIPoolable(poolable);
                #endif

                Transform pooledTransform = poolable.transform;
                pooledTransform.position = position;
                pooledTransform.rotation = rotation;
                poolable.transform.gameObject.SetActive(true);
                poolable.OnSpawn();

                return poolable;
            }
            
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.CheckSpawnPoolable(prefab);
            #endif

            return InstantiatePoolable(prefab, position, rotation);
        }

        public static PoolableThingy SpawnThingy(Transform prefab, Vector3 pos, Quaternion rot)
        {
            var id = prefab.GetInstanceID();
            if(PoolWithThingies.ContainsKey(id) && PoolWithThingies[id].Count > 0)
            {
                PoolableThingy poolable;

                poolable.t = PoolWithThingies[id].Dequeue();
                poolable.id = id;

                Transform pooledTransform = poolable.t;
                pooledTransform.position = pos;
                pooledTransform.rotation = rot;
                poolable.t.gameObject.SetActive(true);

                return poolable;
            }           

            return InstantiateThingy(prefab, pos, rot);
        }

        private static PoolableThingy InstantiateThingy(
            Transform prefab, 
            Vector3 position, 
            Quaternion rotation)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(prefab == null)
            {
                Debug.LogError("Pool.InstantiateTransform(Transform prefab, Vector3 position, Quaternion rotation) prefab == null");
                return new PoolableThingy();
            }
            #endif
            
            PoolableThingy pooledObject;
            pooledObject.id = prefab.GetInstanceID();
            pooledObject.t = Object.Instantiate(prefab, position, rotation) as Transform;
        
            pooledObject.t.name = prefab.name;
            pooledObject.t.parent = _container;

            return pooledObject;
        }

        public static void DespawnThingy(PoolableThingy instance)
        {
            instance.t.gameObject.SetActive(false);

            if(PoolWithThingies.ContainsKey(instance.id))
            {
                Queue<Transform> instances = PoolWithThingies[instance.id];
                instances.Enqueue(instance.t);
            }
            else
            {
                Queue<Transform> instances = new Queue<Transform>();
                instances.Enqueue(instance.t);

                PoolWithThingies.Add(instance.id, instances);
            }

            instance.t.parent = _container;
        }

        /// <summary>
        /// Get object of type Transform from Spawn Pool or load from 'Resources' folder.
        /// </summary>
        /// <param name="prefabName">Prefab name</param>
        /// <param name="path">Full path to prefab without 'Resources/' forlder
        /// <code>Prefabs/Buildings/House/HousePrefabName</code>
        /// </param>
        /// <param name="position">Instance position on spawn</param>
        /// <param name="rotation">Instance rotation on spawn</param>
        /// <returns>Returns reference to spawned Transform</returns>
        public static Transform SpawnTransform(
            string prefabName, 
            string path, 
            Vector3 position, 
            Quaternion rotation)
        {
            if(PoolWithTransforms.ContainsKey(prefabName) && PoolWithTransforms[prefabName].Count > 0)
            {
                Transform instance = DequeueTransform(prefabName, position, rotation);
                
                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.TrySpawnTransform(instance);
                #endif

                return instance;
            }
            
            Transform prefab = Resources.Load<Transform>(path);
            
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(prefab == null)
            {
                Debug.LogError("Pool.SpawnTransform(string prefabName, string path, Vector3 position, Quaternion rotation) prefab == null. Path: " + path);
                return null;
            }
            PoolStatistics.CheckSpawnTransform(prefab);
            #endif

            return InstantiateTransform(prefab, position, rotation);
        }
    
        /// <summary>
        /// Get object of type IPoolable from Spawn Pool or load from 'Resources' folder
        /// </summary>
        /// <param name="prefabName">Prefab name</param>
        /// <param name="path">Full path to prefab without 'Resources/' forlder
        /// <code>Prefabs/Buildings/House/HousePrefabName</code>
        /// </param>
        /// <param name="position">Instance position on spawn</param>
        /// <param name="rotation">Instance rotation on spawn</param>
        /// <returns>Returns reference to spawned Transform</returns>
        public static IPoolable SpawnPoolable(
            string prefabName, 
            string path, 
            Vector3 position, 
            Quaternion rotation)
        {
            if(PoolWithScripts.ContainsKey(prefabName) && PoolWithScripts[prefabName].Count > 0)
            {
                IPoolable poolable = DequeuePoolable(prefabName, position, rotation);
                
                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.TrySpawnIPoolable(poolable);
                #endif

                return poolable;
            }
            
            Transform prefab = Resources.Load<Transform>(path);
            
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(prefab == null)
            {
                Debug.LogError("Pool.SpawnPoolable(string prefabName, string path, Vector3 position, Quaternion rotation) prefab == null. Path: " + path);
                return null;
            }
            PoolStatistics.CheckSpawnPoolable(prefab);
            #endif

            return InstantiatePoolable(prefab, position, rotation);
        }

        

        /// <summary>
        /// Asynchronously get object of type Transform from Spawn Pool or load from 'Resources' folder
        /// </summary>
        /// <param name="prefabName">Prefab name</param>
        /// <param name="path">Full path to prefab without 'Resources/' forlder
        /// <code>Prefabs/Buildings/House/HousePrefabName</code>
        /// </param>
        /// <param name="position">Instance position on spawn</param>
        /// <param name="rotation">Instance rotation on spawn</param>
        /// <param name="callback">Callback action. Runs after Transform has been loaded. Can be null</param>
        public static void SpawnTransformAsync(
            string prefabName, 
            string path, 
            Vector3 position, 
            Quaternion rotation, 
            Action<Transform> callback)
        {
            Transform pooled;
            if (PoolWithTransforms.ContainsKey(prefabName) 
                && PoolWithTransforms[prefabName].Count > 0)
            {
                pooled = DequeueTransform(prefabName, position, rotation);

                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.TrySpawnTransform(pooled);
                #endif

                if(callback != null)
                    callback(pooled);

                return;
            }

            ResourceLoadHelper.StartResourceLoadAsync<Transform>(path, (prefab) =>
            {
                #if(POOL_STATISTICS && UNITY_EDITOR)
                if(prefab == null)
                {
                    Debug.LogError("Pool.SpawnTransformAsync(string prefabName, string path, Vector3 position, Quaternion rotation, Action<Transform> callback) prefab == null. Path: " + path);
                    return;
                }
                PoolStatistics.CheckSpawnTransform(prefab);
                #endif

                pooled = InstantiateTransform(prefab, position, rotation);

                if(callback != null)
                    callback(pooled);
            });
        }
    
        /// <summary>
        /// Asynchronously get object of type IPoolable from Spawn Pool or load from 'Resources' folder
        /// </summary>
        /// <param name="prefabName">Prefab name</param>
        /// <param name="path">Full path to prefab without 'Resources/' forlder
        /// <code>Prefabs/Buildings/House/HousePrefabName</code>
        /// </param>
        /// <param name="position">Instance position on spawn</param>
        /// <param name="rotation">Instance rotation on spawn</param>
        /// <param name="callback">Callback action. Runs after IPoolable has been loaded. Can be null</param>
        public static void SpawnPoolableAsync(
            string prefabName, 
            string path, 
            Vector3 position, 
            Quaternion rotation, 
            Action<IPoolable> callback)
        {
            IPoolable pooled;
            if(PoolWithScripts.ContainsKey(prefabName) && PoolWithScripts[prefabName].Count > 0)
            {
                pooled = DequeuePoolable(prefabName, position, rotation);

                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.TrySpawnIPoolable(pooled);
                #endif

                if(callback != null)
                    callback(pooled);

                return;
            }
    
            ResourceLoadHelper.StartResourceLoadAsync<Transform>(path, (prefab) =>
            {
                #if(POOL_STATISTICS && UNITY_EDITOR)
                if(prefab == null)
                {
                    Debug.LogError("Pool.SpawnPoolableAsync(string prefabName, string path, Vector3 position, Quaternion rotation, Action<Transform> callback) prefab == null. Path: " + path);
                    return;
                }
                PoolStatistics.CheckSpawnPoolable(prefab);
                #endif

                pooled = InstantiatePoolable(prefab, position, rotation);

                if(callback != null)
                    callback(pooled);
            });
        }
    
        /// <summary>
        /// Push Tranform into Spawn Pool
        /// </summary>
        /// <param name="instance">Reference to Tranasform instance</param>
        public static void DespawnTransform(Transform instance)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(instance == null)
            {
                Debug.LogError("Pool.DespawnTransform(Transform instance) instance == null");
                return;
            }
            #endif

            instance.gameObject.SetActive(false);

            if(PoolWithTransforms.ContainsKey(instance.name))
            {
                Queue<Transform> instances = PoolWithTransforms[instance.name];
                instances.Enqueue(instance);
            }
            else
            {
                Queue<Transform> instances = new Queue<Transform>();
                instances.Enqueue(instance);

                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.CheckDespawnTransform(instance);
                #endif

                PoolWithTransforms.Add(instance.name, instances);
            }

            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TryDespawnTransform(instance);
            #endif

            instance.parent = _container;
        }
    
        /// <summary>
        /// Push IPoolable into Spawn Pool
        /// </summary>
        /// <param name="instance">Reference to IPoolable instance</param>
        public static void DespawnPoolable(IPoolable instance)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(instance == null)
            {
                Debug.LogError("Pool.DespawnPoolable(IPoolable instance) instance == null");
                return;
            }
            #endif

            instance.transform.gameObject.SetActive(false);
            instance.OnDespawn();
            string groupName = instance.transform.gameObject.name;

            if(PoolWithScripts.ContainsKey(groupName))
            {
                Queue<IPoolable> instances = PoolWithScripts[groupName];
                instances.Enqueue(instance);
            }
            else
            {
                Queue<IPoolable> instances = new Queue<IPoolable>();
                instances.Enqueue(instance);

                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.CheckDespawnPoolable(instance.transform);
                #endif

                PoolWithScripts.Add(groupName, instances);
            }

            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TryDespawnIPoolable(instance);
            #endif

            instance.transform.parent = _container;
        }
    
        /// <summary>
        /// Push Transform into Spawn Pool after 'lifeTime' seconds
        /// </summary>
        /// <param name="instance">Reference to Transform instance</param>
        /// <param name="lifeTime">Time in seconds after wich Transform will be pushed back into Spawn Pool</param>
        public static void DespawnTransform(Transform instance, float lifeTime)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(instance == null)
            {
                Debug.LogError("Pool.DespawnTransform(Transform instance, float lifeTime) instance == null");
                return;
            }
            #endif

            if(lifeTime <= 0)
                DespawnTransform(instance);
            else
                _poolEntity.AddToTrash(instance, lifeTime);
        }
    
        /// <summary>
        /// Push IPoolable into Spawn Pool after 'lifeTime' seconds
        /// </summary>
        /// <param name="instance">Reference to IPoolable instance</param>
        /// <param name="lifeTime">Time in seconds after wich IPoolable will be pushed back into Spawn Pool</param>
        public static void DespawnPoolable(IPoolable instance, float lifeTime)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(instance == null)
            {
                Debug.LogError("Pool.DespawnPoolable(IPoolable instance, float lifeTime) instance == null");
                return;
            }
            #endif

            if(lifeTime <= 0)
                DespawnPoolable(instance);
            else
                _poolEntity.AddToTrash(instance, lifeTime);
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

                    IPoolable instance = InstantiatePoolable(prefab, Vector3.zero, Quaternion.identity);
                    DespawnPoolable(instance);
                }
            }
            else
            {
                for(int i = 0; i < amount; i++)
                {
                    #if(POOL_STATISTICS && UNITY_EDITOR)
                    PoolStatistics.CheckSpawnTransform(prefab);
                    #endif

                    Transform instance = InstantiateTransform(prefab, Vector3.zero, Quaternion.identity);
                    DespawnTransform(instance);
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
            ResourceLoadHelper.StartResourceLoadAsync<Transform>(path, (t) =>
            {
                PreSpawn(t, amount);
            });
        }
    
        private static Transform DequeueTransform(
            string prefabName, 
            Vector3 position, 
            Quaternion rotation)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(!PoolWithTransforms.ContainsKey(prefabName))
            {
                Debug.LogError("Pool.DequeueTransform(string prefabName, Vector3 position, Quaternion rotation) PoolWithTransform doesn't contains prefabName");
                return null;
            }
            #endif

            Transform poolable = PoolWithTransforms[prefabName].Dequeue();
            Transform pooledTransform = poolable.transform;
            pooledTransform.position = position;
            pooledTransform.rotation = rotation;
            poolable.gameObject.SetActive(true);

            return poolable;
        }
    
        private static Transform InstantiateTransform(
            Transform prefab, 
            Vector3 position, 
            Quaternion rotation)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(prefab == null)
            {
                Debug.LogError("Pool.InstantiateTransform(Transform prefab, Vector3 position, Quaternion rotation) prefab == null");
                return null;
            }
            #endif
            
            Transform pooledObject = Object.Instantiate(prefab, position, rotation) as Transform;
        
            pooledObject.name = prefab.name;
            pooledObject.parent = _container;

            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TrySpawnTransform(pooledObject);
            #endif

            return pooledObject;
        }
    
        private static IPoolable DequeuePoolable(
            string prefabName, 
            Vector3 position, 
            Quaternion rotation)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(!PoolWithScripts.ContainsKey(prefabName))
            {
                Debug.LogError("Pool.DequeuePoolable(string prefabName, Vector3 position, Quaternion rotation) PoolWithScripts doesn't contains prefabName");
                return null;
            }
            #endif

            IPoolable poolable = PoolWithScripts[prefabName].Dequeue();
            Transform pooledTransform = poolable.transform;
            pooledTransform.position = position;
            pooledTransform.rotation = rotation;
            poolable.transform.gameObject.SetActive(true);
            poolable.OnSpawn();

            return poolable;
        }
    
        private static IPoolable InstantiatePoolable(
            Transform prefab, 
            Vector3 position, 
            Quaternion rotation)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(prefab == null)
            {
                Debug.LogError("Pool.InstantiatePoolable(Transform prefab, Vector3 position, Quaternion rotation) prefab == null");
                return null;
            }
            #endif
            
            Transform instance = Object.Instantiate(prefab, position, rotation) as Transform;
            instance.name = prefab.name;
            instance.parent = _container;
            IPoolable poolable = instance.GetComponent<IPoolable>();
            poolable.OnSpawn();

            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TrySpawnIPoolable(poolable);
            #endif

            return poolable;
        }

        public struct PoolableThingy
        {
            public Transform t;
            public int id;
        }
    }
}
