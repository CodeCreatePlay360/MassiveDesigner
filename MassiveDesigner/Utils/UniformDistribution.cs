using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.Geometry;
using System.Linq;


namespace CodeCreatePlay
{
    public class UniformDistribution : MonoBehaviour
    {
        List<Vector3> points = new List<Vector3>();
        List<Triangle> triangles = new List<Triangle>();

        public void Start()
        {
            if (!GetComponent<LocationTool.Location>())
                return;

            var loc = GetComponent<LocationTool.Location>();
            triangles = TriangulatePolygon.TriangulateConcavePolygon(loc.locationBase.Boundaries.ToList());
            points = UniformDistributions.GetRandUniformInPolygon(triangles, 3000);
        }

        public void OnDrawGizmos()
        {
            foreach (var item in triangles)
            {
                Gizmos.DrawLine(item.a.position, item.b.position);
                Gizmos.DrawLine(item.a.position, item.c.position);
                Gizmos.DrawLine(item.b.position, item.c.position);
            }

            foreach (var item in points)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(item, 0.05f);
            }
        }
    }

    public static class UniformDistributions
    {
        public static void GenUniformInCamFOV(Camera cam, int spacing, out List<Vector3> points)
        {
            Vector3[] clipPoints = new Vector3[5];

            float halfFOV = (cam.fieldOfView / 2) * Mathf.Deg2Rad;
            float aspect = cam.aspect;
            float distance = cam.farClipPlane;
            float height = distance * Mathf.Tan(halfFOV);
            float width = height * aspect;
            // width = height / aspect;

            // lower right.
            clipPoints[0] = cam.transform.position + cam.transform.right * width;
            clipPoints[0] -= cam.transform.up * height;
            clipPoints[0] += cam.transform.forward * distance;

            // lower left.
            clipPoints[1] = cam.transform.position - cam.transform.right * width;
            clipPoints[1] -= cam.transform.up * height;
            clipPoints[1] += cam.transform.forward * distance;

            // upper right.
            clipPoints[2] = cam.transform.position + cam.transform.right * width;
            clipPoints[2] += cam.transform.up * height;
            clipPoints[2] += cam.transform.forward * distance;

            // upper left.
            clipPoints[3] = cam.transform.position - cam.transform.right * width;
            clipPoints[3] += cam.transform.up * height;
            clipPoints[3] += cam.transform.forward * distance;

            // middle.
            clipPoints[4] = cam.transform.position + cam.transform.forward * -cam.nearClipPlane;

            points = new List<Vector3>();

            float s = Vector3.Distance(clipPoints[0], cam.transform.position);
            float numPointsVertical = s / spacing;

            // TO:DO Reverse directions dir_1 & dir_2 lower clip points for "-" camera  

            Vector3 dir1 = (clipPoints[0] - cam.transform.position);
            dir1.Normalize();
            Vector3 dir2 = (clipPoints[1] - cam.transform.position);
            dir2.Normalize();

            for (int i = 1; i <= numPointsVertical; ++i)
            {
                Vector3 p1 = cam.transform.position + (dir1 * (i * spacing));
                points.Add(p1);

                Vector3 p2 = cam.transform.position + (dir2 * (i * spacing));
                points.Add(p2);

                s = Vector3.Distance(p1, p2);
                var numPointsHorizontal = s / spacing;

                Vector3 dir3 = p1 - p2; // right - left
                dir3.Normalize();


                for (int j = 1; j <= numPointsHorizontal - 1; j++)
                {
                    Vector3 p3 = p2 + (dir3 * (j * spacing));
                    points.Add(p3);
                }

                if (Vector3.Distance(points[points.Count - 1], p1) > spacing)
                {
                    var p = (points[points.Count - 1] + p1) / 2;
                    points.Add(p);
                }
            }
        }

        public static void GenPointsGridInRectangle(Vector3[] rectangle, int spacing, out List<Vector3> points)
        {
            // rectangle[0] = top left
            // rectangle[2] = bottom left

            // rectangle[1] = top right
            // rectangle[3] = bottom right

            points = new List<Vector3>();

            // ...for vertical.........................................
            Vector3 p0 = rectangle[0]; // top left
            Vector3 p1 = rectangle[2]; // bottom left

            var distance = Vector3.Distance(p0, p1);
            var dir_v = p0 - p1; // top left - bottom left
            dir_v.Normalize();
            var points_count_v = Mathf.CeilToInt(distance / spacing);
            // .........................................

            // ...for horizontal.........................................
            var distance_h = Vector3.Distance(rectangle[2], rectangle[3]);
            var dir_h = rectangle[3] - rectangle[2]; // bottom left - bottom right
            dir_h.Normalize();
            float points_count_h = Mathf.CeilToInt(distance_h / spacing);
            // .........................................

            Vector3 p;
            Vector3 lastVerticalPoint = Vector3.zero;
            Vector3 firsthorizontalPoint = Vector3.zero;
            for (int i = 0; i <= points_count_v - 1; i++)
            {
                p = p1 + (dir_v * (i * spacing));
                points.Add(p);

                lastVerticalPoint = p;

                for (int j = 1; j < points_count_h; j++)
                {
                    var pp = p + (dir_h * (j * spacing));
                    points.Add(pp);
                    if (i == 0)
                        firsthorizontalPoint = pp;
                }
            }

            var v = false;

            // if distance between last vertical point and top left corner is greater then spacing/2,
            // then add another point p = top_left of rectangle.
            if (Vector3.Distance(p0, lastVerticalPoint) > spacing / 2)
            {
                points.Add(p0);

                for (int j = 1; j < points_count_h; j++)
                    points.Add(p0 + (dir_h * (j * spacing)));

                v = true;
            }

            if (Vector3.Distance(rectangle[3], firsthorizontalPoint) > spacing / 2)
            {
                if (v)
                    points.Add(rectangle[1]);

                for (int j = 0; j < points_count_v; j++)
                    points.Add(rectangle[3] + (dir_v * (j * spacing)));
            }
        }

        /// <summary>
        /// polygon[0] = ORIGIN;
        /// polygon[1] = BOTTOM_RIGHT
        /// polygon[2] = TOP_RIGHT
        /// polygon[3] = TOP_LEFT
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Vector3 GetUniformInParallelogram(Vector3[] polygon, float scale)
        {
            // polygon[0] = ORIGIN;
            // polygon[1] = BOTTOM_RIGHT
            // polygon[2] = TOP_RIGHT
            // polygon[3] = TOP_LEFT

            var a = polygon[1] - polygon[0];
            var b = polygon[3] - polygon[0];

            var u1 = Random.Range(0f, 1f);
            var u2 = Random.Range(0f, 1f);

            return u1 * (a * scale) + u2 * (b * scale);
        }

        public static void GenUniformInParallelogram(Vector3[] polygon, float scale, int samplesCount, out List<Vector3> points)
        {
            points = new List<Vector3>();

            for (int i = 0; i < samplesCount; i++)
                points.Add(GetUniformInParallelogram(polygon, scale));
        }

        public static Vector3 GetUniformInCircle(float radius)
        {
            float theta = 2 * Mathf.PI * UnityEngine.Random.Range(0f, 1f);
            var r = radius * Mathf.Pow(Random.Range(0f, 1f), 1 / 2f);

            return new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
        }

        public static void GenUniformInCircle(float radius, float count, ref List<Vector3> points)
        {
            points.Clear();

            for (int i = 0; i < count; i++)
            {
                float theta = 2 * Mathf.PI * UnityEngine.Random.Range(0f, 1f);
                var r = radius * Mathf.Pow(Random.Range(0f, 1f), 1 / 2f);

                var point = new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));

                points.Add(point);
            }
        }

        public static Vector3 GetUniformInTriangle(Vector3[] polygon, float scale)
        {
            var a = polygon[1] - polygon[0];
            var b = polygon[2] - polygon[0];

            var u1 = Random.Range(0f, 1f);
            var u2 = Random.Range(0f, 1f);

            // the reflection step
            if ((u1 + u2) > 1)
            {
                u1 = 1 - u1;
                u2 = 1 - u2;
            }

            return u1 * (a * scale) + u2 * (b * scale);
        }
         
        public static List<Vector3> GetRandUniformInPolygon(List<Triangle> triangles, int count = 1000)
        {
            List<Vector3> pointsGenerated = new();

            float totalPolyArea = 0f;
            List<float> triangleAreas = new();
            foreach (var item in triangles)
            {
                totalPolyArea += CalculateTriangleArea(item, ref triangleAreas);
            }
             
            List<float> probabilites = new();
            for (int i = 0; i < triangleAreas.Count; i++)
            {
                probabilites.Add(triangleAreas[i] / totalPolyArea);
            }

            pointsGenerated.Clear();
            Vector3[] triangle = new Vector3[3];

            for (int i = 0; i < count; i++)
            {
                // choose a triangle based on it's area
                int choice = Utils.CommonMaths.WeightedChoice(probabilites);

                triangle[0] = triangles[choice].a.position;
                triangle[1] = triangles[choice].b.position;
                triangle[2] = triangles[choice].c.position;

                pointsGenerated.Add(UniformDistributions.GetUniformInTriangle(triangle, 1f) + triangles[choice].a.position);
            }

            return pointsGenerated;
        }

        public static List<Vector3> GetRandUniformInPolygon(GameObject polygon, int count=1000)
        {
            List<Triangle> triangles = new();
            GetAllTriangles(polygon, ref triangles);
            return GetRandUniformInPolygon(triangles);
        }

        public static List<Vector3> UniformInLocation(LocationTool.Location location)
        {
            return location.locationBase.GetUniformPointsDistribution();
        }

        static List<Triangle> GetAllTriangles(GameObject polygon, ref List<Triangle> triangles)
        {
            var verts = polygon.GetComponent<MeshFilter>().sharedMesh.vertices;
            var indices = polygon.GetComponent<MeshFilter>().sharedMesh.triangles;

            int nextStop = 3;
            for (int i = 0; i < indices.Length + 4; i++)
            {
                if (i > nextStop)
                {
                    int indice1 = indices[nextStop - 3];
                    int indice2 = indices[nextStop - 2];
                    int indice3 = indices[nextStop - 1];

                    // get points for the new triangle.
                    Vector3 v1 = polygon.transform.TransformPoint(verts[indice1]);
                    Vector3 v2 = polygon.transform.TransformPoint(verts[indice2]);
                    Vector3 v3 = polygon.transform.TransformPoint(verts[indice3]);

                    triangles.Add(new Triangle(v1, v2, v3));

                    nextStop += 3;
                }
            }

            return triangles;
        }

        static float CalculateTriangleArea(Triangle triangle, ref List<float> triangleAreas)
        {
            // area of triangle = Area = ?s (s?a)(s?b)(s?c) where s = perimeter of triangle, a=b=c = lengths of sides of triangle

            float length_ab = (triangle.b.position - triangle.a.position).magnitude;
            float length_bc = (triangle.c.position - triangle.b.position).magnitude;
            float length_ca = (triangle.c.position - triangle.a.position).magnitude;

            float perimeter = (length_ab + length_bc + length_ca) / 2f;
            float area = Mathf.Sqrt(perimeter * (perimeter - length_ab) * (perimeter - length_bc) * (perimeter - length_ca));

            triangleAreas.Add(area);

            return area;
        }
    }
}
