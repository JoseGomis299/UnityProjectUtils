using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUtils.ObjectPooling
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private List<PoolObject> initialObjectPool;
        private List<PoolObject> _objectPool;
        
        private GameObject _objectPoolParent;
        
        [Serializable]
        struct PoolObject
        {
            public GameObject prefab;
            public int quantity;
            public float lifeTime;
            [HideInInspector] public float activeTime;
        }

        public static ObjectPool Instance;

        private void Start()
        {
            if (Instance != null && Instance != this) Destroy(this);
            Instance = this;

            _objectPool = new List<PoolObject>();
            _objectPoolParent = new GameObject("ObjectsPool");

            //For every object in the list, instantiate it the requested number of times
            for (int i = 0; i < initialObjectPool.Count; i++)
            {
                for (int j = 0; j < initialObjectPool[i].quantity; j++)
                {
                    var temp = Instantiate(initialObjectPool[i].prefab, _objectPoolParent.transform);
                    _objectPool.Add(new PoolObject
                    {
                        prefab = temp,  
                        quantity = 1,
                        lifeTime = initialObjectPool[i].lifeTime 
                    });
                    temp.SetActive(false);
                }
            }
        }

        public GameObject InstantiateFromPool(GameObject prefab, Vector3 position, Quaternion rotation, bool disappearsWithTime)
        {
            //Searches for every object in the pool and see if it is an instance of the prefab and it is inactive
            for (var i = 0; i < _objectPool.Count; i++)
            {
                var prefabFromPool = _objectPool[i];
                if (prefabFromPool.prefab.name != $"{prefab.name}(Clone)" ||
                    prefabFromPool.prefab.activeInHierarchy) continue;

                prefabFromPool.prefab.SetActive(true);
                prefabFromPool.activeTime = disappearsWithTime ? Time.time : float.MaxValue;
                prefabFromPool.prefab.transform.position = position;
                prefabFromPool.prefab.transform.localRotation = rotation;
                _objectPool[i] = prefabFromPool;
                return prefabFromPool.prefab;
            }

            //If haven't found any valid object, create a new one
            var temp = Instantiate(prefab, _objectPoolParent.transform);
            PoolObject poolObject = new PoolObject
            {
                prefab = temp,
                quantity = 1,
                lifeTime = GetObjectLifeSpan(temp),
                activeTime = disappearsWithTime ? Time.time : float.MaxValue
            };
            _objectPool.Add(poolObject);
            temp.transform.position = position;
            temp.transform.localRotation = rotation;
            return temp;
        }
        
        public GameObject InstantiateFromPoolIndex(int prefabIndex, Vector3 position, Quaternion rotation, bool disappearsWithTime)
        {
            return InstantiateFromPool(initialObjectPool[prefabIndex].prefab, position, rotation, disappearsWithTime);
        }


        private void Update()
        {
            if (_objectPool.Count <= 0) return;
            
            foreach (var poolObject in _objectPool)
            {
                if (!poolObject.prefab.activeInHierarchy
                    || poolObject.activeTime > Time.time - poolObject.lifeTime) continue;

                poolObject.prefab.SetActive(false);
            }
        }

        private float GetObjectLifeSpan(GameObject prefab)
        {
            for (int i = 0; i < initialObjectPool.Count; i++)
            {
                if(prefab.name == $"{initialObjectPool[i].prefab.name}(Clone)")
                {
                    return initialObjectPool[i].lifeTime;
                }
            }
            return -1;
        }
    }
}
