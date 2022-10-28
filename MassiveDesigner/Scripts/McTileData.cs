using UnityEngine;


namespace MassiveDesinger
{
    [System.Serializable]
    public class TileData
    {
        public PaintMesh paintMesh;

        public Vector3 pos;
        public Vector3 scale;
        public Quaternion rot;

        public Vector3 unityTreePos;

        public int layerIdx = -1;

        public Matrix4x4 Matrix
        {
            get { return Matrix4x4.TRS(pos, rot, scale); }
        }

        public TileData(PaintMesh paintMesh, Vector3 pos, Quaternion rot, Vector3 scale, TreeInstance treeInstance, int layerIdx)
        {
            this.paintMesh = paintMesh;

            this.pos = pos;
            this.rot = rot;
            this.scale = scale;

            unityTreePos = treeInstance.position;

            this.layerIdx = layerIdx;
        }

        public TileData(PaintMesh paintMesh, Vector3 pos, Quaternion rot, Vector3 scale, Vector3 unityTreePos, int layerIdx)
        {
            this.paintMesh = paintMesh;

            this.pos = pos;
            this.rot = rot;
            this.scale = scale;

            this.unityTreePos = unityTreePos;

            this.layerIdx = layerIdx;
        }
    }


    [System.Serializable]
    public class TileDataObj
    {
        public Vector3 pos;
        public TileData data;


        public TileDataObj(Vector3 pos, TileData data)
        {
            this.pos = pos;
            this.data = data;
        }
    }
}
