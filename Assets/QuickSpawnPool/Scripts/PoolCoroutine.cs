using UnityEngine;
using System.Collections;

namespace QuickSpawnPool
{
    public class PoolCoroutine : MonoBehaviour
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
                        _Instance = go.GetComponent<PoolCoroutine>();
                        if (_Instance == null)
                        {
                            _Instance = go.AddComponent<PoolCoroutine>();
                            DontDestroyOnLoad(go);
                        }
                    }
                    else
                    {
                        go = new GameObject("$CoroutinesHelper");
                        go.hideFlags = HideFlags.HideAndDontSave;
                        _Instance = go.AddComponent<PoolCoroutine>();
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
    }
}
