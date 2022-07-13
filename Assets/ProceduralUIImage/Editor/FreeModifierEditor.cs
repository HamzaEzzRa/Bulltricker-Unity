using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI.ProceduralImage;

[CustomEditor(typeof(FreeModifier), true)]
[CanEditMultipleObjects]
public class FreeModifierEditor : Editor
{
    protected SerializedProperty radiusX;
    protected SerializedProperty radiusY;
    protected SerializedProperty radiusZ;
    protected SerializedProperty radiusW;

    protected void OnEnable()
    {
        radiusX = serializedObject.FindProperty("radius.x");
        radiusY = serializedObject.FindProperty("radius.y");
        radiusZ = serializedObject.FindProperty("radius.z");
        radiusW = serializedObject.FindProperty("radius.w");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUILayout.Space(8);
        RadiusGUI();
        serializedObject.ApplyModifiedProperties();
    }

    protected void RadiusGUI()
    {
        EditorGUILayout.PropertyField(radiusX, new GUIContent("Upper Left"));
        GUILayout.Space(8);
        EditorGUILayout.PropertyField(radiusY, new GUIContent("Upper Right"));
        GUILayout.Space(8);
        EditorGUILayout.PropertyField(radiusW, new GUIContent("Lower Left"));
        GUILayout.Space(8);
        EditorGUILayout.PropertyField(radiusZ, new GUIContent("Lower Right"));
        GUILayout.Space(8);
    }
}
