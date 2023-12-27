using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SDFNav.Editor
{
    public static class EdgeEditorUtil
    {
        public static Rect CalcBounds(EdgeData edgeData, float expand)
        {
            float xMin = float.MaxValue;
            float yMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMax = float.MinValue;
            foreach (var seg in edgeData.Segments)
            {
                var from = edgeData.Vertices[seg.From];
                var to = edgeData.Vertices[seg.To];
                xMin = Mathf.Min(xMin, from.x);
                xMin = Mathf.Min(xMin, to.x);

                xMax = Mathf.Max(xMax, from.x);
                xMax = Mathf.Max(xMax, to.x);

                yMin = Mathf.Min(yMin, from.y);
                yMin = Mathf.Min(yMin, to.y);

                yMax = Mathf.Max(yMax, from.y);
                yMax = Mathf.Max(yMax, to.y);
            }
            return Rect.MinMaxRect(xMin - expand, yMin - expand, xMax + expand, yMax + expand);
        }

        public static EdgeData SubMeshToEdgeByOffset(SubMeshData subMesh)
        {
            var mesh = subMesh.Mesh;
            EdgeData edgeData = new EdgeData { Vertices = mesh.Vertices.Select(it => SDFNavEditorUtil.ToV2(it)).ToArray() };
            List<SegmentIndice> segments = new List<SegmentIndice>();
            for (int i = 0; i < subMesh.TriangleIndices.Count; ++i)
            {
                var t = mesh.Triangles[subMesh.TriangleIndices[i]];
                if (!segments.Exists(it => (t.A == it.From && t.B == it.To) || t.A == it.To && t.B == it.From))
                {
                    segments.Add(new SegmentIndice { From = t.A, To = t.B });
                }
                if (!segments.Exists(it => (t.B == it.From && t.C == it.To) || t.B == it.To && t.C == it.From))
                {
                    segments.Add(new SegmentIndice { From = t.B, To = t.C });
                }
                if (!segments.Exists(it => (t.C == it.From && t.A == it.To) || t.C == it.To && t.A == it.From))
                {
                    segments.Add(new SegmentIndice { From = t.C, To = t.A });
                }
            }

            foreach (var seg in segments)
            {
                Vector2 from = edgeData.Vertices[seg.From];
                Vector2 to = edgeData.Vertices[seg.To];
                Vector2 normal = to - from;
                Vector2 center = from + normal * 0.5f;

                normal = new Vector2(-normal.y, normal.x);
                normal.Normalize();
                if (!SDFNavEditorUtil.IsPointInSubMesh(center + normal * 0.05f, subMesh))
                {
                    edgeData.Segments.Add(seg);
                }
            }
            RemoveUnUsedVertice(edgeData);
            return edgeData;
        }

        public static EdgeSDFResult SDF(Vector2 p, EdgeData edgeData)
        {
            EdgeSDFResult result = new EdgeSDFResult { Point = p };
            float sqrMagnitude = float.MaxValue;
            for (int i = 0; i < edgeData.Segments.Count; ++i)
            {
                var seg = edgeData.Segments[i];
                var from = edgeData.Vertices[seg.From];
                var to = edgeData.Vertices[seg.To];
                float sqrM = SDFNavEditorUtil.SqrMagnitudeToSegment(p, from, to);
                //如果点在线上就直接返回
                if (Mathf.Abs(sqrM) < 0.0001f)
                {
                    result.Segment = seg;
                    result.Distance = 0;
                    return result;
                }
                float diff = sqrM - sqrMagnitude;
                if (diff < 0)
                {
                    result.Segment = seg;
                    sqrMagnitude = sqrM;
                }
            }
            result.Distance = Mathf.Sqrt(sqrMagnitude);
            return result;
        }

        public static void RemoveUnUsedVertice(EdgeData edgeData)
        {
            HashSet<int> usedIndex = new HashSet<int>();
            foreach (var s in edgeData.Segments)
            {
                usedIndex.Add(s.From);
                usedIndex.Add(s.To);
            }
            Vector2[] vertices = new Vector2[usedIndex.Count];
            int indx = usedIndex.Count - 1;
            for (int i= edgeData.Vertices.Length-1; i>=0; --i)
            {
                if (usedIndex.Contains(i))
                {
                    vertices[indx--] = edgeData.Vertices[i];
                }
                else
                {
                    RemoveVertice(edgeData.Segments, i);
                }
            }
            edgeData.Vertices = vertices;
        }

        private static void RemoveVertice(List<SegmentIndice> segments, int idx)
        {
            for (int i=0; i<segments.Count; ++i)
            {
                var seg = segments[i];
                if (seg.From > idx)
                    --seg.From;
                if (seg.To > idx)
                    --seg.To;
                segments[i] = seg;
            }
        }
    }
}