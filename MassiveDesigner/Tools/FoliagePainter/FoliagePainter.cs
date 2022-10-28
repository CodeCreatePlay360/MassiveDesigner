using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.FastTiles;
using CodeCreatePlay.Utils;
using CodeCreatePlay.AutoInspector;


namespace MassiveDesinger
{
    namespace Tools
    {
        public enum PaintMode
        {
            Normal, 
            Replace,
        }

        [System.Serializable]
        public class FoliagePainter : ToolBase
        {
            public struct SpawnData
            {
                public PaintMesh paintMesh;
                public Vector3 pos;
            }

            /// <summary>
            /// Brush Settings
            /// </summary>
            [System.Serializable]
            public class BrushSettings
            {
                [EditorFieldAttr(ControlType.boldLabel, "PaintSettings")]
                [EditorFieldAttr(ControlType.BrushPaintMode, "paintMode")]
                public PaintMode paintMode = PaintMode.Normal;

                [FloatSliderAttr(ControlType.floatSlider, "paintRadius", 0.5f, 500)]
                public float paintRadius = 50f;

                [EditorFieldAttr(ControlType.boolField, "useAllLayers", layoutHorizontal:1)]
                public bool useAllLayers = false;

                [EditorFieldAttr(ControlType.boolField, "useWeightedProbability", layoutHorizontal:-1)]
                public bool useWeightedProbability = false;


                [EditorFieldAttr(ControlType.boldLabel, "RemoveSettings")]
                public string eraseSettingsLabel = "";

                [FloatSliderAttr(ControlType.floatSlider, "removeRadius", 0.5f, 500)]
                public float removeRadius = 80f;

                [FloatSliderAttr(ControlType.floatSlider, "removeStrength", 0.1f, 1f)]
                public float removeStrength = 1f;

                [EditorFieldAttr(ControlType.boolField, "removeOnlyOnSelectedLayer")]
                public bool removeOnlyOnSelectedLayer = true;


                [EditorFieldAttr(ControlType.boldLabel, "BrushSettings")]
                [FloatSliderAttr(ControlType.floatSlider, "spawnDelay", 0.001f, 0.1f)]
                public float spawnDelay = 0.01f;

                [IntSliderAttr(ControlType.intSlider, "paintStrength", 1, 3)]
                public int paintStrength = 1;
            }


            public Vector3 lastMouseHitPos = Vector3.zero;
            public Vector3 lastHitNormal = Vector3.zero;
            public bool paintBrushEnabled = false;

            public AutoInspector autoInspector = default;
            public bool isPainting = false;
            public bool mainFoldPanel = false;
            public bool mouseInEditorWin = false;

            [SerializeField] private BrushSettings settings = new BrushSettings();
            private Transform brushRefTransform = null;


            public BrushSettings Settings { get { return settings; } }
            public Transform BrushRefTransform
            {
                get
                {
                    if (!brushRefTransform)
                    {
                        if (MassiveDesigner.Instance.gameObject.transform.childCount > 0)
                            brushRefTransform = MassiveDesigner.Instance.gameObject.transform.Find("BrushRefObject");

                        if (brushRefTransform == null)
                        {
                            brushRefTransform = new GameObject("BrushRefObject").transform;
                            brushRefTransform.transform.parent = MassiveDesigner.Instance.transform;
                        }
                    }

                    return brushRefTransform;
                }
            }

            public override void Initialize()
            {
                isPainting = false;
                paintBrushEnabled = false;
                mouseInEditorWin = false;
            }

            public override bool IsOK()
            {
                return false;
            }

            public override void Refresh()
            {
            }

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

            public void OnBeforePaint()
            {
                MassiveDesigner.Externals.Enable();
                MassiveDesigner.Instance.UpdateLayersData();
            }

            Vector3 targetPos = Vector3.zero;
            public void Paint(Ray ray)
            {
                if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo, Mathf.Infinity))
                {
                    // BrushRefTransform.position = hitInfo.point;
                    targetPos = hitInfo.point;
                }
            }

