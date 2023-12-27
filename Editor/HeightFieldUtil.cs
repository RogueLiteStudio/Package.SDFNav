using UnityEngine;

namespace SDFNav
{
    public static class HeightFieldUtil
    {
        public static HeightField VoxelToHeightField(VoxelBox voxel, Vector3 validPoint, float angentHeight, float stepHeight, float slope)
        {
            HeightField heightField = new HeightField 
            {
                OriginPoint = voxel.OriginPoint,
                VoxelSize = voxel.VoxelSize,
                Size = new Vector2Int(voxel.Size.x, voxel.Size.z),
                Data = new ushort[voxel.Size.x * voxel.Size.z],
            };


            return heightField;
        }
    }
}
