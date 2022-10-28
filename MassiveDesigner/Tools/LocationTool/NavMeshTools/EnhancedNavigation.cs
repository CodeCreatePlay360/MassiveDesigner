using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnhancedNavigation : MonoBehaviour
{
    [System.Serializable]
    public class Triangle
    {
        public Vector3 x = new Vector3();
        public Vector3 y = new Vector3();
        public Vector3 z = new Vector3();


        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            x = v1;
            y = v2;
            z = v3;
        }

        public Vector3 GetCentre()
        {
            Vector3 centrePos = new Vector3();

            // direct method
            float x = (this.x.x + this.y.x + this.z.x) / 3;
            float y = (this.x.y + this.y.y + this.z.y) / 3;
            float z = (this.x.z + this.y.z + this.z.z) / 3;

            centrePos = new Vector3(x, y, z);

            return centrePos;
        }
    }


    [System.Serializable]
    public class NavMeshArea : Triangle
    {
        public bool isInaccessiable = false;
        public bool isSelected = false;
        public Color areaColor = Color.green;

        public NavMeshArea(Vector3 v1, Vector3 v2, Vector3 v3) : base(v1, v2, v3)
        {
        }

        public void IsVisible(bool isVisible)
        {
            if (isVisible)
            {
                areaColor = Color.blue;
            }
            else
            {
                if (isInaccessiable) areaColor = Color.grey;
                else areaColor = Color.green;
            }
        }
    }


    [Header("Debug")]
    public bool debug = false;
    public float gizmoRadius = 0.5f;

    private List<NavMeshArea> navMeshAreas = new List<NavMeshArea>();


    void Start()
    {
        CreateAreasFromNavMesh();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var item in navMeshAreas)
            Gizmos.DrawWireSphere(item.GetCentre(), gizmoRadius);
    }

    public void CreateAreasFromNavMesh()
    {
        navMeshAreas.Clear();

        NavMeshTriangulation navMeshTris = NavMesh.CalculateTriangulation();

        Vector3[] vertices = navMeshTris.vertices;
        int[] indices = navMeshTris.indices;

        int nextStop = 3;
        for (int i = 0; i < navMeshTris.indices.Length+4; i++)  // add +3 to navMeshTris.indices.Length
        {
            if(i > nextStop)
            {
                int indice1 = indices[nextStop - 3];
                int indice2 = indices[nextStop - 2];
                int indice3 = indices[nextStop - 1];

                // create a new triangle
                Vector3 v1 = vertices[indice1];
                Vector3 v2 = vertices[indice2];
                Vector3 v3 = vertices[indice3];

                navMeshAreas.Add( new NavMeshArea(v1, v2, v3) );

                nextStop += 3;
                continue;
            }
        }
    }

    public NavMeshArea GetClosestArea(Vector3 position)
    {
        float minDistance = float.MaxValue;
        NavMeshArea closest = null;

        foreach (var item in navMeshAreas)
        {
            if(Vector3.Distance(position, item.GetCentre()) < minDistance && !item.isInaccessiable)
            {
                minDistance = Vector3.Distance(position, item.GetCentre());
                closest = item;
            }
        }

        return closest;
    }

    public void Clear()
    {
        navMeshAreas.Clear();
    }

    public NavMeshArea GetRandomArea()
    {
        NavMeshArea area = navMeshAreas[Random.Range(0, navMeshAreas.Count)];

        if (area.isInaccessiable)
            return GetRandomArea();
        else
            return area;
    }
}
