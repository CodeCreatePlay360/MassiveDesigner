using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MassiveDesinger
{
    /// <summary>
    /// An internal structure representing a FoliageSeed useful during foliage simulation.
    /// </summary>
    [System.Serializable]
    public struct FoliageSeed
    {
        public string specieName;
        public Vector3 pos;
        public int scatterRadius;
        public int numNewSeedsSpawnPerIteration;

        private List<PaintMesh> paintMeshes;

        public List<PaintMesh> PaintMeshes { get { return paintMeshes; } }


        public FoliageSeed(Vector3 spawnPos)
        {
            specieName = "";
            pos = spawnPos;
            paintMeshes = new List<PaintMesh>();
            scatterRadius = -1;
            numNewSeedsSpawnPerIteration = -1;
        }
    }
}
