namespace SDFNav
{
    public enum NodeStatus
    {
        Untest,
        Open,
        Close,
    }
    public enum Direction
    {
        Top,
        Left,
        Bottom,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }
    public enum DirectionMaskType
    {
        None = 0,
        Top = 1,
        Left = 2,
        Bottom = 4,
        Right = 8,
        TopLeft = 16,
        TopRight = 32,
        BottomLeft = 64,
        BottomRight = 128,
        All = 255,
    }

    public struct GridPos
    {
        public int x;
        public int y;
    }
    public class PathNode : System.IComparable<PathNode>
    {
        public const int Line = 10;
        public const int Tilted = 14;

        public int g; // 起点到节点代价
        public int h; // 节点到终点代价 估值
        public int f;
        public GridLocation location;
        public PathNode parent;
        public DirectionMaskType dir; // 用于跳点搜索 跳点速度方向(父给的方向 + 自身带的方向)

        public void Clear()
        {
            parent = null;
            g = 0;
            h = 0;
            f = 0;
            dir = DirectionMaskType.None;
        }

        public int CompareTo(PathNode refrence)
        {
            return f.CompareTo(refrence.f);
        }
        public static int ComputeH(GridLocation ori, GridLocation dest)
        {
            int xDelta = dest.X > ori.X ? dest.X - ori.X : ori.X - dest.X;
            int yDelta = dest.Y > ori.Y ? dest.Y - ori.Y : ori.Y - dest.Y;
            return (xDelta + yDelta) * 10;
        }
        public static int ComputeGForJPS(Direction direction, GridLocation ori, GridLocation dest)
        {
            int xDelta, yDelta;
            switch (direction)
            {
                case Direction.Bottom:
                case Direction.Top:
                    yDelta = dest.Y > ori.Y ? dest.Y - ori.Y : ori.Y - dest.Y;
                    return yDelta * Line;
                case Direction.Left:
                case Direction.Right:
                    xDelta = dest.X > ori.X ? dest.X - ori.X : ori.X - dest.X;
                    return xDelta * Line;
                default:
                    xDelta = dest.X > ori.X ? dest.X - ori.X : ori.X - dest.X;
                    return xDelta * Tilted;
            }
        }
    }
}
