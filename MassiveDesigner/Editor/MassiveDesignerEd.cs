using MassiveDesinger;
using MassiveDesinger.Tools;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(MassiveDesigner))]
[System.Serializable]
public class MassiveDesignerEd : Editor
{
    // statics
    public static GUIStyle fdLabelStyle;
    public static GUIStyle mainHeadingStyle;
    public static GUIStyle miniLabelStyle;
    public static GUIStyle boldLabelStyle;
    public static GUIStyle boxStyle = null;
    public static GUIStyle horizontalLine;

    public static MassiveDesignerEd Instance = null;

    // constants
    public readonly Color SELECTED_COLOR = Color.green;
    public readonly Color BTN_NORMAL_COLOR = Color.gray;
    public int CONTROL_HEIGHT = 18;

    // public fields
    public MassiveDesigner massiveDesigner = null;

    // init default tools
    public FoliagePaintEd foliagePainterEd = new("_FoliagePainter_");
    public MassiveDesinger.LocationTool.GrassPaintEd grassPainterEd = new("_GrassPaintEd_");
    public MassiveDesinger.LocationTool.LocationToolEd locationToolEd = new("_LocationToolEd_");

    // ICONS
    [System.NonSerialized] Texture paintBrushIcon;
    [System.NonSerialized] Texture cancelIcon;
    [System.NonSerialized] Texture refreshIcon;
    [System.NonSerialized] Texture newIcon;
    [System.NonSerialized] Texture copyIcon;
    [System.NonSerialized] Texture pasteIcon;
    [System.NonSerialized] Texture removeIcon;

    public Texture PaintBrushIcon
    {
        get
        {
            if(paintBrushIcon == null)
                paintBrushIcon = AssetDatabase.LoadAssetAtPath("Assets/MassiveDesigner/Resources/paintBrush.png", typeof(Texture)) as Texture;
            return paintBrushIcon;
        }
    }

    public Texture CancelIcon
    {
        get
        {
            if (cancelIcon == null)
                cancelIcon = AssetDatabase.LoadAssetAtPath("Assets/MassiveDesigner/Resources/cancel.png", typeof(Texture)) as Texture;
            return cancelIcon;
        }
    }

    public Texture RefreshIcon
    {
        get
        {
            if (refreshIcon == null)
                refreshIcon = AssetDatabase.LoadAssetAtPath("Assets/MassiveDesigner/Resources/refresh.png", typeof(Texture)) as Texture;
            return refreshIcon;
        }
    }

    public Texture NewIcon
    {
        get
        {
            if (newIcon == null)
                newIcon = AssetDatabase.LoadAssetAtPath("Assets/MassiveDesigner/Resources/file.png", typeof(Texture)) as Texture;
            return newIcon;
        }
    }

    public Texture CopyIcon
    {
        get
        {
            if (copyIcon == null)
                copyIcon = AssetDatabase.LoadAssetAtPath("Assets/MassiveDesigner/Resources/copy.png", typeof(Texture)) as Texture;
            return copyIcon;
        }
    }

    public Texture PasteIcon
    {
        get
        {
            if (pasteIcon == null)
                pasteIcon = AssetDatabase.LoadAssetAtPath("Assets/MassiveDesigner/Resources/paste.png", typeof(Texture)) as Texture;
            return pasteIcon;
        }
    }

    public Texture RemoveIcon
    {
        get
        {
            if (removeIcon == null)
                removeIcon = AssetDatabase.LoadAssetAtPath("Assets/MassiveDesigner/Resources/cancel.png", typeof(Texture)) as Texture;
            return removeIcon;
        }
    }

    // private
    [SerializeField] private string[] sections = new string[] { "World", "LocationEditor", "Settings" };
    [SerializeField] private bool layerPrototypesFoldPanel = false;
    private SceneView sceneView = null;


