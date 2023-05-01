using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUtils.Helpers
{
    public class CoroutineControllerNonStatic : MonoBehaviour
    {

        public static CoroutineControllerNonStatic Instance;
        Dictionary<string,IEnumerator> _routines = new Dictionary<string,IEnumerator>(100);

        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
        private void Awake()
        {
            if(Instance != null) Destroy(gameObject);
            else
            {
                Instance = this;
            }
        }

        public Coroutine StartC ( IEnumerator routine ) => StartCoroutine( routine );
        public Coroutine StartC ( IEnumerator routine , string id )
        {
            var coroutine = StartCoroutine( routine );
            if( !_routines.ContainsKey(id) ) _routines.Add( id , routine );
            else
            {
                StopCoroutine( _routines[id] );
                _routines[id] = routine;
            }
            return coroutine;
        }
        public void Stop ( IEnumerator routine ) => StopCoroutine( routine );
        public void Stop ( string id )
        {
            if( _routines.TryGetValue(id,out var routine) )
            {
                StopCoroutine( routine );
                _routines.Remove( id );
            }
            else Debug.LogWarning($"coroutine '{id}' not found");
        }
        public void StopAll () => StopAllCoroutines();
    
    }
}

