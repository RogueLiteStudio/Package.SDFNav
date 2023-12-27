using UnityEditor;
using UnityEngine;

namespace SDFNav.Editor
{
    public class DebugDrawWindow : EditorWindow
    {
        public EdgeData Edge;
        public Transform TestPoint;
        public EdgeSDFResult SDFResult;
        public SDFPreviewRender SDFPreview = new SDFPreviewRender();
        public DebugPathFinder PathFinderDebug = new DebugPathFinder();
        public static void DrawEdge(EdgeData edge)
        {
            var window = GetWindow<DebugDrawWindow>();
            window.Edge = edge;
            window.SDFTest();
        }

        public static void DrawSDF(SDFData data)
        {
            var window = GetWindow<DebugDrawWindow>();
            window.SDFPreview.Build(data);
            window.PathFinderDebug.SDF = data;
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        private void SDFTest()
        {
            if (TestPoint)
            {
                Vector2 pt = SDFNavEditorUtil.ToV2(TestPoint.position);
                SDFResult = EdgeEditorUtil.SDF(pt, Edge);
            }
        }

        private void OnGUI()
        {
            TestPoint = EditorGUILayout.ObjectField(TestPoint, typeof(Transform), true) as Transform;
            if (TestPoint && Edge != null)
            {
                SDFTest();
            }
            PathFinderDebug.OnGUI();
            SDFPreview.OnGUI();
        }

        private void OnSceneGUI(SceneView view)
        {
            if (Edge != null)
            {
                DebugDrawUtil.DrawEdgeOnScene(Edge);
                if (TestPoint)
                {
                    DebugDrawUtil.DrawSDFResult(SDFResult, Edge);
                }
            }
            SDFPreview.OnSceneGUI();
            PathFinderDebug.OnSceneGUI();
        }
    }
}