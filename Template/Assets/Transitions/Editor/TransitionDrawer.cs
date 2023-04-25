using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Transitioner.Transition))]
public class TransitionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PropertyField(position, property, label, true);

        position.y += EditorGUI.GetPropertyHeight(property, true)+10;
        position.height = EditorGUIUtility.singleLineHeight;

        SerializedProperty transitionProp = property.serializedObject.FindProperty(property.propertyPath);
        Transitioner transitioner = (Transitioner)property.serializedObject.targetObject;
        
        if (GUI.Button(position, "Add KeyFrame"))
        {
            if (transitionProp != null)
            {
                Transitioner.Transition transition = new Transitioner.Transition();
                transition.animationCurve = transitionProp.FindPropertyRelative("animationCurve").animationCurveValue;
                transition.duration = transitionProp.FindPropertyRelative("duration").floatValue;
                transition.playOnStart = transitionProp.FindPropertyRelative("playOnStart").boolValue;
                transition.keyFrames = new List<Transitioner.KeyFrame>();
                SerializedProperty keyFramesProp = transitionProp.FindPropertyRelative("keyFrames");
                for (int i = 0; i < keyFramesProp.arraySize; i++)
                {
                    SerializedProperty keyFrameProp = keyFramesProp.GetArrayElementAtIndex(i);
                    Transitioner.KeyFrame keyFrame = new Transitioner.KeyFrame();
                    keyFrame.position = keyFrameProp.FindPropertyRelative("position").vector3Value;
                    keyFrame.rotation = keyFrameProp.FindPropertyRelative("rotation").vector3Value;
                    keyFrame.scale = keyFrameProp.FindPropertyRelative("scale").vector3Value;
                    keyFrame.color = keyFrameProp.FindPropertyRelative("color").colorValue;
                    transition.keyFrames.Add(keyFrame);
                }

                if (transitioner != null)
                {
                    transitioner.AddKeyFrame(transition);
                }
            }
        }
        
        position.y += EditorGUIUtility.singleLineHeight+10;

        if (GUI.Button(position, "Play!"))
        {
            if (transitionProp != null)
            {
                Transitioner.Transition transition = new Transitioner.Transition();
                transition.animationCurve = transitionProp.FindPropertyRelative("animationCurve").animationCurveValue;
                transition.duration = transitionProp.FindPropertyRelative("duration").floatValue;
                transition.playOnStart = transitionProp.FindPropertyRelative("playOnStart").boolValue;
                transition.keyFrames = new List<Transitioner.KeyFrame>();
                SerializedProperty keyFramesProp = transitionProp.FindPropertyRelative("keyFrames");
                for (int i = 0; i < keyFramesProp.arraySize; i++)
                {
                    SerializedProperty keyFrameProp = keyFramesProp.GetArrayElementAtIndex(i);
                    Transitioner.KeyFrame keyFrame = new Transitioner.KeyFrame();
                    keyFrame.position = keyFrameProp.FindPropertyRelative("position").vector3Value;
                    keyFrame.rotation = keyFrameProp.FindPropertyRelative("rotation").vector3Value;
                    keyFrame.scale = keyFrameProp.FindPropertyRelative("scale").vector3Value;
                    keyFrame.color = keyFrameProp.FindPropertyRelative("color").colorValue;
                    transition.keyFrames.Add(keyFrame);
                }

                if (transitioner != null)
                {
                    transitioner.PlayTransition(transition);
                }
            }
        }
        
        
       
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight;
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("keyFrames"), true);
        return height;
    }
}
