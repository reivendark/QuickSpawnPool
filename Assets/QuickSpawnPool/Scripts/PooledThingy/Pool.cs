using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuickSpawnPool
{
    public static partial class Pool
    {
        public static Dictionary<int, Queue<Transform>> PoolWithPooledTransforms { get; private set; }
        public static Dictionary<string, int> TransformNamesCollection { get; private set; }

        public static PoolableThingy SpawnThingy(Transform prefab)
        {
            return SpawnThingy(prefab, Vector3.zero, Quaternion.identity);
        }

        public static PoolableThingy SpawnThingy(Transform prefab, Vector3 pos, Quaternion rot)
        {
            if(prefab == null)
            {
                Debug.LogError("Pool.SpawnTransformAsync(string prefabName, string path, Vector3 position, Quaternion rotation, Action<Transform> callback) prefab == null");
                return new PoolableThingy();
            }

            var id = prefab.GetInstanceID();
            if (PoolWithPooledTransforms.ContainsKey(id) && PoolWithPooledTransforms[id].Count > 0)
            {
                return DequeueThingy(id, pos, rot);
            }

            return InstantiateThingy(prefab, pos, rot);
        }

        public static PoolableThingy SpawnThingy(string prefabName, string path, Vector3 position, Quaternion rotation)
        {
            int id;
            bool nameContained = TransformNamesCollection.ContainsKey(prefabName);
            if (nameContained)
            {
                id = TransformNamesCollection[prefabName];
                if (PoolWithPooledTransforms.ContainsKey(id) && PoolWithPooledTransforms[id].Count > 0)
                {
                    return DequeueThingy(id, position, rotation);
                }
            }

            Transform prefab = Resources.Load<Transform>(path);
            if(prefab == null)
            {
                Debug.LogError("Pool.SpawnThingy(string prefabName, string path, Vector3 position, Quaternion rotation) prefab == null. Path: " + path);
                return new PoolableThingy();
            }

            if (!nameContained)
            {
                id = prefab.GetInstanceID();
                TransformNamesCollection.Add(prefabName, id);
            }

            return InstantiateThingy(prefab, position, rotation);
        }

        public static void SpawnThingyAsync(string prefabName, string path, Vector3 position, Quaternion rotation, Action<PoolableThingy> callback)
        {
            if (TransformNamesCollection.ContainsKey(prefabName))
            {
                int id = TransformNamesCollection[prefabName];
                if (PoolWithPooledTransforms.ContainsKey(id) && PoolWithPooledTransforms[id].Count > 0)
                {
                    //#if(POOL_STATISTICS && UNITY_EDITOR)
                    //PoolStatistics.TrySpawnTransform(pooled);
                    //#endif

                    if(callback != null)
                        callback(DequeueThingy(id, position, rotation));

                    return;
                }
                
            }

            PoolResourceLoader.StartResourceLoadAsync<Transform>(path, prefab =>
            {
                if(prefab == null)
                {
                    Debug.LogError("Pool.SpawnTransformAsync(string prefabName, string path, Vector3 position, Quaternion rotation, Action<Transform> callback) prefab == null. Path: " + path);
                    return;
                }
                //#if(POOL_STATISTICS && UNITY_EDITOR)
                //PoolStatistics.CheckSpawnTransform(prefab);
                //#endif

                if (!TransformNamesCollection.ContainsKey(prefabName))
                {
                    int id = prefab.GetInstanceID();
                    TransformNamesCollection.Add(prefabName, id);
                }

                var pooled = InstantiateThingy(prefab, position, rotation);

                if(callback != null)
                    callback(pooled);
            });
        }

        public static void DespawnThingy(PoolableThingy instance)
        {
            instance.t.gameObject.SetActive(false);

            if(PoolWithPooledTransforms.ContainsKey(instance.id))
            {
                Queue<Transform> instances = PoolWithPooledTransforms[instance.id];
                instances.Enqueue(instance.t);
            }
            else
            {
                Queue<Transform> instances = new Queue<Transform>();
                instances.Enqueue(instance.t);
                #if(POOL_STATISTICS && UNITY_EDITOR)
                PoolStatistics.CheckDespawnTransform(instance);
                #endif
                PoolWithPooledTransforms.Add(instance.id, instances);
            }
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TryDespawnTransform(instance);
            #endif
            instance.t.parent = _container;
        }

        /// <summary>
        /// Push IPoolable into Spawn Pool after 'lifeTime' seconds
        /// </summary>
        /// <param name="instance">Reference to IPoolable instance</param>
        /// <param name="lifeTime">Time in seconds after wich IPoolable will be pushed back into Spawn Pool</param>
        public static void DespawnThingy(PoolableThingy instance, float lifeTime)
        {
            if(instance.t == null)
            {
                Debug.LogError("Pool.DespawnPoolable(IPoolable instance, float lifeTime) instance == null");
                return;
            }

            if(lifeTime <= 0)
                DespawnThingy(instance);
            else
                _poolEntity.AddToTrash(instance, lifeTime);
        }

        private static PoolableThingy DequeueThingy(int id, Vector3 position, Quaternion rotation)
        {
            PoolableThingy poolable;
            poolable.t = PoolWithPooledTransforms[id].Dequeue();
            poolable.id = id;
            Transform pooledTransform = poolable.t;
            pooledTransform.position = position;
            pooledTransform.rotation = rotation;
            poolable.t.gameObject.SetActive(true);
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TrySpawnTransform(poolable);
            #endif
            return poolable;
        }

        private static PoolableThingy InstantiateThingy(Transform prefab, Vector3 position, Quaternion rotation)
        {
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.CheckSpawnTransform(prefab);
            #endif

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
        
            //pooledObject.t.name = prefab.name;
            pooledObject.t.parent = _container;
            #if(POOL_STATISTICS && UNITY_EDITOR)
            PoolStatistics.TrySpawnTransform(pooledObject);
            #endif
            return pooledObject;
        }
    }
}
