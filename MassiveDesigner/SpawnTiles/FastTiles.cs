using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.AutoInspector;
using DataStructures.ViliWonka.KDTree;


namespace MassiveDesinger
{
    namespace FastTiles
    {
        [System.Serializable]
        public class Tile
        {
            public Vector3 worldPos = Vector3.zero;
            public int diameter = 0;

            // KD tree
            public KDTree kdTree = null;
            public KDQuery kdQuery = null;
            public readonly int Array_Length_Increment = 3;
               
            private bool isDirty = false;
            private bool hasKdTree = false;
            public bool isOpen = false;


            // four corners of this cell
            public Vector3 TopLeftCorner { get { return worldPos + ((diameter / 2) * new Vector3(-1, 0, 1)); } }

            public Vector3 TopRightCorner { get { return worldPos + ((diameter / 2) * new Vector3(1, 0, 1)); } }

            public Vector3 BottomLeftCorner { get { return worldPos + ((diameter / 2) * new Vector3(-1, 0, -1)); } }

            public Vector3 BottomRightCorner { get { return worldPos + ((diameter / 2) * new Vector3(1, 0, -1)); } }

            public bool HasKD { get { return hasKdTree; } }


            public Tile(Vector3 _worldPos, int diam, bool init_kd_tree=true)
            {
                worldPos = _worldPos;
                diameter = diam;
                isDirty = false;

                if(init_kd_tree)
                {
                    kdTree = new KDTree(32);
                    kdQuery = new KDQuery();
                    hasKdTree = true;
                }
                else
                {
                    hasKdTree = false;
                }
            }

            public Tile(Tile other)
            {
                worldPos = other.worldPos;
                diameter = other.diameter;
                isDirty = other.isDirty;

                if(other.HasKD)
                {
                    kdTree = other.kdTree;
                    kdQuery = other.kdQuery;
                    hasKdTree = true;
                }
                else
                {
                    hasKdTree = false;
                }
            }

            public bool IsPointInCell(Vector3 point)
            {
                // this is basically like drawing diagnols
                var x1 = point.x > BottomLeftCorner.x && point.x < TopRightCorner.x;
                var x2 = point.z > BottomRightCorner.z && point.z < TopLeftCorner.z;

                return x1 && x2;
            }

            public bool IsRectInCell(Vector3[] rect)
            {
                foreach (var point in rect)
                    if (IsPointInCell(point) == false)
                        return false;
                return true;
            }

            public void AddCellData(MassiveDesinger.TileDataObj data)
            {
                kdTree.SetCount(kdTree.Count + 1);
                kdTree.Points[^1] = data;
                kdTree.Rebuild();
                isDirty = true;
            }
                
            public void AddTileData(MassiveDesinger.TileDataObj[] data, bool append=true, bool removeNullData=true)
            {
                if(removeNullData)
                    kdTree.Points = kdTree.Points.Where(item => item != null).ToArray();

                if (append)
                    data = kdTree.Points.Concat(data).ToArray();

                kdTree.SetCount(data.Length);
                kdTree.Points = data;
                kdTree.Rebuild();
                isDirty = false;
            }

            public void QueryNearestNeighbours(Vector3 atPos, float radius, ref List<int> kdQueryResultIndices)
            {
                if (kdTree.Count == 0)
                    return;

                kdQueryResultIndices = new List<int>();
                kdQuery.Radius(kdTree, atPos, radius, kdQueryResultIndices);
            }

            public void QueryNearestNeighbours(Vector3 atPos, float radius,
                ref List<int> kdQueryResultIndices,
                ref List<MassiveDesinger.TileDataObj> foundData,
                bool setQueriedIndexesAsInvalid = false)
            {
                if (kdTree.Count == 0)
                    return;

                kdQueryResultIndices = new List<int>();
                kdQuery.Radius(kdTree, atPos, radius, kdQueryResultIndices);

                for (int i = 0; i < kdQueryResultIndices.Count; i++)
                {
                    foundData.Add(kdTree.Points[kdQueryResultIndices[i]]);
                    if (setQueriedIndexesAsInvalid)
                        kdTree.Points[kdQueryResultIndices[i]] = null;

                }
                isDirty = true;
            }

