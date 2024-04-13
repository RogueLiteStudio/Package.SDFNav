namespace TSDFNav
{
    public class JPSPathFinder : PathFinder
    {
        enum JumpType { Line, Tilted }

        public JPSPathFinder(ISDF sdf) : base(sdf) { }
        protected override bool Search(PathNode node)
        {
            node.dir = DirectionMaskType.All;
            while (node != null)
            {
                SetClose(node.location);
                if (node.location.Index == endLocation.Index)
                    return true;
                for (Direction i = Direction.Top; i <= Direction.BottomRight; ++i)
                {
                    DirectionMaskType mask = (DirectionMaskType)(1 << (int)i);
                    if ((node.dir & mask) != DirectionMaskType.None)
                    {
                        SearchDir(node, i);
                    }
                }
                node = openHeap.Count > 0 ? openHeap.Dequeue() : null;
            }
            return false;
        }

        private void SearchDir(PathNode fromNode, Direction dir)
        {
            var node = fromNode;
            while (node != null)
            {
                GridLocation location = GetNeighbor(node.location, dir);
                if (!IsWalkable(location) || IsClose(location))
                    break;
                node = GetNode(location);
                node = GetNext(node, fromNode, dir, dir <= Direction.Right ? JumpType.Line : JumpType.Tilted);
            }
        }

        private PathNode GetNext(PathNode toCheck, PathNode fromNode, Direction dir, JumpType jumpType)
        {
            DirectionMaskType jumpNodeDir;
            var tempValue = jumpType == JumpType.Line ? IsLineJumpNode(toCheck.location, dir, out jumpNodeDir) : IsTitleJumpNode(toCheck.location, dir, out jumpNodeDir);
            if (tempValue)  // toCheck是跳点
            {
                if (IsOpen(toCheck.location))
                {
                    var cost = PathNode.ComputeGForJPS(dir, fromNode.location, toCheck.location);
                    var gTemp = fromNode.g + cost;
                    if (gTemp < toCheck.g)
                    {
                        var oldDir = GetDirection(toCheck.parent.location, toCheck.location);
                        toCheck.dir = (toCheck.dir ^ oldDir) | jumpNodeDir; // 去掉旧的父亲方向 保留自身方向 添加新的方向 
                        toCheck.parent = fromNode;
                        toCheck.g = gTemp;
                        toCheck.f = gTemp + toCheck.h;
                    }
                    openHeap.TryUpAdjust(toCheck);
                    return null;
                }
                //加入openlist
                toCheck.parent = fromNode;
                toCheck.g = fromNode.g + PathNode.ComputeGForJPS(dir, toCheck.location, fromNode.location);
                toCheck.h = PathNode.ComputeH(toCheck.location, endLocation);
                toCheck.f = toCheck.g + toCheck.h;
                SetOpen(toCheck.location);
                toCheck.dir = jumpNodeDir;
                openHeap.Enqueue(toCheck);
                return null;
            }
            return toCheck;
        }
        private bool IsLineJumpNode(GridLocation toCheck, Direction dir, out DirectionMaskType jumpNodeDir)
        {
            jumpNodeDir = (DirectionMaskType) (1 << (int)dir);
            if (!IsWalkable(toCheck))
                return false;
            if (dir == Direction.Right)
                return IsRightJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == Direction.Left)
                return IsLeftJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == Direction.Top)
                return IsTopJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == Direction.Bottom)
                return IsBottomJumpNode(toCheck, ref jumpNodeDir);
            return false;
        }

        private bool IsRightJumpNode(GridLocation toCheck, ref DirectionMaskType jumpNodeDir)
        {
            if (toCheck.Index == endLocation.Index)
                return true;
            var result = false;
            var right = GetNeighbor(toCheck, Direction.Right);
            if (IsWalkable(right))
            {
                var up = GetNeighbor(toCheck, Direction.Top);
                var down = GetNeighbor(toCheck, Direction.Bottom);
                var topRight = GetNeighbor(toCheck, Direction.TopRight);
                var bottomRight = GetNeighbor(toCheck, Direction.BottomRight);
                if (!IsWalkable(up) && IsWalkable(topRight))
                {
                    jumpNodeDir |= DirectionMaskType.TopRight;
                    result = true;
                }
                if (!IsWalkable(down) && IsWalkable(bottomRight))
                {
                    jumpNodeDir |= DirectionMaskType.BottomRight;
                    result = true;
                }
            }
            return result;
        }

        private bool IsLeftJumpNode(GridLocation toCheck, ref DirectionMaskType jumpNodeDir)
        {
            if (toCheck.Index == endLocation.Index)
                return true;
            var result = false;
            var left = GetNeighbor(toCheck, Direction.Left);
            if (IsWalkable(left))
            {
                var top = GetNeighbor(toCheck, Direction.Top);
                var bottom = GetNeighbor(toCheck, Direction.Bottom);
                var topLeft = GetNeighbor(toCheck, Direction.TopLeft);
                var bottomLeft = GetNeighbor(toCheck, Direction.BottomLeft);
                if (!IsWalkable(top) && IsWalkable(topLeft))
                {
                    jumpNodeDir |= DirectionMaskType.TopLeft;
                    result = true;
                }
                if (!IsWalkable(bottom) && IsWalkable(bottomLeft))
                {
                    jumpNodeDir |= DirectionMaskType.BottomLeft;
                    result = true;
                }
            }
            return result;
        }
        private bool IsTopJumpNode(GridLocation toCheck, ref DirectionMaskType jumpNodeDir)
        {
            if (toCheck.Index == endLocation.Index)
                return true;
            var result = false;
            var top = GetNeighbor(toCheck, Direction.Top);
            if (IsWalkable(top))
            {
                var left = GetNeighbor(toCheck, Direction.Left);
                var right = GetNeighbor(toCheck, Direction.Right);
                var topLeft = GetNeighbor(toCheck, Direction.TopLeft);
                var topRight = GetNeighbor(toCheck, Direction.TopRight);

                if (!IsWalkable(left) && IsWalkable(topLeft))
                {
                    jumpNodeDir |= DirectionMaskType.TopLeft;
                    result = true;
                }
                if (!IsWalkable(right) && IsWalkable(topRight))
                {
                    jumpNodeDir |= DirectionMaskType.TopRight;
                    result = true;
                }
            }
            return result;
        }
        private bool IsBottomJumpNode(GridLocation toCheck, ref DirectionMaskType jumpNodeDir)
        {
            if (toCheck.Index == endLocation.Index)
                return true;
            var result = false;
            var top = GetNeighbor(toCheck, Direction.Top);
            if (IsWalkable(top))
            {
                var left = GetNeighbor(toCheck, Direction.Left);
                var right = GetNeighbor(toCheck, Direction.Right);
                var bottomLeft = GetNeighbor(toCheck, Direction.BottomLeft);
                var bottomRight = GetNeighbor(toCheck, Direction.BottomRight);

                if (!IsWalkable(left) && IsWalkable(bottomLeft))
                {
                    jumpNodeDir |= DirectionMaskType.BottomLeft;
                    result = true;
                }
                if (!IsWalkable(right) && IsWalkable(bottomRight))
                {
                    jumpNodeDir |= DirectionMaskType.BottomRight;
                    result = true;
                }
            }
            return result;
        }
        private bool IsTitleJumpNode(GridLocation toCheck, Direction dir, out DirectionMaskType jumpNodeDir)  //是否是斜方向的跳点
        {
            jumpNodeDir = (DirectionMaskType) (1 << (int)dir);
            if (toCheck.Index == endLocation.Index || !IsWalkable(toCheck))
                return true;
            if (dir == Direction.TopRight)
                return IsTopRightJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == Direction.TopLeft)
                return IsTopLeftJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == Direction.BottomRight)
                return IsBottomRightJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == Direction.BottomLeft)
                return IsBottomLeftJumpNode(toCheck, ref jumpNodeDir);
            return false;
        }
        private bool IsTopRightJumpNode(GridLocation toCheck, ref DirectionMaskType jumpNodeDir)
        {
            var result = false;
            result |= IsTopJumpNode(toCheck, ref jumpNodeDir);
            result |= IsRightJumpNode(toCheck, ref jumpNodeDir);  // 先检查自身是否符合line跳点, 是的话追加方向到jumpNodeDir
            if (!result) // 自身不符合line跳点 检查line方向有无跳点
            {
                var temp = DirectionMaskType.None;
                var node = GetNeighbor(toCheck, Direction.Top);
                while (IsWalkable(node))
                {
                    if (IsTopJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = GetNeighbor(node, Direction.Top);
                }
                node = GetNeighbor(toCheck, Direction.Right);
                while (IsWalkable(node))
                {
                    if (IsRightJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = GetNeighbor(node, Direction.Right);
                }
            }
            if (result)
            {
                jumpNodeDir |= DirectionMaskType.Top;
                jumpNodeDir |= DirectionMaskType.Right;
            }
            return result;
        }

        private bool IsTopLeftJumpNode(GridLocation toCheck, ref DirectionMaskType jumpNodeDir)
        {
            var result = false;
            result |= IsTopJumpNode(toCheck, ref jumpNodeDir);
            result |= IsLeftJumpNode(toCheck, ref jumpNodeDir);
            if (!result)
            {
                var temp = DirectionMaskType.None;
                var node = GetNeighbor(toCheck, Direction.Top);
                while (IsWalkable(node))
                {
                    if (IsTopJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = GetNeighbor(node, Direction.Top);
                }
                node = GetNeighbor(toCheck, Direction.Left);
                while (IsWalkable(node))
                {
                    if (IsLeftJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = GetNeighbor(node, Direction.Left);
                }
            }
            if (result)
            {
                jumpNodeDir |= DirectionMaskType.Top;
                jumpNodeDir |= DirectionMaskType.Left;
            }
            return result;
        }

        private bool IsBottomRightJumpNode(GridLocation toCheck, ref DirectionMaskType jumpNodeDir)
        {
            var result = false;
            result |= IsBottomJumpNode(toCheck, ref jumpNodeDir);
            result |= IsRightJumpNode(toCheck, ref jumpNodeDir);
            if (!result)
            {
                var temp = DirectionMaskType.None;
                var node = GetNeighbor(toCheck, Direction.Bottom);
                while (IsWalkable(node))
                {
                    if (IsBottomJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = GetNeighbor(node, Direction.Bottom);
                }
                node = GetNeighbor(toCheck, Direction.Right);
                while (IsWalkable(node))
                {
                    if (IsRightJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = GetNeighbor(node, Direction.Right);
                }
            }
            if (result)
            {
                jumpNodeDir |= DirectionMaskType.Bottom;
                jumpNodeDir |= DirectionMaskType.Right;
            }
            return result;
        }

        bool IsBottomLeftJumpNode(GridLocation toCheck, ref DirectionMaskType jumpNodeDir)
        {
            var result = false;
            result |= IsBottomJumpNode(toCheck, ref jumpNodeDir);
            result |= IsLeftJumpNode(toCheck, ref jumpNodeDir);
            if (!result)
            {
                var temp = DirectionMaskType.None;
                var node = GetNeighbor(toCheck, Direction.Bottom);
                while (IsWalkable(node))
                {
                    if (IsBottomJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = GetNeighbor(node, Direction.Bottom);
                }
                node = GetNeighbor(toCheck, Direction.Left);
                while (IsWalkable(node))
                {
                    if (IsLeftJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = GetNeighbor(node, Direction.Left);
                }
            }
            if (result)
            {
                jumpNodeDir |= DirectionMaskType.Bottom;
                jumpNodeDir |= DirectionMaskType.Left;
            }
            return result;
        }

        public static DirectionMaskType GetDirection(GridLocation ori, GridLocation dest)
        {
            int xDelta = dest.X - ori.X, yDelta = dest.Y - ori.Y;
            if (xDelta > 0 && yDelta > 0)
                return DirectionMaskType.TopRight;
            if (xDelta > 0 && yDelta == 0)
                return DirectionMaskType.Right;
            if (xDelta > 0 && yDelta < 0)
                return DirectionMaskType.BottomRight;
            if (xDelta < 0 && yDelta > 0)
                return DirectionMaskType.TopLeft;
            if (xDelta < 0 && yDelta == 0)
                return DirectionMaskType.Left;
            if (xDelta < 0 && yDelta < 0)
                return DirectionMaskType.BottomLeft;
            if (xDelta == 0 && yDelta > 0)
                return DirectionMaskType.Top;
            if (xDelta == 0 && yDelta < 0)
                return DirectionMaskType.Bottom;
            return DirectionMaskType.None;
        }
    }
}
