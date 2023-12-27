using UnityEngine;
namespace SDFNav
{
    public struct NeighborAgentInfo
    {
        public int ID;
        public string DebugName;
        public Vector2 Direction;//相对自己的方向
        public float Radius;//碰撞半径
        public float Distance;//两个圆心的距离
        public Vector2 MoveDirection;//移动方向
        public float MoveDistance;//一帧移动的距离
    }
}