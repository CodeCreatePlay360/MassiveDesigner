using UnityEngine;
using UnityEditor;


namespace MassiveDesinger
{
    namespace Tools
    {
        [System.Serializable]
        public class FoliagePaintEd : EdToolBase
        {
            private SceneView sceneView = default;
            private readonly string appTitle = "PaintBrush Editor";

            public FoliagePaintEd(string name) : base(name)
            {
            }

            public override void Initialize(SceneView _sceneView)
            {
                sceneView = _sceneView;
            }

            public override void OnInspectorUpdate()
            {
                MassiveDesigner.Instance.foliagePainter.mainFoldPanel = EditorGUILayout.Foldout(MassiveDesigner.Instance.foliagePainter.mainFoldPanel, "FoliagePainter", MassiveDesignerEd.fdLabelStyle);

                if (MassiveDesigner.Instance.foliagePainter.mainFoldPanel)
                {
                    MassiveDesigner.Instance.foliagePainter.AutoInspector.Build();

                    GUILayout.Space(5);
                    // GUILayout.BeginHorizontal();

                    // start and stop brush tool buttons
                    if (!MassiveDesigner.Instance.foliagePainter.paintBrushEnabled)
                    {
                        if (GUILayout.Button(new GUIContent("  Start Paint", MassiveDesignerEd.Instance.PaintBrushIcon, "")))
                        {
                            MassiveDesigner.Instance.foliagePainter.OnBeforePaint();
                            MassiveDesigner.Instance.foliagePainter.paintBrushEnabled = true;
                        }
                    }

                    else if (MassiveDesigner.Instance.foliagePainter.paintBrushEnabled)
                    {
                        if (GUILayout.Button(new GUIContent("  End Paint", MassiveDesignerEd.Instance.CancelIcon, "")))
                        {
                            MassiveDesigner.Instance.foliagePainter.Reset();
                            MassiveDesigner.Instance.foliagePainter.paintBrushEnabled = false;
                        }
                    }

                    if (GUILayout.Button(new GUIContent("  Refresh", MassiveDesignerEd.Instance.RefreshIcon, "")))
                    {
                        MassiveDesigner.Instance.foliagePainter.Refresh();
                    }

                    // GUILayout.EndHorizontal();
                    // -----------------------------------
                }
            }

            public override void OnSceneUpdate()
            {
                MassiveDesigner.Instance.foliagePainter.mouseInEditorWin = Event.current.isMouse;

                if (MassiveDesigner.Instance.foliagePainter.paintBrushEnabled)
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
                            MassiveDesigner.Instance.foliagePainter.Paint(GetRay(sceneView));
                            MassiveDesigner.Instance.foliagePainter.BeginPaint();
                        }
                        else if (Event.current.shift)
                            MassiveDesigner.Instance.foliagePainter.ClearPaint(GetRay(sceneView));
                        break;

                    case EventType.MouseDrag:
                        if (Event.current.control)
                            MassiveDesigner.Instance.foliagePainter.Paint(GetRay(sceneView));
                        else if (Event.current.shift)
                            MassiveDesigner.Instance.foliagePainter.ClearPaint(GetRay(sceneView));
                        break;

                    case EventType.MouseUp:
                        MassiveDesigner.Instance.foliagePainter.Reset();
                        break;

                    case EventType.Layout:
                        HandleUtility.AddDefaultControl(ctrlID);
                        break;

                    case EventType.MouseMove:
                        HandleUtility.Repaint();
                        break;
                }

                // to prevent deselection
                if (Selection.activeGameObject != MassiveDesigner.Instance.gameObject)
                    Selection.activeGameObject = MassiveDesigner.Instance.gameObject;
            }

            private void DrawBrushHandles()
            {
                Ray ray = GetRay(sceneView);
                var layerMask = 1 << (MassiveDesigner.Instance.foliagePainter.Settings.overrideGroupLayerMask ? MassiveDesigner.Instance.foliagePainter.Settings.layerMask : 
                    MassiveDesigner.Instance.SelectedLayer.settings.layerMask);

                // var layerMask = 1 << MassiveDesigner.Instance.SelectedLayer.settings.layerMask;

                if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    if (Event.current.shift)
                    {
                        // draw handles for remove radius
                        Handles.color = Color.red;
                        Handles.DrawWireArc(hit.point, Vector3.up, Vector3.forward, 360, MassiveDesigner.Instance.foliagePainter.Settings.removeRadius);
                    }
                    else
                    {
                        // draw handles for spray radius
                        Handles.color = Color.green;
                        Handles.DrawWireArc(hit.point, Vector3.up, Vector3.forward, 360, MassiveDesigner.Instance.foliagePainter.Settings.paintRadius);
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
