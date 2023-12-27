using UnityEngine;
namespace SDFNav.Editor
{
    public static class SDFNavEditorUtil
    {
        public static Vector2 ToV2(Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
        public static float Cross(Vector2 lhs, Vector2 rhs)
        {
            return lhs.x * rhs.y - lhs.y * rhs.x;
        }

        public static float PointOnSegmentSide(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ap = p - a;
            Vector2 ab = b - a;
            return Mathf.Sign(Cross(ap, ab));
        }

        public static float SqrMagnitudeToSegment(Vector2 point, Vector2 from, Vector2 to)
        {
            Vector2 ap = point - from;
            Vector2 ab = to - from;
            float h = Mathf.Clamp01(Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab));
            return (ap - h * ab).sqrMagnitude;
        }

        public static bool IsInTriangle(Vector2 point, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            Vector2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
            Vector2 v0 = point - p0, v1 = point - p1, v2 = point - p2;
            float s = Mathf.Sign(e0.x * e2.y - e0.y * e2.x);

            float y = Mathf.Min(s * (v0.x * e0.y - v0.y * e0.x), s * (v1.x * e1.y - v1.y * e1.x));
            y = Mathf.Min(y, s * (v2.x * e2.y - v2.y * e2.x));
            return y >= 0;
        }
        public static bool IsPointInSubMesh(Vector2 p, SubMeshData subMesh)
        {
            foreach (var idx in subMesh.TriangleIndices)
            {
                var t = subMesh.Mesh.Triangles[idx];
                var a = subMesh.Mesh.Vertices[t.A];
                var b = subMesh.Mesh.Vertices[t.B];
                var c = subMesh.Mesh.Vertices[t.C];
                if (IsInTriangle(p, new Vector2(a.x, a.z), new Vector2(b.x, b.z), new Vector2(c.x, c.z)))
                    return true;
            }
            return false;
        }

        public static Vector3 GetTriangleCenter(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = Vector3.LerpUnclamped(a, b, 0.5f);
            return Vector3.LerpUnclamped(ab, c, 0.5f);
        }
    }
}