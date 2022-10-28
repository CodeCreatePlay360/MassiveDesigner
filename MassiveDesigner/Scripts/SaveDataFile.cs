using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.FastTiles;


namespace MassiveDesinger
{

    [CreateAssetMenu(fileName = "SaveDataFile", menuName = "MassiveDesigner/SaveData")]
    [System.Serializable]
    public class SaveDataFile : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private List<TileData> savedData = null;

        [SerializeField]
        [HideInInspector]
        private int tileSize;

        [SerializeField]
        [HideInInspector]
        private int gridSize;


        public void Save(FastTiles spawnTiles)
        {
            tileSize = spawnTiles.tileSize;
            gridSize = spawnTiles.gridSize;
            TileData tileData;

            foreach (var key in spawnTiles.cellDict.Keys)
            {
                for (int i = 0; i < spawnTiles.cellDict[key].kdTree.Count; i++)
                {
                    savedData.Add(spawnTiles.cellDict[key].kdTree.Points[i].data);
                }
            }
        }

        public void Load(ref FastTiles spawnTiles)
        {
            if (savedData == null)
            { 
                Debug.Log("No saved data found");
                return;
            }

            spawnTiles = new FastTiles();
            spawnTiles.tileSize = tileSize;
            spawnTiles.gridSize = gridSize;
            spawnTiles.Create();

            Dictionary<Vector3, List<TileDataObj>> data = new Dictionary<Vector3, List<TileDataObj>>();
            Vector3 pos;
            for (int i = 0; i < savedData.Count; i++)
            {
                pos = spawnTiles.GetTilePos(savedData[i].pos);
                if (!data.ContainsKey(pos))
                    data[pos] = new List<TileDataObj>();

                data[pos].Add(new TileDataObj(savedData[i].pos, savedData[i]));
            }

            Tile tile;
            foreach (var item in data.Keys)
            {
                tile = spawnTiles.GetTileAtPos(item);
                tile.AddTileData(data[item].ToArray(), append: false);
                Debug.Log(data[item].Count);
            }
        }
    }
}
