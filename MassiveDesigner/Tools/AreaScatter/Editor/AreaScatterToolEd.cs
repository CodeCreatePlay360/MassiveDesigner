using UnityEditor;
using UnityEngine;


namespace MassiveDesinger
{
    namespace Tools
    {
        [System.Serializable]
        public class AreaScatterToolEd : EdToolBase
        {
            private MassiveDesigner worldEditor;
            [SerializeField] private bool mainFd = false;


            public override void Initialize(MassiveDesigner _worldEditor, SceneView sceneView)
            {
                worldEditor = _worldEditor;
            }

            public override void OnInspectorUpdate()
            {
                mainFd = EditorGUILayout.Foldout(mainFd, "ScatteringTool", MassiveDesignerEd.fdLabelStyle);

                if (mainFd)
                {
                    worldEditor.areaScatterTool.AutoInspector.Build();

                    switch (worldEditor.areaScatterTool.settings.areaShape)
                    {
                        case AreaShape.Polygon:
                            EditorGUILayout.HelpBox("This feature is available only in patreon version...!", MessageType.Info);
                            return;
                    }

                    if (worldEditor.areaScatterTool.settings.referenceObject == null)
                    {
                        EditorGUILayout.HelpBox("Reference object not found.", MessageType.Info);
                        return;
                    }

                    if (worldEditor.paintBrush.isPainting)
                    {
                        EditorGUILayout.HelpBox("Another spawner is currently running.", MessageType.Info);
                        return;
                    }

                    GUILayout.Space(5);

                    using (new GUILayout.HorizontalScope())
                    {
                        if(!worldEditor.areaScatterTool.IsRunning)
                        {
                            Color original = GUI.backgroundColor;
                            GUI.backgroundColor = Color.green;
                            if (GUILayout.Button("BeginScatter"))
                                worldEditor.areaScatterTool.BeginScatter();
                            GUI.backgroundColor = original;
                        }
                        else if(worldEditor.areaScatterTool.IsRunning)
                        {
                            using(new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(-2);
                                    GUILayout.Label("[CurrentSpawnCount]", MassiveDesignerEd.boldLabelStyle);
                                    GUILayout.Space(5);
                                    GUILayout.Label(worldEditor.areaScatterTool.currentSpawnCount.ToString());
                                }
                                 
                                Color original = GUI.backgroundColor;
                                GUI.backgroundColor = Color.red;

                                if (GUILayout.Button("Stop"))
                                    worldEditor.areaScatterTool.IsRunning = false;

                                GUI.backgroundColor = original;
                            }
                        }
                    }
                }
            }
                
            public override void OnSceneUpdate()
            {
                if(worldEditor.areaScatterTool.settings.referenceObject != null && worldEditor.areaScatterTool.settings.debug)
                    return;
            }
        }
    }
}
