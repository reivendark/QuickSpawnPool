﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if(ADVANCED_COROUTINES)
using AdvancedCoroutines;
#endif

#if(UNITY_EDITOR)
using UnityEditor;
#endif

namespace QuickSpawnPool
{
    public class PoolEntity : MonoBehaviour
    {
        public PoolPrefab[] PreloadPrefabs;
        public PoolPath[] PreloadPaths;
    
        private List<IPoolableTimer> _poolableTrashList;
        private List<ThingyTimer> _thingyTrashList;
        private List<IThingyTimer> _iThingyTrashList;

        #if(ADVANCED_COROUTINES)
        private Routine deleteByTimeCoroutine;
        #else
        private Coroutine deleteByTimeCoroutine;
        #endif

        protected void Awake()
        {
            _poolableTrashList = new List<IPoolableTimer>();
            _thingyTrashList = new List<ThingyTimer>();
            _iThingyTrashList = new List<IThingyTimer>();
            
            #if(ADVANCED_COROUTINES)
            deleteByTimeCoroutine = CoroutineManager.StartCoroutine(DeleteByTimeCoroutine(), gameObject);
            #else
            deleteByTimeCoroutine = StartCoroutine(DeleteByTimeCoroutine());
            #endif

            if(!Pool.IsInitialized)
                Pool.Reset();
        }

        protected void OnDestroy()
        {
            if(Pool.IsInitialized)
                Pool.Destroy();
        }
    
        /// <summary>
        /// Reset Entitiy state
        /// </summary>
        public void Reset()
        {
            if(deleteByTimeCoroutine != null)
            {
                #if(ADVANCED_COROUTINES)
                CoroutineManager.StopCoroutine(deleteByTimeCoroutine);
                #else
                PoolCoroutine.StopCoroutine(deleteByTimeCoroutine);
                #endif

                deleteByTimeCoroutine = null;
            }

            _poolableTrashList = null;
            _thingyTrashList = null;
            _iThingyTrashList = null;
        }
    
        /// <summary>
        /// Load all prefabs from pre-spawn lists
        /// </summary>
        public void LoadAll()
        {
            for(int iPrefab = 0; PreloadPrefabs != null && iPrefab < PreloadPrefabs.Length; iPrefab++)
            {
                Pool.PreSpawn(PreloadPrefabs[iPrefab].Prefab, PreloadPrefabs[iPrefab].Count);
            }
    
            for(int iPath = 0; PreloadPaths != null && iPath < PreloadPaths.Length; iPath++)
            {
                Pool.PreSpawn(PreloadPaths[iPath].Path, PreloadPaths[iPath].Count);
            }
        }

        public void AddToTrash(Pool.PoolableThingy thingy, float timer)
        {
            ThingyTimer thingyTimer;
            thingyTimer.thingy = thingy;
            thingyTimer.timer = timer;
            _thingyTrashList.Add(thingyTimer);
        }

        public void AddToTrash(Pool.IPoolableThingy thingy, float timer)
        {
            IThingyTimer thingyTimer;
            thingyTimer.thingy = thingy;
            thingyTimer.timer = timer;
            _iThingyTrashList.Add(thingyTimer);
        }

        /// <summary>
        /// Add IPoolable to the list of timer-tracked objects
        /// </summary>
        /// <param name="poolable">IPoolable instance</param>
        /// <param name="timer">Time in seconds after wich Transform will be pushed back into Spawn Pool</param>
        public void AddToTrash(IPoolable poolable, float timer)
        {
            IPoolableTimer poolableTimer;
            poolableTimer.poolable = poolable;
            poolableTimer.time = timer;
            _poolableTrashList.Add(poolableTimer);
        }

        #if(UNITY_EDITOR)

        /// <summary>
        /// Load pre-spawn objects data from xml. Works only in editor
        /// </summary>
        /// <returns></returns>
        public bool LoadPrespawnDataFromXML()
        {
            bool result = false;

            if(PreloadPrefabs == null) PreloadPrefabs = new PoolPrefab[0];
            if(PreloadPaths == null) PreloadPaths = new PoolPath[0];

            var levelName = EditorApplication.currentScene;
            string fullPath = XMLUtility.GetPath() + XMLUtility.GetFileName(levelName);

            Dictionary<string, int> transformsInstancesData = new Dictionary<string, int>();
            Dictionary<string, int> poolablesInstancesData = new Dictionary<string, int>();

            XMLUtility.ReadXML(fullPath, ref transformsInstancesData, ref poolablesInstancesData);

            foreach (var data in transformsInstancesData)
            {
                result = true;
                ProcessFullPath(data.Key, data.Value);
            }

            foreach (var data in poolablesInstancesData)
            {
                result = true;
                ProcessFullPath(data.Key, data.Value);
            }

            return result;
        }