            public async void BeginPaint()
            {
                if (!isPainting)
                {
                    // Debug.Log("[MassiveDesigner (FoliagePainter)] Begin Paint Operation");
                    isPainting = true;
                    mouseInEditorWin = false;
                    await Spawn();
                }
            }

            /// <summary>
            /// Should be called once, after end of spawn operation.
            /// </summary>
            public void EndPaint()
            {
                if (isPainting)
                {
                    isPainting = false;
                    mouseInEditorWin = false;
                    MassiveDesigner.Instance.MarkDirty();
                }
            }

            public async Task Spawn()
            {
                // ------------------------------
                // create a unity tree prototype
                TreeInstance treeInstance = new()
                {
                    prototypeIndex = 0,
                    heightScale = 0.05f,
                    widthScale = 0.05f,
                    color = Color.white,
                    lightmapColor = Color.white,
                    position = Vector3.zero,
                    rotation = UnityEngine.Random.Range(-180, 180)
                };
                TreeInstance[] existingTrees = MassiveDesigner.Externals.terrainData.treeInstances;
                List<TreeInstance> newTrees = existingTrees.ToList();
                Vector3 unityTreePos;
                // ------------------------------------------------------------------------------
                 
                int spawnsPerIteration = Mathf.RoundToInt(settings.paintRadius) * settings.paintStrength;

                float theta, r;
                float scaleVariation;
                PaintMesh paintMesh;

                Vector3 spawnPoint;
                Vector3 randScale;
                Quaternion randRot = Quaternion.identity;

                Tile tileAtPos;
                TileData tempTileData;
                TileDataObj tempTileDataObj;
                TileDataObj[] newSpawnedTileDataObjs = new TileDataObj[spawnsPerIteration];
                Dictionary<Tile, List<TileDataObj>> tileAndData = new Dictionary<Tile, List<TileDataObj>>();

                Dictionary<PaintMesh, float> paintMeshesAndWeights = MassiveDesigner.Instance.PaintMeshesToWeightsMap(settings.useAllLayers,
                    MassiveDesigner.Instance.SelectedLayer.settings.itemsType);
                Dictionary<int, List<int>> paintMeshesToSplatLayersMap = MassiveDesigner.Instance.PaintMeshesToTextureLayersMap(settings.useAllLayers,
                    MassiveDesigner.Instance.SelectedLayer.settings.itemsType);

                PaintMesh[] paintMeshes = paintMeshesAndWeights.Keys.ToArray();
                float[] weights = paintMeshesAndWeights.Values.ToArray();
                int choice;
                bool useTextureLayers = MassiveDesigner.Instance.SelectedLayer.settings.useTerrainTextureStrength;
                bool isReplaceMode = false;  // is paint mode set to replace ?
                bool canSpawn = false;

                var layerMask = 1 << MassiveDesigner.Instance.SelectedLayer.settings.layerMask;

                List<Tile> foundTiles = new List<Tile>();
                List<int> kdQueryResultIndices = new List<int>();
                List<Vector3> toBeRemovedTrees = new();
                int count = 0;

                while (isPainting && paintMeshes.Length > 0)
                {
                    if (mouseInEditorWin)
                    {
                        newSpawnedTileDataObjs = new TileDataObj[spawnsPerIteration];
                        foundTiles.Clear();
                        toBeRemovedTrees.Clear();
                        tileAndData.Clear();

                        for (int i = 0; i < spawnsPerIteration; i++)
                        {
                            // rand uniform point inside a circle
                            theta = 2 * Mathf.PI * UnityEngine.Random.Range(0f, 1f);
                            r = settings.paintRadius * Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 1 / 2f);
                            spawnPoint = new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
                            spawnPoint += targetPos;
                            spawnPoint += Vector3.up * 2625;

                            if (Physics.Raycast(spawnPoint, Vector3.down, out RaycastHit raycastHit, 2725, layerMask))
                            {
                                if (settings.useWeightedProbability)
                                {
                                    choice = CommonMaths.WeightedChoice(weights);
                                    paintMesh = paintMeshes[choice];
                                }
                                else
                                {
                                    choice = UnityEngine.Random.Range(0, paintMeshes.Length);
                                    paintMesh = paintMeshes[choice];
                                }

                                // paintMesh = worldEditor.SelectedLayer.GetPaintMesh();
                                scaleVariation = UnityEngine.Random.Range(0.01f, paintMesh.properties.scaleVariation);
                                randScale = new()
                                {
                                    x = (paintMesh.transform.localScale.x * paintMesh.properties.scaleMultiplier) * (1f - scaleVariation),
                                    y = (paintMesh.transform.localScale.y * paintMesh.properties.scaleMultiplier) * (1f - scaleVariation),
                                    z = (paintMesh.transform.localScale.z * paintMesh.properties.scaleMultiplier) * (1f - scaleVariation),
                                };
                                randScale = Vector3.one;
                                randRot = Quaternion.identity;

                                isReplaceMode = settings.paintMode == PaintMode.Replace;
                                if (isReplaceMode)
                                {
                                    canSpawn = MassiveDesigner.Instance.CanSpawn(raycastHit.point, paintMesh, randScale, newSpawnedTileDataObjs);
                                }
                                else
                                {
                                    canSpawn = MassiveDesigner.Instance.CanSpawn(raycastHit.point, paintMesh, randScale) &&
                                        MassiveDesigner.Instance.CanSpawn(raycastHit.point, paintMesh, randScale, newSpawnedTileDataObjs);
                                }

                                if (canSpawn)
                                {
                                    if (useTextureLayers && !MassiveDesigner.Instance.CheckTerrainTextureSpawnProbability(raycastHit.point,
                                        paintMeshesToSplatLayersMap[choice]))
                                        continue;

                                    // Unity terrain tree spawn
                                    unityTreePos = new()
                                    {
                                        x = CommonMaths.ConvertToRange(0f, MassiveDesigner.Externals.terrainData.size.x, 0f, 1f, raycastHit.point.x),
                                        y = CommonMaths.ConvertToRange(0f, MassiveDesigner.Externals.terrainData.size.y, 0f, 1f, raycastHit.point.y),
                                        z = CommonMaths.ConvertToRange(0f, MassiveDesigner.Externals.terrainData.size.z, 0f, 1f, raycastHit.point.z)
                                    };

                                    treeInstance.prototypeIndex = paintMesh.terrainItemIdx;
                                    treeInstance.widthScale = randScale.x;
                                    treeInstance.heightScale = randScale.z;
                                    treeInstance.rotation = 0.5f * Mathf.Deg2Rad;
                                    treeInstance.position = unityTreePos;
                                    newTrees.Add(treeInstance);

                                    tempTileData = new TileData(paintMesh, raycastHit.point, randRot, randScale, treeInstance, MassiveDesigner.Instance.SelectedLayer.layerIndex);
                                    tempTileDataObj = new TileDataObj(raycastHit.point, tempTileData);
                                    newSpawnedTileDataObjs[i] = tempTileDataObj;

                                    tileAtPos = MassiveDesigner.Instance.spawnTiles.GetTileAtPos(raycastHit.point);
                                    if (!tileAndData.ContainsKey(tileAtPos))
                                        tileAndData[tileAtPos] = new List<TileDataObj>();
                                    tileAndData[tileAtPos].Add(tempTileDataObj);

                                    // ------------------------------------------------------------------------------------------------ //
                                    if (isReplaceMode)
                                    {
                                        // check collisions aginst this spawned object and set them to null
                                        List<Tile> tiles = MassiveDesigner.Instance.spawnTiles.TilesInRadius(raycastHit.point, 3.5f);
                                        foreach (var item in tiles)
                                            if (!foundTiles.Contains(item))
                                                foundTiles.Add(item);

                                        for (int j = 0; j < tiles.Count; j++)
                                        {
                                            if (tiles[j].kdTree.Points.Length > 0)
                                            {
                                                tiles[j].QueryNearestNeighbours(raycastHit.point, 3.5f, ref kdQueryResultIndices);
                                                for (int k = 0; k < kdQueryResultIndices.Count; k++)
                                                {
                                                    tempTileDataObj = tiles[j].kdTree.Points[kdQueryResultIndices[k]];
                                                    if (MassiveDesigner.Instance.SelectedLayer.layerIndex == tempTileDataObj.data.layerIdx)
                                                    {
                                                        toBeRemovedTrees.Add(tempTileDataObj.data.unityTreePos);
                                                        tiles[j].kdTree.Points[kdQueryResultIndices[k]] = null;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // ------------------------------------------------------------------------------------------------ //
                                }
                            }
                        }

                        if (mouseInEditorWin)
                        {
                            if (isReplaceMode)
                            {
                                foreach (var item in foundTiles)
                                {
                                    item.RemoveNullData();
                                }

                                newTrees = newTrees.Where(c => !toBeRemovedTrees.Contains(c.position)).ToList();
                            }

                            //foreach (var item in tileAndData.Keys)
                            //    count += tileAndData[item].Count;

                            //Debug.LogFormat("Total points spawned: {0}", count);
                            //Debug.LogFormat("Total trees spawned: {0}", newTrees.Count);

                            MassiveDesigner.Instance.AddData(tileAndData);
                            MassiveDesigner.Externals.terrainData.SetTreeInstances(newTrees.ToArray(), snapToHeightmap: false);
                            newTrees = MassiveDesigner.Externals.terrainData.treeInstances.ToList();
                        }
                    }
                     
                    await Task.Delay(TimeSpan.FromSeconds(settings.spawnDelay));
                }
                // Debug.Log("End spawn");
            }

            public SpawnData[] GeneratePaintMeshes()
            {
                SpawnData[] spawnData = new SpawnData[100];

                for (int i = 0; i < 100; i++)
                {
                    // generate a random point


                    // get a paint mesh


                    // wrap into SpawnDataObject


                    // add to array

                }

                return spawnData;
            }

            public void ClearPaint(Ray ray)
            {
                TreeInstance[] existingTrees = MassiveDesigner.Externals.terrainData.treeInstances;
                List<Vector3> toBeRemovedTrees = new();

                List<TileDataObj> foundData = new List<TileDataObj>();
                List<Tile> foundTiles;
                TileDataObj tileData;

                var layerMask = 1 << MassiveDesigner.Instance.SelectedLayer.settings.layerMask;

                if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo, Mathf.Infinity, layerMask))
                {
                    foundTiles = MassiveDesigner.Instance.spawnTiles.TilesInRadius(hitInfo.point, settings.removeRadius);

                    for (int i = 0; i < foundTiles.Count; i++)
                    {
                        foundData.Clear();
                        foundTiles[i].QueryNearestNeighbours(hitInfo.point, settings.removeRadius, ref foundData, true);

                        for (int j = 0; j < foundData.Count; j++)
                        {
                            tileData = foundData[j];
                            toBeRemovedTrees.Add(tileData.data.unityTreePos);
                        }

                        foundTiles[i].RemoveNullData();
                    }

                    existingTrees = existingTrees.Where(c => !toBeRemovedTrees.Contains(c.position)).ToArray();
                    MassiveDesigner.Externals.terrainData.SetTreeInstances(existingTrees, snapToHeightmap: false);
                    MassiveDesigner.Instance.IsDirty = true;
                }
            }

            public void Reset()
            {
                BrushRefTransform.transform.position = Vector3.zero;
                EndPaint();
            }
        }
    }
}
