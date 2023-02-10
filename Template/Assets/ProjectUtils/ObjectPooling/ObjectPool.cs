using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUtils.ObjectPooling
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private List<GameObject> prefabsInitialPool;
        [SerializeField] private List<int> prefabsInitialCount;

        [SerializeField] private List<float> objectsLifeSpan;

        private List<GameObject> _prefabsPool;
        private GameObject _objectPool;
        private List<float> _activeGameObjectsTime;


        public static ObjectPool Instance;

        private void Start()
        {
            if (Instance != null && Instance != this) Destroy(this);
            Instance = this;

            _prefabsPool = new List<GameObject>();
            _objectPool = new GameObject("ObjectsPool");

            //For every object in the list, instantiate it the requested number of times
            for (int i = 0; i < prefabsInitialPool.Count; i++)
            {
                for (int j = 0; j < prefabsInitialCount[i]; j++)
                {
                    var temp = Instantiate(prefabsInitialPool[i], _objectPool.transform);
                    _prefabsPool.Add(temp);
                    temp.SetActive(false);
                }
            }

            //Initialize Time List
            _activeGameObjectsTime = new List<float>(_prefabsPool.Count);
            for (int i = 0; i < _prefabsPool.Count; ++i) _activeGameObjectsTime.Add(0f);

        }

        public GameObject InstantiateFromPool(GameObject prefab, Vector3 position, Quaternion rotation, bool disappearsWithTime)
        {
            //Searches for every object in the pool and see if it is an instance of the prefab and it is inactive
            for (var i = 0; i < _prefabsPool.Count; i++)
            {
                var prefabFromPool = _prefabsPool[i];
                if (prefabFromPool.name != $"{prefab.name}(Clone)" ||
                    prefabFromPool.gameObject.activeInHierarchy) continue;

                prefabFromPool.SetActive(true);
                _activeGameObjectsTime[i] = disappearsWithTime ? Time.time : float.MaxValue;
                prefabFromPool.transform.position = position;
                prefabFromPool.transform.localRotation = rotation;
                return prefabFromPool;
            }

            //If haven't found any valid object, create a new one
            var temp = Instantiate(prefab, _objectPool.transform);
            _prefabsPool.Add(temp);
            _activeGameObjectsTime.Add(disappearsWithTime ? Time.time : float.MaxValue);
            temp.transform.position = position;
            temp.transform.localRotation = rotation;
            return temp;
        }


        private void Update()
        {
            if (_activeGameObjectsTime.Count > 0)
            {
                for (int i = 0; i < _activeGameObjectsTime.Count; i++)
                {
                    if (_activeGameObjectsTime[i] > Time.time - GetObjectLifeSpan(_prefabsPool[i]) ||
                        !_prefabsPool[i].activeInHierarchy) continue;

                    _prefabsPool[i].SetActive(false);
                }
            }
        }

        private float GetObjectLifeSpan(GameObject prefab)
        {
            for (int i = 0; i < prefabsInitialPool.Count; i++)
            {
                if(prefab.name == $"{prefabsInitialPool[i].name}(Clone)")
                {
                    return objectsLifeSpan[i];
                }
            }

            Debug.Log("VAR");
            return -1;
        }
    }
}
