using TrueSync;
namespace SDFNav
{
    public static class SDFUtil
    {
        public static TFloat CircleSDF(TVector2 point, TVector2 center, TFloat radius)
        {
            return TVector2.Distance(point, center) - radius;
        }
        public static TVector2 Rotate(this TVector2 vec, TVector2 normal)
        {
            return new TVector2(vec.x * normal.x - vec.y * normal.y, vec.x * normal.y + vec.y * normal.x);
        }
        public static TVector2 Abs(this TVector2 vector)
        {
            return new TVector2(TMath.Abs(vector.x), TMath.Abs(vector.y));
        }
        public static TVector2 Rotate(this TVector2 vec, TFloat angle)
        {
            TFloat radians = TMath.Deg2Rad * angle;
            TFloat cos = TMath.Cos(radians);
            TFloat sin = TMath.Sin(radians);
            return new TVector2(vec.x * cos - vec.y * sin, vec.x * sin + vec.y * cos);
        }

        //反向旋转向量
        public static TVector2 InvertRotate(this TVector2 vec, TVector2 normal)
        {
            return new TVector2(vec.x * normal.x + vec.y * normal.y, -vec.x * normal.y + vec.y * normal.x);
        }

        public static TFloat SegmentSDF(TVector2 point, TVector2 from, TVector2 to)
        {
            TVector2 ap = point - from;
            TVector2 ab = to - from;
            TFloat h = TMath.Clamp01(TVector2.Dot(ap, ab) / TVector2.Dot(ab, ab));
            return (ap - h * ab).magnitude;
        }

        public static TFloat BoxSDF(TVector2 point, TVector2 center, TVector2 halfSize)
        {
            point -= center;
            TVector2 d = point.Abs() - halfSize;
            return TVector2.Max(d, TVector2.zero).magnitude + TMath.Min(TMath.Max(d.x, d.y), TFloat.Zero);
        }

        public static TFloat OrientedBoxSDF(TVector2 point, TVector2 center, TVector2 rotation, TVector2 halfSize)
        {
            point -= center;
            point = point.Rotate(rotation);
            TVector2 d = point.Abs() - halfSize;
            return TVector2.Max(d, TVector2.zero).magnitude + TMath.Min(TMath.Max(d.x, d.y), TFloat.Zero);
        }

        public static TFloat TriangleSDF(TVector2 point, TVector2 p0, TVector2 p1, TVector2 p2)
        {
            TVector2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
            TVector2 v0 = point - p0, v1 = point - p1, v2 = point - p2;
            TVector2 pq0 = v0 - e0 * TMath.Clamp01(TVector2.Dot(v0, e0) / TVector2.Dot(e0, e0));
            TVector2 pq1 = v1 - e1 * TMath.Clamp01(TVector2.Dot(v1, e1) / TVector2.Dot(e1, e1));
            TVector2 pq2 = v2 - e2 * TMath.Clamp01(TVector2.Dot(v2, e2) / TVector2.Dot(e2, e2));
            TFloat s = TMath.Sign(e0.x * e2.y - e0.y * e2.x);

            TVector2 d = TVector2.Min(TVector2.Min(new TVector2(TVector2.Dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
                         new TVector2(TVector2.Dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
                         new TVector2(TVector2.Dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
            return -TMath.Sqrt(d.x) * TMath.Sign(d.y);
        }

        public static bool IsInTriangle(TVector2 point, TVector2 p0, TVector2 p1, TVector2 p2)
        {
            TVector2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
            TVector2 v0 = point - p0, v1 = point - p1, v2 = point - p2;
            TFloat s = TMath.Sign(e0.x * e2.y - e0.y * e2.x);

            TFloat y = TMath.Min(s * (v0.x * e0.y - v0.y * e0.x), s * (v1.x * e1.y - v1.y * e1.x));
            y = TMath.Min(y, s * (v2.x * e2.y - v2.y * e2.x));
            return y > TFloat.Zero;
        }
    }

}
