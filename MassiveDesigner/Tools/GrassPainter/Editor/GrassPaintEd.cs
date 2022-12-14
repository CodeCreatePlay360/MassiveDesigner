using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MassiveDesinger.Tools;


namespace MassiveDesinger
{
    namespace Tools_Pro
    {
        [System.Serializable]
        public class GrassPaintEd : EdToolBase
        {
            private SceneView sceneView;

            public GrassPainter Painter
            {
                get
                {
                    return MassiveDesigner.Instance.grassPainter;
                }
            }

            public GrassPaintEd(string name):base(name)
            {
            }

            public override void Initialize(SceneView sceneView)
            {
                this.sceneView = sceneView;
            }

            public override void OnInspectorUpdate()
            {
                Painter.mainFd = EditorGUILayout.Foldout(Painter.mainFd, "GrassPainter", MassiveDesignerEd.fdLabelStyle);

                if (Painter.mainFd)
                {
                    EditorGUILayout.HelpBox("This feature is available only in Pro_version", MessageType.Info);
                }
            }

            public override void OnSceneUpdate()
            {
            }
        }
    }
}
