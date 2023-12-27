using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace SDFNav.Editor
{
    [System.Serializable]
    public class DebugPathFinder
    {
        public List<Vector2> Path = new List<Vector2>();
        public Transform Start;
        public Transform End;
        public SDFData SDF;
        public float Radius = 0.5f;
        private JPSPathFinder PathFinder;

        public void OnGUI()
        {
            GUILayout.Label("寻路", EditorStyles.boldLabel);
            Radius = EditorGUILayout.FloatField("碰撞半径", Radius);
            Start = EditorGUILayout.ObjectField("起点", Start, typeof(Transform), true) as Transform;
            End = EditorGUILayout.ObjectField("终点", End, typeof(Transform), true) as Transform;

            if (SDF != null && SDF.Width != 0 && Start && End)
            {
                if (GUILayout.Button("刷新寻路"))
                {
                    Path.Clear();
                    Vector3 start = Start.position;
                    Vector3 end = End.position;
                    if (PathFinder == null)
                    {
                        PathFinder = new JPSPathFinder(SDF);
                    }
                    PathFinder.Find(new Vector2(start.x, start.z), new Vector2(end.x, end.z), Radius, Path);
                    SceneView.RepaintAll();
                }
            }
            GUILayout.Label($"路点数量 {Path.Count}");
        }

        public void OnSceneGUI()
        {
            DebugDrawUtil.DrawPath(Path, Color.blue, 0.5f);
            Color color = Color.white;
            if (!Start || !End || Path.Count == 0)
            {
                color = Color.red;
            }
            if (Start)
            {
                DebugDrawUtil.DrawPoint(Start.position, Radius, color);
                Vector2 pos = new Vector2(Start.position.x, Start.position.z);
                Vector2 gradiend = SDF.Gradiend(pos);
                DebugDrawUtil.DrawLine(pos, pos - gradiend.normalized * SDF.Sample(pos), Color.red);
            }
            if (End)
                DebugDrawUtil.DrawPoint(End.position, Radius, color);
        }
    }
}
