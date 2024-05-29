using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDFNav.Editor
{
    [System.Serializable]
    public class SDFPreviewRender
    {
        public Texture SDFTexture;
        public Mesh PlaneMesh;
        public Material Mat;
        public bool ShowEnableAlpha = true;
        public string ShowInfo;
        public float Height = 0.1f;

        public void Build(TSDFNav.SDFData data)
        {
            Clear();
            ShowInfo = $"宽：{data.Width} \n长：{data.Height} \n采样精度：{data.Grain} \n 缩放精度：{data.Scale}";
            SDFTexture = SDFExportUtil.ToTextureWithDistance(data);
            SDFTexture.hideFlags = HideFlags.HideAndDontSave;
            Vector3 origin = new Vector3((float)data.Origin.x, 0, (float)data.Origin.y);
            float width = data.Width * (float)data.Grain;
            float height = data.Height * (float)data.Grain;
            PlaneMesh = new Mesh
            {
                hideFlags = HideFlags.HideAndDontSave,
                vertices = new Vector3[]
                {
                    origin,
                    origin + new Vector3(0, 0, height),
                    origin + new Vector3(width, 0, height),
                    origin + new Vector3(width, 0, 0),
                },
                uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                },
                triangles = new int[]
                {
                    0, 1, 2,
                    2, 3, 0,
                }
            };
            PlaneMesh.RecalculateNormals();
            var shader = Shader.Find("SDFPreview");
            if (shader)
            {
                Mat = new Material(shader);
                Mat.hideFlags = HideFlags.HideAndDontSave;
                Mat.SetTexture("_MainTex", SDFTexture);
                Mat.doubleSidedGI = true;
            }
        }

        public void Build(SDFData data)
        {
            Clear();
            ShowInfo = $"宽：{data.Width} \n长：{data.Height} \n采样精度：{data.Grain} \n 缩放精度：{data.Scale}";
            SDFTexture = SDFExportUtil.ToTextureWithDistance(data);
            SDFTexture.hideFlags = HideFlags.HideAndDontSave;
            Vector3 origin = new Vector3(data.Origin.x, 0, data.Origin.y);
            float width = data.Width * data.Grain;
            float height = data.Height * data.Grain;
            PlaneMesh = new Mesh
            {
                hideFlags = HideFlags.HideAndDontSave,
                vertices = new Vector3[]
                {
                    origin,
                    origin + new Vector3(0, 0, height),
                    origin + new Vector3(width, 0, height),
                    origin + new Vector3(width, 0, 0),
                },
                uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                },
                triangles = new int[]
                {
                    0, 1, 2,
                    2, 3, 0,
                }
            };
            PlaneMesh.RecalculateNormals();
            var shader = Shader.Find("SDFPreview");
            if (shader)
            {
                Mat = new Material(shader);
                Mat.hideFlags = HideFlags.HideAndDontSave;
                Mat.SetTexture("_MainTex", SDFTexture);
                Mat.doubleSidedGI = true;
            }
        }

        public void OnGUI()
        {
            if (SDFTexture)
            {
                GUILayout.Label(ShowInfo);
                Height = EditorGUILayout.FloatField("预览偏移高度", Height);
                EditorGUI.BeginChangeCheck();
                ShowEnableAlpha = GUILayout.Toggle(ShowEnableAlpha, "按照距离显示");
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
                GUILayout.Label("SDF图：", EditorStyles.boldLabel);
                GUILayout.Label(SDFTexture, GUILayout.MinHeight(SDFTexture.width), GUILayout.MinWidth(SDFTexture.height));
            }
        }

        public void OnDrawGizmo(Matrix4x4 matrix)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            if (Mat && PlaneMesh)
            {
                Mat.SetFloat("_OffsetA", ShowEnableAlpha ? 0 : 1);
                CommandBuffer commandBuffer = new CommandBuffer();
                commandBuffer.DrawMesh(PlaneMesh, matrix * Matrix4x4.Translate(new Vector3(0, Height, 0)), Mat);
                Graphics.ExecuteCommandBuffer(commandBuffer);
            }
        }

        public void Clear()
        {
            if (SDFTexture)
                Object.DestroyImmediate(SDFTexture);
            if (PlaneMesh)
                Object.DestroyImmediate(PlaneMesh);
            if (Mat)
                Object.DestroyImmediate(Mat);
        }
    }
}