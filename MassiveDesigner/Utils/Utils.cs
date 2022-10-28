using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CodeCreatePlay
{
    namespace Utils
    {
        public static class CommonMaths
        {
            // Clamp list indices
            // Will even work if index is larger/smaller than listSize, so can loop multiple times
            public static int ClampListIndex(int index, int listSize)
            {
                index = ((index % listSize) + listSize) % listSize;

                return index;
            }

            public static int WeightedChoice(List<float> weights)
            {
                var totals = new List<float>();
                float runningTotal = 0;

                foreach (var weight in weights)
                {
                    runningTotal += weight;
                    totals.Add(runningTotal);
                }

                var rand = UnityEngine.Random.Range(0f, 1f) * runningTotal;

                for (int i = 0; i < totals.Count; i++)
                {
                    if (rand < totals[i])
                        return i;
                }

                return -1;
            }

            public static int WeightedChoice(float[] weights)
            {
                var totals = new List<float>();
                float runningTotal = 0;

                foreach (var weight in weights)
                {
                    runningTotal += weight;
                    totals.Add(runningTotal);
                }

                var rand = UnityEngine.Random.Range(0f, 1f) * runningTotal;

                for (int i = 0; i < totals.Count; i++)
                {
                    if (rand < totals[i])
                        return i;
                }

                return -1;
            }

            public static bool SphereCollision(Vector3 pos1, float r1, Vector3 pos2, float r2)
            {
                static float DistanceSquared(Vector3 v1, Vector3 v2)
                {
                    Vector3 delta = v2 - v1;
                    return Vector3.Dot(delta, delta);
                }

                float rsquard = r1 + r2;
                rsquard *= rsquard;
                if (DistanceSquared(pos1, pos2) < rsquard)
                    return true;

                return false;
            }

            public static float ConvertToRange(float oldMin, float oldMax, float newMin, float newMax, float value)
            {
                float oldRange = oldMax - oldMin;

                if (oldRange == 0)
                {
                    float newValue = newMin;
                    return newValue;
                }
                else
                {
                    float newRange = newMax - newMin;
                    float newValue = (((value - oldMin) * newRange) / oldRange) + newMin;
                    return newValue;
                }
            }
        }

        [System.Serializable]
        public class Rect
        {
            public Vector3[] points;
            public System.Collections.Generic.List<Vector3> gridPoints = new System.Collections.Generic.List<Vector3>();

            public Rect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
            {
                points = new Vector3[4];
                points[0] = p1;
                points[1] = p2;
                points[2] = p3;
                points[3] = p4;
            }

            public Vector3 Center
            {
                get
                {
                    var sum = points[0] + points[1] + points[2] + points[3];
                    var center = sum / 4;
                    return center;
                }
            }

            public Vector3[] GetFlatPoints()
            {
                Vector3[] flatPoints = new Vector3[4];
                flatPoints[0] = new Vector3(points[0].x, 0, points[0].z);
                flatPoints[1] = new Vector3(points[1].x, 0, points[1].z);
                flatPoints[2] = new Vector3(points[2].x, 0, points[2].z);
                flatPoints[3] = new Vector3(points[3].x, 0, points[3].z);

                return flatPoints;
            }

            public bool ContainsPoint(Vector3 p)
            {
                bool SameSide(Vector3 p1, Vector3 p2, Vector3 _a, Vector3 _b)
                {
                    var cp1 = Vector3.Cross(_b - _a, p1 - _a);
                    var cp2 = Vector3.Cross(_b - _a, p2 - _a);

                    if (Vector3.Dot(cp1, cp2) >= 0)
                        return true;

                    return false;
                }

                Vector3[] points = GetFlatPoints();

                var inTriangle_1 = SameSide(p, points[0], points[1], points[2]) && SameSide(p, points[1], points[0], points[2])
                    && SameSide(p, points[2], points[0], points[1]);

                var inTriangle_2 = SameSide(p, points[1], points[2], points[3]) && SameSide(p, points[2], points[1], points[3])
                    && SameSide(p, points[3], points[1], points[2]);

                if (inTriangle_1 || inTriangle_2)
                    return true;

                return false;
            }

            public void CalculateGrid()
            {
                List<Vector3> lowerPoints = new List<Vector3>();
                Vector3 dir = points[1] - points[0];
                dir.Normalize();
                float distance = Vector3.Distance(points[0], points[1]);
                for (float i = 0; i <= distance; i++)
                    lowerPoints.Add((dir * i) + points[0]);

                List<Vector3> upperPoints = new List<Vector3>();
                distance = Vector3.Distance(points[2], points[3]);
                dir = points[3] - points[2];
                dir.Normalize();
                for (float i = 0; i <= distance; i++)
                    upperPoints.Add((dir * i) + points[2]);


                gridPoints.Clear();
                var _points = upperPoints.Count > lowerPoints.Count ? lowerPoints : upperPoints;
                for (int i = 0; i < _points.Count; i++)
                {
                    gridPoints.Add(lowerPoints[i]);
                    dir = upperPoints[i] - lowerPoints[i];
                    dir.Normalize();
                    distance = Vector3.Distance(lowerPoints[i], upperPoints[i]);
                    for (float j = 1; j < distance; j++)
                    {
                        gridPoints.Add((dir * j) + lowerPoints[i]);
                    }
                }
            }

            public void _Debug()
            {
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireDisc(points[0], Vector3.up, 0.15f);
                UnityEditor.Handles.DrawWireDisc(points[1], Vector3.up, 0.15f);

                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(points[2], Vector3.up, 0.15f);
                UnityEditor.Handles.DrawWireDisc(points[3], Vector3.up, 0.15f);

                UnityEditor.Handles.color = Color.blue;
                UnityEditor.Handles.DrawLine(points[0], points[1]);
                UnityEditor.Handles.DrawLine(points[2], points[3]);

                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawLine(points[0], points[2]);
                UnityEditor.Handles.DrawLine(points[1], points[3]);

                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.DrawWireDisc(Center, Vector3.up, 0.2f);
            }
        }

        [System.Serializable]
        public class BBox
        {
            public Vector3[] points = new Vector3[8];
            public Transform transform;
            public List<Geometry.Triangle> faces = new();
            bool _hit;

            public BBox()
            {
            }

            public BBox(Transform t, Vector3[] _transformedPoints)
            {
                transform = t;
                points = _transformedPoints;
            }

            public void SetData(Vector3[] _transformedPoints)
            {
                points = _transformedPoints;
            }

            public Vector3[] GetVertices()
            {
                return points;
            }

            public Vector3[] GetAxes()
            {
                Geometry.Triangle[] finalFaces = new Geometry.Triangle[3];
                finalFaces[0] = new Geometry.Triangle(points[0], points[1], points[3]);
                finalFaces[1] = new Geometry.Triangle(points[2], points[3], points[7]);
                finalFaces[2] = new Geometry.Triangle(points[2], points[6], points[4]);

                Vector3[] axes = new Vector3[finalFaces.Length];
                for (int i = 0; i < finalFaces.Length; i++)
                {
                    Geometry.Triangle face = finalFaces[i];
                    Vector3 edge1 = face.a.position - face.b.position;
                    Vector3 edge2 = face.b.position - face.c.position;
                    Vector3 normal = Vector3.Cross(edge1, edge2).normalized;

                    // switch direction, to face +z direction, all other normals are corrent
                    if (i == 1)
                        normal *= -1;

                    axes[i] = normal;
                }

                return axes;
            }

            public bool Hit
            {
                get
                {
                    return _hit;
                }
                set
                {
                    if (_hit != value)
                    {
                        _hit = value;
                        OnHit();
                    }
                }
            }

            void OnHit()
            {
                // Debug.Log("hit detected");
            }

        }
    }

    namespace Geometry
    {
        public static class TriangulatePolygon
        {
            /// <summary>
            /// EAR_CLIPPING, This assumes that we have a polygon and now we want to triangulate it
            /// The points on the polygon should be ordered counter-clockwise
            /// This alorithm is called ear clipping and it's O(n*n) Another common algorithm is dividing it into trapezoids and it's O(n log n)
            /// One can maybe do it in O(n) time but no such version is known
            /// Assumes we have at least 3 points
            /// </summary>
            /// <param name="points"></param>
            /// <returns></returns>
            public static List<Triangle> TriangulateConcavePolygon(List<Vector3> points)
            {
                //The list with triangles the method returns
                List<Triangle> triangles = new List<Triangle>();

                //If we just have three points, then we dont have to do all calculations
                if (points.Count == 3)
                {
                    triangles.Add(new Triangle(points[0], points[1], points[2]));

                    return triangles;
                }



                //Step 1. Store the vertices in a list and we also need to know the next and prev vertex
                List<Vertex> vertices = new List<Vertex>();

                for (int i = 0; i < points.Count; i++)
                {
                    vertices.Add(new Vertex(points[i]));
                }

                //Find the next and previous vertex
                for (int i = 0; i < vertices.Count; i++)
                {
                    int nextPos = Utils.CommonMaths.ClampListIndex(i + 1, vertices.Count);
                    int prevPos = Utils.CommonMaths.ClampListIndex(i - 1, vertices.Count);

                    vertices[i].prevVertex = vertices[prevPos];
                    vertices[i].nextVertex = vertices[nextPos];
                }



                //Step 2. Find the reflex (concave) and convex vertices, and ear vertices
                for (int i = 0; i < vertices.Count; i++)
                {
                    CheckIfReflexOrConvex(vertices[i]);
                }

                //Have to find the ears after we have found if the vertex is reflex or convex
                List<Vertex> earVertices = new List<Vertex>();

                for (int i = 0; i < vertices.Count; i++)
                {
                    IsVertexEar(vertices[i], vertices, earVertices);
                }



                //Step 3. Triangulate!
                while (true)
                {
                    //This means we have just one triangle left
                    if (vertices.Count == 3)
                    {
                        //The final triangle
                        triangles.Add(new Triangle(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));

                        break;
                    }

                    //Make a triangle of the first ear
                    Vertex earVertex = earVertices[0];

                    Vertex earVertexPrev = earVertex.prevVertex;
                    Vertex earVertexNext = earVertex.nextVertex;

                    Triangle newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);

                    triangles.Add(newTriangle);

                    //Remove the vertex from the lists
                    earVertices.Remove(earVertex);

                    vertices.Remove(earVertex);

                    //Update the previous vertex and next vertex
                    earVertexPrev.nextVertex = earVertexNext;
                    earVertexNext.prevVertex = earVertexPrev;

                    //...see if we have found a new ear by investigating the two vertices that was part of the ear
                    CheckIfReflexOrConvex(earVertexPrev);
                    CheckIfReflexOrConvex(earVertexNext);

                    earVertices.Remove(earVertexPrev);
                    earVertices.Remove(earVertexNext);

                    IsVertexEar(earVertexPrev, vertices, earVertices);
                    IsVertexEar(earVertexNext, vertices, earVertices);
                }

                //Debug.Log(triangles.Count);

                return triangles;
            }

            //Check if a vertex if reflex or convex, and add to appropriate list
            private static void CheckIfReflexOrConvex(Vertex v)
            {
                v.isReflex = false;
                v.isConvex = false;

                //This is a reflex vertex if its triangle is oriented clockwise
                Vector3 a = v.prevVertex.position;
                Vector3 b = v.position;
                Vector3 c = v.nextVertex.position;

                if (Triangle.IsTriangleOrientedClockwise(a, b, c))
                {
                    v.isReflex = true;
                }
                else
                {
                    v.isConvex = true;
                }
            }

            //Check if a vertex is an ear
            private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
            {
                //A reflex vertex cant be an ear!
                if (v.isReflex)
                {
                    return;
                }

                //This triangle to check point in triangle
                Vector3 a = v.prevVertex.position;
                Vector3 b = v.position;
                Vector3 c = v.nextVertex.position;

                bool hasPointInside = false;

                for (int i = 0; i < vertices.Count; i++)
                {
                    //We only need to check if a reflex vertex is inside of the triangle
                    if (vertices[i].isReflex)
                    {
                        Vector3 p = vertices[i].position;

                        //This means inside and not on the hull
                        if (Triangle.IsPointInTriangle(a, b, c, p))
                        {
                            hasPointInside = true;
                            break;
                        }
                    }
                }

                if (!hasPointInside)
                {
                    earVertices.Add(v);
                }
            }
        }

        [System.Serializable]
        public class Triangle
        {
            public Vertex a;
            public Vertex b;
            public Vertex c;

            public Triangle(Vertex v1, Vertex v2, Vertex v3)
            {
                a = v1;
                b = v2;
                c = v3;
            }

            public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                a = new Vertex(v1);
                b = new Vertex(v2);
                c = new Vertex(v3);
            }

            public Vector3 GetCentre()
            {
                // direct method
                float x = (a.position.x + b.position.x + c.position.x) / 3;
                float y = (a.position.y + b.position.y + c.position.y) / 3;
                float z = (a.position.z + b.position.z + c.position.z) / 3;

                Vector3 centrePos = new Vector3(x, y, z);

                return centrePos;
            }

            //
            // Is a triangle in 2d space oriented clockwise or counter-clockwise
            //
            //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
            //https://en.wikipedia.org/wiki/Curve_orientation
            public static bool IsTriangleOrientedClockwise(Vector3 p1, Vector3 p2, Vector3 p3)
            {
                bool isClockWise = true;
                float determinant = p1.x * p2.z + p3.x * p1.z + p2.x * p3.z - p1.x * p3.z - p3.x * p2.z - p2.x * p1.z;

                if (determinant > 0f)
                {
                    isClockWise = false;
                }

                return isClockWise;
            }

            // From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
            // p is the testpoint, and the other points are corners in the triangle
            public static bool IsPointInTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 p)
            {
                bool isWithinTriangle = false;

                //Based on Barycentric coordinates
                float denominator = ((p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z));

                float a = ((p2.z - p3.z) * (p.x - p3.x) + (p3.x - p2.z) * (p.y - p3.z)) / denominator;
                float b = ((p3.z - p1.z) * (p.x - p3.x) + (p1.x - p3.z) * (p.y - p3.z)) / denominator;
                float c = 1 - a - b;

                // The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
                // if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
                // {
                //    isWithinTriangle = true;
                // }

                //The point is within the triangle
                if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
                {
                    isWithinTriangle = true;
                }

                return isWithinTriangle;
            }
        }
         
        public class Vertex
        {
            public Vector3 position;

            //Which triangle is this vertex a part of?
            public Triangle triangle;

            //The previous and next vertex this vertex is attached to
            public Vertex prevVertex;
            public Vertex nextVertex;

            //Properties this vertex may have
            //Reflex is concave
            public bool isReflex;
            public bool isConvex;
            public bool isEar;

            public Vertex(Vector3 position)
            {
                this.position = position;
            }

            //Get 2d pos of this vertex
            public Vector2 GetPos2D_XZ()
            {
                Vector2 pos_2d_xz = new Vector2(position.x, position.z);

                return pos_2d_xz;
            }
        }
    }

    public static class Geo2dUtils
    {
        public static int Orientation(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float val = (p2.y - p1.y) * (p3.x - p2.x) -
                        (p2.x - p1.x) * (p3.y - p2.y);
            if (val == 0) return 0; // collinear
            return (val > 0) ? 1 : 2; // clockwise or counterclockwise
        }

        public static bool OnSegment(Vector2 p, Vector2 r, Vector2 q)
        {
            if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
                q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
                return true;

            return false;
        }

        public static bool IsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);

            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            if ((o1 != o2) && (o3 != o4)) return true;

            // special case when
            // p1, q1, p2 are collinear and p2 lies on p1q1

            if (o1 == 0 && OnSegment(p1, p2, p2)) return true;

            // special case when
            // p1, p1, q2 are collinear and q2 lies on p1q1

            if (o2 == 0 && OnSegment(p1, p2, q2)) return true;

            // special case when
            // p2, q2, p1 are collinear and p1 lies on p2q2

            if (o3 == 0 && OnSegment(p2, q2, p1)) return true;

            // special case when
            // p2, q2, q1 are collinear and q1 lies on p2q2

            if (o4 == 0 && OnSegment(p2, q2, q1)) return true;

            return false;
        }

        public static bool PointInsidePolygon(Vector2[] polygon, int n, Vector2 p)
        {
            if (n < 3) return false; // vertext count must be > 3

            int count = 0; int i = 0;

            do
            {
                int next = (i + 1) % n;

                if (IsIntersect(polygon[i], polygon[next], p, new Vector2(float.MaxValue, p.y)))
                {
                    if (Orientation(polygon[i], polygon[next], p) == 0)
                    {
                        return OnSegment(polygon[i], polygon[next], p);
                    }

                    count++;
                }
                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise.
            return count % 2 == 1;
        }

        public static Vector2 Compute2DPolygonCentroid(Vector2[] vertices, int vertexCount)
        {
            Vector2 centroid = new Vector2(0, 0);

            double signedArea = 0.0f;
            float x0 = 0.0f; // Current vertex X
            float y0 = 0.0f; // Current vertex Y
            float x1 = 0.0f; // Next vertex X
            float y1 = 0.0f; // Next vertex Y
            float a = 0.0f;  // Partial signed area

            // For all vertices
            int i = 0;
            for (i = 0; i < vertexCount - 1; ++i)
            {
                x0 = vertices[i].x;
                y0 = vertices[i].y;
                x1 = vertices[(i + 1) % vertexCount].x;
                y1 = vertices[(i + 1) % vertexCount].y;
                a = x0 * y1 - x1 * y0;
                signedArea += a;
                centroid.x += (x0 + x1) * a;
                centroid.y += (y0 + y1) * a;
            }

            // Do last vertex separately to avoid performing an expensive
            // modulus operation in each iteration.
            x0 = vertices[i].x;
            y0 = vertices[i].y;
            x1 = vertices[0].x;
            y1 = vertices[0].y;
            a = x0 * y1 - x1 * y0;
            signedArea += a;
            centroid.x += (x0 + x1) * a;
            centroid.y += (y0 + y1) * a;

            signedArea *= 0.5;

            centroid.x /= (float)(vertexCount * signedArea);
            centroid.y /= (float)(vertexCount * signedArea);

            return centroid;
        }

        public static bool IsColinear(Vector3 start, Vector3 end, Vector3 inBetween)
        {
            return Vector3.Cross(end - start, inBetween - start) == Vector3.zero;
        }

        public static bool OnLine(Vector3 start, Vector3 end, Vector3 inBetween)
        {
            return inBetween.x <= end.x && inBetween.x >= start.x
              && inBetween.y <= end.y && inBetween.y >= start.y
              && inBetween.z <= end.z && inBetween.z >= start.z
              && Vector3.Cross(end - start, inBetween - start) == Vector3.zero;
        }
    }
}
