using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RiverManager))]
public class RiverEditor : Editor
{
    private RiverManager manager;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if(GUILayout.Button("Create All River"))
        {
            manager.GenerateAllRivers();
        }
    }

    private void OnEnable()
    {
        manager = (RiverManager)target;
    }
}