    // utility methods
    public static void HorizontalLine(Color color)
    {
        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, horizontalLine);
        GUI.color = c;
    }

     
    public void OnEnable()
    {
        massiveDesigner = target as MassiveDesigner;
        Instance = this;
        SceneView.duringSceneGui += OnSceneGUI;
        EnableTools();
    }

    public void OnDisable() 
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void EnableTools()
    {
        // sometimes Instance can be null if for example this is called from OnSceneGUI
        if (MassiveDesigner.Instance != null)
        {
            foliagePainterEd.Initialize(sceneView);
            grassPainterEd.Initialize(sceneView);
            locationToolEd.Initialize(sceneView);
        }
    }

    public override void OnInspectorGUI()
    {
        if (fdLabelStyle == null || mainHeadingStyle == null || boldLabelStyle == null)
        {
            // init editor gui styles
            fdLabelStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
            };

            mainHeadingStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 15,
            };
            mainHeadingStyle.normal.textColor = new(1f, 0.8f, 0.4f, 1f);

            miniLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Normal,
                fontSize = 9,
            };
            miniLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);

            boldLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
            };

            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;
        }

        GUILayout.Space(3);
        using (new GUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("MassiveDesigner", mainHeadingStyle);
            EditorGUILayout.LabelField(MassiveDesigner.VERSION, miniLabelStyle);
        }

        if (EditorGUILayout.LinkButton("Visit Github for latest updates"))
            System.Diagnostics.Process.Start("https://github.com/CodeCreatePlay360/MassiveDesigner");

        GUILayout.Space(2);

        using (new GUILayout.HorizontalScope())
        {
            if (EditorGUILayout.LinkButton("Support on patreon"))
                System.Diagnostics.Process.Start("https://www.patreon.com/CodeCreatePlay360");

            if (EditorGUILayout.LinkButton("Discord"))
                System.Diagnostics.Process.Start("https://discord.gg/EKmhB8xTq9");
        }

        GUILayout.Space(5);
        massiveDesigner.currentTabIndex = GUILayout.SelectionGrid(massiveDesigner.currentTabIndex, sections, 3, EditorStyles.toolbarButton);

        GUILayout.Space(10);
        if (massiveDesigner.currentTabIndex == 0) DrawTab_1_UI();
        if (massiveDesigner.currentTabIndex == 1) DrawTab_2_UI();
        if (massiveDesigner.currentTabIndex == 2) DrawTab_3_UI();

        GUILayout.Space(20);
    }
     
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color32[] pix = new Color32[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new (width, height);
        result.SetPixels32(pix);
        result.Apply();
        return result;
    }
     
    float boxYPos;
    void CreateBox()
    {
        boxStyle = new(GUI.skin.box);
        boxStyle.normal.background = MakeTex(1, 1, new Color(0.9f, 0.9f, 0.8f, 0.1f));
    }

    void DrawTab_1_UI()
    {
        if(!massiveDesigner.tilesOK)
        {
            EditorGUILayout.HelpBox("TilesInstance not initialized !", MessageType.Error);
            return;
        }

        if (MassiveDesigner.Externals.unityTerrain == null)
        {
            EditorGUILayout.HelpBox("Missing unity terrain reference !", MessageType.Error);
            return;
        }

        DrawLayersInspector();

        if (massiveDesigner.SelectedLayer != null)
        {
            if (boxStyle == null)
                CreateBox();

            //boxYPos = (113 + (massiveDesigner.Layers.Count * 20));
            //if (massiveDesigner.Layers.Count > 1)
            //    boxYPos += massiveDesigner.Layers.Count;
            //GUI.Box(new Rect(0, boxYPos, 800, 138), "", boxStyle);
             
            DrawLayerSettings();

            GUILayout.Space(8f);
            DrawPrototypesInspector();
            DrawPrototypesThumbnails();

            if(massiveDesigner.SelectedLayer.prototypes.Count > 1)
            {
                GUILayout.Space(8F);
                DrawPaintMeshesInspector();
            }
        }

        HorizontalLine(Color.grey);
        GUILayout.Space(20);

        // update tool inspectors
        foliagePainterEd.OnInspectorUpdate();
        HorizontalLine(Color.grey);
        GUILayout.Space(4);

        grassPainterEd.OnInspectorUpdate();
        HorizontalLine(Color.grey);
        GUILayout.Space(4);

        // scatterTool.OnInspectorUpdate();
        // HorizontalLine(Color.grey);
        // ------------------------

        GUILayout.Space(15);
        DrawCntrlBtnsPanel();
    } 

    void DrawTab_2_UI()
    {
        locationToolEd.OnInspectorUpdate();
    }

    void DrawTab_3_UI()
    {
        massiveDesigner.spawnTiles.AutoInspector.Build();
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Minimum grid size is 1km.", MessageType.Info);
        EditorGUILayout.HelpBox("Minimum tile size is 500m.", MessageType.Info);

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Initialize Spawn Tiles"))
                massiveDesigner.InitTiles();
        }

        GUILayout.Space(10);
        GUILayout.Space(10);
        massiveDesigner.externals.AutoInspector.Build();
        massiveDesigner.saveFile = (SaveDataFile)EditorGUILayout.ObjectField("SaveDataObj", massiveDesigner.saveFile, typeof(SaveDataFile), false);
    }

    void DrawLayersInspector()
    {
        // make sure at least one layer always exist
        if(massiveDesigner.Layers.Count == 0)
            massiveDesigner.AddLayer();
        // ------------------------------------------

        for (int i = 0; i < massiveDesigner.Layers.Count; i++)
        {
            GUILayout.BeginHorizontal();

            var layer = massiveDesigner.Layers[i];
            layer.layerIndex = i;

            // layer count
            GUILayout.Label(i.ToString(), GUILayout.MinHeight(CONTROL_HEIGHT), GUILayout.MaxWidth(20));

            // a field for layer's name
            layer.layerName = GUILayout.TextField(layer.layerName, GUILayout.MinWidth(100), GUILayout.MaxWidth(220), GUILayout.MinHeight(CONTROL_HEIGHT));

            // a button to select this layer
            // change btn color tint according to layer's selected status.
            Color oldColor = GUI.backgroundColor;
            if (massiveDesigner.SelectedLayerIdx == i)
            { GUI.backgroundColor = SELECTED_COLOR; }

            if (GUILayout.Button("Select", GUILayout.MinHeight(CONTROL_HEIGHT)))
            { massiveDesigner.SelectLayer(i); }
            GUI.backgroundColor = oldColor;

            // a toggle field to toggle this layer on and off
            layer.active = EditorGUILayout.Toggle(layer.active, GUILayout.MaxHeight(20), GUILayout.MaxWidth(20));

            //
            if (GUILayout.Button(new GUIContent("", RemoveIcon, "Remove Layer"), GUILayout.MaxWidth(40)))
            { massiveDesigner.RemoveLayer(i); break; }

            // update paintmesh layerIndexes according to layer index
            for (int j = 0; j < layer.paintMeshes.Count; j++)
                if (layer.paintMeshes[j] != null)
                    layer.paintMeshes[j].layerIdx = i;

            GUILayout.EndHorizontal();
        }
    }

    void DrawLayerSettings()
    {
        PaintMesh.ItemType layerItemsType = massiveDesigner.SelectedLayer.settings.itemsType;

        massiveDesigner.SelectedLayer.layerSettingsAutoCntrls.Build();

        if (layerItemsType != massiveDesigner.SelectedLayer.settings.itemsType)
        {
            foreach (var paintMesh in massiveDesigner.SelectedLayer.paintMeshes)
            {
                if(paintMesh)
                {
                    paintMesh.properties.itemType = massiveDesigner.SelectedLayer.settings.itemsType;
                }
            }
            // Debug.LogFormat("Updated item types for layer {0}", worldEditor.SelectedLayer.layerName);
            massiveDesigner.SelectedLayer.VerifyPaintMeshes();
            MassiveDesigner.Externals.UpdateTerrainPrototypes(massiveDesigner.Layers);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button(new GUIContent("  Copy Layer Settings", CopyIcon, "Copy Layer Settings")))
                massiveDesigner.SelectedLayer.CopySettings();
            if (GUILayout.Button(new GUIContent("  Paste Layer Settings", PasteIcon, "Paste Layer Settings")))
                massiveDesigner.SelectedLayer.PasteSettings();
        }

        if (GUILayout.Button(new GUIContent("  Create New Layer", NewIcon, "CreateNewLayer")))
            massiveDesigner.AddLayer();
    }

    void DrawPrototypesInspector()
    {
        layerPrototypesFoldPanel = EditorGUILayout.Foldout(layerPrototypesFoldPanel, "Layer Prototypes", fdLabelStyle);

        if (layerPrototypesFoldPanel)
        {
            CodeCreatePlay.AutoInspector.AutoInspector.DrawGameObjectList<PaintMesh>(
                massiveDesigner.SelectedLayer.prototypes,
                massiveDesigner.SelectedLayer.OnAddPrototype,
                massiveDesigner.SelectedLayer.OnRemovePrototype,
                massiveDesigner.SelectedLayer.OnPrototypeChange);
        }
    }

    void DrawPrototypesThumbnails()
    {
        CodeCreatePlay.AutoInspector.AutoInspector.DrawThumbnailsInspector(massiveDesigner.SelectedLayer.prototypes,
            massiveDesigner.SelectedLayer.SelectPaintMesh,
            massiveDesigner.SelectedLayer.GetSelectedPaintMeshIdx
            );
    }

    void DrawPaintMeshesInspector()
    {
        if (massiveDesigner.SelectedLayer.SelectedPaintMesh != null &&
            massiveDesigner.SelectedLayer.SelectedPaintMesh.autoInspector != null)
        {
            massiveDesigner.SelectedLayer.SelectedPaintMesh.autoInspector.Build();
        }
    }

    void DrawCntrlBtnsPanel()
    {
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Save Data"))
            {
                massiveDesigner.SaveDataToFile();
                if (massiveDesigner.saveFile != null)
                    EditorUtility.SetDirty(massiveDesigner.saveFile);
            }

            if (GUILayout.Button("Load Data From File"))
                massiveDesigner.ReloadDataFromFile();
        }

        using (new GUILayout.HorizontalScope())
        {
            var refreshIcon = AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/refresh.png", typeof(Texture)) as Texture;
            if (GUILayout.Button(new GUIContent("  Refresh And Update", refreshIcon, "")))
            {
                massiveDesigner.Enable();
                MassiveDesigner.Externals.UpdateTerrainPrototypes(massiveDesigner.Layers);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            Color original = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.2f, 0.2f, 1f);
             
            if (GUILayout.Button("Clear All Data"))
                massiveDesigner.ClearAll();

            GUI.backgroundColor = original;
        }
    }
     
    public void OnSceneGUI(SceneView sv)
    {
        if (sceneView == null)
        {
            sceneView = sv;
            EnableTools();
        }

        DebugGrid();

        // update tools
        foliagePainterEd.OnSceneUpdate();
        grassPainterEd.OnSceneUpdate();
        locationToolEd.OnSceneUpdate();

        sceneView.Repaint();
    }
      
    void DebugGrid()
    {
        if (massiveDesigner.tilesOK && massiveDesigner.spawnTiles.drawTiles)
        {
            Handles.color = Color.white;
            foreach (var cellEntry in massiveDesigner.spawnTiles.cellDict.Keys)
            {
                Handles.DrawWireCube(massiveDesigner.spawnTiles.cellDict[cellEntry].worldPos,
                    new Vector3(massiveDesigner.spawnTiles.tileSize, 0.05f, massiveDesigner.spawnTiles.tileSize));
            }

            //Handles.color = Color.white;
            //foreach (var tile in worldEditor.tilesInstance.NearestCellPlusNeighbours(worldEditor.paintBrush.BrushRefTransform.position, 1))
            //    Handles.DrawWireCube(tile.worldPos, new Vector3(worldEditor.tilesInstance.tileSize, 0.1f, worldEditor.tilesInstance.tileSize));
        }
    }
}
