using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SDFNav.Editor
{
    [System.Serializable]
    public class HeightMapPreviewRender
    {
        enum DirMask : byte
        {
            Right = 1,
            Top = 2,
            TopRight = 4,
        }
        public Vector3[] Vertices;
        private byte GetRawUnSafe(HeightMapData data, int x, int y)
        {
            return data.Data[x + y * data.Width];
        }

        private Vector3 GetPointUnSafe(HeightMapData data, int x, int y)
        {
            float range = data.Max - data.Min;
            int idx = x + y * data.Width;
            float v = range * (data.Data[idx] / 255f) + data.Min;
            return new Vector3(x * data.Grain + data.Origin.x, v, y * data.Grain + data.Origin.y);
        }

        public void Build(HeightMapData data)
        {
            Clear();
            DirMask[] dirMasks = new DirMask[data.Data.Length];
            List<Vector3> vertices = new List<Vector3>();
            for (int x = 0; x < data.Width; ++x)
            {
                for (int y = 0; y < data.Height; ++y)
                {
                    int idx = x + y * data.Width;
                    byte raw = data.Data[idx];
                    DirMask mask = dirMasks[idx];
                    if (x > 0 && x < data.Width -1 && (mask & DirMask.Right) == 0)
                    {
                        byte right = GetRawUnSafe(data, x + 1, y);
                        if (right == raw)
                        {
                            mask |= DirMask.Right;
                        }
                    }
                    if (y> 0 && y < data.Height - 1 && (mask & DirMask.Top) == 0)
                    {
                        byte top = GetRawUnSafe(data, x, y + 1);
                        if (top == raw)
                        {
                            mask |= DirMask.Top;
                        }
                    }
                    if (x < data.Width - 1 && y < data.Height - 1 && (mask & DirMask.TopRight) == 0)
                    {
                        byte topRight = GetRawUnSafe(data, x + 1, y + 1);
                        if (topRight == raw)
                        {
                            mask |= DirMask.TopRight;
                        }
                    }
                    dirMasks[idx] = mask;
                }
            }
            DirMask ignoreMask = DirMask.Right | DirMask.Top | DirMask.TopRight;
            for (int x = 0; x < data.Width - 1; ++x)
            {
                for (int y = 0; y < data.Height - 1; ++y)
                {
                    int idx = x + y * data.Width;
                    DirMask mask = dirMasks[idx];
                    if (mask == ignoreMask)
                        continue;
                    Vector3 pt = GetPointUnSafe(data, x, y);
                    if ((mask & DirMask.Right) == 0)
                    {
                        for (int z = x + 1; z < data.Width; z++)
                        {
                            var targetMask = dirMasks[z + y * data.Width];
                            if (targetMask == ignoreMask)
                                break;
                            if ((targetMask & DirMask.Right) == 0)
                            {
                                vertices.Add(pt);
                                vertices.Add(GetPointUnSafe(data, z, y));
                                break;
                            }
                        }
                    }
                    if ((mask & DirMask.Top) == 0)
                    {
                        for (int z = y + 1; z < data.Height; z++)
                        {
                            var targetMask = dirMasks[x + z * data.Width];
                            if (targetMask == ignoreMask)
                                break;
                            if ((targetMask & DirMask.Top) == 0)
                            {
                                vertices.Add(pt);
                                vertices.Add(GetPointUnSafe(data, x, z));
                                break;
                            }
                        }
                    }
                    if ((mask & DirMask.TopRight) == 0)
                    {
                        vertices.Add(pt);
                        vertices.Add(GetPointUnSafe(data, x+1, y+1));
                    }
                    dirMasks[idx] = mask;
                }
            }
            Vertices = vertices.ToArray();
        }
        public void OnSceneGUI()
        {
            if (Event.current.type != EventType.Repaint || Vertices == null)
                return;
            for (int i=0; i< Vertices.Length; i+=2)
            {
                Handles.DrawLine(Vertices[i], Vertices[i + 1]);
            }
        }

        public void Clear()
        {
            Vertices = null;
        }
    }
}