            public void QueryNearestNeighbours(Vector3 atPos, float radius, 
                ref List<int> kdQueryResultIndices,
                ref List<MassiveDesinger.TileDataObj> foundData,
                ref List<Vector3> toBeRemovedTreeInstances,
                bool setQueriedIndexesAsInvalid=false,
                bool onlyOnSelectedLayer=false,
                int selectedLayer=-1)
            {
                if (kdTree.Count == 0)
                    return;

                kdQueryResultIndices = new List<int>();
                kdQuery.Radius(kdTree, atPos, radius, kdQueryResultIndices);

                for (int i = 0; i < kdQueryResultIndices.Count; i++)
                {
                    if(onlyOnSelectedLayer)
                    {
                        if(kdTree.Points[kdQueryResultIndices[i]].data.layerIdx == selectedLayer)
                        {
                            foundData.Add(kdTree.Points[kdQueryResultIndices[i]]);
                            toBeRemovedTreeInstances.Add(kdTree.Points[kdQueryResultIndices[i]].data.unityTreeInstance.position);
                            if (setQueriedIndexesAsInvalid)
                                kdTree.Points[kdQueryResultIndices[i]] = null;
                        }
                    }
                    else
                    {
                        foundData.Add(kdTree.Points[kdQueryResultIndices[i]]);
                        toBeRemovedTreeInstances.Add(kdTree.Points[kdQueryResultIndices[i]].data.unityTreeInstance.position);
                        if (setQueriedIndexesAsInvalid)
                            kdTree.Points[kdQueryResultIndices[i]] = null;
                    }

                }
                isDirty = true;
            }

            public void RemoveNullData()
            {
                kdTree.Points = kdTree.Points.Where(item => item != null).ToArray();
                kdTree.SetCount(kdTree.Points.Length);
                kdTree.Rebuild();
                isDirty = false;
            }
        }


        [System.Serializable]
        public class FastTiles
        {
            // publics
            [EditorFieldAttr(ControlType.boldLabel, "SpawnTiles")]
            [EditorFieldAttr(ControlType.intField, "gridSize")]
            public int gridSize = 4000;

            [EditorFieldAttr(ControlType.intField, "tileSize")]
            public int tileSize = 500;

            [EditorFieldAttr(ControlType.intField, "minGridSize")]
            public int minGridSize = 500;

            [EditorFieldAttr(ControlType.intField, "minTileSize")]
            public int minTileSize = 1000;

            [EditorFieldAttr(ControlType.space, "space")]
            [System.NonSerialized] public int space = 5;

            [EditorFieldAttr(ControlType.boolField, "drawTiles", layoutHorizontal: 1)]
            public bool drawTiles = false;

            [EditorFieldAttr(ControlType.boolField, "drawVisibleTiles", layoutHorizontal: -1)]
            public bool drawVisibleTiles = false;

            public Dictionary<Vector3, Tile> cellDict = new Dictionary<Vector3, Tile>();
            public bool isOk = false;

            // KD tree
            public KDTree kdTree = null;
            public KDQuery kdQuery = null;
            public List<int> kdQueryResultIndices = null;

            // privates
            private int nodeRadius = 0;
            private AutoInspector autoInspector = null;

            // getter / setters
            public int NodeRadius { get { return nodeRadius; } }
            public AutoInspector AutoInspector
            {
                get
                {
                    if(autoInspector == null)
                    {
                        System.Object obj = this;
                        autoInspector = new AutoInspector(typeof(FastTiles), ref obj);
                    }
                    return autoInspector;
                }
            }


