using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace CodeCreatePlay
{
    namespace AutoInspector
    {
        public static class AutoInspectorCntrlBuilder
        {
            static FieldInfo info;

            public static void CreateControl(EditorFieldAttr attr, Type t, ref System.Object obj)
            {
                if (attr.CtrlType == ControlType.intField)
                {
                    FieldInfo info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        int intVal = (int)info.GetValue(obj);
                        intVal = EditorGUILayout.IntField(char.ToUpper(attr.Name[0]) + attr.Name[1..], intVal);
                        info.SetValue(obj, intVal);
                    }

                }

                else if (attr.CtrlType == ControlType.floatField)
                {
                    GUILayout.BeginHorizontal();

                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        float floatVal = (float)info.GetValue(obj);
                        floatVal = EditorGUILayout.FloatField(char.ToUpper(attr.Name[0]) + attr.Name[1..], floatVal);
                        info.SetValue(obj, floatVal);
                    }

                    GUILayout.EndHorizontal();
                }

                else if (attr.CtrlType == ControlType.boolField)
                {
                    GUILayout.BeginHorizontal();
                    info = t.GetField(attr.Name);
                    bool boolField = false;
                    if (info != null)
                    {
                        boolField = (bool)info.GetValue(obj);
                        boolField = EditorGUILayout.Toggle("", boolField, GUILayout.Width(17));
                        GUILayout.Label(char.ToUpper(attr.Name[0]) + attr.Name[1..]);
                        info.SetValue(obj, boolField);
                    }
                    GUILayout.EndHorizontal();

                    if (boolField && attr.Message != "")
                        EditorGUILayout.HelpBox(attr.Message, MessageType.Info);
                }

                else if (attr.CtrlType == ControlType.vector2)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2);
                    GUILayout.Label(char.ToUpper(attr.Name[0]) + attr.Name[1..], GUILayout.MinWidth(125));
                    GUILayout.Space(-19);
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        Vector2 vector2 = (Vector2)info.GetValue(obj);
                        vector2 = EditorGUILayout.Vector2Field("", (Vector2)vector2);
                        info.SetValue(obj, (Vector2)vector2);
                    }
                    GUILayout.EndHorizontal();
                }

                else if (attr.CtrlType == ControlType.vector3)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2);
                    GUILayout.Label(char.ToUpper(attr.Name[0]) + attr.Name[1..], GUILayout.MinWidth(125));
                    GUILayout.Space(-19);
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        Vector3 vector3 = (Vector3)info.GetValue(obj);
                        vector3 = EditorGUILayout.Vector3Field("", (Vector3)vector3);
                        info.SetValue(obj, (Vector3)vector3);
                    }
                    GUILayout.EndHorizontal();
                }

                else if (attr.CtrlType == ControlType.boldLabel)
                {
                    GUILayout.Space(5f);
                    GUILayout.Label(char.ToUpper(attr.Name[0]) + attr.Name[1..], AutoInspector.BoldLabel_Style);
                    GUILayout.Space(3f);
                }

                else if (attr.CtrlType == ControlType.text)
                {
                    GUILayout.BeginHorizontal();
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        GUILayout.Label(char.ToUpper(attr.Name[0]) + attr.Name[1..]);
                        GUILayout.Space(-250f);
                        GUILayout.Label(info.GetValue(obj).ToString());
                    }
                    GUILayout.EndHorizontal();
                }

                else if (attr.CtrlType == ControlType.textControl)
                {
                    GUILayout.BeginHorizontal();

                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        GUILayout.Space(2);
                        GUILayout.Label(char.ToUpper(attr.Name[0]) + attr.Name[1..]);
                        string stringCtrl = (string)info.GetValue(obj);
                        stringCtrl = GUILayout.TextField(stringCtrl, GUILayout.MinWidth(160f));
                        info.SetValue(obj, stringCtrl);
                    }

                    GUILayout.EndHorizontal();
                }

                else if (attr.CtrlType == ControlType.tag)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        // GUILayout.Label(attr.Name);
                        string stringCtrl = (string)info.GetValue(obj);
                        stringCtrl = EditorGUILayout.TagField(char.ToUpper(attr.Name[0]) + attr.Name[1..], stringCtrl);
                        info.SetValue(obj, stringCtrl);
                    }
                }

                else if (attr.CtrlType == ControlType.space)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        int intVal = (int)info.GetValue(obj);
                        GUILayout.Space(intVal);
                    }
                }

                else if (attr.CtrlType == ControlType.layerField)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        LayerMask mask = (LayerMask)info.GetValue(obj);
                        mask = EditorGUILayout.LayerField(char.ToUpper(attr.Name[0]) + attr.Name[1..], (LayerMask)mask);
                        info.SetValue(obj, (LayerMask)mask);
                    }
                }

                else if (attr.CtrlType == ControlType.gameObject)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        GameObject go;
                        try
                        {
                            go = (GameObject)info.GetValue(obj);
                        }
                        catch (InvalidCastException)
                        {
                            info.SetValue(obj, null);
                            go = (GameObject)info.GetValue(obj);
                        }

                        go = (UnityEngine.GameObject)EditorGUILayout.ObjectField(char.ToUpper(attr.Name[0]) + attr.Name[1..], (UnityEngine.GameObject)go,
                            typeof(GameObject), allowSceneObjects: false);

                        info.SetValue(obj, (UnityEngine.GameObject)go);
                    }
                }

                else if (attr.CtrlType == ControlType.sceneGameObj)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        GameObject go;
                        try
                        {
                            go = (GameObject)info.GetValue(obj);
                        }
                        catch (InvalidCastException)
                        {
                            info.SetValue(obj, null);
                            go = (GameObject)info.GetValue(obj);
                        }

                        go = (UnityEngine.GameObject)EditorGUILayout.ObjectField(char.ToUpper(attr.Name[0]) + attr.Name[1..], (UnityEngine.GameObject)go,
                            typeof(GameObject), allowSceneObjects: true);

                        info.SetValue(obj, (UnityEngine.GameObject)go);
                    }
                }

                else if (attr.CtrlType == ControlType.texture2d)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        Texture2D texture = (Texture2D)info.GetValue(obj);
                        texture = (UnityEngine.Texture2D)EditorGUILayout.ObjectField(char.ToUpper(attr.Name[0]) + attr.Name[1..], (UnityEngine.Texture2D)texture,
                            typeof(Texture2D), allowSceneObjects: false);
                        info.SetValue(obj, (UnityEngine.Texture2D)texture);
                    }
                }

                else if (attr.CtrlType == ControlType.unityTerrainField)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        Terrain terrain = (Terrain)info.GetValue(obj);
                        terrain = (Terrain)EditorGUILayout.ObjectField(char.ToUpper(attr.Name[0]) + attr.Name[1..], (Terrain)terrain,
                            typeof(Terrain), allowSceneObjects: true);
                        info.SetValue(obj, (Terrain)terrain);
                    }
                }

                else if (attr.CtrlType == ControlType.intSlider)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        int intVal = (int)info.GetValue(obj);

                        // upcast EditorFieldControl to it's inherited FloatSliderAttr
                        IntSliderAttr upcastAttr = attr as IntSliderAttr;

                        intVal = EditorGUILayout.IntSlider(char.ToUpper(attr.Name[0]) + attr.Name[1..], intVal, upcastAttr.minVal, upcastAttr.maxVal);
                        info.SetValue(obj, intVal);
                    }
                }

                else if (attr.CtrlType == ControlType.floatSlider)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        float floatVal = (float)info.GetValue(obj);

                        // upcast EditorFieldControl to it's inherited FloatSliderAttr
                        FloatSliderAttr upcastAttr = attr as FloatSliderAttr;

                        floatVal = EditorGUILayout.Slider(char.ToUpper(attr.Name[0]) + attr.Name[1..], floatVal, upcastAttr.minVal, upcastAttr.maxVal);
                        info.SetValue(obj, floatVal);
                    }
                }

                else if (attr.CtrlType == ControlType.color)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        Color c = (Color)info.GetValue(obj);
                        c = EditorGUILayout.ColorField(char.ToUpper(attr.Name[0]) + attr.Name[1..], c);
                        info.SetValue(obj, (Color)c);
                    }
                }

                else if (attr.CtrlType == ControlType.transformsList)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        List<Transform> transforms = (List<Transform>)info.GetValue(obj);

                        if (transforms.Count == 0)
                        { transforms.Add(null); }

                        for (int i = 0; i < transforms.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(2);

                            transforms[i] = (Transform)EditorGUILayout.ObjectField(
                            transforms[i], typeof(Transform), allowSceneObjects: false);

                            Color oldColor = GUI.backgroundColor;
                            GUI.backgroundColor = Color.red;

                            // create a button to remove this transform.
                            if (transforms[i] != null && GUILayout.Button("-"))
                            {
                                transforms.RemoveAt(i); // on remove
                            }

                            GUI.backgroundColor = oldColor;
                            GUILayout.EndHorizontal();
                        }

                        // add a new null PaintMesh if count is > 0.
                        if (transforms.Count > 0 && transforms[^1] != null)
                            transforms.Add(null);

                        info.SetValue(obj, (List<Transform>)transforms);
                    }
                }

                else if (attr.CtrlType == ControlType.LocationCategory)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        LocationTool.LocationCategory ctrl = (LocationTool.LocationCategory)info.GetValue(obj);
                        ctrl = (LocationTool.LocationCategory)EditorGUILayout.EnumPopup(char.ToUpper(attr.Name[0]) + attr.Name[1..], ctrl);
                        info.SetValue(obj, ctrl);
                    }
                }

                else if (attr.CtrlType == ControlType.ItemsType)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        MassiveDesinger.PaintMesh.ItemType ctrl = (MassiveDesinger.PaintMesh.ItemType)info.GetValue(obj);
                        ctrl = (MassiveDesinger.PaintMesh.ItemType)EditorGUILayout.EnumPopup(char.ToUpper(attr.Name[0]) + attr.Name[1..], ctrl);
                        info.SetValue(obj, ctrl);
                    }
                }

                else if(attr.CtrlType == ControlType.BrushPaintMode)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        MassiveDesinger.Tools.PaintMode ctrl = (MassiveDesinger.Tools.PaintMode)info.GetValue(obj);
                        ctrl = (MassiveDesinger.Tools.PaintMode)EditorGUILayout.EnumPopup(char.ToUpper(attr.Name[0]) + attr.Name[1..], ctrl);
                        info.SetValue(obj, ctrl);
                    }
                }

                else if (attr.CtrlType == ControlType.AreaShape)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        MassiveDesinger.Tools.AreaShape ctrl = (MassiveDesinger.Tools.AreaShape)info.GetValue(obj);
                        ctrl = (MassiveDesinger.Tools.AreaShape)EditorGUILayout.EnumPopup(char.ToUpper(attr.Name[0]) + attr.Name[1..], ctrl);
                        info.SetValue(obj, ctrl);
                    }
                }

                else if (attr.CtrlType == ControlType.PaintMode)
                {
                    info = t.GetField(attr.Name);
                    if (info != null)
                    {
                        MassiveDesinger.Tools.GrassPainter.PaintMode ctrl = (MassiveDesinger.Tools.GrassPainter.PaintMode)info.GetValue(obj);
                        ctrl = (MassiveDesinger.Tools.GrassPainter.PaintMode)EditorGUILayout.EnumPopup(char.ToUpper(attr.Name[0]) + attr.Name[1..], ctrl);
                        info.SetValue(obj, ctrl);
                    }
                }
            }
        }

        public class AutoInspector
        {
            public static float PREV_ICON_SIZE = 80;
            [System.NonSerialized] static Color textColor = new (1f, 0.8f, 0.4f, 1f);

            [System.NonSerialized] static GUIStyle boldLabel_Style = null;

            public static GUIStyle BoldLabel_Style
            {
                get
                {
                    if (boldLabel_Style == null)
                    {
                        boldLabel_Style = new GUIStyle(EditorStyles.label)
                        {
                            fontStyle = FontStyle.Bold,
                            fontSize = 11
                        };
                        boldLabel_Style.normal.textColor = textColor;
                    }

                    return boldLabel_Style;
                }
            }


            readonly Type type;
            System.Object obj;


            public AutoInspector(Type _type, ref System.Object _obj, bool _createFoldout = false)
            {
                type = _type;
                obj = _obj;
            }

            public void Build(float vOffset = 0, float hOffset = 0)
            {
                Layout(vOffset, hOffset);
            }

            private void Layout(float vOffset, float hOffset)
            {
                GUILayout.Space(vOffset);
                GUILayout.BeginHorizontal();
                GUILayout.Space(hOffset);

                GUILayout.BeginVertical();

                foreach (FieldInfo field in type.GetFields())
                {
                    foreach (Attribute attr in field.GetCustomAttributes(true))
                    {
                        if (attr is EditorFieldAttr atr)
                        {
                            if(atr.Space > 0)
                                GUILayout.Space(atr.Space);

                            // 1 = start layout horizontal
                            if (atr.LayoutHorizontal == 1)
                            {
                                GUILayout.BeginHorizontal();
                                AutoInspectorCntrlBuilder.CreateControl(atr, type, ref obj);
                            }

                            // -1 = end layout horizontal
                            else if (atr.LayoutHorizontal == -1)
                            {
                                AutoInspectorCntrlBuilder.CreateControl(atr, type, ref obj);
                                GUILayout.EndHorizontal();
                            }
                            else
                            {
                                AutoInspectorCntrlBuilder.CreateControl(atr, type, ref obj);
                            }
                        }
                    }
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            public static void DrawGameObjectList(List<GameObject> gameObjects,
                System.Action<GameObject> onAdd,
                System.Action<int> onRemove,
                System.Action<int, GameObject> onChangeDetect)
            {
                if (gameObjects.Count == 0)
                    onAdd(null);

                GameObject previousGO = null;
                Color oldColor = default;

                for (int i = 0; i < gameObjects.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2);

                    previousGO = gameObjects[i];

                    // create an object field for this gameObject
                    gameObjects[i] = (GameObject)EditorGUILayout.ObjectField(
                    gameObjects[i], typeof(GameObject), false);

                    if (previousGO != gameObjects[i])
                    { onChangeDetect(i, gameObjects[i]); }

                    // create a button to remove this Gameobject.
                    oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;

                    if (gameObjects[i] != null && GUILayout.Button("-"))
                        onRemove(i);

                    GUI.backgroundColor = oldColor;

                    GUILayout.EndHorizontal();
                }

                // add a new null PaintMesh if count is > 0.
                if (gameObjects.Count > 0 && gameObjects[gameObjects.Count - 1] != null)
                    onAdd(null);
            }

            public static void DrawGameObjectList<T>(List<GameObject> gameObjects,
                System.Action<GameObject> onAdd,
                System.Action<int> onRemove,
                System.Action<int, GameObject> onChangeDetect)
            {
                if (gameObjects.Count == 0)
                    onAdd(null);

                GameObject previousGO = null;
                Color oldColor = default;

                for (int i = 0; i < gameObjects.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2);

                    previousGO = gameObjects[i];

                    // create an object field for this gameObject
                    gameObjects[i] = (GameObject)EditorGUILayout.ObjectField(
                    gameObjects[i], typeof(GameObject), false);

                    if (gameObjects[i] != null && gameObjects[i].GetComponent<T>() == null)
                    {
                        gameObjects[i] = null;
                    }
                    else if (previousGO != gameObjects[i])
                    {
                        onChangeDetect(i, gameObjects[i]);
                    }

                    // create a button to remove this Gameobject.
                    oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;

                    if (i < gameObjects.Count-1 && gameObjects[i] != null && GUILayout.Button("-"))
                        onRemove(i);

                    GUI.backgroundColor = oldColor;

                    GUILayout.EndHorizontal();
                }

                // add a new null PaintMesh if count is > 0.
                if (gameObjects.Count > 0 && gameObjects[gameObjects.Count - 1] != null)
                    onAdd(null);
            }

            public static void DrawGameObjectList<T>(List<GameObject> gameObjects,
                System.Action<int> onSelect,
                System.Action<GameObject> onAdd,
                System.Action<int> onRemove,
                System.Func<int> getSelectedItemIndex,
                System.Action<int, GameObject> onChangeDetect,
                bool allowSceneObjects=false)
            {
                if (gameObjects.Count == 0)
                    onAdd(null);

                GameObject previousGO = null;
                Color oldColor = default;

                for (int i = 0; i < gameObjects.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2);

                    // if object is deleted in scene
                    if(i != gameObjects.Count-1)
                    {
                        if (gameObjects[i] == null)
                        {
                            gameObjects.RemoveAt(i);
                            continue;
                        }
                    }

                    previousGO = gameObjects[i];

                    // create an object field for this PaintMesh
                    gameObjects[i] = (GameObject)EditorGUILayout.ObjectField(
                    gameObjects[i], typeof(GameObject), allowSceneObjects);

                    if (gameObjects[i] != null && gameObjects[i].GetComponent<T>() == null)
                    {
                        gameObjects[i] = null;
                    }
                    else if (previousGO != gameObjects[i])
                    {
                        onChangeDetect(i, gameObjects[i]);
                    }

                    // --------------------------------------------------------------
                    // switch to selected button colour if this PaintMesh is selected.
                    oldColor = GUI.backgroundColor;
                    if (i == getSelectedItemIndex())
                    {
                        GUI.backgroundColor = Color.green;
                    }

                    // create a button to select this PaintMesh.
                    if (gameObjects[i] != null && GUILayout.Button("Select"))
                        onSelect(i);

                    // revert to old colour.
                    GUI.backgroundColor = oldColor;
                    // -----------------------------------------------------------------

                    // create a button to remove this Gameobject.
                    oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                    if (gameObjects[i] != null && GUILayout.Button("-"))
                    {
                        onRemove(i);

                        if (i == 0 && gameObjects.Count > 0)
                            onSelect(i); // set selected

                        else if (i > 0 && gameObjects.Count > 0)
                            onSelect(i - 1); // set selected

                        else
                            onSelect(-1); // set selected
                    }
                    GUI.backgroundColor = oldColor;
                    // -----------------------------------------------------------------

                    GUILayout.EndHorizontal();
                }

                // add a new null PaintMesh if count is > 0.
                if (gameObjects.Count > 0 && gameObjects[gameObjects.Count - 1] != null)
                {
                    var xx = gameObjects[gameObjects.Count - 1].GetComponent<T>();
                    if (xx != null)
                        onAdd(null);
                }
            }

            public static void DrawThumbnailsInspector(
                List<GameObject> gameObjects,
                System.Action<int> onSelect,
                System.Func<int> getSelectedItemIndex)
            {
                // number of icons per row
                int maxIconsPerRow = (int)(EditorGUIUtility.currentViewWidth / PREV_ICON_SIZE);
                int idx = 0;

                for (int i = 0; i < 8; i += maxIconsPerRow)
                {
                    // start horizontal
                    GUILayout.BeginHorizontal();

                    for (int r = 0; r < maxIconsPerRow; r++)
                    {
                        if (idx >= gameObjects.Count)
                            continue;

                        if (gameObjects[idx] == null)
                            continue;

                        Rect fullArea = GUILayoutUtility.GetRect(
                            PREV_ICON_SIZE,
                            PREV_ICON_SIZE,
                            GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)
                            );

                        // draw a background texture
                        Texture2D tex = new Texture2D(1, 1);
                        Color c = new Color(0.35f, 0.35f, 0.35f, 1f);
                        tex.SetPixel(0, 0, c);
                        tex.Apply();
                        GUI.DrawTexture(fullArea, tex);

                        // Draw the icon preview area
                        Rect previewArea = new Rect(
                            fullArea.position + new Vector2(0f, 0f),
                            fullArea.size - new Vector2(0f, 0f));

                        if (idx < gameObjects.Count)
                        {
                            // selection
                            if (previewArea.Contains(Event.current.mousePosition))
                            {
                                if (Event.current.type == EventType.MouseDown)
                                {
                                    onSelect(idx);
                                    // selectionIndex = idx;
                                    // Repaint();
                                }
                            }
                            // Debug.LogFormat("idx {0} sel {1}", idx, getSelectedItemIndex());
                            if (getSelectedItemIndex() == idx)
                            {
                                c = Color.yellow;
                                tex.SetPixel(0, 0, c);
                                tex.Apply();
                                GUI.DrawTexture(fullArea, tex);
                            }

                            // Draw prefab preview
                            GUI.Box(
                                previewArea,
                                new GUIContent(AssetPreview.GetAssetPreview(gameObjects[idx]), ""));

                            // Draw the activation toggle
                            // bool selected = GUI.Toggle(new Rect(previewArea.xMin + 4, previewArea.yMin + 4, 20, 20), type.m_PaintInfo.m_PaintEnabled, GUIContent.none);
                            //gameObjects[idx].b_active = GUI.Toggle(new Rect(previewArea.xMin + 4,
                            //    previewArea.yMin + 4, 20, 20),
                            //    gameObjects[idx].b_active,
                            //    GUIContent.none);
                        }

                        idx++;
                    }
                    GUILayout.EndHorizontal(); // end horizontal
                }
            }
        }


        [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
        public class BaseAttr :Attribute
        {
            public ControlType CtrlType { get; }

            public string Name { get; } = "";

            public BaseAttr(ControlType t, string _name)
            {
                CtrlType = t;
                Name = _name;
            }
        }


        public class EditorFieldAttr : BaseAttr
        {
            // 1 = start layout horizontal
            // 0 = keep layout horizontal
            // -1 = end layout horizontal
            // some other int = dont layout horizontal
            public int LayoutHorizontal { get; set; } = -1;

            public string Message { get; set; } = "";


            public int Space { get; set; } = 0;


            public EditorFieldAttr(ControlType t, string name, int layoutHorizontal = -23, string msg = "", int space=0) : base(t, name)
            {
                LayoutHorizontal = layoutHorizontal;
                Message = msg;
                Space = space;
            }
        }

        public class GameObjAttr : EditorFieldAttr
        {
            public bool allowSceneObjs;
            
            public GameObjAttr(ControlType t, string name, bool allowSceneObjs = false) : base(t, name)
            {
                this.allowSceneObjs = allowSceneObjs;
            }
        }

        public class IntSliderAttr : EditorFieldAttr
        {
            public int minVal = 1;
            public int maxVal = 100;

            public IntSliderAttr(ControlType t, string name, int min, int max) : base(t, name)
            {
                minVal = min;
                maxVal = max;
            }
        }


        public class FloatSliderAttr : EditorFieldAttr
        {
            public float minVal = 0.5f;
            public float maxVal = 50f;

            public FloatSliderAttr(ControlType t, string name, float min, float max) : base(t, name)
            {
                minVal = min;
                maxVal = max;
            }
        }


        public enum ControlType
        {
            floatField,
            intField,
            boolField,
            vector2,
            vector3,
            boldLabel,
            text,
            textControl,
            space,
            layerField,
            tag,
            intSlider,
            floatSlider,
            color,
            gameObject,
            sceneGameObj,
            texture2d,
            unityTerrainField,
            transformsList,
            gameObjectsList,

            LocationCategory, // LocationTool
            ItemsType, // 
            BrushPaintMode, // FoliagePainter
            AreaShape, // ScatterTool
            PaintMode, // GarassPainter
            TileSize,  // SpawnTiles
        }
    }
}
