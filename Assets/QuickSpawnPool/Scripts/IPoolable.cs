using UnityEngine;

namespace QuickSpawnPool
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
        Transform transform {get;}
    }
}
