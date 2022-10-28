using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CodeCreatePlay.AutoInspector;
using UnityEngine;
using CodeCreatePlay.LocationTool;
using System.Linq;
using CodeCreatePlay.Utils;


namespace MassiveDesinger
{
    namespace Tools
    {
        public enum AreaShape
        {
            Parallelogram,
            Polygon,
        }

        [System.Serializable]
        public class AreaScatterTool : ToolBase
        {
            [System.Serializable]
            public class Settings
            {
                [EditorFieldAttr(ControlType.AreaShape, "areaShape")]
                public AreaShape areaShape = AreaShape.Parallelogram;

                [EditorFieldAttr(ControlType.boldLabel, "ParallelogramSettings")]
                [EditorFieldAttr(ControlType.sceneGameObj, "referenceObject")]
                public GameObject referenceObject = null;

                [IntSliderAttr(ControlType.intSlider, "areaScale", 20, 1000)]
                public int areaScale = 10;

                [EditorFieldAttr(ControlType.boldLabel, "PolygonSettings")]
                public int polygon;

                [EditorFieldAttr(ControlType.boldLabel, "SpawnSettings")]
                [IntSliderAttr(ControlType.intSlider, "spawnCount", 1000, 100000)]
                public int spawnCount = 1000;

                [IntSliderAttr(ControlType.intSlider, "spawnCountPerIteration", 10, 100)]
                public int spawnCountPerIteration = 50;

                [EditorFieldAttr(ControlType.boldLabel, "DebugSettings")]
                [EditorFieldAttr(ControlType.boolField, "debug")]
                public bool debug = false;
            }

            public Settings settings = new Settings();
            public AutoInspector autoInspector = null;
            public int currentSpawnCount = 0;
            public bool IsRunning = false;
             

            public override void Initialize()
            {
                IsRunning = false;
            }

            public AutoInspector AutoInspector
            {
                get
                {
                    if(autoInspector == null)
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



            public override bool IsOK()
            {
                return true;
            }

            public override void Refresh()
            {
            }

            public async void BeginScatter()
            {
            }
        }
    }
}
