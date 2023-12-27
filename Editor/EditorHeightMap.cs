using System.Collections.Generic;
using UnityEngine;
namespace SDFNav.Editor
{
    public class EditorHeightMap
    {
        public int Width;
        public int Height;
        public float Grain;
        public Vector2 Origin;
        public float[] Data;

        public float MinHeight = float.MinValue;
        public float MaxHeight = float.MaxValue;

        public override string ToString()
        {
            return $"W:{Width},H:{Height},P:{Origin},Range;{MinHeight} -> {MaxHeight}";
        }

        public HeightMapData ToRuntimeData()
        {
            HeightMapData data = new HeightMapData();
            data.Width = Width;
            data.Height = Height;
            data.Grain = Grain;
            data.Origin = new Vector2(Origin.x, Origin.y);
            data.Min = MaxHeight;
            data.Max = MinHeight;
            for (int i = 0; i < Data.Length; i++)
            {
                data.Min = Mathf.Min(data.Min, Data[i]);
                data.Max = Mathf.Max(data.Max, Data[i]);
            }
            float range = data.Max - data.Min;
            if (range > 0.01)
            {
                data.Data = new byte[Data.Length];
                for (int i = 0; i < Data.Length; i++)
                {
                    data.Data[i] = (byte)((Data[i] - data.Min) / range * 255);
                }
            }
            return data;
        }

        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[Data.Length];
            for (int x=0; x<Width; ++x)
            {
                for (int y=0; y<Height; ++y)
                {
                    int idx = x + y * Width;
                    vertices[idx] = new Vector3(x * Grain + Origin.x, Data[idx], y * Grain + Origin.y);
                }
            }
            List<int> triangles = new List<int>(/*(Width - 1) * (Height - 1) * 6*/);
            for (int x = 0; x < Width-1; ++x)
            {
                for (int y = 0; y < Height-1; ++y)
                {
                    triangles.Add(x + y * Width);
                    triangles.Add(x + 1 + (y + 1) * Width);
                    triangles.Add(x + 1 + y * Width);
                    
                    triangles.Add(x + y * Width);
                    triangles.Add(x + (y + 1) * Width);
                    triangles.Add(x + 1 + (y + 1) * Width);
                }
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }

        public static EditorHeightMap Create(Vector2 origin, Vector2 size, float grain, float minValue)
        {
            EditorHeightMap heightMap = new EditorHeightMap();
            heightMap.Origin = origin;
            heightMap.Width = Mathf.CeilToInt(size.x / grain);
            heightMap.Height = Mathf.CeilToInt(size.y / grain);
            heightMap.Grain = grain;
            heightMap.Data = new float[heightMap.Width * heightMap.Height];
            for (int i = 0; i < heightMap.Data.Length; i++)
            {
                heightMap.Data[i] = minValue;
            }
            return heightMap;
        }
    }
}