using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.AutoInspector;
using MassiveDesinger.Tools;


namespace MassiveDesinger
{
    namespace Tools
    {
        [System.Serializable]
        public class GrassPainter : ToolBase
        {
            public enum PaintMode
            {
                Global,
                Area,
            }

            [System.Serializable]
            public class Settings
            {
            }

            // 
            public Settings settings = new Settings();

            // editor fields
            public bool mainFd;


            AutoInspector autoInspector = default;
            public AutoInspector AutoInspector
            {
                get
                {
                    if (autoInspector == null)
                    {
                        System.Type t;
                        object obj;

                        t = settings.GetType();
                        obj = settings;
                        autoInspector = new AutoInspector(t, ref obj);
                    }

                    return autoInspector;
                }
            }

            public GrassPainter()
            {
                toolName = "__GrassPainter__";
            }

            public override void Initialize()
            {
            }

            public override bool IsOK()
            {
                return true;
            }

            public override void Refresh()
            {
            }
        }
    }
}
