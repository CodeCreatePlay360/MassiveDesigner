using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.AutoInspector;
using MassiveDesinger.FastTiles;
using MassiveDesinger.Utils;


namespace MassiveDesinger
{
    [System.Serializable]
    public class MassiveDesigner : MonoBehaviour, ISerializationCallbackReceiver
    {
        [System.Serializable]
        public class Externals
        {
            public struct UnityTreeInstanceData
            {
                public Vector3 worldPos;
                public int treePrototypeIndex;


                public UnityTreeInstanceData(Vector3 worldPos, int treePrototypeIndex)
                {
                    this.worldPos = worldPos;
                    this.treePrototypeIndex = treePrototypeIndex;
                }
            }


            [EditorFieldAttr(ControlType.boldLabel, "External References")]
            public string label = "";

            [EditorFieldAttr(ControlType.unityTerrainField, "unityTerrain")]
            public static Terrain unityTerrain = null;

            public static TerrainData terrainData = null;
            public static TreePrototype[] treePrototypes = default;
            public static DetailPrototype[] detailPrototypes = default;
            public static int numSplatLayers = -1;

            private AutoInspector autoInspector = null;

            public AutoInspector AutoInspector
            {
                get
                {
                    if (autoInspector == null)
                    {
                        System.Object obj = this;
                        autoInspector = new AutoInspector(typeof(Externals), ref obj);
                    }
                    return autoInspector;
                }
            }


            public static void Enable()
            {
                unityTerrain = Terrain.activeTerrain;
                terrainData = unityTerrain != null ? unityTerrain.terrainData : null;
                numSplatLayers = terrainData != null ? terrainData.alphamapLayers : -1;
            }

            public static bool VerifyUnityTerrainItem(GameObject prototype, PaintMesh.ItemType itemType)
            {
                // should have a mesh filter and mesh renderer compnent
                if (!prototype.GetComponent<MeshFilter>() || !prototype.GetComponent<MeshRenderer>())
                {
                    Debug.LogErrorFormat("[MassiveDesigner] Prototype {0} missing MeshFilter or MeshRenderer component.", prototype.name);
                    return false;
                }

                // trees can contain 2 materials at most, details can have 1 material per object
                Material[] materials = prototype.GetComponent<MeshRenderer>().sharedMaterials;
                if (materials.Length == 0)
                    return false;

                if (itemType == PaintMesh.ItemType.Trees && materials.Length > 3)
                { Debug.LogErrorFormat("[MassiveDesigner] Tree {0} has more then 2 materials this might reduce performance", prototype); return false; }

                if (itemType == PaintMesh.ItemType.GrassAndGroundCover && materials.Length > 1)
                { Debug.LogErrorFormat("[MassiveDesigner] GrassAndGroundCover objects can contain only 1 materials per object {0}.", prototype); return false; }

                // materials should support instancing
                for (int i = 0; i < materials.Length; i++)
                {
                    if (!materials[i].enableInstancing)
                    {
                        materials[i].enableInstancing = true;
                        Debug.LogWarningFormat("[MassiveDesigner] Materaials should support instancing.");
                    }
                }

                // log a warning if there is no LOD group
                if (!prototype.GetComponent<LODGroup>())
                    Debug.LogWarningFormat("[MassiveDesigner] prototype {0} have no LODGroup component.", prototype.name);

                return true;
            }

