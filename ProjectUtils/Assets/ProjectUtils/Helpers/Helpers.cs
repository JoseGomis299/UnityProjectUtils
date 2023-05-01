using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

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

        /// <summary>Returns the angle in degrees between this position and the mouse position</summary>
        public static float GetAngleToPointer(this Vector3 position)
        {
            Vector3 attackDirection = Input.mousePosition;
            attackDirection = Camera.ScreenToWorldPoint(attackDirection);
            attackDirection.z = position.z;
            attackDirection = (attackDirection-position).normalized;

            float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            while (angle<0) angle += 360;

            return angle;
        }
    }
}
