using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectUtils.Helpers
{
    public static class Helpers 
    {
        private static Camera _camera;
        public static Camera Camera
        {
            get
            {
                if(_camera == null) _camera = Camera.main;
                return _camera;
            }
        }
    
        private static PointerEventData _eventDataCurrentPosition;
        private static List<RaycastResult> _results;

        public static bool PointerIsOverUi()
        {
            _eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            _results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_eventDataCurrentPosition, _results);
            return _results.Count > 0;
        }

        public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera, out var result);
            return result;
        }

        public static void DeleteChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// <para>Scales the object to a target scale in a determined time</para>
        /// <param name="targetScale">The target scale</param>
        /// <param name="time">The duration in seconds of the scaling effect</param>
        /// </summary>
        public static void DoScale(this Transform transform, Vector3 targetScale, float time)  =>  CoroutineController.Start(DoScaleEnumerator(transform, targetScale, time));
        private static IEnumerator DoScaleEnumerator(Transform transform, Vector3 targetScale, float time)
        {
            float timer = 0;
            Vector3 initialScale = transform.localScale;
            Vector3 scaleDelta = targetScale - initialScale;

            while (timer < time)
            {
                timer += Time.deltaTime;
                transform.localScale = initialScale + scaleDelta * (timer/time);
                yield return null;
            }

            transform.localScale = targetScale;
        }
    
        /// <summary>
        /// <para>Moves the object, making a shake movement with a certain magnitude in a determined time</para>
        /// <param name="magnitude">The magnitude of the movement</param>
        /// <param name="time">The duration in seconds of the shaking effect</param>
        /// <param name="moveZ">Determines if the object moves in the z axis</param>
        /// </summary>
        public static void DoShake(this Transform transform, float magnitude, float time, bool moveZ = false) => CoroutineController.Start(DoShakeEnumerator(transform, magnitude, time, moveZ));
        private static IEnumerator DoShakeEnumerator(Transform transform, float magnitude, float time, bool moveZ)
        {
            float timer = 0;
            Vector3 initialPosition = transform.position;
            Vector3 newPosition = initialPosition;

            while (timer < time)
            {
                newPosition.x = initialPosition.x + Random.value * magnitude;
                newPosition.y = initialPosition.y + Random.value * magnitude;
                if(moveZ) newPosition.z = initialPosition.z + Random.value * magnitude;
                transform.position = newPosition;

                timer += Time.deltaTime;
                yield return null;
            }

            transform.position = initialPosition;
        }

        private static Dictionary<int, bool> _blinking;

        public static bool IsBlinking(this SpriteRenderer spriteRenderer)
        {
            _blinking ??= new Dictionary<int, bool>();
        
            int id = spriteRenderer.GetHashCode();
            if (!_blinking.ContainsKey(id)) _blinking.Add(id , false);
        
            return _blinking[id];
        } 
    
        /// <summary>
        /// <para>Makes a blinking effect to the object</para>
        /// <param name="duration">The duration in seconds of the blinking effect</param>
        /// <param name="ticks">The number of times you want the object to blink</param>
        /// <param name="targetColor">The color to change in every blink, normally transparent or white</param>
        /// </summary>
        public static void DoBlink(this SpriteRenderer spriteRenderer, float duration, int ticks, Color targetColor) => CoroutineController.Start(DoBlinkEnumerator(spriteRenderer, duration, ticks, targetColor));
        private static IEnumerator DoBlinkEnumerator(SpriteRenderer spriteRenderer, float duration, int ticks, Color targetColor)
        {
            if (ticks <= 0) yield break;
        
            _blinking ??= new Dictionary<int, bool>();
            int id = spriteRenderer.GetHashCode();
            if(!_blinking.ContainsKey(id)) _blinking.Add( id , true);
            else _blinking[id] = true;

            float timer = 0;
            Color initialColor = spriteRenderer.color;
            WaitForSeconds waitForSeconds = new WaitForSeconds(duration / ticks/2);
        
            while (timer<duration)
            {
                spriteRenderer.color = targetColor;
                yield return waitForSeconds;
                spriteRenderer.color = initialColor;
                yield return waitForSeconds;
                timer += duration / ticks;
            }

            spriteRenderer.color = initialColor;
            _blinking[id] = false;
        }
    
        /// <summary>
        /// <para>Makes a blinking effect to the object</para>
        /// <param name="duration">The duration in seconds of the blinking effect</param>
        /// <param name="ticks">The number of times you want the object to blink</param>
        /// <param name="targetColor">The color to change in every blink, normally transparent or white</param>
        /// </summary>
        public static void DoBlink(this Image image, float duration, int ticks, Color targetColor) => CoroutineController.Start(DoBlinkEnumerator(image, duration, ticks, targetColor));
        private static IEnumerator DoBlinkEnumerator(Image image, float duration, int ticks, Color targetColor)
        {
            if (ticks <= 0) yield break;
        
            _blinking ??= new Dictionary<int, bool>();
            int id = image.GetHashCode();
            if(!_blinking.ContainsKey(id)) _blinking.Add( id , true);
            else _blinking[id] = true;

            float timer = 0;
            Color initialColor = image.color;
            WaitForSeconds waitForSeconds = new WaitForSeconds(duration / ticks/2);
        
            while (timer<duration)
            {
                image.color = targetColor;
                yield return waitForSeconds;
                image.color = initialColor;
                yield return waitForSeconds;
                timer += duration / ticks;
            }

            image.color = initialColor;
            _blinking[id] = false;
        }
    }
}
