using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.AutoInspector;


namespace CodeCreatePlay
{
    namespace LocationTool
    {
        [System.Serializable]
        public class Point
        {
            public Vector3 position = new ();
            public bool isSelected = false;

            // the local position of this destination relative the gameobject it is attached to
            public Vector3 localPosition = Vector3.zero;
            public Vector3 lastPosition = Vector3.zero;

            public Point()
            {
                position = Vector3.zero;
            }

            public Point(Vector3 pos)
            {
                position = pos;
            }

            public void UpdatePosition(Transform refTrans)
            {
                position = refTrans.TransformPoint(localPosition);
            }

            public Vector3 GetGroundNormal()
            {
                Ray ray = new (position, Vector3.down);
                Vector3 groundNormal = Vector3.up;

                if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.5f))
                    groundNormal = hitInfo.normal;

                return groundNormal;
            }

            public Vector2 Position2D { get { return new Vector2(position.x, position.z); } }
        }


        [System.Serializable]
        public class Destination : Point
        {
            public bool isOccupied = false;
            public string destName = "Destination";

            public Destination(Vector3 pos) : base(pos)
            {
            }
        }


        [System.Serializable]
        public class LocationBase
        {
            [EditorFieldAttr(ControlType.textControl, "locationName")]
            public string locationName = "";

            [EditorFieldAttr(ControlType.LocationCategory, "category")]
            public LocationCategory category = LocationCategory.Area;

            [EditorFieldAttr(ControlType.color, "boundaryColor")]
            public Color boundaryColor = Color.blue;

            [EditorFieldAttr(ControlType.color, "destinationColor")]
            public Color destinationColor = Color.red;

            [EditorFieldAttr(ControlType.color, "lineColor")]
            public Color lineColor = Color.red;

            [EditorFieldAttr(ControlType.boolField, "drawBoundaries", layoutHorizontal:1)]
            public bool drawBoundaries = true;

            [EditorFieldAttr(ControlType.boolField, "drawDestinations", layoutHorizontal:-1)]
            public bool drawDestinations = true;

            [EditorFieldAttr(ControlType.boolField, "connectBoundaryPoints")]
            public bool connectBoundaryPoints = true;

            [EditorFieldAttr(ControlType.boolField, "drawHandles")]
            public bool drawHandles = true;

            [HideInInspector] public float markerRadius = 0.15f;
            [HideInInspector] public List<Point> boundaries = new();
            [HideInInspector] public List<Destination> destinations = new();


            public LocationBase()
            {
            }

            public Point AddBoundaryPoint(Vector3 position)
            {
                Point newPoint = new (position);
                boundaries.Add(newPoint);
                return newPoint;
            }

            public Destination AddDestinationPoint(Vector3 position)
            {
                // create the new destination at position.
                Destination newDest = new (position);
                destinations.Add(newDest);
                return newDest;
            }

            public void RemoveDestination(int at)
            {
                destinations.RemoveAt(at);
            }

            public void ClearBoundaries()
            {
                boundaries.Clear();
            }

            public void ClearDestinations()
            {
                destinations.Clear();
            }

            public Vector3 GetRandomDestination()
            {
                if (destinations.Count == 0)
                    return new Vector3(-1, -1, -1);

                return destinations[Random.Range(0, destinations.Count - 1)].position;
            }

            public bool IsPointInside()
            {
                return false;
            }

            public Vector3 GetMinBounds()
            {
                Vector3 min = new (float.MaxValue, float.MaxValue, float.MaxValue);

                foreach (Point point in boundaries)
                {
                    min.x = Mathf.Min(point.position.x, min.x);
                    min.y = Mathf.Min(point.position.y, min.y);
                    min.z = Mathf.Min(point.position.z, min.z);
                }
                return min;
            }

            public Vector3 GetMaxBounds()
            {
                Vector3 max = new (-float.MaxValue, -float.MaxValue, -float.MaxValue);

                foreach (Point point in boundaries)
                {
                    max.x = Mathf.Max(point.position.x, max.x);
                    max.y = Mathf.Max(point.position.y, max.y);
                    max.z = Mathf.Max(point.position.z, max.z);
                }

                return max;
            }

            public List<Vector3> GetUniformPointsDistribution(int spacing = 1)
            {
                List<Vector3> uniformPoints = new ();

                if (boundaries.Count < 3)
                { return uniformPoints; }

                Vector3 maxBounds = GetMaxBounds();
                Vector3 minBounds = GetMinBounds();

                float s = Vector3.Distance(maxBounds, minBounds);
                s /= spacing;

                Vector3 point;
                 
                // distribute uniform points.
                for (int i = 0; i < s; i++)
                {
                    // point = new Vector3(i, 0, 0);

                    for (int j = 0; j < s; j++)
                    {
                        point = new Vector3(i * spacing, 0, j * spacing);
                        
                        if (Geo2dUtils.PointInsidePolygon(Boundaries2d, Boundaries2d.Length, new Vector2(minBounds.x + point.x, minBounds.z + point.z)))
                        {
                            uniformPoints.Add(minBounds + point);
                        }
                    }
                }

                return uniformPoints;
            }

            public Vector3 GetCenter()
            {
                if (boundaries.Count < 2)
                    return Vector3.zero;

                Vector2 centroid = Geo2dUtils.Compute2DPolygonCentroid(Boundaries2d, Boundaries2d.Length);
                return new Vector3(centroid.x, 0, centroid.y);
            }

            public Vector3 Diameter()
            {
                return new Vector3(GetMaxBounds().x - GetMinBounds().x, 0f, GetMaxBounds().z - GetMinBounds().z);
            }

            public Vector2[] Boundaries2d
            {
                get
                {
                    Vector2[] positions = new Vector2[boundaries.Count];
                    int i = 0;
                    foreach (Point point in boundaries) { positions[i] = point.Position2D; i++; }
                    return positions;
                }
            }

            public Vector3[] Boundaries
            {
                get
                {
                    Vector3[] positions = new Vector3[boundaries.Count];
                    int i = 0;
                    foreach (Point point in boundaries) { positions[i] = point.position; i++; }
                    return positions;
                }
            }
        }

         
        /// <summary>
        /// Location contains administrative settings for the actual LocationBase.
        /// </summary>
        public class Location : MonoBehaviour
        {
            [HideInInspector] public LocationBase locationBase = new ();

            AutoInspector.AutoInspector locationAutoEd = null;
            public AutoInspector.AutoInspector AutoEditor
            {
                get
                {
                    if (locationAutoEd == null)
                    {
                        System.Object obj = locationBase;
                        locationAutoEd = new AutoInspector.AutoInspector(typeof(LocationBase), ref obj);
                    }

                    return locationAutoEd;
                }
            }

            [HideInInspector] public Point selectedPoint = null;
            [HideInInspector] public  Vector3 lastPos = Vector3.zero;
            [HideInInspector] public Vector3 lastScale = Vector3.zero;
            [HideInInspector] public Quaternion lastRot = Quaternion.identity;

            [HideInInspector] public bool editMode = false;
            [HideInInspector] public string editModeBtnTxt = "BeginEdit";
            [HideInInspector] public bool destinationsFoldout = false;

            public static readonly float POINT_DISTANCE_FROM_GROUND = 0.005f;
             
             
            public void Awake()
            {
                try
                {
                    GameObject ltGlobals = GameObject.FindGameObjectWithTag("LT_Globals");
                    if (ltGlobals && ltGlobals.GetComponent<LT_Globals>())
                    {
                        ltGlobals.GetComponent<LT_Globals>().Locations.Add(locationBase);
                    }
                }
                catch(UnityException)
                {
                    Debug.LogWarningFormat("[LocationTool] Unable to add location {0} to global repo, make sure tag" +
                        "LT_Globals is defined and LT_Globals object exist in scene", locationBase.locationName);
                }
  
            }

            public void UpdatePositions()
            {
                foreach (var point in locationBase.boundaries)
                    point.UpdatePosition(transform);

                foreach (var point in locationBase.destinations)
                    point.UpdatePosition(transform);
            }
        }

    }
}
