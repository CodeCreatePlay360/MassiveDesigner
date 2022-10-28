using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace MassiveDesinger
{
    namespace Tools
    {
        [System.Serializable]
        public class FoliagePaintEd : EdToolBase
        {
            private MassiveDesigner worldEd = default;
            private SceneView sceneView = default;
            private readonly string appTitle = "PaintBrush Editor";


            public override void Initialize(MassiveDesigner _worldEditor, SceneView _sceneView)
            {
                worldEd = _worldEditor;
                sceneView = _sceneView;
            }

            public override void OnInspectorUpdate()
            {
                // this sometimes happens, even though Initialize is being called from OnEnable
                if (worldEd == null)
                    return;

                worldEd.paintBrush.mainFoldPanel = EditorGUILayout.Foldout(worldEd.paintBrush.mainFoldPanel, "FoliagePainter", MassiveDesignerEd.fdLabelStyle);

                if (worldEd.paintBrush.mainFoldPanel)
                {
                    worldEd.paintBrush.AutoInspector.Build();

                    //if (worldEd.areaScatterTool.IsRunning)
                    //{
                    //    EditorGUILayout.HelpBox("Another spawner is currently running.", MessageType.Info);
                    //    return;
                    //}

                    //switch (worldEd.paintBrush.Settings.paintMode)
                    //{
                    //    case PaintMode.Replace:
                    //        EditorGUILayout.HelpBox("This feature is available only in patreon version...!", MessageType.Info);
                    //        return;
                    //}

                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();

                    // start and stop brush tool buttons
                    if (!worldEd.paintBrush.paintBrushEnabled)
                    {
                        var paintIcon = AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/paintBrush.png", typeof(Texture)) as Texture;
                        if (GUILayout.Button(new GUIContent("  Start Paint", paintIcon, "")))
                        {
                            worldEd.paintBrush.OnBeforePaint();
                            worldEd.paintBrush.paintBrushEnabled = true;
                        }
                    }

                    else if (worldEd.paintBrush.paintBrushEnabled)
                    {
                        var cancelIcon = AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/cancel.png", typeof(Texture)) as Texture;
                        if (GUILayout.Button(new GUIContent("  End Paint", cancelIcon, "")))
                        {
                            worldEd.paintBrush.Reset();
                            worldEd.paintBrush.paintBrushEnabled = false;
                        }
                    }

                    var refreshIcon = AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/refresh.png", typeof(Texture)) as Texture;
                    if (GUILayout.Button(new GUIContent("  Refresh", refreshIcon, "")))
                    {
                        worldEd.paintBrush.Refresh();
                    }

                    GUILayout.EndHorizontal();
                    // -----------------------------------
                }
            }

            public override void OnSceneUpdate()
            {
                MassiveDesigner.Instance.paintBrush.mouseInEditorWin = Event.current.isMouse;

                if (worldEd.paintBrush.paintBrushEnabled)
                {
                    CheckUserInput();
                    DrawBrushHandles();
                }
            }

            private void CheckUserInput()
            {
                var ctrlID = GUIUtility.GetControlID(appTitle.GetHashCode(), FocusType.Passive);

                Event currentEvent = Event.current;

                switch (currentEvent.type)
                {
                    case EventType.MouseDown:
                        if (Event.current.control)
                        {
                            worldEd.paintBrush.Paint(GetRay(sceneView));
                            worldEd.paintBrush.BeginPaint();
                        }
                        else if (Event.current.shift)
                            worldEd.paintBrush.ClearPaint(GetRay(sceneView));
                        break;

                    case EventType.MouseDrag:
                        if (Event.current.control)
                            worldEd.paintBrush.Paint(GetRay(sceneView));
                        else if (Event.current.shift)
                            worldEd.paintBrush.ClearPaint(GetRay(sceneView));
                        break;

                    case EventType.MouseUp:
                        worldEd.paintBrush.Reset();
                        break;

                    case EventType.Layout:
                        HandleUtility.AddDefaultControl(ctrlID);
                        break;

                    case EventType.MouseMove:
                        HandleUtility.Repaint();
                        break;
                }

                // to prevent deselection
                if (Selection.activeGameObject != worldEd.gameObject)
                    Selection.activeGameObject = worldEd.gameObject;
            }

            private void DrawBrushHandles()
            {
                List<Vector3> GetPointsForCircle(Vector3 refPoint, float radius)
                {
                    Vector3 currentPosition;
                    float currentAngle = 0;
                    int segments = 30;
                    List<Vector3> points = new List<Vector3>();
                    float xPos, yPos;

                    float angleStep = (180 / segments);

                    for (int i = 0; i < segments + 1; i++)
                    {
                        xPos = Mathf.Sin(currentAngle * (Mathf.PI / 180)) * radius;
                        yPos = Mathf.Cos(currentAngle * (Mathf.PI / 180)) * radius * 0.5f;

                        currentPosition = new Vector3(xPos, yPos, 0);
                        currentPosition += refPoint;
                        currentAngle += angleStep;

                        points.Add(currentPosition);
                    }

                    return points;
                }

                Ray ray = GetRay(sceneView);
                var layerMask = 1 << worldEd.SelectedLayer.settings.layerMask;

                if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    if (Event.current.shift)
                    {
                        // draw handles for remove radius
                        Handles.color = Color.red;
                        Handles.DrawWireArc(hit.point, Vector3.up, Vector3.forward, 360, worldEd.paintBrush.Settings.removeRadius);
                    }
                    else
                    {
                        // draw handles for spray radius
                        Handles.color = Color.green;
                        Handles.DrawWireArc(hit.point, Vector3.up, Vector3.forward, 360, worldEd.paintBrush.Settings.paintRadius);
                        //Handles.DrawWireArc(hit.point, Vector3.right, -Vector3.forward, 360, worldEd.paintBrush.Settings.paintRadius);
                        //Handles.DrawWireArc(hit.point, Vector3.forward, Vector3.right, 360, worldEd.paintBrush.Settings.paintRadius);
                    }
                }
            }

            public static Ray GetRay(SceneView sceneView)
            {
                Vector3 mousePos = Event.current.mousePosition;
                mousePos.y = sceneView.camera.pixelHeight - mousePos.y;
                Ray ray = sceneView.camera.ScreenPointToRay(mousePos);
                return ray;
            }
        }
    }
}
