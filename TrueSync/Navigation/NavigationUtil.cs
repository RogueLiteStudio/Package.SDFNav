using TrueSync;

/*移动处理
 * ->摇杆控制的直线移动
 *   ->AdjustMoveByNeighbor 选取周围的其它角色，如果相撞，调整移动方向让其从旁边绕开
 *   ->AdjustMoveByObstacle 如果目标点在障碍物内部，则让其沿着障碍物边缘移动
 * ->寻路方式的移动：先计算到目标点的路径Path
 *   ->OptimizePath 如果路径点个数大于1，则取倒数第二个路径点，计算当前位置到改点是否被障碍物阻挡，如果没有，则移除最后一个路径点
 *   ->AdjustMoveByPathMove 修正当前移动方向
 * ->最终处理：和障碍物以及其它相邻角色进行碰撞处理，得出最终的移动位置
 */

namespace TSDFNav
{
    public static class NavigationUtil
    {
        public static readonly TFloat Epsilon = TFloat.EN5;

        public static TVector2 FindNearestValidPoint(this SDFNavContext nav, TVector2 point, TFloat radius)
        {
            return nav.SDFMap.FindNearestValidPoint(point, radius);
        }

        public static void OptimizePath(this SDFNavContext nav, in MoveAgentInfo agent, NavPathMoveInfo path, TFloat spaceToNeighbor)
        {
            if (path.HasFinished)
                return;
            TFloat distance = TVector2.Distance(path.Path[path.Path.Count - 1], agent.Position);
            if (distance < agent.MoveDistance)//如果到下一个目标点的距离小于移动距离就停止移动了，因为再移动会穿过去
            {
                path.RemoveLastPoint();
            }
            if (path.Path.Count > 1)
            {
                //寻路路点本来就是不平滑的，有很多多余的点，每次移动时从开始进行检查，这样会逐步平滑移动路径
                //移动过程中目标点也可能改变目标点导致重新寻路，这样可以省掉不必要的处理
                //同时也可以处理因为碰撞导致位置偏移路径，不好判断是否移动到下个路径点的问题
                if (nav.SDFMap.CheckStraightMove(agent.Position, path.Path[path.Path.Count - 2], agent.Radius))
                {
                    path.RemoveLastPoint();
                }
            }
            
        }

        public static void NavDestinationCheck(this SDFNavContext nav, in MoveAgentInfo agent, NavPathMoveInfo path, TFloat spaceToNeighbor)
        {
            //仅剩下最后一个路点时
            if (path.Path.Count == 1)
            {
                TVector2 nextPoint = path.Path[0];
                TFloat distance = TVector2.Distance(nextPoint, agent.Position);
                if (distance < agent.MoveDistance)
                {
                    path.RemoveLastPoint();
                    return;
                }
                int idx = OverlapPointNeighbor(nav, agent, nextPoint);
                if (idx >= 0)
                {
                    var neighbor = nav.Neighbors[idx];
                    TFloat space = neighbor.Distance - neighbor.Radius - agent.Radius;
                    if (space > spaceToNeighbor)
                        return;//距离过远，还可以继续移动
                    path.RemoveLastPoint();
                }
            }
        }

        public static int OverlapPointNeighbor(SDFNavContext nav, in MoveAgentInfo agent, TVector2 point)
        {
            for(int i=0; i< nav.Neighbors.Count; ++i)
            {
                var neighbor = nav.Neighbors[i];
                TVector2 targetPos = agent.Position + neighbor.Direction * neighbor.Distance;
                TFloat sqrMagnitude = (targetPos - point).sqrMagnitude;
                if (sqrMagnitude < Sqr(neighbor.Radius))
                {
                    return i;
                }
            }
            return -1;
        }

