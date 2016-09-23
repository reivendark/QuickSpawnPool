using System;
using System.Collections;
using QuickSpawnPool;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuickSpawnPool
{
    public class PoolResourceLoader
    {
        public static void StartResourceLoadAsync(string assetPath, Action<Object> action)
        {
            PoolCoroutine.Instance.StartCoroutine(WaitForResourceLoadCoroutine(Resources.LoadAsync(assetPath), action));
        }

        public static void StartResourceLoadAsync<T>(string assetPath, Action<T> action) where T : Object
        {
            PoolCoroutine.Instance.StartCoroutine(WaitForResourceLoadCoroutine(Resources.LoadAsync<T>(assetPath), action));
        }

        private static IEnumerator WaitForResourceLoadCoroutine(ResourceRequest resourceRequest, Action<Object> action)
        {
            while(!resourceRequest.isDone) yield return 0;
            action(resourceRequest.asset);
        }

        private static IEnumerator WaitForResourceLoadCoroutine<T>(ResourceRequest resourceRequest, Action<T> action) where T : Object
        {
            while(!resourceRequest.isDone) yield return 0;
            action((T)Convert.ChangeType(resourceRequest.asset, typeof(T)));
        }

        public struct ResourceAction
        {
            public string path;
            public Action<Object> action;

            public ResourceAction(string path, Action<Object> action)
            {
                this.path = path;
                this.action = action;
            }
        }
    }
}