using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataStructures.ViliWonka.KDTree;
using System.Diagnostics;
using MassiveDesinger;


public class KDTest : MonoBehaviour
{

    [HideInInspector] public TileDataObj[] pointCloud = null;
    public int pointCloudCount = 1000;
    public int pointCloudRadius = 100;
    public int queryRadius = 10;
    public bool debug = false;

    [HideInInspector] public KDTree kdTree = null;
    [HideInInspector] public KDQuery kdQuery = null;
    [HideInInspector] public List<int> kdQueryResultIndices = null;

    Stopwatch stopwatch;


    private void Start()
    {
    }


    void Update()
    {
    }


    public void BuildTree()
    {
        pointCloud = new TileDataObj[pointCloudCount];

        Vector3 point;
        for (int i = 0; i < pointCloudCount; i++)
        {
            point = UnityEngine.Random.insideUnitSphere * pointCloudRadius;
            pointCloud[i].pos = point;
        }

        kdTree = new KDTree(pointCloud);
        kdQuery = new KDQuery();
        stopwatch = new Stopwatch();
    }

    public void RestructureTree()
    {
        int oldLen = kdTree.Count;
        pointCloudCount = oldLen + 100;

        // update both point cloud and kd tree
        Array.Resize(ref pointCloud, pointCloudCount);
        kdTree.SetCount(pointCloudCount);

        // update values
        Vector3 point;
        for (int i = oldLen; i < kdTree.Points.Length; i++)
        {
            point = UnityEngine.Random.insideUnitSphere * pointCloudRadius;
            pointCloud[i].pos = point;
            kdTree.Points[i].pos = point;
        }

        kdTree.Rebuild();
    }

    Vector3 gizmoSize = Vector3.one * 0.2f;
    public int foundCount = 0;
    public float elapsedTime = 0f;

    private void OnDrawGizmos()
    {
        if (kdTree == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, queryRadius);

        stopwatch.Reset();
        stopwatch.Start();
        kdQueryResultIndices = new List<int>();
        kdQuery.Radius(kdTree, transform.position, queryRadius, kdQueryResultIndices);
        foundCount = kdQueryResultIndices.Count;
        stopwatch.Stop();

        elapsedTime = stopwatch.ElapsedMilliseconds;

        Gizmos.color = Color.red;
        if(debug)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < pointCloudCount; i++)
            {
                Gizmos.DrawWireSphere(pointCloud[i].pos, 1);
            }

            for (int i = 0; i < kdQueryResultIndices.Count; i++)
            {
                var xx = kdQueryResultIndices[i];
                Gizmos.DrawWireSphere(pointCloud[kdQueryResultIndices[i]].pos, 1);
            }
        }
    }

    public void Clear()
    {
        kdTree = null;
        kdQuery = null;
        pointCloud = null;  // TODO check how to properly dispose off an array
        kdQueryResultIndices = null;
    }
}