        //调整寻路移动过程中的移动方向
        public static bool AdjustMoveByPathMove(this SDFNavContext nav, ref MoveAgentInfo agent, NavPathMoveInfo path, TFloat spaceToNeighbor)
        {
            if (path.Path.Count == 0)
            {
                path.Clear();
                return false;
            }
            TVector2 targetPoint = path.Path[path.Path.Count - 1];
            TVector2 direction = targetPoint - agent.Position;
            TFloat distance = direction.magnitude;
            if (distance <= Epsilon)
            {
                //路径点已经处理过，理论上不会出现这种情况
                path.RemoveLastPoint();
                path.LastMoveDirection = TVector2.zero;
                path.LastAdjustAngle = TFloat.Zero;
                return AdjustMoveByPathMove(nav, ref agent, path, spaceToNeighbor);
            }
            direction /= distance;
            agent.Direction = direction;
            //if (path.Path.Count == 1)
            //{
            //    //前往最后一个目标点的时候，如果可以直接移动过去，或者阻挡自己的角色已经占了目标位置，则直接移动过去，不做调整
            //    int idx = MoveTest(nav, direction, agent.Radius, distance);
            //    do
            //    {
            //        if (idx >= 0)
            //        {
            //            var blockNeighbor = nav.Neighbors[idx];
            //            TVector2 targetPos = agent.Position + blockNeighbor.Direction * blockNeighbor.Distance;
            //            TFloat sqrMagnitude = (targetPos - targetPoint).sqrMagnitude;
            //            if (sqrMagnitude > Sqr(blockNeighbor.Radius))
            //                break;
            //        }
            //        path.LastMoveDirection = direction;
            //        path.LastAdjustAngle = 0;
            //        return false;
            //    } while (false);
            //}
            nav.MoveBlock.Clear();
            TVector2 lastDir = path.LastAdjustAngle != TFloat.Zero ? path.LastMoveDirection : direction;

            /* TODO:
             * 1、计算移动方向的时候加入目标移动方向处理，如果目标在移动，并且不在自己的移动路径上
             * 2、如果目标点有别的角色，并且自己朝着目标点移动不会撞到其它的角色，则只加入障碍物处理
             */
            BuildMoveDirectionRange(nav, agent, lastDir, TFloat.Zero);
            TFloat leftAngle = nav.MoveBlock.GetLeftMinAngle();
            TFloat rightAngle = nav.MoveBlock.GetRightMinAngle();
            bool useRight = rightAngle < 179;
            TFloat adjustAngle = rightAngle;
            if (useRight && leftAngle < 179)
            {
                if (TMath.Abs(path.LastAdjustAngle + leftAngle) < TMath.Abs(path.LastAdjustAngle - rightAngle))
                {
                    useRight = false;
                }
            }
            if (!useRight)
                adjustAngle = -leftAngle;
            TFloat absAngle = TMath.Abs(adjustAngle);
            if (absAngle <= Epsilon || absAngle > 179)
                return false;
            path.LastAdjustAngle = adjustAngle;
            agent.Direction = Rotate(agent.Direction, adjustAngle);
            path.LastMoveDirection = agent.Direction;
            return true;
        }

        private static void BuildMoveDirectionRange(this SDFNavContext nav, in MoveAgentInfo agent, TVector2 lastDir, TFloat spaceToNeighbor)
        {
            //TVector2 obPoint = agent.Position - gradiend * sd;
            foreach (var neighbor in nav.Neighbors)
            {
                if (neighbor.Distance <= Epsilon)
                    continue;//中心重叠
                if (TMath.Abs(agent.Radius - neighbor.Radius) > neighbor.Distance)
                    continue;//完全在另外一个的内部

                bool isFront = TVector2.Dot(agent.Direction, neighbor.Direction) > Epsilon;
                if (!isFront)
                {
                    isFront = TVector2.Dot(lastDir, neighbor.Direction) > Epsilon;
                }
                //将自己的半径加到目标的半径上，这样方便计算
                TFloat radius = neighbor.Radius + agent.Radius;
                TFloat moveDistance = neighbor.MoveDistance + agent.MoveDistance;
                if (!isFront && spaceToNeighbor + moveDistance + radius < neighbor.Distance)
                    continue;//不会发生碰撞
                //如果和自己不在障碍物的同一侧则忽略
                //TVector2 pos = agent.Position + neighbor.Direction * neighbor.Distance;
                //TVector2 normal = (pos - obPoint).normalized;
                //if (TVector2.Dot(gradiend, normal) < 0)
                //    continue;
                TFloat targetAngle = Angle360(agent.Direction, neighbor.Direction);
                if (neighbor.Distance <= spaceToNeighbor + moveDistance + radius)
                {
                    nav.MoveBlock.AddAngle(targetAngle, 95);
                }
                else
                {
                    //如果角色时超远离自己的方向移动，则忽略该角色
                    bool isMoving = neighbor.MoveDistance > Epsilon && neighbor.MoveDirection.sqrMagnitude > Epsilon;
                    if (isMoving && TVector2.Dot(neighbor.Direction, neighbor.MoveDirection) > Epsilon)
                    {
                        continue;
                    }
                    //计算当前可能发生碰撞的移动方向范围
                    TFloat offsetAngle = TMath.Asin((spaceToNeighbor + moveDistance + radius) / neighbor.Distance) * TMath.Rad2Deg;
                    nav.MoveBlock.AddAngle(targetAngle, offsetAngle);
                }
            }
            if (!nav.MoveBlock.IsEmpty)
            {
                //如果有其它角色阻挡则需要加入障碍物的阻挡，否则不要添加会影响寻路路径
                TFloat sd = nav.SDFMap.Sample(agent.Position);
                TVector2 gradiend = nav.SDFMap.Gradiend(agent.Position).normalized;
                if (sd < agent.Radius + agent.MoveDistance)
                {
                    TFloat targetAngle = Angle360(agent.Direction, -gradiend);
                    nav.MoveBlock.AddAngle(targetAngle, 90);
                }
            }
        }

