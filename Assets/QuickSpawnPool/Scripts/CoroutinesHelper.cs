using System.Collections;
using UnityEngine;

public class CoroutinesHelper : MonoBehaviour
{
    private static MonoBehaviour _Instance;
    public static MonoBehaviour Instance
    {
        get
        {
            if(_Instance == null)
            {
                GameObject go = GameObject.Find("$CoroutinesHelper");
                if(go != null)
                {
                    _Instance = go.GetComponent<CoroutinesHelper>();
                    if (_Instance == null)
                    {
                        _Instance = go.AddComponent<CoroutinesHelper>();
                        DontDestroyOnLoad(go);
                    }
                }
                else
                {
                    go = new GameObject("$CoroutinesHelper");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    _Instance = go.AddComponent<CoroutinesHelper>();
                    DontDestroyOnLoad(go);
                }
            }
            return _Instance;
        }
    }

    public new static void StartCoroutine(IEnumerator enumerator)
    {
        _Instance.StartCoroutine(enumerator);
    }

    public new static void StopCoroutine(Coroutine coroutine)
    {
        _Instance.StopCoroutine(coroutine);
    }

    //private class EditorCoroutine
    //{
    //    public readonly IEnumerator enumerator;
    //    public readonly Action<object> callback;
    //    private DateTime waitDateTime;
    //    public bool isWaiting { get { return waitDateTime > DateTime.UtcNow; } }
    //    public bool wasWaiting = false;
    //
    //    public EditorCoroutine(IEnumerator enumerator, Action<object> callback)
    //    {
    //        this.enumerator = enumerator;
    //        this.callback = callback;
    //        this.waitDateTime = DateTime.UtcNow;
    //    }
    //
    //    public void ProcessWaitForSeconds()
    //    {
    //        wasWaiting = true;
    //        waitDateTime = DateTime.UtcNow.AddSeconds((float)typeof(WaitForSeconds)
    //                                                         .GetField("m_Seconds", BindingFlags.Instance
    //                                                                              | BindingFlags.Public
    //                                                                              | BindingFlags.NonPublic
    //                                                                              | BindingFlags.Static)
    //                                                         .GetValue(enumerator.Current));
    //    }
    //}
    //
    //private static MonoBehaviour _Instance;
    //public static MonoBehaviour Instance
    //{
    //    get
    //    {
    //        if(_Instance == null)
    //        {
    //            GameObject go = GameObject.Find("$CoroutinesHelper");
    //            if(go != null)
    //            {
    //                _Instance = go.GetComponent<CoroutinesHelper>();
    //            }
    //            else
    //            {
    //                go = new GameObject("$CoroutinesHelper");
    //                go.hideFlags = HideFlags.HideAndDontSave;
    //                _Instance = go.AddComponent<CoroutinesHelper>();
    //                DontDestroyOnLoad(go);
    //            }
    //        }
    //        return _Instance;
    //    }
    //}
    //
    //private static readonly List<EditorCoroutine> runningEditorCoroutines = new List<EditorCoroutine>();
    //
    //public static Coroutine StartCoroutine(IEnumerator enumerator, Action<object> callback = null)
    //{
    //    #if(UNITY_EDITOR)
    //    if (Application.isPlaying)
    //    #endif
    //        return Instance.StartCoroutine(enumerator);
    //
    //    #if(UNITY_EDITOR)
    //    if (runningEditorCoroutines.Count == 0)
    //        EditorApplication.update += Update;
    //
    //    runningEditorCoroutines.Add(new EditorCoroutine(enumerator, callback));
    //    #endif
    //
    //    return null;
    //}
    //
    //
    //
    //#if(UNITY_EDITOR)
    //private static void Update()
    //{
    //    for (int i = 0; i < runningEditorCoroutines.Count; i++)
    //    {
    //        EditorCoroutine editorCoroutine = runningEditorCoroutines[i];
    //        if (editorCoroutine.enumerator.Current is WaitForSeconds)
    //        {
    //            if (editorCoroutine.isWaiting)
    //                continue;
    //
    //            if (!editorCoroutine.wasWaiting)
    //            {
    //                editorCoroutine.ProcessWaitForSeconds();
    //                continue;
    //            }
    //            
    //            editorCoroutine.wasWaiting = false;
    //        }
    //
    //        if (editorCoroutine.enumerator.Current is WWW && !(editorCoroutine.enumerator.Current as WWW).isDone)
    //            continue;
    //
    //        if (!editorCoroutine.enumerator.MoveNext())
    //        {
    //            runningEditorCoroutines.Remove(editorCoroutine);
    //            if (runningEditorCoroutines.Count == 0)
    //                EditorApplication.update -= Update;
    //            i--;
    //            if (editorCoroutine.callback != null)
    //                editorCoroutine.callback(editorCoroutine.enumerator.Current);
    //            continue;
    //        }
    //    }
    //}
    //#endif
    //
    //public new static void StopCoroutine(IEnumerator enumerator) { Instance.StopCoroutine(enumerator); }
    //public new static void StopCoroutine(Coroutine coroutine) { Instance.StopCoroutine(coroutine); }
}
