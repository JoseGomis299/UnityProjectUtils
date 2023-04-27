using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Transitions.Editor
{
    [CustomPropertyDrawer(typeof(Transition))]
    public class TransitionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
        
            EditorGUI.PropertyField(position, property, label, true);

            position.y += EditorGUI.GetPropertyHeight(property, true);
            position.height = EditorGUIUtility.singleLineHeight;
        
            var transitionProp = property.serializedObject.FindProperty(property.propertyPath);
            TransitionPlayer transitionPlayer = (TransitionPlayer)property.serializedObject.targetObject;
        
            if (GUI.Button(position, "Add KeyFrame"))
            {
                if (transitionProp != null)
                {
                    Transition transition = new Transition();
                    transition.animationCurve = transitionProp.FindPropertyRelative("animationCurve").animationCurveValue;
                    transition.duration = transitionProp.FindPropertyRelative("duration").floatValue;
                    transition.playOnStart = transitionProp.FindPropertyRelative("playOnStart").boolValue;
                    transition.keyFrames = new List<KeyFrame>();
                    transition.id = transitionProp.FindPropertyRelative("id").intValue;
                    SerializedProperty keyFramesProp = transitionProp.FindPropertyRelative("keyFrames");
                    for (int i = 0; i < keyFramesProp.arraySize; i++)
                    {
                        SerializedProperty keyFrameProp = keyFramesProp.GetArrayElementAtIndex(i);
                        KeyFrame keyFrame = new KeyFrame();
                        keyFrame.position = keyFrameProp.FindPropertyRelative("position").vector3Value;
                        keyFrame.rotation = keyFrameProp.FindPropertyRelative("rotation").vector3Value;
                        keyFrame.scale = keyFrameProp.FindPropertyRelative("scale").vector3Value;
                        keyFrame.color = keyFrameProp.FindPropertyRelative("color").colorValue;
                        transition.keyFrames.Add(keyFrame);
                    }

                    if (transitionPlayer != null)
                    {
                        transitionPlayer.AddKeyFrame(transition);
                    }
                }
            }
        
            position.y += EditorGUIUtility.singleLineHeight;

            if (GUI.Button(position, "Play!"))
            {
                if (transitionProp != null)
                {
                    Transition transition = new Transition();
                    transition.animationCurve = transitionProp.FindPropertyRelative("animationCurve").animationCurveValue;
                    transition.duration = transitionProp.FindPropertyRelative("duration").floatValue;
                    transition.playOnStart = transitionProp.FindPropertyRelative("playOnStart").boolValue;
                    transition.id = transitionProp.FindPropertyRelative("id").intValue;
                    transition.keyFrames = new List<KeyFrame>();
                    SerializedProperty keyFramesProp = transitionProp.FindPropertyRelative("keyFrames");
                    for (int i = 0; i < keyFramesProp.arraySize; i++)
                    {
                        SerializedProperty keyFrameProp = keyFramesProp.GetArrayElementAtIndex(i);
                        KeyFrame keyFrame = new KeyFrame();
                        keyFrame.position = keyFrameProp.FindPropertyRelative("position").vector3Value;
                        keyFrame.rotation = keyFrameProp.FindPropertyRelative("rotation").vector3Value;
                        keyFrame.scale = keyFrameProp.FindPropertyRelative("scale").vector3Value;
                        keyFrame.color = keyFrameProp.FindPropertyRelative("color").colorValue;
                        transition.keyFrames.Add(keyFrame);
                    }

                    if (transitionPlayer != null)
                    {
                        transitionPlayer.PlayTransitionAsync(transition);
                    }
                }
            }
        
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("keyFrames"), false);
            return height;
        }
    }
}
