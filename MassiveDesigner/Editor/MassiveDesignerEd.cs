using UnityEngine;
using UnityEditor;
using MassiveDesinger;


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

    // constants
    public readonly Color SELECTED_COLOR = Color.green;
    public readonly Color BTN_NORMAL_COLOR = Color.gray;
    public int CONTROL_HEIGHT = 18;

    // public fields
    public MassiveDesigner worldEditor = null;

    // init default tools
    public MassiveDesinger.Tools.FoliagePaintEd foliagePainter = new();
    public MassiveDesinger.Tools.GrassPainterEd grassPainterEd = new();
    public MassiveDesinger.Tools.AreaScatterToolEd scatterTool = new();
    public MassiveDesinger.Tools.LocationToolEd locationToolEd = new();

    // private
    [SerializeField] private int currentTabIndex = 0; // geo paint editor = 0, spline editor = 1
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
        worldEditor = target as MassiveDesigner;
        SceneView.duringSceneGui += OnSceneGUI;
        EnableTools();
    }

    public void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void EnableTools()
    {
        foliagePainter.Initialize(worldEditor, sceneView);
        // grassPainterEd.Initialize(worldEditor, sceneView);
        // scatterTool.Initialize(worldEditor, sceneView);
        locationToolEd.Initialize(worldEditor, sceneView);
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
            mainHeadingStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);

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
            EditorGUILayout.LabelField("v0.1", miniLabelStyle);
        }

        if (EditorGUILayout.LinkButton("Visit Github for latest updates"))
            Debug.Log("Link clicked");

        GUILayout.Space(2);

        using (new GUILayout.HorizontalScope())
        {
            if (EditorGUILayout.LinkButton("Support on patreon"))
                Debug.Log("Manual");

            if (EditorGUILayout.LinkButton("Discord"))
                Debug.Log("Manual");
        }

        GUILayout.Space(5);
        currentTabIndex = GUILayout.SelectionGrid(currentTabIndex, new string[]
        { "World", "LocationEditor", "Settings" }, 3, EditorStyles.toolbarButton);

        GUILayout.Space(10);
        if (currentTabIndex == 0) DrawTab_1_UI();
        if (currentTabIndex == 1) DrawTab_2_UI();
        if (currentTabIndex == 2) DrawTab_3_UI();

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
        if(!worldEditor.tilesOK)
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

        if (worldEditor.SelectedLayer != null)
        {
            if (boxStyle == null)
                CreateBox();

            boxYPos = (110 + (worldEditor.Layers.Count * 20));
            if (worldEditor.Layers.Count > 1)
                boxYPos += worldEditor.Layers.Count;
            GUI.Box(new Rect(0, boxYPos, 800, 120), "", boxStyle);
             
            DrawLayerSettings();

            GUILayout.Space(8f);
            DrawPrototypesInspector();
            DrawPrototypesThumbnails();

            if(worldEditor.SelectedLayer.prototypes.Count > 1)
            {
                GUILayout.Space(8F);
                DrawPaintMeshesInspector();
            }
        }

        HorizontalLine(Color.grey);
        GUILayout.Space(20);

        // update tool inspectors
        foliagePainter.OnInspectorUpdate();
        HorizontalLine(Color.grey);
        GUILayout.Space(4);

        // grassPainterEd.OnInspectorUpdate();
        // HorizontalLine(Color.grey);
        // GUILayout.Space(4);

        // scatterTool.OnInspectorUpdate();
        // HorizontalLine(Color.grey);
        // ------------------------

        GUILayout.Space(15);
        DrawCntrlBtnsPanel();
    } 

    void DrawTab_2_UI()
    {
        locationToolEd.OnInspectorUpdate();
        // grassPatchTool.OnInspectorUpdate();
    }

    void DrawTab_3_UI()
    {
        worldEditor.spawnTiles.AutoInspector.Build();
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("GridSize is limited to 8km.", MessageType.Info);

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("InitializeSpawnTiles"))
                worldEditor.InitTiles();
        }

        GUILayout.Space(10);
        GUILayout.Space(10);
        worldEditor.externals.AutoInspector.Build();
        worldEditor.saveFile = (SaveDataFile)EditorGUILayout.ObjectField("SaveDataObj", worldEditor.saveFile, typeof(SaveDataFile), false);
    }

    public static string iconPrefix => EditorGUIUtility.isProSkin ? "d_" : "";

    void DrawLayersInspector()
    {
        // make sure at least one layer always exist
        if(worldEditor.Layers.Count == 0)
            worldEditor.AddLayer();
        // ------------------------------------------

        for (int i = 0; i < worldEditor.Layers.Count; i++)
        {
            GUILayout.BeginHorizontal();

            var layer = worldEditor.Layers[i];
            layer.layerIndex = i;

            // layer count
            GUILayout.Label(i.ToString(), GUILayout.MinHeight(CONTROL_HEIGHT), GUILayout.MaxWidth(20));

            // a field for layer's name
            layer.layerName = GUILayout.TextField(layer.layerName, GUILayout.MinWidth(100), GUILayout.MaxWidth(220), GUILayout.MinHeight(CONTROL_HEIGHT));

            // a button to select this layer
            // change btn color tint according to layer's selected status.
            Color oldColor = GUI.backgroundColor;
            if (worldEditor.SelectedLayerIdx == i)
            { GUI.backgroundColor = SELECTED_COLOR; }

            if (GUILayout.Button("Select", GUILayout.MinHeight(CONTROL_HEIGHT)))
            { worldEditor.SelectLayer(i); }
            GUI.backgroundColor = oldColor;

            // a toggle field to toggle this layer on and off
            layer.active = EditorGUILayout.Toggle(layer.active, GUILayout.MaxHeight(20), GUILayout.MaxWidth(20));

            //
            var copyIcon =  AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/copy.png", typeof(Texture)) as Texture;
            var pasteIcon = AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/paste.png", typeof(Texture)) as Texture;
            var removeIcon = AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/cancel.png", typeof(Texture)) as Texture;

            if (GUILayout.Button(new GUIContent("", copyIcon, "Copy Layer Settings To Clipboard")))
            { worldEditor.SelectedLayer.CopySettings(); }

            if (GUILayout.Button(new GUIContent("", pasteIcon, "Paste Settings From Clipboard")))
            { worldEditor.SelectedLayer.PasteSettings(); }

            if (GUILayout.Button(new GUIContent("", removeIcon, "Remove Layer")))
            { worldEditor.RemoveLayer(i); break; }

            // update paintmesh layerIndexes according to layer index
            for (int j = 0; j < layer.paintMeshes.Count; j++)
                if (layer.paintMeshes[j] != null)
                    layer.paintMeshes[j].layerIdx = i;

            GUILayout.EndHorizontal();
        }
    }

    void DrawLayerSettings()
    {
        PaintMesh.ItemType layerItemsType = worldEditor.SelectedLayer.settings.itemsType;

        worldEditor.SelectedLayer.layerSettingsAutoCntrls.Build();

        if (layerItemsType != worldEditor.SelectedLayer.settings.itemsType)
        {
            foreach (var paintMesh in worldEditor.SelectedLayer.paintMeshes)
            {
                if(paintMesh)
                {
                    paintMesh.properties.itemType = worldEditor.SelectedLayer.settings.itemsType;
                }
            }
            Debug.LogFormat("Updated item type for layer {0}", worldEditor.SelectedLayer.layerName);
            worldEditor.SelectedLayer.VerifyPaintMeshes();
            MassiveDesigner.Externals.UpdateTerrainPrototypes(worldEditor.Layers);
        }

        GUILayout.Space(10);

        var newLayerIcon = AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/file.png", typeof(Texture)) as Texture;
        if (GUILayout.Button(new GUIContent("  Create New Layer", newLayerIcon, "CreateNewLayer")))
            worldEditor.AddLayer();
    }

    void DrawPrototypesInspector()
    {
        layerPrototypesFoldPanel = EditorGUILayout.Foldout(layerPrototypesFoldPanel, "Layer Prototypes", fdLabelStyle);

        if (layerPrototypesFoldPanel)
        {
            CodeCreatePlay.AutoInspector.AutoInspector.DrawGameObjectList<PaintMesh>(
                worldEditor.SelectedLayer.prototypes,
                worldEditor.SelectedLayer.OnAddPrototype,
                worldEditor.SelectedLayer.OnRemovePrototype,
                worldEditor.SelectedLayer.OnPrototypeChange);
        }
    }

    void DrawPrototypesThumbnails()
    {
        CodeCreatePlay.AutoInspector.AutoInspector.DrawThumbnailsInspector(worldEditor.SelectedLayer.prototypes,
            worldEditor.SelectedLayer.SelectPaintMesh,
            worldEditor.SelectedLayer.GetSelectedPaintMeshIdx
            );
    }

    void DrawPaintMeshesInspector()
    {
        if (worldEditor.SelectedLayer.SelectedPaintMesh != null &&
            worldEditor.SelectedLayer.SelectedPaintMesh.autoInspector != null)
        {
            worldEditor.SelectedLayer.SelectedPaintMesh.autoInspector.Build();
        }
    }

    void DrawCntrlBtnsPanel()
    {
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Save Data"))
            {
                worldEditor.SaveDataToFile();
                if (worldEditor.saveFile != null)
                    EditorUtility.SetDirty(worldEditor.saveFile);
            }

            if (GUILayout.Button("Load Data From File"))
                worldEditor.ReloadDataFromFile();
        }

        using (new GUILayout.HorizontalScope())
        {
            var refreshIcon = AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/refresh.png", typeof(Texture)) as Texture;
            if (GUILayout.Button(new GUIContent("  Refresh And Update", refreshIcon, "")))
            {
                worldEditor.Enable();
                MassiveDesigner.Externals.UpdateTerrainPrototypes(worldEditor.Layers);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            Color original = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.2f, 0.2f, 1f);
             
            if (GUILayout.Button("Clear All Data"))
                worldEditor.ClearAll();

            GUI.backgroundColor = original;
        }
    }
     
    public void OnSceneGUI(SceneView sv)
    {
        if (sceneView == null)
        {
            sceneView = sv;
            foliagePainter.Initialize(worldEditor, sceneView);
            // grassPainterEd.Initialize(worldEditor, sceneView);
            // scatterTool.Initialize(worldEditor, sceneView);
        }

        DebugGrid();

        // update tools
        foliagePainter.OnSceneUpdate();
        // grassPainterEd.OnSceneUpdate();
        // scatterTool.OnSceneUpdate();
        locationToolEd.OnSceneUpdate();

        sceneView.Repaint();
    }
      
    void DebugGrid()
    {
        if (worldEditor.tilesOK && worldEditor.spawnTiles.drawTiles)
        {
            Handles.color = Color.white;
            foreach (var cellEntry in worldEditor.spawnTiles.cellDict.Keys)
            {
                Handles.DrawWireCube(worldEditor.spawnTiles.cellDict[cellEntry].worldPos,
                    new Vector3(worldEditor.spawnTiles.tileSize, 0.05f, worldEditor.spawnTiles.tileSize));
            }

            //Handles.color = Color.white;
            //foreach (var tile in worldEditor.tilesInstance.NearestCellPlusNeighbours(worldEditor.paintBrush.BrushRefTransform.position, 1))
            //    Handles.DrawWireCube(tile.worldPos, new Vector3(worldEditor.tilesInstance.tileSize, 0.1f, worldEditor.tilesInstance.tileSize));
        }
    }
}