        //移动时和其它角色碰撞方向调整,用于按照方向直线移动过程中避让其它角色
        //返回true，则代表进行过方向的调整
        public static bool AdjustMoveByNeighbor(this SDFNavContext nav, ref MoveAgentInfo agent)
        {
            nav.MoveBlock.Clear();
            foreach (var neighbor in nav.Neighbors)
            {
                if (neighbor.Distance <= Epsilon)
                    continue;//几乎重叠就允许移动直接忽略
                if (neighbor.Distance > agent.Radius + agent.MoveDistance + neighbor.Radius)
                    continue;//不会与其发生碰撞
                if (TMath.Abs(agent.Radius - neighbor.Radius) > neighbor.Distance)
                    continue;//如果一个在另一个的内部，就让其走出去
                TFloat targetAngle = Angle360(agent.Direction, neighbor.Direction);
                if (neighbor.Distance <= agent.Radius + neighbor.Radius)
                {
                    nav.MoveBlock.AddAngle(targetAngle, 90);
                    continue;
                }
                TFloat offsetAngle = TMath.Asin((agent.Radius + neighbor.Radius) / neighbor.Distance) * TMath.Rad2Deg;
                nav.MoveBlock.AddAngle(targetAngle, offsetAngle);
            }
            TFloat adjustAngle = nav.MoveBlock.GetMinOffsetAngle();
            if (TMath.Abs(adjustAngle) <= Epsilon || adjustAngle < -90 && adjustAngle > 90)
                return false;
            agent.Direction = Rotate(agent.Direction, adjustAngle);
            return true;
        }

        //贴墙移动处理
        //如果移动路线和撞到障碍物，则调整移动方向和移动距离，让其贴着墙滑动
        //返回true，则代表进行过方向和距离的调整
        public static bool AdjustMoveByObstacle(this SDFNavContext nav, ref MoveAgentInfo agentInfo)
        {
            TVector2 newPos = agentInfo.Position + agentInfo.Direction * agentInfo.MoveDistance;
            TFloat sd = nav.SDFMap.Sample(newPos);
            if (sd >= (agentInfo.Radius + Epsilon))
                return false;
            var gradient = nav.SDFMap.Gradiend(newPos).normalized;
            newPos = newPos + (agentInfo.Radius - sd + TFloat.EN3) * gradient;
            TVector2 diff = newPos - agentInfo.Position;
            //如果夹角大于等于90度，则不处理，让它撞墙
            if (TVector2.Dot(diff, agentInfo.Direction) <= Epsilon)
                return false;
            TFloat magnitude = diff.magnitude;
            if (magnitude <= Epsilon)
                return false;//退回到当前位置也不处理
            agentInfo.Direction = diff / magnitude;
            if (magnitude <= agentInfo.MoveDistance)
                agentInfo.MoveDistance = magnitude;
            return true;
        }

        public static TVector2 AdjustMoveByObstacle(this SDFRuntimeScene scene, TVector2 pos, TVector2 nextPos, TFloat radius)
        {
            TFloat sd = scene.Sample(pos);
            TVector2 direction = nextPos - pos;
            TFloat distance = direction.magnitude;
            if (distance < Epsilon)
                return nextPos;
            direction /= distance;
            if (sd <= radius)
            {
                //当前位置有问题
                var currentGradiend = scene.Gradiend(pos).normalized;
                TVector2 newPos = pos;
                newPos += currentGradiend * (radius + radius - sd);
                return newPos;
            }
            sd = scene.Sample(nextPos);
            if (sd >= (radius + Epsilon))
                return nextPos;
            var gradient = scene.Gradiend(nextPos).normalized;
            var adjustDir = direction - gradient * TVector2.Dot(gradient, direction);
            adjustDir.Normalize();
            nextPos = pos + adjustDir * distance;
            for (int i=0; i<3; ++i)
            {
                sd = scene.Sample(nextPos);
                if (sd >= radius)
                    break;
                gradient = scene.Gradiend(nextPos).normalized;
                nextPos += gradient * (radius - sd);
            }
            //避免往返
            if (TVector2.Dot(nextPos - pos, direction) < TFloat.Zero)
            {
                return pos;
            }
            TVector2 diff = nextPos - pos;
            TFloat magnitude = diff.magnitude;
            if (magnitude > distance)
            {
                return pos + (diff / magnitude) * distance;
            }
            return nextPos;
        }

