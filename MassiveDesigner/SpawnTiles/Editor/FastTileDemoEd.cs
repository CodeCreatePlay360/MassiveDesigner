using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(FastTilesDemo))]
public class FastTileDemoEd : Editor
{

    FastTilesDemo fastTilesDemo = null;

    void OnEnable()
    {
        fastTilesDemo = target as FastTilesDemo;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("InitFastTiles"))
        {
            fastTilesDemo.Init();
        }

        if (GUILayout.Button("Remove"))
        {
            fastTilesDemo.RemoveOP();
        }

        if (GUILayout.Button("Save"))
        {
            // fastTilesDemo.Save();
        }
    }
}