            public void Create(bool initTilekdTree=true)
            {
                if(gridSize < tileSize)
                {
                    Debug.LogError("[FastTiles] Unable to create grid...! GridSize must be greater then TileSize");
                    cellDict.Clear();
                    isOk = false;
                    return;
                }

                int gridSize_ = Mathf.Clamp(gridSize, minGridSize, 8000);
                int tileSize_ = Mathf.Clamp(tileSize, minTileSize, 1000);
                gridSize = gridSize_;
                tileSize = tileSize_;

                int tileCount = 0;

                if ((gridSize % tileSize_) != 0)
                {
                    UnityEngine.Debug.Log("[FastTiles] Unable to create grid...! GridSize must be completly divisible by TileSize");
                    isOk = false;
                    return;
                }

                if (cellDict != null)
                    cellDict.Clear();

                nodeRadius = tileSize_ / 2;
                gridSize_ = Mathf.RoundToInt(gridSize / tileSize_); // size of grid in one dimension x or y
                MassiveDesinger.TileDataObj[] tileData = new MassiveDesinger.TileDataObj[gridSize_ * gridSize_];

                for (int x = 0; x < gridSize_; x++)
                {
                    for (int y = 0; y < gridSize_; y++)
                    {
                        Vector3 worldPos = Vector3.right * (x * tileSize_ + nodeRadius) + Vector3.forward * (y * tileSize_ + nodeRadius);

                        cellDict[worldPos] = new Tile(worldPos, tileSize_, init_kd_tree: initTilekdTree);
                        tileData[tileCount] = new MassiveDesinger.TileDataObj(worldPos, null);
                        tileCount++;
                    }
                }

                kdTree = new KDTree();
                kdQuery = new KDQuery();
                kdTree.Build(tileData, 32);
                isOk = true;

                Debug.LogFormat("[FastTiles] Created {0} tiles", tileCount);
            }

            public void Clear()
            {
                if (cellDict != null)
                    cellDict.Clear();
            }

            public Tile GetTileAtPos(Vector3 pos)
            {
                int row = (int)(pos.x / tileSize);
                int col = (int)(pos.z / tileSize);

                Vector3 worldPos = Vector3.right * (row * tileSize + nodeRadius) + Vector3.forward * (col * tileSize + nodeRadius);

                if (!cellDict.ContainsKey(worldPos))
                {
                    return null;
                }
                else
                {
                    return cellDict[worldPos];
                }
            }

            public Tile GetRandomTile()
            {
                Vector3 randPos = new Vector3(Random.Range(0, gridSize), 0, Random.Range(0, gridSize));
                return GetTileAtPos(randPos);
            }

            public Vector3 GetTilePos(Vector3 pos)
            {
                int row = (int)(pos.x / tileSize);
                int col = (int)(pos.z / tileSize);

                Vector3 worldPos = Vector3.right * (row * tileSize + nodeRadius) + Vector3.forward * (col * tileSize + nodeRadius);
                return worldPos;
            }

            public List<Tile> NearestCellPlusNeighbours(Vector3 pos, int count = 1)
            {
                kdQueryResultIndices = new List<int>();
                kdQuery.KNearest(kdTree, GetTilePos(pos), count, kdQueryResultIndices);
                List<Tile> foundTiles = new();

                for (int i = 0; i < kdQueryResultIndices.Count; i++)
                {
                    var tileDataObj = kdTree.Points[kdQueryResultIndices[i]];
                    foundTiles.Add(GetTileAtPos(tileDataObj.pos));
                }
       
                return foundTiles;
            }

            public List<Tile> TilesInRadius(Vector3 pos, float radius)
            {
                List<Tile> foundTiles = new();

                if (radius < nodeRadius)
                {
                    Vector3[] rect = new Vector3[4];
                    Tile tile;
                     
                    rect[0] = pos + (Vector3.back * radius) + (Vector3.left * radius); // bottom left
                    rect[1] = pos + (Vector3.back * radius) + (Vector3.right * radius); // bottom right
                    rect[2] = pos + (Vector3.forward * radius) + (Vector3.left * radius); // top left
                    rect[3] = pos + (Vector3.forward * radius) + (Vector3.right * radius); // top right

                    for (int i = 0; i < rect.Length; i++)
                    {
                        tile = GetTileAtPos(rect[i]);
                        if (!foundTiles.Contains(tile))
                            foundTiles.Add(tile);
                    }

                    return foundTiles;
                }

                foundTiles.Clear();
                kdQueryResultIndices = new List<int>();
                kdQuery.Radius(kdTree, GetTilePos(pos), radius*1.25f, kdQueryResultIndices);

                for (int i = 0; i < kdQueryResultIndices.Count; i++)
                {
                    var tileDataObj = kdTree.Points[kdQueryResultIndices[i]];
                    foundTiles.Add(GetTileAtPos(tileDataObj.pos));
                }

                return foundTiles;
            }

            public bool IsOK()
            {
                return isOk && cellDict != null && cellDict.Count > 0;
            }
        }
    }
}
