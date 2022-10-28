using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CodeCreatePlay.FastTiles;
using System.Diagnostics;


public class FastTilesDemo : MonoBehaviour
{
    [System.Serializable]
    public class SaveableTile
    {
        public Vector3 worldPos;
        public List<Vector3> positions;

        public SaveableTile(Vector3 _worldPos, List<Vector3> _positions)
        {
            worldPos = _worldPos;
            positions = _positions;
        }
    }


    public enum TestType
    {
        AtPos,
        NearestNeighbours,
        CircularRadius,
    }

    public TestType testType = TestType.AtPos;

    [Header("Grid debug settings")]
    public bool debug = false;
    public bool drawGrid = true;
    public bool drawGridTileData = false;
    public bool checkFromNullItems = false;

    [Header("TileData query settings")]
    public bool queryNearestNeighbours = false;
    public bool drawfoundData = false;
    public float queryRadius = 100f;
    public float elapsedTime = 0;

    [Header("1. Neighbouring cells")]
    public int nearestNeighboursCount = 2;

    [Header("2. Cells in circular radius")]
    public float radius = 5f;

    [Header("Debug")]
    public int foundTilesCount = 0;


    [Header("References")]
    public GameObject testObject = null;
    public Camera cam = null;

    private FastTiles grid = null;
    private List<MassiveDesinger.TileDataObj> foundData = null;
    private List<Tile> foundTiles = new List<Tile>();

    [HideInInspector]
    [SerializeField]
    List<SaveableTile> savedTiles;

    [HideInInspector]
    [SerializeField]
    bool ser = false;

    Stopwatch stopwatch;


    public void Start()
    {
    }

    public void Init()
    {
        savedTiles.Clear();
        grid = new FastTiles();
        grid.gridSize = 8000;
        grid.tileSize = 500;
        grid.Create();

        Vector3[] gridPoly = new Vector3[4];
        gridPoly[0] = Vector3.zero;
        gridPoly[1] = Vector3.right;
        gridPoly[2] = Vector3.forward;
        gridPoly[3] = Vector3.forward + Vector3.right;

        MassiveDesinger.TileData tempTileDataObj = null;
        MassiveDesinger.TileDataObj[] tileData = new MassiveDesinger.TileDataObj[100000];
        Dictionary<Tile, List<MassiveDesinger.TileDataObj>> cellAndData = new Dictionary<Tile, List<MassiveDesinger.TileDataObj>>();

        Vector3 randPos;
        Tile tile;
        for (int i = 0; i < 500; i++)
        {
            randPos = CodeCreatePlay.UniformDistributions.GetUniformInParallelogram(gridPoly, grid.gridSize); // generate a random pos on grid
            tileData[i] = new MassiveDesinger.TileDataObj(randPos, tempTileDataObj);
            tile = grid.GetTileAtPos(randPos);
            if (tile != null)
            {
                if (!cellAndData.ContainsKey(tile))
                    cellAndData[tile] = new List<MassiveDesinger.TileDataObj>();
                cellAndData[tile].Add(tileData[i]);
            }
        }

        Dictionary<Tile, MassiveDesinger.TileDataObj[]> cellAndData_ = new Dictionary<Tile, MassiveDesinger.TileDataObj[]>();
        foreach (var tile_ in cellAndData.Keys)
        {
            tile_.AddTileData(cellAndData[tile_].ToArray());
        }

        int maxIterations = 2;
        int currentIteration = 0;
        int spawnCountPerIteration = 250;
        Vector3[] positions = new Vector3[spawnCountPerIteration];
        while(currentIteration <= maxIterations)
        {
            for (int i = 0; i < spawnCountPerIteration; i++)
            {
                randPos = CodeCreatePlay.UniformDistributions.GetUniformInParallelogram(gridPoly, 50); // generate a random pos on grid
                if(CanSpawn(positions, randPos))
                {
                    positions[i] = randPos;
                }
            }
            positions = new Vector3[spawnCountPerIteration];
            currentIteration++;
        }

    }

    bool CanSpawn(Vector3[] fromPositions, Vector3 pos)
    {
        for (int i = 0; i < fromPositions.Length; i++)
        {
            if (Vector3.Distance(pos, fromPositions[i]) < 2f)
            { return false; }
        }

        return true;
    }

