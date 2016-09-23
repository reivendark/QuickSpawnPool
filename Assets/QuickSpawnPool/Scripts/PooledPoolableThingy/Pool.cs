using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuickSpawnPool
{
    public static partial class Pool
    {
        public static Dictionary<int, Queue<IPoolable>> PoolWithPooledIPoolable { get; private set; }
        public static Dictionary<string, int> IPoolableNamesCollection { get; private set; }

        /// <summary>
        /// Get object of type IPoolable from Spawn Pool or Instantiate from prefab
        /// </summary>
        /// <param name="prefab">Reference to prefab</param>
        /// <returns></returns>
        public static IPoolableThingy SpawnPoolableThingy(Transform prefab)
        {
            return SpawnPoolableThingy(prefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Get object of type IPoolable from Spawn Pool or Instantiate from prefab
        /// </summary>
        /// <param name="prefab">Reference to prefab</param>
        /// <param name="position">Instance position on spawn</param>
        /// <param name="rotation">Instance rotation on spawn</param>
        /// <returns>Returns reference to spawned IPoolable</returns>
        public static IPoolableThingy SpawnPoolableThingy(Transform prefab, Vector3 pos, Quaternion rot)
        {
            if(prefab == null)
            {
                Debug.LogError("Pool.SpawnTransformAsync(string prefabName, string path, Vector3 position, Quaternion rotation, Action<Transform> callback) prefab == null");
                return new IPoolableThingy();
            }

            var id = prefab.GetInstanceID();
            if (PoolWithPooledIPoolable.ContainsKey(id) && PoolWithPooledIPoolable[id].Count > 0)
            {
                return DequeueIThingy(id, pos, rot);
            }

            return InstantiateIThingy(prefab, pos, rot);
        }

        public static IPoolableThingy SpawnIThingy(string prefabName, string path, Vector3 position, Quaternion rotation)
        {
            int id;
            bool nameContained = IPoolableNamesCollection.ContainsKey(prefabName);
            if (nameContained)
            {
                id = IPoolableNamesCollection[prefabName];
                if (PoolWithPooledIPoolable.ContainsKey(id) && PoolWithPooledIPoolable[id].Count > 0)
                {
                    return DequeueIThingy(id, position, rotation);
                }
            }

            Transform prefab = Resources.Load<Transform>(path);
            if(prefab == null)
            {
                Debug.LogError("Pool.SpawnIThingy(string prefabName, string path, Vector3 position, Quaternion rotation) prefab == null. Path: " + path);
                return new IPoolableThingy();
            }

            if (!nameContained)
            {
                id = prefab.GetInstanceID();
                IPoolableNamesCollection.Add(prefabName, id);
            }

            return InstantiateIThingy(prefab, position, rotation);
        }

        public static void SpawnIThingyAsync(string prefabName, string path, Vector3 position, Quaternion rotation, Action<IPoolableThingy> callback)
        {
            if (IPoolableNamesCollection.ContainsKey(prefabName))
            {
                int id = IPoolableNamesCollection[prefabName];
                if (PoolWithPooledIPoolable.ContainsKey(id) && PoolWithPooledIPoolable[id].Count > 0)
                {
                    //#if(POOL_STATISTICS && UNITY_EDITOR)
                    //PoolStatistics.TrySpawnTransform(pooled);
                    //#endif

                    if(callback != null)
                        callback(DequeueIThingy(id, position, rotation));

                    return;
                }
                
            }

            PoolResourceLoader.StartResourceLoadAsync<Transform>(path, prefab =>
            {
                if(prefab == null)
                {
                    Debug.LogError("Pool.SpawnIThingyAsync(string prefabName, string path, Vector3 position, Quaternion rotation, Action<Transform> callback) prefab == null. Path: " + path);
                    return;
                }
                //#if(POOL_STATISTICS && UNITY_EDITOR)
                //PoolStatistics.CheckSpawnTransform(prefab);
                //#endif

                if (!IPoolableNamesCollection.ContainsKey(prefabName))
                {
                    int id = prefab.GetInstanceID();
                    IPoolableNamesCollection.Add(prefabName, id);
                }

                var pooled = InstantiateIThingy(prefab, position, rotation);

                if(callback != null)
                    callback(pooled);
            });
        }

        public static void DespawnIThingy(IPoolableThingy instance)
        {
            instance.poolable.transform.gameObject.SetActive(false);
            instance.poolable.OnDespawn();

            if(PoolWithPooledIPoolable.ContainsKey(instance.id))
            {
                Queue<IPoolable> instances = PoolWithPooledIPoolable[instance.id];
                instances.Enqueue(instance.poolable);
            }
            else
            {
                Queue<IPoolable> instances = new Queue<IPoolable>();
                instances.Enqueue(instance.poolable);
                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.CheckDespawnPoolable(instance);
                #endif
                PoolWithPooledIPoolable.Add(instance.id, instances);
            }
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TryDespawnIPoolable(instance);
            #endif
            instance.poolable.transform.parent = _container;
        }

        /// <summary>
        /// Push IPoolable into Spawn Pool after 'lifeTime' seconds
        /// </summary>
        /// <param name="instance">Reference to IPoolable instance</param>
        /// <param name="lifeTime">Time in seconds after wich IPoolable will be pushed back into Spawn Pool</param>
        public static void DespawnIThingy(IPoolableThingy instance, float lifeTime)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(instance.poolable == null)
            {
                Debug.LogError("Pool.DespawnIThingy(IPoolable instance, float lifeTime) instance == null");
                return;
            }
            #endif

            if(lifeTime <= 0)
                DespawnIThingy(instance);
            else
                _poolEntity.AddToTrash(instance, lifeTime);
        }

        private static IPoolableThingy DequeueIThingy(int id, Vector3 position, Quaternion rotation)
        {
            IPoolableThingy poolable;
            poolable.poolable = PoolWithPooledIPoolable[id].Dequeue();
            poolable.id = id;
            Transform pooledTransform = poolable.poolable.transform;
            pooledTransform.position = position;
            pooledTransform.rotation = rotation;
            poolable.poolable.transform.gameObject.SetActive(true);
            poolable.poolable.OnSpawn();
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TrySpawnIPoolable(poolable);
            #endif
            return poolable;
        }

        private static IPoolableThingy InstantiateIThingy(Transform prefab, Vector3 position, Quaternion rotation)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.CheckSpawnPoolable(prefab);
            #endif

            #if(POOL_STATISTICS && UNITY_EDITOR)
            if(prefab == null)
            {
                Debug.LogError("Pool.InstantiateTransform(Transform prefab, Vector3 position, Quaternion rotation) prefab == null");
                return new IPoolableThingy();
            }
            #endif
            
            IPoolableThingy pooledObject;
            pooledObject.id = prefab.GetInstanceID();

            Transform instance = Object.Instantiate(prefab, position, rotation) as Transform;
            pooledObject.poolable = instance.GetComponent<IPoolable>();
            pooledObject.poolable.OnSpawn();
        
            //instance.name = prefab.name;
            instance.parent = _container;
            
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TrySpawnIPoolable(pooledObject);
            #endif
            return pooledObject;
        }
    }
}
