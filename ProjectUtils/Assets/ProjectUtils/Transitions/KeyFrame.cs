using System;
using UnityEngine;
using UnityEngine.UI;

namespace Transitions
{
    [Serializable]
    public struct KeyFrame
    {
        public bool Equals(KeyFrame other)
        {
            return position.Equals(other.position) && rotation.Equals(other.rotation) && scale.Equals(other.scale) && color.Equals(other.color);
        }

        public override bool Equals(object obj)
        {
            return obj is KeyFrame other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(position, rotation, scale, color);
        }

        [Header("Key Frame Parameters")]
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Color color;

        public void SetValues(Transform transform, SpriteRenderer spriteRenderer, Image image)
        {
            position = transform.position;
            rotation = transform.eulerAngles;
            scale = transform.localScale;

            if (spriteRenderer != null) color = spriteRenderer.color;
            else if (image != null) color = image.color;
        }
        
        public static KeyFrame operator -(KeyFrame a)
        {
            return new KeyFrame
                { position = -a.position, rotation = -a.rotation, scale = -a.scale, color =  -1*a.color };
        }

        public static KeyFrame operator +(KeyFrame a, KeyFrame b)
        {
            return new KeyFrame
                { position = a.position+b.position, rotation = a.rotation+b.rotation, scale = a.scale+b.scale, color = a.color + b.color };  
        }
        
        public static KeyFrame operator -(KeyFrame a, KeyFrame b)
        {
            return a + (-b);
        }
        
        public static KeyFrame operator *(KeyFrame a, float b)
        { 
            return new KeyFrame
                { position = a.position*b, rotation = a.rotation*b, scale = a.scale*b, color = a.color*b};
        }
        
        public static bool operator ==(KeyFrame a, KeyFrame b)
        { 
            return a.position == b.position && a.rotation == b.rotation && a.scale == b.scale && a.color == b.color;
        }

        public static bool operator !=(KeyFrame a, KeyFrame b)
        {
            return !(a == b);
        }
    }
}