    public void RemoveOP()
    {
        /*
        // grid remove operation
        int total = 0;
        List<Vector3> allKeys = grid.cellDict.Keys.ToList();
        Vector3 randKey;
        for (int i = 0; i < 250; i++)
        {
            randKey = allKeys[UnityEngine.Random.Range(0, allKeys.Count - 1)]; // pick a random key

            if (grid.cellDict[randKey].tileData.Length > 0)
            {
                int rand = UnityEngine.Random.Range(0, grid.cellDict[randKey].nextTileDataIdx - 1);
                if (grid.cellDict[randKey].RemoveDataFromCell(rand))
                    total++;
            }
        }
        Debug.LogFormat("total removed {0}", total);
        */
    }    

    public void Update()
    {
    }

    public void OnDrawGizmos()
    {
        if (!debug || grid == null || testObject == null)
            return;

        if (drawGrid)
            DrawGrid();

        switch (testType)
        {
            case TestType.AtPos:
                DebugAtPos();
                break;

            case TestType.CircularRadius:
                DebugCircularRadius();
                break;

            case TestType.NearestNeighbours:
                DebugNearestNeighbours();
                break;
        }

        if (queryNearestNeighbours)
            QueryNearestNeighbours();
    }

    void QueryNearestNeighbours()
    {
         /*
         * Vector3 btmLeft = testObject.transform.position + (Vector3.back * queryRadius) + (Vector3.left * queryRadius); // bottom left
         * Vector3 btmRight = testObject.transform.position + (Vector3.back * queryRadius) + (Vector3.right * queryRadius); // bottom right
         * Vector3 topLeft = testObject.transform.position + (Vector3.forward * queryRadius) + (Vector3.left * queryRadius); // top left
         * Vector3 topRight = testObject.transform.position + (Vector3.forward * queryRadius) + (Vector3.right * queryRadius); // top right
         * 
         * Gizmos.color = Color.green;
         * 
         * Gizmos.DrawSphere(testObject.transform.position, queryRadius);
         * Gizmos.DrawLine(btmLeft, btmRight);
         * 
         * Gizmos.DrawSphere(btmLeft, 2f);
         * Gizmos.DrawSphere(btmRight, 2f);
         * Gizmos.DrawSphere(topLeft, 2f);
         * Gizmos.DrawSphere(topRight, 2f);
         */

        Gizmos.color = Color.red;
        foundData = new List<MassiveDesinger.TileDataObj>();

        //stopwatch.Reset();
        //stopwatch.Start();

        foreach (var tile in foundTiles)
        {
            if (tile.kdTree.Count == 0)
                continue;
            tile.QueryNearestNeighbours(testObject.transform.position, queryRadius, ref foundData);
        }

        //stopwatch.Stop();
        //elapsedTime = stopwatch.ElapsedMilliseconds;

        if(drawfoundData)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < foundData.Count; i++)
            {
                Gizmos.DrawSphere(foundData[i].pos, 3f);
            }
        }
    }

    void DrawGrid()
    {
        Gizmos.color = Color.blue;
        foreach (var cellEntry in grid.cellDict.Keys)
            Gizmos.DrawWireCube(grid.cellDict[cellEntry].worldPos, new Vector3(grid.tileSize, 0.05f, grid.tileSize));

        if(drawGridTileData)
        {
        }
    }    

    void DebugAtPos()
    {
        Gizmos.color = Color.white;
        Tile cell = grid.GetTileAtPos(testObject.transform.position);
        if(cell != null)
        {
            foundTiles.Clear();
            foundTiles.Add(cell);
            Gizmos.DrawWireCube(cell.worldPos, new Vector3(grid.tileSize, 0.1f, grid.tileSize));
        }

    }

    void DebugCircularRadius()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(testObject.transform.position, radius);

        Gizmos.color = Color.white;
        foundTiles.Clear();
        foreach (var tile in grid.TilesInRadius(testObject.transform.position, radius))
        {
            foundTiles.Add(tile);
            Gizmos.DrawWireCube(tile.worldPos, new Vector3(grid.tileSize, 0.1f, grid.tileSize));
        }
    }

    void DebugNearestNeighbours()
    {
        foundTiles.Clear();
        Gizmos.color = Color.white;

        foreach (var tile in grid.NearestCellPlusNeighbours(testObject.transform.position, nearestNeighboursCount))
        {
            foundTiles.Add(tile);
            Gizmos.DrawWireCube(tile.worldPos, new Vector3(grid.tileSize, 0.1f, grid.tileSize));
        }

        foundTilesCount = foundTiles.Count;
    }    

}
