using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace SDFNav.Editor
{
    public static class DebugDrawUtil
    {
        private static Vector3 ToV3(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }
        public static void DrawEdgeOnScene(EdgeData edgeData)
        {
            foreach (var seg in edgeData.Segments)
            {
                Vector3 from = edgeData.Vertices[seg.From].ToV3();
                Vector3 to = edgeData.Vertices[seg.To].ToV3();
                Handles.DrawLine(from, to);
                Vector3 diff = to - from;
                using(new Handles.DrawingScope(Color.green))
                {
                    Handles.ArrowHandleCap(0, from, Quaternion.LookRotation(diff.normalized), 1f, EventType.Repaint);
                }
            }
        }

        public static void DrawSDFResult(EdgeSDFResult result, EdgeData edgeData)
        {
            using (new Handles.DrawingScope(result.Distance < 0 ? Color.red : Color.blue))
            {
                Vector3 from = edgeData.Vertices[result.Segment.From].ToV3();
                Vector3 to = edgeData.Vertices[result.Segment.To].ToV3();

                Handles.DrawLine(from, to);
                Handles.DrawLine(result.Point.ToV3(), from);
                Handles.DrawLine(result.Point.ToV3(), to);
            }
        }

        public static void DrawPoint(Vector3 pt, float size, Color color)
        {
            using (new Handles.DrawingScope(color))
            {
                pt.y = 0;
                Handles.SphereHandleCap(0, pt, Quaternion.identity, size, EventType.Repaint);
            }
        }

        public static void DrawLine(Vector2 from, Vector2 to, Color color, float thickness = 0)
        {
            using (new Handles.DrawingScope(color))
            {
                Handles.DrawLine(from.ToV3(), to.ToV3(), thickness);
            }
        }

        public static void DrawPath(List<Vector2> path, Color color, float thickness = 0)
        {
            using (new Handles.DrawingScope(color))
            {
                for (int i = 1; i < path.Count; ++i)
                {
                    Handles.DrawLine(path[i - 1].ToV3(), path[i].ToV3(), thickness);
                }
            }
        }
    }
}