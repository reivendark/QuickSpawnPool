using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace QuickSpawnPool
{
    [CustomEditor(typeof(PoolEntity))]
    public class PoolEntity_Editor : UnityEditor.Editor
    {
        private PoolEntity _script;

        private void 
        Awake()
        {
            _script = (PoolEntity)target;
        }

        public override void 
        OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Prefabs");
            if(_script.PreloadPrefabs != null)
            {
                EditorGUILayout.BeginVertical("box");
                for(int iPrefab = 0; iPrefab < _script.PreloadPrefabs.Length; iPrefab++)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    ET.DrawObject("Prefab", ref _script.PreloadPrefabs[iPrefab].Prefab, false, null, 50);
                    ET.DrawInt("Count", ref _script.PreloadPrefabs[iPrefab].Count, 45, 75, "Количество элементов для предзагрузки");
                    ET.Button("X", ()=>
                    {
                        List<PoolEntity.PoolPrefab> temp = _script.PreloadPrefabs.ToList();
                        temp.RemoveAt(iPrefab);
                        _script.PreloadPrefabs = temp.ToArray();
                        iPrefab--;
                    }, null, Color.red, 30);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            GUI.color = Color.green;
            if(GUILayout.Button("Add prefab"))
            {
                if(_script.PreloadPrefabs == null)
                {
                    _script.PreloadPrefabs = new PoolEntity.PoolPrefab[1]
                    {
                        new PoolEntity.PoolPrefab(null, 0)
                    };
                }
                else
                {
                    List<PoolEntity.PoolPrefab> temp = _script.PreloadPrefabs.ToList();
                    temp.Add(new PoolEntity.PoolPrefab(null, 0));
                    _script.PreloadPrefabs = temp.ToArray();
                }
            }
            GUI.color = Color.white;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Paths");
            if(_script.PreloadPaths != null)
            {
                EditorGUILayout.BeginVertical("box");
                for(int iPath = 0; iPath < _script.PreloadPaths.Length; iPath++)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    ET.DrawString("Path", ref _script.PreloadPaths[iPath].Path, 40, null, "Path to prefab. Example: Root/Path1/Path2/PrefabName");
                    ET.DrawInt("Count", ref _script.PreloadPaths[iPath].Count, 45, 75, "Количество элементов для предзагрузки");
                    ET.Button("X", ()=>
                    {
                        List<PoolEntity.PoolPath> temp = _script.PreloadPaths.ToList();
                        temp.RemoveAt(iPath);
                        _script.PreloadPaths = temp.ToArray();
                        iPath--;
                    }, null, Color.red, 30);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            GUI.color = Color.green;
            if(GUILayout.Button("Add prefab"))
            {
                if(_script.PreloadPaths == null)
                {
                    _script.PreloadPaths = new PoolEntity.PoolPath[1]
                    {
                        new PoolEntity.PoolPath("", 0)
                    };
                }
                else
                {
                    List<PoolEntity.PoolPath> temp = _script.PreloadPaths.ToList();
                    temp.Add(new PoolEntity.PoolPath("", 0));
                    _script.PreloadPaths = temp.ToArray();
                }
            }
            GUI.color = Color.white;
            EditorGUILayout.EndVertical();

            ET.Button("Load pre-spawn data from XML", ()=> { _script.LoadPrespawnDataFromXML(); }, null, Color.yellow);

            if(GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}