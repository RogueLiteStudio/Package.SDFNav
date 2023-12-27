using SDFNav.Editor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace mygame
{
    public class SceneSDFPreviewWindow : EditorWindow
    {
        const string Dir = "Assets/InApp/Bytes/SceneSDF";
        public SDFPreviewRender MainRender;
        public HeightMapPreviewRender HeightMap;
        public List<string> Files = new List<string>();
        public string SelectFile;
        public bool ShowSDF = true;
        public bool ShowHeightMap = true;
        //[MenuItem("Tools/美术/场景SDF预览", false, 5000)]
        static void Open()
        {
            GetWindow<SceneSDFPreviewWindow>();
        }

        private void Awake()
        {
            RefreshFileList();
        }

        private void RefreshFileList()
        {
            MainRender?.Clear();
            MainRender = null;
            HeightMap?.Clear();
            HeightMap = null;
            Files.Clear();
            var files = Directory.GetFiles(Dir, "*.bytes");
            foreach ( var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (name.EndsWith("_sdf"))
                {
                    Files.Add(name.Substring(0, name.Length - 4));
                }
            }
        }

        private void OnSelectChange()
        {
            if (string.IsNullOrEmpty(SelectFile))
                return;
            string filePath = $"{Dir}/{SelectFile}_sdf.bytes";
            if (!File.Exists(filePath))
                return;
            using var file = new FileStream(filePath, FileMode.Open);
            using BinaryReader reader = new BinaryReader(file);
            SDFNav.SDFScene scene = new SDFNav.SDFScene();
            scene.Read(reader);
            MainRender ??= new SDFPreviewRender();
            MainRender.Build(scene.Data);
            if (scene.HeightMap.Data != null && scene.HeightMap.Data.Length > 0)
            {
                HeightMap ??= new HeightMapPreviewRender();
                HeightMap.Build(scene.HeightMap);
            }
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnDestroy()
        {
            MainRender?.Clear();
            HeightMap?.Clear();
        }
        private void OnSceneGUI(SceneView view)
        {
            if(ShowSDF)
                MainRender?.OnSceneGUI();

            if (ShowHeightMap)
                HeightMap?.OnSceneGUI();
        }
        public Vector2 listScrollPos;
        public Vector2 detailScrollPos;
        private void OnGUI()
        {
            using(new GUILayout.HorizontalScope())
            {
                using(new GUILayout.VerticalScope(GUILayout.Width(200)))
                {
                    GUILayout.Label("列表", EditorStyles.boldLabel);
                    using (var scroll = new GUILayout.ScrollViewScope(listScrollPos))
                    {
                        listScrollPos = scroll.scrollPosition;
                        foreach (var name in Files)
                        {
                            if (name == SelectFile)
                            {
                                GUILayout.Label(name, EditorStyles.boldLabel);
                            }
                            else if (GUILayout.Button(name))
                            {
                                SelectFile = name;
                                MainRender?.Clear();
                                MainRender = null;
                                HeightMap?.Clear();
                                HeightMap = null;
                            }
                            if (name == SelectFile && MainRender == null)
                            {
                                OnSelectChange();
                            }
                        }
                    }
                    if (GUILayout.Button("刷新文件列表"))
                    {
                        RefreshFileList();
                    }
                }
                EditorGUI.BeginChangeCheck();
                using (new GUILayout.VerticalScope())
                {
                    using var scroll = new GUILayout.ScrollViewScope(detailScrollPos);
                    detailScrollPos = scroll.scrollPosition;
                    ShowSDF = EditorGUILayout.ToggleLeft("显示SDF", ShowSDF);
                    MainRender?.OnGUI();
                    ShowHeightMap = EditorGUILayout.ToggleLeft("显示高度图模型", ShowHeightMap);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }
        }
    }
}