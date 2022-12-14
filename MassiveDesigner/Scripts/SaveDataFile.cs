using System.Collections.Generic;
using UnityEngine;
using MassiveDesinger.FastTiles;


namespace MassiveDesinger
{
    [CreateAssetMenu(fileName = "SaveDataFile", menuName = "MassiveDesigner/SaveData")]
    [System.Serializable]
    public class SaveDataFile : ScriptableObject
    {
        [System.Serializable]
        public class SaveUnityTreeData
        { 
            public int prototypeIndex;
            public float widthScale;
            public float heightScale;
            public float rotation;
            public Vector3 position;

            public SaveUnityTreeData(int prototypeIndex, float widthScale, float heightScale, float rotation, Vector3 position)
            {
                this.prototypeIndex = prototypeIndex;
                this.widthScale = widthScale;
                this.heightScale = heightScale;
                this.rotation = rotation;
                this.position = position;
            }
        }

        [SerializeField]
        [HideInInspector]
        private List<TileData> savedData = new List<TileData>();

        [SerializeField]
        [HideInInspector]
        private List<SaveUnityTreeData> savedUnityTrees = new List<SaveUnityTreeData>();

        [SerializeField]
        [HideInInspector]
        private int tileSize;

        [SerializeField]
        [HideInInspector]
        private int gridSize;


        public void Save(FastTiles.FastTiles spawnTiles)
        {
            savedData.Clear();
            savedUnityTrees.Clear();
            tileSize = spawnTiles.tileSize;
            gridSize = spawnTiles.gridSize;
             
            TreeInstance treeInstance;
              
            foreach (var key in spawnTiles.cellDict.Keys)
            {
                for (int i = 0; i < spawnTiles.cellDict[key].kdTree.Points.Length; i++)
                {
                    savedData.Add(spawnTiles.cellDict[key].kdTree.Points[i].data);

                    treeInstance = spawnTiles.cellDict[key].kdTree.Points[i].data.unityTreeInstance;
                    savedUnityTrees.Add(new SaveUnityTreeData(treeInstance.prototypeIndex, treeInstance.widthScale, treeInstance.heightScale, 
                        treeInstance.rotation, treeInstance.position));
                }
            }

            //Debug.LogFormat("MassiveDesigner Saved {0} points", savedData.Count);
            //Debug.LogFormat("MassiveDesigner Saved {0} tree instances", savedData.Count);
        }

        public void Load(ref FastTiles.FastTiles spawnTiles, ref Terrain terrain)
        {
            if (savedData == null)
            { 
                Debug.Log("[MassiveDesigner] No saved data found");
                return;
            }

            if(terrain == null)
            {
                Debug.Log("[MassiveDesigner] No active terrain found");
                return;
            }

            spawnTiles = new ();
            spawnTiles.tileSize = tileSize;
            spawnTiles.gridSize = gridSize;
            spawnTiles.Create();

            //Debug.LogFormat("Reloaded data count {0}", savedData.Count);
            //Debug.LogFormat("Reloaded tree instances count {0}", savedUnityTrees.Count);

            TreeInstance[] treeInstances = new TreeInstance[savedData.Count];
            Dictionary<Vector3, List<TileDataObj>> data = new Dictionary<Vector3, List<TileDataObj>>();
            Vector3 pos;

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
              
            for (int i = 0; i < savedData.Count; i++)
            {
                pos = spawnTiles.GetTilePos(savedData[i].pos);

                if (!data.ContainsKey(pos))
                    data[pos] = new List<TileDataObj>();

                treeInstance.prototypeIndex = savedUnityTrees[i].prototypeIndex;
                treeInstance.widthScale = savedUnityTrees[i].widthScale;
                treeInstance.heightScale = savedUnityTrees[i].heightScale;
                treeInstance.rotation = savedUnityTrees[i].rotation;
                treeInstance.position = savedUnityTrees[i].position;
                treeInstances[i] = treeInstance;

                savedData[i].unityTreeInstance = treeInstance;
                data[pos].Add(new TileDataObj(savedData[i].pos, savedData[i]));
            }
                  
            Tile tile;
            foreach (var item in data.Keys)
            {
                tile = spawnTiles.GetTileAtPos(item);
                tile.AddTileData(data[item].ToArray(), append: false);
            }

            terrain.terrainData.SetTreeInstances(treeInstances, snapToHeightmap: false);
        }
    }
}
