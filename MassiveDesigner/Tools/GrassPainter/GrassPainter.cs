using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.AutoInspector;


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
                [EditorFieldAttr(ControlType.boldLabel, "PaintSettings")]
                [EditorFieldAttr(ControlType.PaintMode, "paintMode")]
                public PaintMode paintMode = PaintMode.Global;

                [EditorFieldAttr(ControlType.boolField, "useAllLayers")]
                public bool useAllLayers = false;

                [EditorFieldAttr(ControlType.boolField, "useWeightedProbability", layoutHorizontal:1)]
                public bool useWeightedProbability = false;

                [EditorFieldAttr(ControlType.boolField, "useSimulation", layoutHorizontal: -1)]
                public bool useSimulation = false;

                [IntSliderAttr(ControlType.intSlider, "density", 1, 5)]
                public int density;

                [IntSliderAttr(ControlType.intSlider, "seedCount", 1, 500)]
                public int seedCount;

                [EditorFieldAttr(ControlType.boldLabel, "RemoveSettings")]
                [EditorFieldAttr(ControlType.boolField, "removeOnlyOnSelectedLayer")]
                public bool removeOnlyOnSelectedLayer = true;

                [EditorFieldAttr(ControlType.boldLabel, "Debug")]
                [EditorFieldAttr(ControlType.boolField, "debug")]
                public bool debug = false;
            }

            public Settings settings = new Settings();
            AutoInspector autoInspector = default;

            // private
            public Vector3[] parallelogramVertices = null;


            // editor fields
            public bool mainFd;

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

            public Vector3[] ParallelogramVertices
            {
                get
                {
                    parallelogramVertices = new Vector3[4];
                    parallelogramVertices[0] = Vector3.zero * MassiveDesigner.Externals.terrainData.detailWidth;
                    parallelogramVertices[1] = Vector3.right * MassiveDesigner.Externals.terrainData.detailWidth;
                    parallelogramVertices[2] = (Vector3.right + Vector3.forward) * MassiveDesigner.Externals.terrainData.detailWidth;
                    parallelogramVertices[3] = Vector3.forward * MassiveDesigner.Externals.terrainData.detailWidth;

                    return parallelogramVertices;
                }
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

            public void Scatter()
            {
                MassiveDesigner.Externals.UpdateTerrainPrototypes(MassiveDesigner.Instance.Layers);

                // This works by
                // 1. collecting PaintMeshes from all groups, assigning them an index and
                // mapping them to their corresponding (unity terrain's) density map.
                // 2. Then going over each pixel in terrains density detail map and for each pixel selects a density map and 
                // assigning it a value.

                var currentTerrainData = MassiveDesigner.Externals.terrainData;
                Dictionary<int, int[,]> detailObjAndMap = new Dictionary<int, int[,]>();
                List<float> weights = new List<float>();
                Dictionary<int, List<Vector3>> paintMeshesIdxAndseedPositionsMap = new Dictionary<int, List<Vector3>>();  
                int size = currentTerrainData.detailWidth;

                // collect prototypes for painting
                if (settings.useAllLayers)
                {
                    int paintMeshIdx = 0;
                    for (int i = 0; i < MassiveDesigner.Instance.Layers.Count; i++)
                    {
                        if (MassiveDesigner.Instance.Layers[i].settings.itemsType == PaintMesh.ItemType.GrassAndGroundCover)
                        {
                            for (int j = 0; j < MassiveDesigner.Instance.Layers[i].paintMeshes.Count - 1; j++)
                            {
                                detailObjAndMap[paintMeshIdx] = currentTerrainData.GetDetailLayer(0, 0, size, size, 0);
                                weights.Add(MassiveDesigner.Instance.Layers[i].paintMeshes[i].properties.spawnProbability);
                                paintMeshIdx++;
                            }
                        }
                    }
                }
                else if (MassiveDesigner.Instance.SelectedLayer.settings.itemsType == PaintMesh.ItemType.GrassAndGroundCover)
                {
                    for (int i = 0; i < MassiveDesigner.Instance.SelectedLayer.paintMeshes.Count - 1; i++)
                    {
                        detailObjAndMap[i] = currentTerrainData.GetDetailLayer(0, 0, size, size, i);
                        weights.Add(MassiveDesigner.Instance.SelectedLayer.paintMeshes[i].properties.spawnProbability);
                    }
                }

                if(detailObjAndMap.Count == 0)
                    return;

                float textureStrengthAtPos;
                int choice;

                if (settings.useSimulation)
                {
                    for (int i = 0; i < settings.seedCount; i++)
                    {
                        if (settings.useWeightedProbability)
                        {
                            // weighted
                            choice = CodeCreatePlay.Utils.CommonMaths.WeightedChoice(weights);
                        }
                        else
                        {
                            // random
                            choice = UnityEngine.Random.Range(0, detailObjAndMap.Count);
                            Vector3 randPos = CodeCreatePlay.UniformDistributions.GetUniformInParallelogram(ParallelogramVertices, 1);
                            if (!paintMeshesIdxAndseedPositionsMap.ContainsKey(choice))
                                paintMeshesIdxAndseedPositionsMap[choice] = new List<Vector3>();
                            paintMeshesIdxAndseedPositionsMap[choice].Add(randPos);
                        }
                    }
                }

                int GetClosest(Vector3 worldPos)
                {
                    int closestPoint = 0;
                    float closestDistance = float.MaxValue;
                    float currDistance;
                    foreach (var key in paintMeshesIdxAndseedPositionsMap.Keys)
                    {
                        for (int i = 0; i < paintMeshesIdxAndseedPositionsMap[key].Count; i++)
                        {
                            currDistance = Vector3.Distance(paintMeshesIdxAndseedPositionsMap[key][i], worldPos);
                            if(currDistance < closestDistance)
                            {
                                closestPoint = key;
                                closestDistance = currDistance;
                            }
                        }
                    }
                    return closestPoint;
                }


                // for each pixel in the detail map...
                Vector3 worldPos = new();
                Vector3 posOnGrid = new();

                if(settings.useSimulation)
                {
                    for (int i = 0; i < size*size; i++)
                    {
                        worldPos = CodeCreatePlay.UniformDistributions.GetUniformInParallelogram(ParallelogramVertices, 1);

                        int row = (int)(size / worldPos.x);
                        int col = (int)(size / worldPos.z);

                        detailObjAndMap[GetClosest(worldPos)][row, col] = settings.density;
                    }
                }
                else
                {
                    for (var y = 0; y < currentTerrainData.detailHeight; y++)
                    {
                        for (var x = 0; x < currentTerrainData.detailWidth; x++)
                        {
                            if (settings.useWeightedProbability)
                            {
                                // weighted
                                choice = CodeCreatePlay.Utils.CommonMaths.WeightedChoice(weights);
                                detailObjAndMap[choice][x, y] = settings.density;
                            }
                            else
                            {
                                // random
                                choice = UnityEngine.Random.Range(0, detailObjAndMap.Count);
                                detailObjAndMap[choice][x, y] = settings.density;
                            }
                        }
                    }
                }

                for (int i = 0; i < detailObjAndMap.Count; i++)
                {
                    currentTerrainData.SetDetailLayer(0, 0, i, detailObjAndMap[i]);
                }
            }

            public void ClearGrass()
            {
                var currentTerrainData = MassiveDesigner.Externals.terrainData;
                int[,] emptyMap = new int[currentTerrainData.detailWidth, currentTerrainData.detailHeight];
                for (int i = 0; i < currentTerrainData.detailPrototypes.Length; i++)
                {
                    currentTerrainData.SetDetailLayer(0, 0, i, emptyMap);
                }
            }
        }
    }
}
