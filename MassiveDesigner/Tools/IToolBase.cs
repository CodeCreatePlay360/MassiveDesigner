
namespace MassiveDesinger
{
    namespace Tools
    {
        [System.Serializable]
        public class ToolBase
        {
            public string toolName;
            public virtual void Initialize() { }
            public virtual void Refresh() { }
            public virtual bool IsOK() { return true; }
            public virtual void OnGizmos() { }
        }
    }
}
