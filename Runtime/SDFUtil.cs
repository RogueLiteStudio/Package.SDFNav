using UnityEngine;
namespace SDFNav
{
    public static class SDFUtil
    {
        public static float CircleSDF(Vector2 point, Vector2 center, float radius)
        {
            return Vector2.Distance(point, center) - radius;
        }
        public static Vector2 Rotate(this Vector2 vec, Vector2 normal)
        {
            return new Vector2(vec.x * normal.x - vec.y * normal.y, vec.x * normal.y + vec.y * normal.x);
        }
        public static Vector2 Abs(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }
        public static Vector2 Rotate(this Vector2 vec, float angle)
        {
            float radians = Mathf.Deg2Rad * angle;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(vec.x * cos - vec.y * sin, vec.x * sin + vec.y * cos);
        }

        //反向旋转向量
        public static Vector2 InvertRotate(this Vector2 vec, Vector2 normal)
        {
            return new Vector2(vec.x * normal.x + vec.y * normal.y, -vec.x * normal.y + vec.y * normal.x);
        }

        public static float SegmentSDF(Vector2 point, Vector2 from, Vector2 to)
        {
            Vector2 ap = point - from;
            Vector2 ab = to - from;
            float h = Mathf.Clamp01(Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab));
            return (ap - h * ab).magnitude;
        }

        public static float BoxSDF(Vector2 point, Vector2 center, Vector2 halfSize)
        {
            point -= center;
            Vector2 d = point.Abs() - halfSize;
            return Vector2.Max(d, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0);
        }

        public static float OrientedBoxSDF(Vector2 point, Vector2 center, Vector2 rotation, Vector2 halfSize)
        {
            point -= center;
            point = point.Rotate(rotation);
            Vector2 d = point.Abs() - halfSize;
            return Vector2.Max(d, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0);
        }

        public static float TriangleSDF(Vector2 point, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            Vector2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
            Vector2 v0 = point - p0, v1 = point - p1, v2 = point - p2;
            Vector2 pq0 = v0 - e0 * Mathf.Clamp01(Vector2.Dot(v0, e0) / Vector2.Dot(e0, e0));
            Vector2 pq1 = v1 - e1 * Mathf.Clamp01(Vector2.Dot(v1, e1) / Vector2.Dot(e1, e1));
            Vector2 pq2 = v2 - e2 * Mathf.Clamp01(Vector2.Dot(v2, e2) / Vector2.Dot(e2, e2));
            float s = Mathf.Sign(e0.x * e2.y - e0.y * e2.x);

            Vector2 d = Vector2.Min(Vector2.Min(new Vector2(Vector2.Dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
                         new Vector2(Vector2.Dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
                         new Vector2(Vector2.Dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
            return -Mathf.Sqrt(d.x) * Mathf.Sign(d.y);
        }

        public static bool IsInTriangle(Vector2 point, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            Vector2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
            Vector2 v0 = point - p0, v1 = point - p1, v2 = point - p2;
            float s = Mathf.Sign(e0.x * e2.y - e0.y * e2.x);

            float y = Mathf.Min(s * (v0.x * e0.y - v0.y * e0.x), s * (v1.x * e1.y - v1.y * e1.x));
            y = Mathf.Min(y, s * (v2.x * e2.y - v2.y * e2.x));
            return y > 0;
        }
    }

}
