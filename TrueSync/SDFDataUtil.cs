using TrueSync;

namespace TSDFNav
{
    public static class SDFDataUtil
    {
        public static void VectorCheck(TVector2 v)
        {
            if (TFloat.IsNaN(v.x) || TFloat.IsNaN(v.y))
            {
                throw new System.Exception($"TVector2 is NaN => ({v.x}, {v.y})");
            }
        }

        public static bool CheckStraightMove(this ISDF data, TVector2 from, TVector2 to, TFloat moveRadius)
        {
            VectorCheck(from);
            VectorCheck(to);
            TVector2 diff = to - from;
            TFloat distance = diff.sqrMagnitude;
            if (distance <= TFloat.EN5)
                return true;
            distance = TMath.Sqrt(distance);

            diff /= distance;

            TFloat t = TFloat.Zero;

            while (true)
            {
                TFloat sd = data.Sample(from + diff * t);
                TFloat step = sd - moveRadius;
                if (step <= -TFloat.EN3)
                    return false;
                t += TMath.Max(TMath.Abs(step), TFloat.EN3);
                if (t >= distance)
                    return true;
            }
        }
        //梯度：远离障碍物的方向
        public static TVector2 Gradiend(this ISDF data, TVector2 point)
        {
            TFloat delta = TFloat.One;
            TVector2 v1 = new TVector2(point.x + delta, point.y);
            TVector2 v2 = new TVector2(point.x - delta, point.y);
            TVector2 v3 = new TVector2(point.x, point.y + delta);
            TVector2 v4 = new TVector2(point.x, point.y - delta);
            return TFloat.Half * new TVector2(data.Sample(v1) - data.Sample(v2),
                data.Sample(v3) - data.Sample(v4));
        }

        public static TVector2 FindNearestValidPoint(this ISDF data, TVector2 point, TFloat radius)
        {
            TVector2 newPos = point;
            for (int i=0; i<10; ++i)
            {
                TFloat sdf = data.Sample(newPos);
                if (sdf >= radius)
                   break;
                TFloat t = radius - sdf;
                if (t > 0)
                    t = TMath.Max(t, TFloat.EN3);

                newPos += Gradiend(data, newPos).normalized * t;
            }
            return newPos;

        }

        public static TFloat TryMoveTo(this ISDF data, TVector2 origin, TVector2 dir, TFloat radius, TFloat maxDistance)
        {
            VectorCheck(origin);
            VectorCheck(dir);
            if (TFloat.IsNaN(maxDistance))
                return 0;
            TFloat sd = data.Sample(origin);
            //if (sd < 0)
            //    return 0;
            TFloat t = 0;
            if (radius > sd)
            {
                //如果已经撞，当移动方向和梯度方向接近时，可以判定为远离障碍物，则允许它移动出去
                TVector2 gradiend = Gradiend(data, origin).normalized;
                if (TVector2.Dot(dir, gradiend) < TFloat.Zero)
                    return TFloat.Zero;
                radius = sd;
            }
            while (true)
            {
                TVector2 p = origin + dir * (t + TFloat.EN3);
                sd = data.Sample(p);
                TFloat step = sd - radius;
                if (step <= -TFloat.EN3)
                    return t;
                t += TMath.Max(TMath.Abs(step), data.Grain / 4);
                if (t >= maxDistance)
                    return maxDistance;
            }
        }

        public static TFloat DiskCast(this ISDF data, TVector2 origin, TVector2 dir, TFloat radius, TFloat maxDistance)
        {
            VectorCheck(origin);
            VectorCheck(dir);
            TFloat sdStart = data.Sample(origin);
            if (sdStart < TFloat.Zero)
                return TFloat.Zero;
            TFloat t = TFloat.Zero;
            if (sdStart <= radius)
            {
                //这里是为了防止起点与边界的距离过短导致直接返回失败，如果需要限制起点的范围，需要额外处理
                t = radius - sdStart;
            }
            while (true)
            {
                TVector2 p = origin + dir * (t + TFloat.EN3);
                TFloat sd = data.Sample(p);
                TFloat step = sd - radius;
                if (step <= -TFloat.EN3)
                    return t;
                t += TMath.Max(TMath.Abs(step), TFloat.EN3);
                if (t >= maxDistance)
                    return maxDistance;
            }
        }
    }
}
