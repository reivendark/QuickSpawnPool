using UnityEngine;
using QuickSpawnPool;
using System.Collections;
using System.Collections.Generic;

public class TestScript : MonoBehaviour
{
    private const float BORDER = 20f;
    private const int AMOUNT_OF_INSTANCES = 5000;

    public Transform prefab;
    public GameObject prefab2;

    private List<Transform> _instances;
    private List<GameObject> _instances2; 
    private List<Pool.PoolableThingy> _instances3; 

    private Coroutine _createFiftyPrefabsEverySecond;

    private Coroutine _createFiftyPrefabsEverySecond2;

    private Coroutine _createFiftyPrefabsEverySecond3;

    private void Awake()
    {
        Pool.Reset();

        if(Pool.IsInitialized)
            Debug.Log("Pool is initialized");
        else
            Debug.LogError("Pool is not initialized");

        _instances = new List<Transform>();
        _instances2 = new List<GameObject>();
        _instances3 = new List<Pool.PoolableThingy>();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 150, 50), "Start simulation"))
        {
            _createFiftyPrefabsEverySecond = StartCoroutine(CreateFiftyPrefabsEverySecond());
        }

        if (_createFiftyPrefabsEverySecond != null && GUI.Button(new Rect(160, 0, 150, 50), "Stop simulation"))
        {
            StopCoroutine(_createFiftyPrefabsEverySecond);
            _createFiftyPrefabsEverySecond = null;

            for (int i = 0; i < _instances.Count; i++)
            {
                Pool.DespawnTransform(_instances[i]);
            }
            _instances.Clear();
        }

        ////////////////////////////////////////////////////////////////////////////////////
        if (GUI.Button(new Rect(0, 60, 150, 50), "Start simulation"))
        {
            _createFiftyPrefabsEverySecond2 = StartCoroutine(CreateFiftyPrefabsEverySecond2());
        }

        if (_createFiftyPrefabsEverySecond2 != null && GUI.Button(new Rect(160, 60, 150, 50), "Stop simulation"))
        {
            StopCoroutine(_createFiftyPrefabsEverySecond2);
            _createFiftyPrefabsEverySecond2 = null;

            for (int i = 0; i < _instances2.Count; i++)
            {
                Destroy(_instances2[i]);
            }
            _instances2.Clear();
        }

        ////////////////////////////////////////////////////////////////////////////////////
        if (GUI.Button(new Rect(0, 120, 150, 50), "Start simulation"))
        {
            _createFiftyPrefabsEverySecond3 = StartCoroutine(CreateFiftyPrefabsEverySecond3());
        }

        if (_createFiftyPrefabsEverySecond3 != null && GUI.Button(new Rect(160, 120, 150, 50), "Stop simulation"))
        {
            StopCoroutine(_createFiftyPrefabsEverySecond3);
            _createFiftyPrefabsEverySecond3 = null;

            for (int i = 0; i < _instances3.Count; i++)
            {
                Pool.DespawnThingy(_instances3[i]);
            }
            _instances3.Clear();
        }
    }

    private IEnumerator CreateFiftyPrefabsEverySecond()
    {
        while (true)
        {
            for (int i = 0; i < AMOUNT_OF_INSTANCES; i++)
            {
                var randomPos = 
                    new Vector3(
                        Random.Range(-BORDER, BORDER), 
                        Random.Range(-BORDER, BORDER), 
                        Random.Range(-BORDER, BORDER));

                var instance = Pool.SpawnTransform(prefab, randomPos, Quaternion.identity);
                _instances.Add(instance);
            }

            yield return new WaitForSeconds(1f);

            for (int i = 0; i < _instances.Count; i++)
            {
                Pool.DespawnTransform(_instances[i]);
            }
            
            _instances.Clear();
        }
    }

    private IEnumerator CreateFiftyPrefabsEverySecond3()
    {
        while (true)
        {
            for (int i = 0; i < AMOUNT_OF_INSTANCES; i++)
            {
                var randomPos = 
                    new Vector3(
                        Random.Range(-BORDER, BORDER), 
                        Random.Range(-BORDER, BORDER), 
                        Random.Range(-BORDER, BORDER));

                var instance = Pool.SpawnThingy(prefab, randomPos, Quaternion.identity);
                _instances3.Add(instance);
            }

            yield return new WaitForSeconds(1f);

            for (int i = 0; i < _instances3.Count; i++)
            {
                Pool.DespawnThingy(_instances3[i]);
            }
            
            _instances3.Clear();
        }
    }

    private IEnumerator CreateFiftyPrefabsEverySecond2()
    {
        while (true)
        {
            for (int i = 0; i < AMOUNT_OF_INSTANCES; i++)
            {
                var randomPos = 
                    new Vector3(
                        Random.Range(-BORDER, BORDER), 
                        Random.Range(-BORDER, BORDER), 
                        Random.Range(-BORDER, BORDER));

                GameObject instance = (GameObject)Instantiate(prefab2, randomPos, Quaternion.identity);
                _instances2.Add(instance);
            }

            yield return new WaitForSeconds(1f);

            for (int i = 0; i < _instances2.Count; i++)
            {
                Destroy(_instances2[i]);
            }

            _instances2.Clear();
        }
    }
}
