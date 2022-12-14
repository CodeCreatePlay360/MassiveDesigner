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
        EditorGUILayout.HelpBox("Object should be marked dirty after any change to PaintMesh's properties", MessageType.Info);
        if(GUILayout.Button("Mark Dirty"))
            UnityEditor.EditorUtility.SetDirty(paintMesh);
    }

    float _radius;
    Vector3 _offset;

    public void OnSceneGUI()
    {
        if (paintMesh.properties.debug)
        {
            if(paintMesh.properties.drawFirstCollider)
            {
                _radius = paintMesh.gameObject.transform.localScale.magnitude * paintMesh.properties.firstColliderRadius;
                _offset = paintMesh.properties.firstColliderOffset * paintMesh.gameObject.transform.localScale.magnitude;
                Handles.color = new(1f, 0.9f, 0.25f, 0.5f);
                Handles.DrawWireArc(paintMesh.transform.position + _offset, Vector3.up, Vector3.right, 360f, _radius);
                Handles.DrawWireArc(paintMesh.transform.position + _offset, Vector3.right, -Vector3.forward, 180f, _radius);
                Handles.DrawWireArc(paintMesh.transform.position + _offset, Vector3.forward, Vector3.right, 180f, _radius);
            }
        }
    }
}
