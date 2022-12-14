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
            AreaScatterTool areaScatterTool;

            public AreaScatterToolEd(string name):base(name)
            {
            }

            public override void Initialize(SceneView sceneView)
            {
            }

            public override void OnInspectorUpdate()
            {
                mainFd = EditorGUILayout.Foldout(mainFd, "ScatteringTool", MassiveDesignerEd.fdLabelStyle);

                if (mainFd)
                {
                    areaScatterTool.AutoInspector.Build();

                    switch (areaScatterTool.settings.areaShape)
                    {
                        case AreaShape.Polygon:
                            EditorGUILayout.HelpBox("This feature is available only in patreon version...!", MessageType.Info);
                            return;
                    }

                    if (areaScatterTool.settings.referenceObject == null)
                    {
                        EditorGUILayout.HelpBox("Reference object not found.", MessageType.Info);
                        return;
                    }

                    if (worldEditor.foliagePainter.isPainting)
                    {
                        EditorGUILayout.HelpBox("Another spawner is currently running.", MessageType.Info);
                        return;
                    }

                    GUILayout.Space(5);

                    using (new GUILayout.HorizontalScope())
                    {
                        if(!areaScatterTool.IsRunning)
                        {
                            Color original = GUI.backgroundColor;
                            GUI.backgroundColor = Color.green;
                            if (GUILayout.Button("BeginScatter"))
                                areaScatterTool.BeginScatter();
                            GUI.backgroundColor = original;
                        }
                        else if(areaScatterTool.IsRunning)
                        {
                            using(new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(-2);
                                    GUILayout.Label("[CurrentSpawnCount]", MassiveDesignerEd.boldLabelStyle);
                                    GUILayout.Space(5);
                                    GUILayout.Label(areaScatterTool.currentSpawnCount.ToString());
                                }
                                 
                                Color original = GUI.backgroundColor;
                                GUI.backgroundColor = Color.red;

                                if (GUILayout.Button("Stop"))
                                    areaScatterTool.IsRunning = false;

                                GUI.backgroundColor = original;
                            }
                        }
                    }
                }
            }
                
            public override void OnSceneUpdate()
            {
                if(areaScatterTool.settings.referenceObject != null && areaScatterTool.settings.debug)
                    return;
            }
        }
    }
}