            public static void UpdateTerrainPrototypes(List<Layer> layers)
            {
                if (unityTerrain == null || terrainData == null)
                    Enable();

                if (unityTerrain == null || terrainData == null)
                    return;


                List<PaintMesh> paintMeshes = new();  // list of all PaintMeshes
                int numTrees = 0;
                int numDetailObjects = 0;
                foreach (var layer in layers)
                {
                    foreach (var pm in layer.paintMeshes)
                    {
                        if (pm != null)
                        {
                            if (pm.properties.itemType == PaintMesh.ItemType.Trees)
                                numTrees++;

                            if (pm.properties.itemType == PaintMesh.ItemType.GrassAndGroundCover)
                                numDetailObjects++;

                            paintMeshes.Add(pm);
                        }
                    }
                }

                treePrototypes = new TreePrototype[numTrees];
                detailPrototypes = new DetailPrototype[numDetailObjects];
                int treeObjIdx = 0;
                int detailObjIdx = 0;
                for (int i = 0; i < paintMeshes.Count; i++)
                {
                    if (paintMeshes[i].properties.itemType == PaintMesh.ItemType.Trees)
                    {
                        treePrototypes[treeObjIdx] = new TreePrototype { prefab = paintMeshes[i].gameObject };
                        paintMeshes[i].terrainItemIdx = treeObjIdx;
                        treeObjIdx++;
                    }


                    if (paintMeshes[i].properties.itemType == PaintMesh.ItemType.GrassAndGroundCover)
                    {
                        detailPrototypes[detailObjIdx] = new DetailPrototype
                        {
                            useInstancing = true,
                            usePrototypeMesh = true,
                            prototype = paintMeshes[i].gameObject,
                            renderMode = DetailRenderMode.VertexLit,

                            minHeight = paintMeshes[i].properties.scaleMultiplier,
                            maxHeight = paintMeshes[i].properties.scaleMultiplier,

                            minWidth = paintMeshes[i].properties.scaleMultiplier,
                            maxWidth = paintMeshes[i].properties.scaleMultiplier,
                        };
                        paintMeshes[i].terrainItemIdx = detailObjIdx;
                        detailObjIdx++;
                    }
                }

                terrainData.treePrototypes = treePrototypes;
                terrainData.detailPrototypes = detailPrototypes;
            }

