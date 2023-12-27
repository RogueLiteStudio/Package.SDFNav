using System.Collections.Generic;
using UnityEngine;

namespace SDFNav.Editor
{
    public struct TriangleIndice
    {
        public int A;
        public int B;
        public int C;

        public bool IsVertice(int index)
        {
            return A == index || B == index || C == index;
        }

        public int GetSameSide(int a, int b)
        {
            if ((a == A && b == B) /*|| (b == A && a == B)*/)
                return 0;
            if ((a == B && b == C)/* || (b == B && a == C)*/)
                return 1;
            if ((a == C && b == A)/* || (b == C && a == A)*/)
                return 3;

            return -1;
        }
    }
    public class MeshData
    {
        public Vector3[] Vertices;
        public TriangleIndice[] Triangles;
    }

    public class SubMeshData
    {
        public MeshData Mesh;
        public List<int> TriangleIndices = new List<int>();
    }
    [System.Serializable]
    public struct SegmentIndice
    {
        public int From;
        public int To;

        public bool IsEquals(SegmentIndice other)
        {
            return From == other.From && To == other.To;
        }
    }
    [System.Serializable]
    public class EdgeData
    {
        public Vector2[] Vertices;
        public List<SegmentIndice> Segments = new List<SegmentIndice>();
    }
}
