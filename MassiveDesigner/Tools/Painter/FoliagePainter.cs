using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using MassiveDesinger.FastTiles;
using MassiveDesinger.Utils;
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
            /// <summary>
            /// Brush Settings
            /// </summary>
            [System.Serializable]
            public class BrushSettings
            {
                [EditorFieldAttr(ControlType.boldLabel, "PaintSettings")]
                [EditorFieldAttr(ControlType.BrushPaintMode, "paintMode")]
                public PaintMode paintMode = PaintMode.Normal;

                [EditorFieldAttr(ControlType.layerField, "layerMask")]
                public LayerMask layerMask;

                [FloatSliderAttr(ControlType.floatSlider, "paintRadius", 0.5f, 500)]
                public float paintRadius = 50f;

                [EditorFieldAttr(ControlType.boolField, "useAllLayers", layoutHorizontal:1)]
                public bool useAllLayers = false;

                [EditorFieldAttr(ControlType.boolField, "useWeightedProbability", layoutHorizontal:-1)]
                public bool useWeightedProbability = false;

                [EditorFieldAttr(ControlType.boolField, "overrideGroupLayerMask")]
                public bool overrideGroupLayerMask = true;

                [EditorFieldAttr(ControlType.boldLabel, "RemoveSettings")]
                public string eraseSettingsLabel = "";

                [FloatSliderAttr(ControlType.floatSlider, "removeRadius", 0.5f, 500)]
                public float removeRadius = 80f;

                // [FloatSliderAttr(ControlType.floatSlider, "removeStrength", 0.1f, 1f)]
                // public float removeStrength = 1f;

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

            // float gizmoRadius = 0;
            public override void OnGizmos()
            {
                //if (!isPainting)
                //    return;

                //if (Event.current.shift)
                //    gizmoRadius = settings.removeRadius;
                //else
                //    gizmoRadius = settings.paintRadius;

                //Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
                //Gizmos.DrawMesh(MassiveDesigner.Instance.DebugMeshHalfSphere, 0, targetPos, Quaternion.identity, Vector3.one * gizmoRadius);
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
                MassiveDesigner.Externals.UpdateTerrainPrototypes(MassiveDesigner.Instance.Layers);
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
                    await Paint();
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

            public async Task Paint()
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

                // 
                PaintMesh[] paintMeshes = MassiveDesigner.Instance.PaintMeshes(settings.useAllLayers,
                    MassiveDesigner.Instance.SelectedLayer.settings.itemsType);
                float[] weights = new float[paintMeshes.Length];
                for (int i = 0; i < paintMeshes.Length; i++)
                    weights[i] = paintMeshes[i].properties.spawnProbability;
                int choice;

                //
                bool canSpawn = false;
                var layerMask = 1 << (settings.overrideGroupLayerMask ? settings.layerMask : MassiveDesigner.Instance.SelectedLayer.settings.layerMask);

                List<Tile> foundTiles = new List<Tile>();
                List<Vector3> toBeRemovedTrees = new();
                int count = 0;

                while (isPainting && paintMeshes.Length > 0)
                {
                    if (mouseInEditorWin)
                    {
                        MassiveDesigner.Instance.treeInstances.Clear();

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

                                scaleVariation = UnityEngine.Random.Range(0.01f, paintMesh.properties.scaleVariation);
                                randScale = new()
                                {
                                    x = (paintMesh.transform.localScale.x * paintMesh.properties.scaleMultiplier) * (1f - scaleVariation),
                                    y = (paintMesh.transform.localScale.y * paintMesh.properties.scaleMultiplier) * (1f - scaleVariation),
                                    z = (paintMesh.transform.localScale.z * paintMesh.properties.scaleMultiplier) * (1f - scaleVariation),
                                };
                                // randScale = Vector3.one;
                                randRot = Quaternion.identity;

                                // check if can spawn on this terrain texture
                                if (MassiveDesigner.Instance.Layers[paintMesh.layerIdx].settings.useTerrainTextureStrength)
                                {
                                    if (!MassiveDesigner.Instance.CanSpawnOnTex(raycastHit.point, MassiveDesigner.Instance.Layers[paintMesh.layerIdx].splatLayers))
                                        continue;
                                }

                                canSpawn = MassiveDesigner.Instance.CanSpawn(raycastHit.point, paintMesh, randScale,
                                    removeLowerPriorityObjs:settings.paintMode == PaintMode.Replace) &&
                                    MassiveDesigner.Instance.CanSpawn(raycastHit.point, paintMesh, randScale, ref newSpawnedTileDataObjs,
                                    removeLowerPriorityObjs: settings.paintMode == PaintMode.Replace);

                                if (canSpawn)
                                {
                                    // -------------------------------------------------------------------------------------------------------------
                                    // UNITY TERRAIN TREE
                                    unityTreePos = new()
                                    {
                                        x = CommonMaths.ConvertToRange(0f, MassiveDesigner.Externals.terrainData.size.x, 0f, 1f, raycastHit.point.x),
                                        y = CommonMaths.ConvertToRange(0f, MassiveDesigner.Externals.terrainData.size.y, 0f, 1f, raycastHit.point.y),
                                        z = CommonMaths.ConvertToRange(0f, MassiveDesigner.Externals.terrainData.size.z, 0f, 1f, raycastHit.point.z)
                                    };
                                    treeInstance.prototypeIndex = paintMesh.terrainItemIdx;
                                    treeInstance.widthScale = randScale.x;
                                    treeInstance.heightScale = randScale.z;
                                    treeInstance.rotation = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                                    treeInstance.position = unityTreePos;
                                    newTrees.Add(treeInstance);
                                    // -------------------------------------------------------------------------------------------------------------

                                    tempTileData = new TileData(paintMesh, raycastHit.point, randRot, randScale, treeInstance, paintMesh.layerIdx, paintMesh.layerPriorityIdx);
                                    tempTileDataObj = new TileDataObj(raycastHit.point, tempTileData);
                                    newSpawnedTileDataObjs[i] = tempTileDataObj;

                                    tileAtPos = MassiveDesigner.Instance.spawnTiles.GetTileAtPos(raycastHit.point);
                                    if (!tileAndData.ContainsKey(tileAtPos))
                                        tileAndData[tileAtPos] = new List<TileDataObj>();
                                    tileAndData[tileAtPos].Add(tempTileDataObj);
                                }
                            }
                        }

                        if (mouseInEditorWin)
                        {
                            if(settings.paintMode == PaintMode.Replace)
                            {
                                newTrees = newTrees.Where(c => !MassiveDesigner.Instance.treeInstances.Contains(c.position)).ToList();
                            }

                            MassiveDesigner.Instance.AddData(tileAndData);
                            MassiveDesigner.Externals.terrainData.SetTreeInstances(newTrees.ToArray(), snapToHeightmap: false);
                            newTrees = MassiveDesigner.Externals.terrainData.treeInstances.ToList();
                            MassiveDesigner.Instance.IsDirty = true;
                        }
                    }
                     
                    await Task.Delay(TimeSpan.FromSeconds(settings.spawnDelay));
                }
                // Debug.Log("End spawn");
            }

            public void ClearPaint(Ray ray)
            {
                TreeInstance[] existingTrees = MassiveDesigner.Externals.terrainData.treeInstances;
                List<Vector3> toBeRemovedTrees = new();

                List<int> queriedIndexes = null;
                List<TileDataObj> foundData = new List<TileDataObj>();
                List<Tile> foundTiles;
                TileDataObj tileData;

                // var layerMask = 1 << MassiveDesigner.Instance.SelectedLayer.settings.layerMask;

                if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo, Mathf.Infinity))
                {
                    foundTiles = MassiveDesigner.Instance.spawnTiles.TilesInRadius(hitInfo.point, settings.removeRadius);
                     
                    for (int i = 0; i < foundTiles.Count; i++)
                    {
                        foundData.Clear();
                        foundTiles[i].QueryNearestNeighbours(
                            hitInfo.point,
                            settings.removeRadius,
                            ref queriedIndexes,
                            ref foundData,
                            ref toBeRemovedTrees,
                            true, onlyOnSelectedLayer:settings.removeOnlyOnSelectedLayer, 
                            selectedLayer:MassiveDesigner.Instance.SelectedLayerIdx);

                        //for (int j = 0; j < foundData.Count; j++)
                        //{
                        //    tileData = foundData[j];

                        //    if (settings.removeOnlyOnSelectedLayer)
                        //    {
                        //        if(tileData.data.layerIdx == MassiveDesigner.Instance.SelectedLayerIdx)
                        //            toBeRemovedTrees.Add(tileData.data.unityTreeInstance.position);
                        //    }
                        //    else
                        //    {
                        //        toBeRemovedTrees.Add(tileData.data.unityTreeInstance.position);
                        //    }
                        //}

                        // this works because all queried items are set to null in QueryNearestNeighbours
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
