using System.Collections;
using UnityEngine;

namespace SDFNav
{
    //暂时不用，使用Navmesh生成
    public class VoxelBox
    {
        public Vector3 OriginPoint;
        public float VoxelSize;
        public Vector3Int Size;
        public BitArray[] Voxels;
        public Bounds[] ValidBounds;//有效区域，帮助裁剪在BoundBox里面的三角形

        public VoxelBox(Bounds bounds, float voxelSize, Bounds[] validBounds = null)
        {
            if (validBounds != null && validBounds.Length > 0)
                ValidBounds = validBounds;
            OriginPoint = bounds.min;
            VoxelSize = voxelSize;
            Size.x = Mathf.CeilToInt(bounds.size.x / voxelSize);
            Size.y = Mathf.CeilToInt(bounds.size.y / voxelSize);
            Size.z = Mathf.CeilToInt(bounds.size.z / voxelSize);
            Voxels = new BitArray[Size.x * Size.y];
        }
        public BitArray Get(int x, int y)
        {
            if (x < 0 || x >= Size.x || y < 0 || y >= Size.y)
                return null;
            int idx = y * Size.x + x;
            var voxel = Voxels[idx];
            if (voxel == null)
            {
                voxel = new BitArray(Size.z);
                Voxels[idx] = voxel;
            }
            return voxel;
        }
    }
}
