using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(KDTest))]
public class KDTestEd : Editor
{
    KDTest kdTest;
    Vector3 gizmoSize = Vector3.one * 0.2f;


    void OnEnable()
    {
        kdTest = target as KDTest;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10f);
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("BuildTree"))
                kdTest.BuildTree();

            if (GUILayout.Button("Restructure"))
                kdTest.RestructureTree();
        }

        if (GUILayout.Button("Clear"))
            kdTest.Clear();
    }
}