            public static float GetTerrainLayerStrength(Vector3 pos, int layerIndex, Vector3 terrainPos)
            {
                float[] GetTextureMix(Vector3 WorldPos)
                {
                    // calculate which splat map cell the worldPos falls within (ignoring y)
                    float mapX = (float)(((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
                    float mapZ = (float)(((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

                    // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
                    float[,,] splatmapData = terrainData.GetAlphamaps((int)mapX, (int)mapZ, 1, 1);

                    // extract the 3D array data to a 1D array
                    float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

                    for (int n = 0; n < cellMix.Length; n++)
                    {
                        cellMix[n] = splatmapData[0, 0, n];
                    }

                    return cellMix;
                }
                     
                float GetTextureThreshold(Vector3 WorldPos)
                {
                    float[] mix = GetTextureMix(WorldPos);
                    return mix[layerIndex];
                }

                return GetTextureThreshold(pos);
            }
        }


        // public
        // tools
        public Externals externals = new();
        public Tools.FoliagePainter foliagePainter = new();
        public Tools.GrassPainter grassPainter = new();
        public LocationTool.LocationTool locationTool = new();

        // 
        public SaveDataFile saveFile = null;
        public Layer.Settings layerCopiedSettings = null;

        // private
        [SerializeField] private List<Layer> layers = new();
        [SerializeField] private int selectedLayerIdx = 0;
        [SerializeField] public bool tilesOK = false;
        public FastTiles.FastTiles spawnTiles = new FastTiles.FastTiles();

        // editor
        public int currentTabIndex = 0; // geo paint editor = 0, spline editor = 1


        Mesh _debugMeshSphere;
        Mesh _debugMeshHalfSphere;

        public Mesh DebugMeshSphere
        {
            get
            {
                if (_debugMeshSphere == null)
                {
                    var sphere = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/MassiveDesigner/Resources/Sphere.fbx", typeof(GameObject)) as GameObject;
                    _debugMeshSphere = sphere.GetComponent<MeshFilter>().sharedMesh;
                }
                return _debugMeshSphere;
            }
        }

        public Mesh DebugMeshHalfSphere
        {
            get
            {
                if (_debugMeshHalfSphere == null)
                {
                    var halfSphere = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/MassiveDesigner/Resources/HalfSphere.fbx", typeof(GameObject)) as GameObject;
                    _debugMeshHalfSphere = halfSphere.GetComponent<MeshFilter>().sharedMesh;
                }
                return _debugMeshHalfSphere;
            }
        }


        // getter / setters
        public List<Layer> Layers { get { return layers; } }

        public bool IsDirty { get; set; } = false;

        public bool IsSerialized { get; set; } = false;

        public int SelectedLayerIdx { get { return selectedLayerIdx; } }
          
        public Layer SelectedLayer
        {
            get
            {
                if (selectedLayerIdx >= 0 && selectedLayerIdx < layers.Count - 1)
                {
                    return layers[selectedLayerIdx];
                }
                else if (layers.Count > 0)
                {
                    return layers[selectedLayerIdx];
                }
     
                return null;
            }
        }

        // statics
        public static MassiveDesigner Instance = null;
        public static readonly int INVALID_INDEX = -1;
        public static readonly string VERSION = "v0.1";

        public static Vector3 RandScale(float scaleVar, Vector3 currentScale)
        {
            float rand = UnityEngine.Random.Range(0, scaleVar);
            return new Vector3()
            {
                x = currentScale.x + rand,
                y = currentScale.y + rand,
                z = currentScale.z + rand,
            };
        }

        public static Quaternion RandQuat(Vector3 originalRotation)
        {
            Quaternion randQuat = Quaternion.Euler(originalRotation.x, UnityEngine.Random.Range(originalRotation.y, 360f), originalRotation.z);
            return randQuat;
        }

        private void OnEnable()
        {
            Enable();
        }

        public void OnValidate()
        {
            Enable();
        }

        private void Start()
        {
            Enable();
        }

        public void OnDrawGizmos()
        {
            foliagePainter.OnGizmos();
            locationTool.OnGizmos();
        }

        public void Enable()
        {
            Instance = this;
            gameObject.name = "MassiveDesigner"; 
            gameObject.transform.position = Vector3.zero;

            tilesOK = spawnTiles != null && spawnTiles.cellDict != null && spawnTiles.cellDict.Count > 0;

            if (SelectedLayer != null)
                SelectedLayer.OnEnable();

            // tools
            foliagePainter.Initialize();
            grassPainter.Initialize();
            locationTool.Initialize();

            //
            UpdateLayersData();

            // others
            Externals.Enable();
            IsDirty = true;
        }
          
        public void InitTiles()
        {
            spawnTiles.Create();
            tilesOK = spawnTiles != null && spawnTiles.cellDict != null && spawnTiles.cellDict.Count > 0;
        }

        public void UpdateLayersData()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].layerIndex = i;
                layers[i].SortPaintMeshes();
                layers[i].UpdatePaintMeshes(layers[i].settings.priorityIdx);
                layers[i].GetSplatLayers();
            }

            Debug.Log("[MassiveDesigner] Updated layers data.");
        }

        public void AddLayer()
        {
            layers.Add(new Layer());
            layers[^1].OnSelect();
            selectedLayerIdx = layers.Count - 1;
            UpdateLayersData();
        }

        public void SelectLayer(int idx)
        {
            selectedLayerIdx = idx;
            SelectedLayer.OnSelect();
        }

        public void RemoveLayer(int idx)
        {
            // one layer must always exist...!
            if (idx == 0)
                return;

            RemoveAllItems(onLayer:true, layerIdx:layers[idx].layerIndex);

            layers.RemoveAt(idx);

            selectedLayerIdx--;
            if (selectedLayerIdx < 0) // sometimes this can happen
                selectedLayerIdx = 0;

            UpdateLayersData();
            Externals.UpdateTerrainPrototypes(layers);

            SelectedLayer.OnSelect();
        }

        public void RemoveAllItems(bool onLayer = false, int layerIdx=-1, PaintMesh paintMesh=null)
        {
            // collect all tree instances to be removed
            List<Vector2> toBeRemoved = new();
            Tile tile;

            foreach (var key in spawnTiles.cellDict.Keys)
            {
                for (int i = 0; i < spawnTiles.cellDict[key].kdTree.Points.Length; i++)
                {
                    tile = spawnTiles.cellDict[key];

                    if(onLayer)
                    {
                        if (tile.kdTree.Points[i].data.layerIdx == layerIdx)
                        {
                            toBeRemoved.Add(new Vector2(tile.kdTree.Points[i].pos.x, tile.kdTree.Points[i].pos.z));
                            tile.kdTree.Points[i] = null;
                        }
                    }
                    else
                    {
                        if (tile.kdTree.Points[i].data.paintMesh == paintMesh)
                        {
                            toBeRemoved.Add(new Vector2(tile.kdTree.Points[i].pos.x, tile.kdTree.Points[i].pos.z));
                            tile.kdTree.Points[i] = null;
                        }
                    }

                }
            }

            TreeInstance[] existingTrees = MassiveDesigner.Externals.terrainData.treeInstances;
            existingTrees = existingTrees.Where(c => !toBeRemoved.Contains(new Vector2(c.position.x, c.position.z))).ToArray();
            Externals.terrainData.SetTreeInstances(existingTrees, snapToHeightmap: false);

            foreach (var key in spawnTiles.cellDict.Keys)
            {
                spawnTiles.cellDict[key].RemoveNullData();
            }
        }

        public void AddInstancedMesh(PaintMesh paintMesh, Vector3 pos, Vector3 scale, Quaternion rot, TreeInstance treeInstance, int layerIdx, int priorityIdx)
        {
            var foundTiles = spawnTiles.GetTileAtPos(pos);
            TileData data = new TileData(paintMesh, pos, rot, scale, treeInstance, layerIdx, priorityIdx);
            foundTiles.AddCellData(new TileDataObj(pos, data));
        }

        public void AddData(Dictionary<Tile, List<TileDataObj>> data)
        {
            foreach (var tile in data.Keys)
            {
                tile.AddTileData(data[tile].ToArray());
            }
        }

        // =================================Cache====================================== //
        float r1, r2 = -1f;
        float radius;
        Vector3 pos_1, pos_2 = Vector2.zero;
        List<Tile> foundTiles;
        List<TileDataObj> foundData = new List<TileDataObj>();
        TileDataObj tileData;
        bool canSpawn = false;
        public List<int> queriedIndexes = null;
        public List<Vector3> treeInstances = new List<Vector3>();

        public bool CanSpawn(Vector3 atPos, PaintMesh paintMesh, Vector3 scale, bool removeLowerPriorityObjs = false)
        {
            canSpawn = true;

            radius = paintMesh.properties.itemType == PaintMesh.ItemType.GrassAndGroundCover ? paintMesh.properties.firstColliderRadius : 
                paintMesh.properties.secondColliderRadius;
            radius *= 10f;
            foundTiles = spawnTiles.TilesInRadius(atPos, radius);

            for (int i = 0; i < foundTiles.Count; i++)
            {
                foundData.Clear();
                foundTiles[i].QueryNearestNeighbours(atPos, radius, ref queriedIndexes,  ref foundData);

                for (int j = 0; j < foundData.Count; j++)
                {
                    tileData = foundData[j];
                    if (Collision(paintMesh, scale, atPos, tileData))
                    {
                        canSpawn = false;

                        // save the collision 
                        if (removeLowerPriorityObjs && paintMesh.layerPriorityIdx > Layers[tileData.data.layerIdx].settings.priorityIdx)
                        {
                            treeInstances.Add(tileData.data.unityTreeInstance.position);
                            foundTiles[i].kdTree.Points[queriedIndexes[j]] = null;
                        }
                    }
                }
            }

            return canSpawn;
        }

        public bool CanSpawn(Vector3 atPos, PaintMesh paintMesh, Vector3 scale, ref TileDataObj[] fromData, bool removeLowerPriorityObjs=false)
        {
            canSpawn = true;

            for (int i = 0; i < fromData.Length; i++)
            {
                tileData = fromData[i];
                if (tileData != null)
                {
                    if (Collision(paintMesh, scale, atPos, tileData))
                    {
                        canSpawn = false;

                        // save the collision 
                        if (removeLowerPriorityObjs && paintMesh.layerPriorityIdx > Layers[tileData.data.layerIdx].settings.priorityIdx)
                        {
                            treeInstances.Add(tileData.data.unityTreeInstance.position);
                            fromData[i] = null;
                        }
                    }
                }
            }

            return canSpawn;
        }

        public bool Collision(PaintMesh paintMesh, Vector3 scale, Vector3 atPos, TileDataObj tileData)
        {
            // ** 1st collider to 1st collider collision
            // position and collision radius of 1st PaintMesh
            r1 = (scale.magnitude * paintMesh.properties.firstColliderRadius);
            pos_1 = atPos + paintMesh.properties.firstColliderOffset;

            // position and collision radius of 2nd PaintMesh
            r2 = (tileData.data.scale.magnitude * tileData.data.paintMesh.properties.firstColliderRadius);
            pos_2 = tileData.data.pos + tileData.data.paintMesh.properties.firstColliderOffset;

            if (CommonMaths.SphereCollision(pos_1, r1, pos_2, r2))
                return true;


            // if any one of PaintMeshes in using only one collider then return
            if (paintMesh.properties.useFirstColliderOnly || tileData.data.paintMesh.properties.useFirstColliderOnly)
                return false;

            // ------------------------------------------------------------------------------ //

            // ** 2nd collider to 2nd collider collision
            // position and collision radius of 2nd PaintMesh
            r1 = (scale.magnitude * paintMesh.properties.secondColliderRadius);
            pos_1 = atPos + paintMesh.properties.secondColliderOffset;

            // position and collision radius of 2nd PaintMesh
            r2 = (tileData.data.scale.magnitude * tileData.data.paintMesh.properties.secondColliderRadius);
            pos_2 = tileData.data.pos + tileData.data.paintMesh.properties.secondColliderOffset;

            if (CommonMaths.SphereCollision(pos_1, r1, pos_2, r2))
                return true;

            // ------------------------------------------------------------------------------ //

            // ** 1st collider to 2nd collider collision
            // position and collision radius of 1st PaintMesh
            r1 = (scale.magnitude * paintMesh.properties.firstColliderRadius);
            pos_1 = atPos + paintMesh.properties.firstColliderOffset;

            // position and collision radius of 2nd PaintMesh
            r2 = (tileData.data.scale.magnitude * tileData.data.paintMesh.properties.secondColliderRadius);
            pos_2 = tileData.data.pos + tileData.data.paintMesh.properties.secondColliderOffset;

            if (CommonMaths.SphereCollision(pos_1, r1, pos_2, r2))
                return true;

            return false;
        }

        public bool CanSpawnOnTex(Vector3 point, List<int> textureLayers)
        {
            bool CanSpawn(int splatLayer)
            {
                if ((1f - Externals.GetTerrainLayerStrength(point, splatLayer, Vector3.zero)) > UnityEngine.Random.Range(0f, 1f))
                    return false;
                return true;
            }
             
            for (int i = 0; i < textureLayers.Count; i++)
            {
                if (CanSpawn(textureLayers[i]))
                    return true;
            }

            return false;
        }

        public PaintMesh[] PaintMeshes(bool fromAllLayers, PaintMesh.ItemType itemsType)
        {
            List<PaintMesh> paintMeshes = new List<PaintMesh>();

            if(fromAllLayers)
            {
                for (int i = 0; i < Layers.Count; i++)
                {
                    if(Layers[i].settings.itemsType == itemsType)
                    {
                        for (int j = 0; j < Layers[i].paintMeshes.Count; j++)
                        {
                            if (Layers[i].paintMeshes[j] != null && Layers[i].paintMeshes[j].properties.isActive)
                                paintMeshes.Add(Layers[i].paintMeshes[j]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < SelectedLayer.paintMeshes.Count; i++)
                {
                    if (SelectedLayer.settings.itemsType == itemsType && SelectedLayer.paintMeshes[i] != null && SelectedLayer.paintMeshes[i].properties.isActive)
                        paintMeshes.Add(SelectedLayer.paintMeshes[i]);
                }
            }

            return paintMeshes.ToArray();
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }


        /// <summary>
        /// Clears all data from MassiveDesignerSpawnTiles
        /// </summary>
        public void ClearAll()
        {
            ClearUnityTerrainDetails();

            TreeInstance[] treeInstances = new TreeInstance[0];
            Externals.terrainData.SetTreeInstances(treeInstances, snapToHeightmap: true);
            InitTiles();
        }

        public void ClearUnityTerrainDetails()
        {
            var currentTerrainData = MassiveDesigner.Externals.terrainData;
            int[,] emptyMap = new int[currentTerrainData.detailWidth, currentTerrainData.detailHeight];
            for (int i = 0; i < currentTerrainData.detailPrototypes.Length; i++)
                currentTerrainData.SetDetailLayer(0, 0, i, emptyMap);
        }

        #region Serialization
         
        public void OnBeforeSerialize()
        {
            return;
            if (IsDirty)
            {
                if (!tilesOK)
                    return;

                SaveDataToFile();

                IsDirty = false;
                IsSerialized = true;
            }
        }
 
        public void OnAfterDeserialize()
        {
            InitTiles();
        }
                  
        public void SaveDataToFile()
        {
            if(saveFile == null)
            {
                Debug.LogWarning("[MassiveDesigner] No savefile found...!");
                return;
            }

            saveFile.Save(spawnTiles);
        } 
            
        public void ReloadDataFromFile()
        {
            if (saveFile == null)
            {
                Debug.LogWarning("[MassiveDesigner] No savefile found...!");
                return;
            }

            UpdateLayersData();
            saveFile.Load(ref spawnTiles, ref Externals.unityTerrain);
            tilesOK = spawnTiles.isOk;
            UnityEditor.EditorUtility.SetDirty(saveFile);
        }
        #endregion
    }
}