        public static TFloat ColliderMoveByObstacle(this SDFNavContext nav, in MoveAgentInfo agent)
        {
            if (nav.SDFMap.Sample(agent.Position) < (agent.Radius + agent.MoveDistance))
                return nav.SDFMap.TryMoveTo(agent.Position, agent.Direction, agent.Radius, agent.MoveDistance);
            return agent.MoveDistance;
        }

        public static TFloat ColliderMoveByNeighbor(this SDFNavContext nav, in MoveAgentInfo agent)
        {
            TFloat distance = agent.MoveDistance;
            foreach (var neighbor in nav.Neighbors)
            {
                TFloat dot = TVector2.Dot(neighbor.Direction, agent.Direction);
                dot = TMath.Clamp(dot, -TFloat.One, TFloat.One);
                if (dot <= Epsilon)
                    continue;//目标在自己侧后方不处理
                if (neighbor.Distance < neighbor.Radius + agent.Radius)
                    return TFloat.Zero;//已经产生碰撞就不移动了
                //自己中心点A，目标中心点B，B投影到自己移动路径上的点C，组成直角三角形，斜边为AB
                TFloat ac = dot * neighbor.Distance;//AC的长度
                TFloat sqrbc = Sqr(neighbor.Distance) - Sqr(ac);//BC的长度
                TFloat sqrToRadius = Sqr(agent.Radius + neighbor.Radius);
                if (sqrbc >= sqrToRadius)
                    continue;//BC的距离大于两者半径之和说明移动过程中不会产生碰撞
                             //此时把A设为碰撞是自己中心点的位置，BC不变AB则是两者半径之和,
                TFloat newac = TMath.Sqrt(sqrToRadius - sqrbc);
                TFloat collisionDistance = ac - newac;//产生碰撞时移动的距离
                if (collisionDistance < distance)
                    distance = collisionDistance;
            }
            return distance;
        }

        //返回移动到目标点碰撞到的角色索引
        public static int MoveTest(this SDFNavContext nav, TVector2 direction, TFloat radius, TFloat maxDistance)
        {
            for (int i=0; i<nav.Neighbors.Count; ++i)
            {
                var neighbor = nav.Neighbors[i];
                TFloat dot = TVector2.Dot(neighbor.Direction, direction);
                dot = TMath.Clamp(dot, -TFloat.One, TFloat.One);
                if (dot <= Epsilon)
                    continue;//目标在自己侧后方不处理
                if (neighbor.Distance < neighbor.Radius + radius)
                    return i;//已经产生碰撞
                //自己中心点A，目标中心点B，B投影到自己移动路径上的点C，组成直角三角形，斜边为AB
                TFloat ac = dot * neighbor.Distance;//AC的长度
                TFloat bc = TMath.Sqrt(Sqr(neighbor.Distance) - Sqr(ac));//BC的长度
                if (bc >= (radius + neighbor.Radius))
                    continue;
                //BC的距离大于两者半径之和说明移动过程中不会产生碰撞
                //此时把A设为碰撞是自己中心点的位置，BC不变AB则是两者半径之和,
                TFloat newac = TMath.Sqrt(Sqr(radius + neighbor.Radius) - Sqr(bc));
                TFloat collisionDistance = ac - newac;//产生碰撞时移动的距离
                if (collisionDistance < maxDistance)
                    return i;
            }
            return -1;
        }
        public static TVector2 Rotate(TVector2 v, TFloat degree)
        {
            degree *= -TMath.Deg2Rad;
            var ca = TMath.Cos(degree);
            var sa = TMath.Sin(degree);
            return new TVector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
        }

        public static TFloat Cross(TVector2 lhs, TVector2 rhs)
        {
            return lhs.x * rhs.y - lhs.y * rhs.x;
        }

        //顺时针方向，-180 <-> 180
        public static TFloat Angle360(TVector2 from, TVector2 to)
        {
            TFloat dAngle = TMath.Acos(TMath.Clamp(TVector2.Dot(from.normalized, to.normalized), -TFloat.One, TFloat.One)) * TMath.Rad2Deg;
            if (Cross(from, to) > TFloat.Zero)
            {
                dAngle = -dAngle;
            }
            return dAngle;
        }

        public static TFloat Sqr(TFloat v)
        {
            return v * v;
        }

    }
}
