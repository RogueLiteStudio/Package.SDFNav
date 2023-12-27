using UnityEngine;
namespace SDFNav.Editor
{
    public interface IShape
    {
        float SDF(Vector2 pt);
        Rect Bounds();
    }

    public struct BoxObstacle : IShape
    {
        public Vector2 Center;
        public Vector2 Size;
        public float RotateAngle;
        public Rect Bounds()
        {
            return DynamicObstacleExportUtil.CalcBoxBounds(Center, Size, RotateAngle);
        }

        public float SDF(Vector2 pt)
        {
            pt -= Center;
            pt = DynamicObstacleExportUtil.Rotate(pt, -RotateAngle);
            Vector2 d = DynamicObstacleExportUtil.Abs(pt) - Size * 0.5f;
            return Vector2.Max(d, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0);
        }
    }

    public struct CircleObstacle : IShape
    {
        public Vector2 Center;
        public float Radius;
        public Rect Bounds()
        {
            return DynamicObstacleExportUtil.CalcCircleBounds(Center, Radius);
        }

        public float SDF(Vector2 pt)
        {
            return Vector2.Distance(pt, Center) - Radius;
        }
    }
    public static class DynamicObstacleExportUtil
    {

        private static readonly Vector2[] RectPoint = new Vector2[]
        {
            new Vector2(-0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, -0.5f),
            new Vector2(-0.5f, -0.5f),
        };
        public static Vector2 Abs(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        public static Vector2 Rotate(Vector2 v, float degree)
        {
            degree *= -Mathf.Deg2Rad;
            var ca = Mathf.Cos(degree);
            var sa = Mathf.Sin(degree);
            return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
        }

        public static Rect CalcCircleBounds(Vector2 center, float radius)
        {
            float minX = center.x - radius;
            float minY = center.y - radius;
            float maxX = center.x + radius;
            float maxY = center.y + radius;
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        public static Rect CalcBoxBounds(Vector2 center, Vector2 size, float rotAngle)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            foreach (var p in RectPoint)
            {
                Vector2 pt = new Vector2(p.x * size.x, p.y * size.y);
                pt = Rotate(pt, rotAngle) + center;
                minX = Mathf.Min(minX, pt.x);
                maxX = Mathf.Max(maxX, pt.x);
                minY = Mathf.Min(minY, pt.y);
                maxY = Mathf.Max(maxY, pt.y);
            }
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        public static DynamicObstacle ToDynamicObstacle(SDFData data, IShape shape)
        {
            Rect rect = shape.Bounds();
            int x = Mathf.FloorToInt(rect.xMin / data.Grain);
            int y = Mathf.FloorToInt(rect.xMin / data.Grain);
            int width = Mathf.CeilToInt(rect.width / data.Grain);
            int height = Mathf.CeilToInt(rect.height / data.Grain);
            x = Mathf.Clamp(x, 0, data.Width - 1);
            y = Mathf.Clamp(y, 0, data.Height - 1);
            width = Mathf.Min(width, data.Width - x);
            height = Mathf.Min(height, data.Height - y);

            for (int i=x-1; i>=0; --i)
            {
                int count = 0;
                for (int j=0; j<data.Height; ++j)
                {
                    if (CompareSD(data, shape, i, j))
                        count++;
                }
                if (count == 0)
                {
                    x = i + 1;
                    break;
                }
            }
            for (int i = width + x; i < data.Width; ++i)
            {
                int count = 0;
                for (int j = 0; j < data.Height; ++j)
                {
                    if (CompareSD(data, shape, i, j))
                        count++;
                }
                if (count == 0)
                {
                    width = i - x;
                    break;
                }
            }
            for (int i = y-1; i >= 0; --i)
            {
                int count = 0;
                for (int j = 0; j < data.Width; ++j)
                {
                    if (CompareSD(data, shape, j, i))
                        count++;
                }
                if (count == 0)
                {
                    y = i+1;
                    break;
                }
            }
            for (int i = height + y; i < data.Height; ++i)
            {
                int count = 0;
                for (int j = 0; j < data.Width; ++j)
                {
                    if (CompareSD(data, shape, j, i))
                        count++;
                }
                if (count == 0)
                {
                    height = i - y;
                    break;
                }
            }
            if (height <= 0 || width <= 0 || x >= data.Width || y >= data.Height)
                return null;
            DynamicObstacle obstacle = new DynamicObstacle()
            { 
                Width= width, 
                Height = height,
                X = x,
                Y = y,
                Data = new short[width * height]
            };
            for (int i=0; i<width; ++i)
            {
                for (int j=0; j< height; ++j)
                {
                    Vector2 pos = new Vector2((i + x) * data.Grain, (j + y) * data.Grain);
                    short sd = (short)(shape.SDF(pos) / data.Scale);
                    obstacle.Data[i + width * j] = sd;
                }
            }
            return obstacle;
        }

        public static bool CompareSD(SDFData data, IShape shape, int x, int y)
        {
            float original = data.Get(x, y);
            if (original < 0)
                return false;
            Vector2 pos = new Vector2(x * data.Grain, y * data.Grain);
            float sd = shape.SDF(pos);
            return sd < original;
        }

        public static DynamicObstacle BoxToDynamicObstacle(SDFData data, Vector2 center, Vector2 size, float rotAngle)
        {
            BoxObstacle obstacle = new BoxObstacle { Center = center - data.Origin, Size = size, RotateAngle = rotAngle };
            return ToDynamicObstacle(data, obstacle);
        }

        public static DynamicObstacle CircleToDynamicObstacle(SDFData data, Vector2 center, float radius)
        {
            CircleObstacle obstacle = new CircleObstacle { Center = center - data.Origin, Radius = radius };
            return ToDynamicObstacle(data, obstacle);
        }

    }
}