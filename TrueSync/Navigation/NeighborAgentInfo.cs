using TrueSync;
namespace TSDFNav
{
    public struct NeighborAgentInfo
    {
        public int ID;
        public string DebugName;
        public TVector2 Direction;//相对自己的方向
        public TFloat Radius;//碰撞半径
        public TFloat Distance;//两个圆心的距离
        public TVector2 MoveDirection;//移动方向
        public TFloat MoveDistance;//一帧移动的距离
    }
}