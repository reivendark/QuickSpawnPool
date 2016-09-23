using System;
using QuickSpawnPool;
using UnityEngine;

namespace Assets.QuickSpawnPool.Examples
{
    public class IPoolableTest : MonoBehaviour, IPoolable
    {
        public new Transform transform;

        private void Awake()
        {
            transform = GetComponent<Transform>();
        }

        public void OnSpawn()
        {
            // print("spawn");
        }

        public void OnDespawn()
        {
            // print("despawn");
        }
    }
}
