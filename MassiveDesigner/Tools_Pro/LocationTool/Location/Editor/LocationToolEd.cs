using UnityEditor;
using MassiveDesinger.Tools;


namespace MassiveDesinger
{
    namespace LocationTool
    {
        [System.Serializable]
        public class LocationToolEd : EdToolBase
        {
            // public
            public SceneView sceneView;


            public LocationToolEd(string name) : base(name)
            {
            }

            public override void Initialize(SceneView sceneView)
            {
                this.sceneView = sceneView;
                OnEnable();
            }

            public void OnEnable()
            {
            }

            public override void OnInspectorUpdate()
            {
                DrawLocationInspector();
            }

            void DrawLocationInspector()
            {
                EditorGUILayout.HelpBox("This feature is available only in Pro_version", MessageType.Info);
                return;
            }
        }
    }
}