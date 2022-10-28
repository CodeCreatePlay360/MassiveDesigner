using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.AutoInspector;


public class AutoInspectorDemo : MonoBehaviour
{
    [System.Serializable]
    public class Settings
    {
        [EditorFieldAttr(ControlType.boldLabel, name: "Settings")]
        [EditorFieldAttr(ControlType.floatField, name: "floatField")]
        public float floatField = 1.0f;

        [EditorFieldAttr(ControlType.intField, "intField")]
        public int intField = 1;

        [EditorFieldAttr(ControlType.textControl, "stringField")]
        public string stringField = "String Value";

        [EditorFieldAttr(ControlType.boolField, "boolField")]
        public bool boolField = false;

        [EditorFieldAttr(ControlType.color, "colorField")]
        public Color colorField = Color.red;

        [IntSliderAttr(ControlType.intSlider, "intSlider", 0, 10)]
        public int intSlider = 5;

        [FloatSliderAttr(ControlType.floatSlider, "floatSlider", 0f, 1f)]
        public float floatSlider = 0.5f;

        [EditorFieldAttr(ControlType.gameObject, "gameObject")]
        public GameObject gameObject = null;

        [EditorFieldAttr(ControlType.texture2d, "texture2d")]
        public Texture2D texture2d = null;

        [EditorFieldAttr(ControlType.space, "space")] // Use controlType.space to add space between two elements
        [System.NonSerialized] public int space = 20; // ControlType.Space should be NonSerialized


        // horizontal layout
        [EditorFieldAttr(ControlType.boolField, "check_0", layoutHorizontal:1)] // begin a horizontal layout by passing layoutHorizontal = 1
        public bool check_0 = false;

        [EditorFieldAttr(ControlType.boolField, "check_1")]
        public bool check_1 = false;

        [EditorFieldAttr(ControlType.boolField, "check_2")]
        public bool check_2 = false;

        [EditorFieldAttr(ControlType.boolField, "check_3", layoutHorizontal: -1)] // end horizontal layout by passing layoutHorizontal = -1
        public bool check_3 = false;
    }

    [SerializeField] 
    private Settings settings = new Settings();
    private AutoInspector settingsAutoInspector = null;

    public AutoInspector SettingsAutoInspector
    {
        get
        {
            if(settingsAutoInspector == null)
            {
                System.Object obj = settings;
                settingsAutoInspector = new AutoInspector(typeof(Settings), ref obj);
            }

            return settingsAutoInspector;
        }
    }

    public List<GameObject> gameObjects = new List<GameObject>();

    private int selGameObjectIndex = -1; // the index of selected game object


    public void OnAddItem(GameObject go)
    {
        gameObjects.Add(go);
        if (gameObjects.Count > 1)
            selGameObjectIndex = gameObjects.Count - 2;
    }

    public void OnItemRemove(int index)
    {
        gameObjects.RemoveAt(index);
    }

    public void OnChangeDetect(int index, GameObject go)
    {
        gameObjects[index] = go;
        OnSelect(index);
    }

    public void OnSelect(int index)
    {
        selGameObjectIndex = index;
    }

    public int GetSelectedItemIdx()
    {
        return selGameObjectIndex;
    }
}