        private void ProcessFullPath(string fullPath, int count)
        {
            Transform prefab = AssetDatabase.LoadAssetAtPath<Transform>(fullPath);
            if (prefab == null)
            {
                Debug.LogError("[PoolEntity]: Cannot find any prefab by path '" + fullPath + "'");
                return;
            }

            if (fullPath.Contains("Resources/"))
            {
                int index = fullPath.LastIndexOf("Resources/");
                string processedPath = fullPath.Remove(0, index + 10);
                processedPath = processedPath.Replace(".prefab", "");

                for(int iPath = 0; iPath < PreloadPaths.Length; iPath++)
                {
                    if(PreloadPaths[iPath].Path == processedPath)
                    {
                        PoolPath pp = PreloadPaths[iPath];
                        pp.Count = count;
                        PreloadPaths[iPath] = pp;

                        return;
                    }
                }

                List<PoolPath> temp = PreloadPaths.ToList();
                PoolPath poolPath;
                poolPath.Path = processedPath;
                poolPath.Count = count;
                temp.Add(poolPath);
                PreloadPaths = temp.ToArray();
            }
            else
            {
                for(int iPrefab = 0; iPrefab < PreloadPrefabs.Length; iPrefab++)
                {
                    if(PreloadPrefabs[iPrefab].Prefab == prefab)
                    {
                        PoolPrefab pp = PreloadPrefabs[iPrefab];
                        pp.Count = count;
                        PreloadPrefabs[iPrefab] = pp;

                        return;
                    }
                }

                List<PoolPrefab> temp = PreloadPrefabs.ToList();
                PoolPrefab poolPrefab;
                poolPrefab.Prefab = prefab;
                poolPrefab.Count = count;
                temp.Add(poolPrefab);
                PreloadPrefabs = temp.ToArray();
            }
        }
        #endif

        protected IEnumerator DeleteByTimeCoroutine()
        {
            do
            {
                #if(ADVANCED_COROUTINES)
                yield return new Wait(0.1f);
                #else
                yield return new WaitForSeconds(0.1f);
                #endif

                for (int i = 0; i < _iThingyTrashList.Count; i++)
                {
                    IThingyTimer timer = _iThingyTrashList[i];
                    timer.timer -= 0.1f;

                    if(timer.timer <= 0)
                    {
                        Pool.DespawnIThingy(timer.thingy);
                        _thingyTrashList.RemoveAt(i);
                        i--;
                        continue;
                    }

                    _iThingyTrashList[i] = timer;
                }

                for (int iThingy = 0; iThingy < _thingyTrashList.Count; iThingy++)
                {
                    ThingyTimer timer = _thingyTrashList[iThingy];
                    timer.timer -= 0.1f;

                    if(timer.timer <= 0)
                    {
                        Pool.DespawnThingy(timer.thingy);
                        _thingyTrashList.RemoveAt(iThingy);
                        iThingy--;
                        continue;
                    }

                    _thingyTrashList[iThingy] = timer;
                }
            } while(true);
        }

        [Serializable]
        public struct PoolPrefab
        {
            public Transform Prefab;
            public int Count;
    
            public PoolPrefab(Transform prefab, int count)
            {
                Prefab = prefab;
                Count = count;
            }
        }
    
        [Serializable]
        public struct PoolPath
        {
            public string Path;
            public int Count;
    
            public PoolPath(string path, int count)
            {
                Path = path;
                Count = count;
            }
        }

        public struct ThingyTimer
        {
            public float timer;
            public Pool.PoolableThingy thingy;
        }

        public struct TransformTimer
        {
            public float time;
            public Transform transform;
        }
    
        public struct IPoolableTimer
        {
            public float time;
            public IPoolable poolable;
        }

        public struct IThingyTimer
        {
            public float timer;
            public Pool.IPoolableThingy thingy;
        }
    }
}