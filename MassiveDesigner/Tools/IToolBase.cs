
namespace MassiveDesinger
{
    namespace Tools
    {
        [UnityEngine.SerializeField]
        public abstract class ToolBase
        {
            public abstract void Initialize();
            public abstract void Refresh();
            public abstract bool IsOK();
        }
    }
}
