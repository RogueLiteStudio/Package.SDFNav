using UnityEngine;

namespace SDFNav
{
    public class HeightField
    {
        public enum GridType : byte
        {
            None,
            Wallable,
            Wall,
            Hole,
        }

        public Vector3 OriginPoint;
        public float VoxelSize;
        public Vector2Int Size;
        public ushort[] Data;

        public GridType GetType(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Size.x || y >= Size.y)
                return GridType.None;
            ushort val = Data[y * Size.x + x];
            return (GridType)(val >> 14);
        }

        public bool IsWalkable(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Size.x || y >= Size.y)
                return false;
            ushort val = Data[y * Size.x + x];
            return val >> 14 == (ushort)GridType.Wallable;
        }
    }
}
