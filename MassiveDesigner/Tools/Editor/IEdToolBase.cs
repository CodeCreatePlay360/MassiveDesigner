namespace MassiveDesinger
{
    namespace Tools
    {
        [System.Serializable]
        public abstract class EdToolBase
        {
            public abstract void Initialize(MassiveDesigner worldEditor, UnityEditor.SceneView sceneView);
            public abstract void OnInspectorUpdate();
            public abstract void OnSceneUpdate();
        }
    }
}
