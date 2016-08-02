using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tanktastic.ObjectScripts
{
    public class ResourceLoadHelper
    {
        public static void StartResourceQueueAsyncLoad(ResourceAction[] resourceActions, Action action, bool keepOrder)
        {
            if(resourceActions == null) return;
            if(keepOrder)
            {
                CoroutinesHelper.Instance.StartCoroutine(WaitForResourcesLoadInOrderCoroutine(resourceActions, action));
            }
            else
            {
                CoroutinesHelper.Instance.StartCoroutine(WaitForResourcesLoadCoroutine(resourceActions, action));
            }
        }

        public static void StartResourceLoadAsync(ResourceAction resourceAction)
        {
            StartResourceLoadAsync(resourceAction.path, resourceAction.action);
        }

        public static void StartResourceLoadAsync(string assetPath, Action<Object> action)
        {
#if(UNITY_EDITOR)
            if(Application.isPlaying)
#endif
            CoroutinesHelper.Instance.StartCoroutine(
                WaitForResourceLoadCoroutine(
                    Resources.LoadAsync(assetPath),
                    action
#if(DEBUG)
                    , assetPath
#endif
                    ));
        }

        public static void StartResourceLoadAsync<T>(string assetPath, Action<T> action) where T : Object
        {
            #if(UNITY_EDITOR)
            if (Application.isPlaying)
            #endif
                CoroutinesHelper.Instance.StartCoroutine(
                    WaitForResourceLoadCoroutine(
                        Resources.LoadAsync<T>(assetPath), 
                        action
                        #if(DEBUG)
                        , assetPath
                        #endif
                        ));
        }

        private static IEnumerator WaitForResourcesLoadCoroutine(ResourceAction[] resourceActions, Action action)
        {
            int finishedActionsCurrentCount = 0;
            int expectedAndtionsCount = resourceActions.Length;

            for(int i = 0; i < resourceActions.Length; ++i)
            {
                resourceActions[i].action += (o) => { finishedActionsCurrentCount++; };
                StartResourceLoadAsync(resourceActions[i].path, resourceActions[i].action);
            }

            while(finishedActionsCurrentCount != expectedAndtionsCount) yield return 0;
            action.Invoke();
        }

        private static IEnumerator WaitForResourcesLoadInOrderCoroutine(ResourceAction[] resourceActions, Action action)
        {
            for(int i = 0; i < resourceActions.Length; ++i)
            {
                ResourceRequest resourceRequest = Resources.LoadAsync(resourceActions[i].path);
                while(!resourceRequest.isDone) yield return 0;
                resourceActions[i].action.Invoke(resourceRequest.asset);
            }
            action.Invoke();
        }

        private static IEnumerator WaitForResourceLoadCoroutine(ResourceRequest resourceRequest, 
                                                                Action<Object> action
                                                                #if(DEBUG)
                                                                , string path
                                                                #endif
                                                                )
        {
            //Debug.Log("Resource [" + path + "] Loading...");
            while(!resourceRequest.isDone) yield return 0;
            action(resourceRequest.asset);
            //Debug.Log("Resource [" + path + "] Loading...DONE");
        }

        private static IEnumerator WaitForResourceLoadCoroutine<T>(ResourceRequest resourceRequest, 
                                                                   Action<T> action
                                                                   #if(DEBUG)
                                                                   , string path
                                                                   #endif
                                                                   ) where T : Object
        {
            //Debug.Log("Resource [" + path + "] Loading...");
            while(!resourceRequest.isDone) yield return 0;
            action((T)Convert.ChangeType(resourceRequest.asset, typeof(T)));
            //Debug.Log("Resource [" + path + "] Loading...DONE");
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