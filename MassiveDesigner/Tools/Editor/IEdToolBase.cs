namespace MassiveDesinger
{
    namespace Tools
    {
        [System.Serializable]
        public class EdToolBase
        {
            public string Name { get; private set; }

            public EdToolBase(string name)
            {
                Name = name;
            }

            public virtual void Initialize(UnityEditor.SceneView sceneView) { }
            public virtual void OnInspectorUpdate() { }
            public virtual void OnSceneUpdate() { }
        }
    }
}
