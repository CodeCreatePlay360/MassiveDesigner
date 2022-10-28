using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(MassiveDesinger.PaintMesh))]
public class PaintMeshEd : Editor
{
    MassiveDesinger.PaintMesh paintMesh;

    private void OnEnable()
    {
        paintMesh = target as MassiveDesinger.PaintMesh;
    }

    public override void OnInspectorGUI()
    {
        paintMesh.AutoInspector.Build();
        SceneView.RepaintAll();
    }

    public void OnSceneGUI()
    {
        if (paintMesh.properties.debug)
        {
            float radius = paintMesh.gameObject.transform.localScale.magnitude * paintMesh.properties.firstColliderRadius;
            Handles.color = new(1f, 0.9f, 0.25f, 0.5f);
            Handles.DrawWireArc(paintMesh.transform.position, Vector3.up, Vector3.right, 360f, radius);
            Handles.DrawWireArc(paintMesh.transform.position, Vector3.right, -Vector3.forward, 180f, radius);
            Handles.DrawWireArc(paintMesh.transform.position, Vector3.forward, Vector3.right, 180f, radius);
        }
    }
}
