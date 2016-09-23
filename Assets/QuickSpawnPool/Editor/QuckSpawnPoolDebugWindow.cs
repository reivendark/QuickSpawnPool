using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSpawnPool
{
    public class QuckSpawnPoolDebugWindow : EditorWindow
    {
        private static string LevelName;
        private static Dictionary<int, PoolStatistics.PoolElementData> TransformStorage;
        private static Dictionary<int, PoolStatistics.PoolElementData> PoolableStorage;

        private Vector2 transformScrollViewPos;
        private Vector2 poolableScrollViewPos;

        private Mode mode = Mode.Both;

        enum Mode
        {
            TransformOnly,
            IPoolableOnly,
            Both
        }

        [MenuItem("Window/QuickSpawnPool/Debug")]
        public static void 
        ShowWindow()
        {
            GetWindow<QuckSpawnPoolDebugWindow>();
        }

        public void 
        OnGUI()
        {
            DrawRetreiveDataButton();
            DrawEraseButton();

            ET.HorizontalBoxBegin();
            {
                ET.Button("Transforms", ()=>
                {
                    switch (mode)
                    {
                        case Mode.TransformOnly:
                            mode = Mode.Both;
                            break;
                        case Mode.IPoolableOnly:
                        case Mode.Both:
                            mode = Mode.TransformOnly;
                            break;
                    }
                }, null, (mode == Mode.TransformOnly ? Color.green : Color.white));

                ET.Button("IPoolables", () => 
                {
                    switch (mode)
                    {
                        case Mode.IPoolableOnly:
                            mode = Mode.Both;
                            break;
                        case Mode.TransformOnly:
                        case Mode.Both:
                            mode = Mode.IPoolableOnly;
                            break;
                    }
                }, null, (mode == Mode.IPoolableOnly ? Color.green : Color.white));
            }
            ET.HorizontalBoxEnd();

            switch (mode)
            {
                case Mode.TransformOnly:
                    ShowTransformsResult(TransformStorage);
                    break;
                case Mode.IPoolableOnly:
                    ShowIPoolablesResult(PoolableStorage);
                    break;
                case Mode.Both:
                    ShowTransformsResult(TransformStorage);
                    ShowIPoolablesResult(PoolableStorage);
                    break;
            }
            ShowSaveToXMLButton();

            if (GUI.changed)
                EditorUtility.SetDirty(this);
        }

        private void DrawRetreiveDataButton()
        {
            ET.Button("Retreive the data", () =>
            {
#if(!POOL_STATISTICS)
                ET.DrawMessageOk("Unable to retrieve the data. POOL_STATISTICS preprocessor directive is not active");
                return;
#endif
                if (!Application.isPlaying)
                {
                    ET.DrawMessageOk("Unable to retrieve the data. The application must be running");
                    return;
                }
                if (!Pool.IsInitialized)
                {
                    ET.DrawMessageOk("Pool is not initialized. Call Pool.Reset() to initialize the pool");
                    return;
                }

                LevelName = EditorApplication.currentScene;
                
#if(POOL_STATISTICS && UNITY_EDITOR)
                TransformStorage = Pool.PoolStatistics.TransformStorage;
                PoolableStorage = Pool.PoolStatistics.PoolableStorage;
#endif
            }, null, Color.green);
        }

        private void DrawEraseButton()
        {
            if (TransformStorage == null && PoolableStorage == null) return;
            ET.Button("Erase", () =>
            {
                TransformStorage = null;
                PoolableStorage = null;
            }, null, Color.red);
        }

        private void ShowTransformsResult(Dictionary<int, PoolStatistics.PoolElementData> transformStorage)
        {
            if (transformStorage == null) return;
            ET.VerticalBoxBegin();
            ET.LabelField("Transforms", null, null, Color.green, "Prefabs spawned as Transforms");
            int index = 0;
            DrawHeader();
            transformScrollViewPos = EditorGUILayout.BeginScrollView(transformScrollViewPos, false, false);
            foreach (var data in transformStorage)
            {
                DrawElement(data, index++);
            }
            EditorGUILayout.EndScrollView();
            ET.VerticalBoxEnd();
        }

        private void ShowIPoolablesResult(Dictionary<int, PoolStatistics.PoolElementData> poolableStorage)
        {
            if (poolableStorage == null) return;
            ET.VerticalBoxBegin();
            ET.LabelField("IPoolables", null, null, Color.green, "Prefabs spawned as IPoolable");
            int index = 0;
            DrawHeader();
            poolableScrollViewPos = EditorGUILayout.BeginScrollView(poolableScrollViewPos, false, false);
            foreach (var data in poolableStorage)
            {
                DrawElement(data, index++);
            }
            EditorGUILayout.EndScrollView();
            ET.VerticalBoxEnd();
        }

        private void ShowSaveToXMLButton()
        {
            if (TransformStorage != null && PoolableStorage != null && TransformStorage.Count > 0 && PoolableStorage.Count > 0)
            {
                ET.Button("Create XML", () => { XMLUtility.SavePoolInstancesStatistics(LevelName, TransformStorage, PoolableStorage); }, null, Color.green);
            }
        }

        private void DrawHeader()
        {
            GUI.color = Color.green;
            ET.HorizontalBoxBegin();
            GUI.color = Color.white;
            ET.DrawString("", "No", 0, 30);
            ET.DrawString("", "Element name");
            ET.DrawString("", "SPAWNED", 0, 80);
            ET.DrawString("", "DESPAWNED", 0, 80);
            ET.DrawString("", "INSTANCES", 0, 80);
            GUI.color = Color.green;
            ET.HorizontalBoxEnd();
            GUI.color = Color.white;
        }

        private void DrawElement(KeyValuePair<int, PoolStatistics.PoolElementData> data, int index)
        {
            ET.HorizontalBoxBegin();
            string elementName = data.Value.name;
            int spawnedAmount = data.Value.spawnCount;
            int despawnedAmount = data.Value.despawnCount;
            int instances = data.Value.instances;

            Color spawnedColor;
            if (spawnedAmount == 0)
                spawnedColor = Color.red;
            else if (spawnedAmount > despawnedAmount)
                spawnedColor = Color.yellow;
            else
                spawnedColor = Color.white;

            Color despawnedColor;
            if (despawnedAmount == 0)
                despawnedColor = Color.red;
            else if (despawnedAmount > spawnedAmount)
                despawnedColor = Color.yellow;
            else
                despawnedColor = Color.white;

            ET.DrawString("", index.ToString(), 0, 30);
            ET.DrawString("", elementName);
            ET.DrawInt("", spawnedAmount, 0, 80, "", spawnedColor);
            ET.DrawInt("", despawnedAmount, 0, 80, "", despawnedColor);
            ET.DrawInt("", instances, 0, 80, "");
            ET.HorizontalBoxEnd();
        }
    }
}