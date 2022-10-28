using UnityEngine;
using UnityEditor;
using CodeCreatePlay.AutoInspector;


[CustomEditor(typeof(AutoInspectorDemo))]
public class AutoInspectorDemoEd : Editor
{
    [System.NonSerialized] 
    private GUIStyle mainHeadingStyle = null;

    [System.NonSerialized]
    private GUIStyle boldLabelStyle = null;

    private GUIStyle MainHeadingStyle
    {
        get
        {
            if (mainHeadingStyle == null)
            {
                mainHeadingStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 15,
                };
               mainHeadingStyle.normal.textColor = new Color(1f, 0.9f, 0.6f, 1f);
            }

            return mainHeadingStyle;
        }
    }

    private GUIStyle BoldLabelStyle
    {
        get
        {
            if (boldLabelStyle == null)
            {
                boldLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 12,
                };
                boldLabelStyle.normal.textColor = Color.white;
            }

            return boldLabelStyle;
        }
    }

    AutoInspectorDemo demo = null;
    bool layout = false;


    private void OnEnable()
    {
        demo = target as AutoInspectorDemo;
    }

    public override void OnInspectorGUI()
    {
        using (new GUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("AutoInspector for Unity", MainHeadingStyle);

            string btn_text = layout == true ? "Hide Inspector" : "Create Inspector";
            Color bgColor = GUI.backgroundColor;
            Color btnColor = layout == true ? Color.white : Color.green;
            GUI.backgroundColor = btnColor;

            if (GUILayout.Button(btn_text))
                layout = !layout;

            GUI.backgroundColor = bgColor;
        }

        if (EditorGUILayout.LinkButton("Github link for latest updates"))
        { }    


        GUILayout.Space(3);


        if (layout)
            Layout();
    }

    void Layout()
    {
        demo.SettingsAutoInspector.Build();

        // add some space and a label.
        EditorGUILayout.Space(25f);
        EditorGUILayout.LabelField("Objects List", BoldLabelStyle);


        // arg 1 = list of gameObjects
        // arg 2 = method to call when select button is pressed
        // arg 3 = method to call when a new gameObject is added
        // arg 4 = method to call when gameObject is removed
        // arg 5 = method to get index of selected item
        // arg 6 = when a gameObject is replaced with another

        AutoInspector.DrawGameObjectList<Transform>(
            demo.gameObjects,
            demo.OnSelect,
            demo.OnAddItem,
            demo.OnItemRemove,
            demo.GetSelectedItemIdx,
            demo.OnChangeDetect);

        EditorGUILayout.Space(5f);
        EditorGUILayout.LabelField("Thumbnails", BoldLabelStyle);

        AutoInspector.DrawThumbnailsInspector(
            demo.gameObjects,
            demo.OnSelect,
            demo.GetSelectedItemIdx);
    }
}
