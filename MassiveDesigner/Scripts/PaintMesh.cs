using System;
using UnityEngine;
using CodeCreatePlay.AutoInspector;


namespace MassiveDesinger
{
    [System.Serializable]
    public class PaintMesh : MonoBehaviour
    {
        public enum ItemType
        {
            Trees,
            GrassAndGroundCover,
        }

        [System.Serializable]
        public class Properties
        {
            public ItemType itemType = ItemType.Trees;

            [EditorFieldAttr(ControlType.boldLabel, "PaintMesh")]
            [EditorFieldAttr(ControlType.textControl, "specieName")]
            public string specieName = "";

            [EditorFieldAttr(ControlType.boldLabel, "SpecieSettings")]
            [EditorFieldAttr(ControlType.floatField, "spawnProbability")]
            public float spawnProbability = 0.5f;

            [EditorFieldAttr(ControlType.floatField, "survivalRate")]
            public float survivalRate = 0.5f;

            [EditorFieldAttr(ControlType.floatField, "dispersionStrength")]
            public float dispersionStrength = 0.5f;

            [EditorFieldAttr(ControlType.boldLabel, "CollisionSettings")]
            [FloatSliderAttr(ControlType.floatSlider, "firstColliderRadius", 0.25f, 5f)]
            public float firstColliderRadius = 0.5f;

            [FloatSliderAttr(ControlType.floatSlider, "secondColliderRadius", 1f, 5f)]
            public float secondColliderRadius = 0.5f;

            [EditorFieldAttr(ControlType.vector3, "secondColliderOffset")]
            public Vector3 secondColliderOffset = Vector3.zero;

            [EditorFieldAttr(ControlType.boldLabel, "Variation")]
            [FloatSliderAttr(ControlType.floatSlider, "scaleMultiplier", 0.1f, 2f)]
            public float scaleMultiplier = 1f;

            [FloatSliderAttr(ControlType.floatSlider, "scaleVariation", 0.1f, 1f)]
            public float scaleVariation = 0.25f;

            [FloatSliderAttr(ControlType.floatSlider, "rotationVariation", 0f, 1f)]
            public float rotationVariation = 0.5f;

            [EditorFieldAttr(ControlType.boldLabel, "Debug")]
            [EditorFieldAttr(ControlType.boolField, "debug")]
            public bool debug = false;
        }

        public Properties properties = new ();
        [HideInInspector] public int terrainItemIdx; // indexes into terrain tree or detail items array
        [HideInInspector] public int layerPriorityIdx;
        [HideInInspector] public int layerIdx;
        [HideInInspector] public Vector3 lastSpawnPos;
        [HideInInspector] public AutoInspector autoInspector;
        [HideInInspector] public string guid = null;

        private void OnValidate()
        {
            if (guid == null)
                guid = Guid.NewGuid().ToString();

            var sphere = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/Sphere.fbx", typeof(GameObject)) as GameObject;
            var halfSphere = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/MaidenLand/MassiveDesigner/Resources/HalfSphere.fbx", typeof(GameObject)) as GameObject;
            debugMeshSphere = sphere.GetComponent<MeshFilter>().sharedMesh;
            debugMeshHalfSphere = halfSphere.GetComponent<MeshFilter>().sharedMesh;
        }

        public AutoInspector AutoInspector
        {
            get
            {
                if(autoInspector == null)
                {
                    System.Type t;
                    object obj;
                    t = properties.GetType();
                    obj = properties;
                    autoInspector = new AutoInspector(t, ref obj);
                }
                return autoInspector;
            }
        }

        public void OnSelect()
        {
            LoadAutoControls();
        }

        public void LoadAutoControls()
        {
            System.Type t;
            object obj;

            t = properties.GetType();
            obj = properties;
            autoInspector = new AutoInspector(t, ref obj);
        }
         
        Material[] materials;
        Mesh sharedMesh;
        Mesh debugMeshSphere;
        Mesh debugMeshHalfSphere;

        public void OnDrawGizmos()
        {
            
            if (properties.debug && UnityEditor.Selection.activeGameObject == this.gameObject)
            {
                if (materials == null || sharedMesh == null)
                {
                    materials = GetComponent<MeshRenderer>().sharedMaterials;
                    sharedMesh = GetComponent<MeshFilter>().sharedMesh;
                }
                 
                float radius = gameObject.transform.localScale.magnitude * properties.firstColliderRadius;

                // draw first collider
                // *** TODO HALF SPHERE USING , currently it is done using Handles in OnSceneGUI of PaintMeshEd.cs
                Gizmos.color = new (1f, 0.9f, 0.25f, 0.5f);
                // Gizmos.DrawWireSphere(transform.position, radius/2);
                Gizmos.DrawMesh(debugMeshHalfSphere, 0, transform.position, Quaternion.identity, Vector3.one * radius);

                // vis second collider for trees
                radius = gameObject.transform.localScale.magnitude * properties.secondColliderRadius;
                var offset = properties.secondColliderOffset * gameObject.transform.localScale.magnitude;

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position + offset, radius);
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawMesh(debugMeshSphere, 0, transform.position + offset, Quaternion.identity, Vector3.one * radius);
            }
        }
    }
}

