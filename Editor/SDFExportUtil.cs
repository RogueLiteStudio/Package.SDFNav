using UnityEngine;
namespace SDFNav.Editor
{
    public struct EdgeSDFResult
    {
        public float Distance;
        public SegmentIndice Segment;
        public Vector2 Point;
    }
    public static class SDFExportUtil
    {
        public static Vector2 To2d(Vector2 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static TSDFNav.SDFData EdgeToTSDF(EdgeData edgeData, SubMeshData subMesh, float adjust, float grain = 0.5f)
        {
            var data = EdgeToSDF(edgeData, subMesh, adjust, grain);
            if (data == null)
                return null;
            var tsdfData = new TSDFNav.SDFData();
            tsdfData.Init(data.Width, data.Height, data.Grain, data.Scale, data.Origin.ToTVec2(), data.Data);
            return tsdfData;
        }

        public static SDFData EdgeToSDF(EdgeData edgeData, SubMeshData subMesh, float adjust, float grain = 0.5f)
        {
            var rect = EdgeEditorUtil.CalcBounds(edgeData, 1);
            Vector2 size = rect.size / grain;//对得到的地图外围进行《栅格化》
            if (size.x < 1 || size.y < 1)
                return null;
            Vector2 min = To2d(rect.min);
            int width = Mathf.CeilToInt(size.x);
            int height = Mathf.CeilToInt(size.y);
            float[] originalData = new float[width * height];
            float sdAbsMax = float.MinValue;
            for (int i = 0; i < width; ++i)
            {
                UnityEditor.EditorUtility.DisplayProgressBar("生成", $"生成SDF 行 {i + 1}", i / (float)width);
                for (int j = 0; j < height; ++j)
                {
                    Vector2 pos = min + new Vector2(i * grain, j * grain);
                    var result = EdgeEditorUtil.SDF(pos, edgeData);
                    float val = result.Distance + adjust;
                    if (Mathf.Abs(val) < 1E-05f)
                    {
                        val = 0;
                    }
                    else if(!SDFNavEditorUtil.IsPointInSubMesh(pos, subMesh))
                    {
                        val = -val;
                    }
                    originalData[i + width * j] = val;
                    sdAbsMax = Mathf.Max(Mathf.Abs(val), sdAbsMax);
                }
            }
            float scale = sdAbsMax / short.MaxValue;
            short[] data = new short[originalData.Length];
            for (int i = 0; i < originalData.Length; ++i)
            {
                data[i] = (short)(originalData[i] / scale);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
            SDFData sdfData = new SDFData();
            sdfData.Init(width, height, grain, scale, min, data);
            return sdfData;
        }

        public static Texture2D ToTextureWithDistance(SDFData sdf)
        {
            Texture2D texture = new Texture2D(sdf.Width, sdf.Height);
            for (int i = 0; i < sdf.Width; ++i)
            {
                for (int j = 0; j < sdf.Height; ++j)
                {
                    short val = sdf[i, j];
                    float pencent = ((float)val)/ short.MaxValue;
                    if (val <= 0)
                    {
                        texture.SetPixel(i, j, new Color(1, 0, 0, -pencent));
                    }
                    else
                    {
                        texture.SetPixel(i, j, new Color(0, 1, 0, pencent));
                    }
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
