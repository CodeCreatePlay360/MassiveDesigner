using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace MassiveDesinger
{
    namespace Tools
    {
        [System.Serializable]
        public class GrassPainterEd : EdToolBase
        {
            private MassiveDesigner worldEd;
            private SceneView sceneView;


            public override void Initialize(MassiveDesigner worldEditor, SceneView sceneView)
            {
                worldEd = worldEditor;
                this.sceneView = sceneView;
            }

            public override void OnInspectorUpdate()
            {
                // this sometimes happens, even though Initialize is being called from OnEnable
                if (worldEd == null)
                    return;

                worldEd.grassPainter.mainFd = EditorGUILayout.Foldout(worldEd.grassPainter.mainFd, "GrassPainter", MassiveDesignerEd.fdLabelStyle);

                if (worldEd.grassPainter.mainFd)
                {
                    worldEd.grassPainter.AutoInspector.Build();

                    GUILayout.Space(5f);

                    Color original = GUI.backgroundColor;
                    using (new GUILayout.HorizontalScope())
                    {
                        GUI.backgroundColor = Color.green;

                        if (worldEd.grassPainter.settings.useSimulation)
                        {
                            if (GUILayout.Button("ScatterSeeds"))
                            {

                            }

                            if (GUILayout.Button("StartSimulation"))
                            {
                                MassiveDesigner.Instance.grassPainter.Scatter();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("ScatterGrass"))
                            {
                                MassiveDesigner.Instance.grassPainter.Scatter();
                            }
                        }

                        GUI.backgroundColor = original;
                    }

                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Clear"))
                    {
                        worldEd.grassPainter.ClearGrass();
                    }
                    GUI.backgroundColor = original;
                }
            }

            public override void OnSceneUpdate()
            {
            }
        }
    }
}
