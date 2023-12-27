using UnityEngine;
using System.Collections.Generic;
namespace SDFNav
{
    public class SDFRuntimeScene : ISDF
    {
        protected SDFScene sceneData;
        protected SDFData StaticData;
        protected List<DynamicObstacle> Obstacles = new List<DynamicObstacle>();
        protected LimitAreaShape Area;

        public int Width => StaticData.Width;

        public int Height => StaticData.Height;

        public float Grain => StaticData.Grain;

        public float Scale => StaticData.Scale;

        public Vector2 Origin => StaticData.Origin;

        public LimitAreaShape LimitArea => Area;

        public float this[int idx]
        {
            get
            {
                int x = idx % Width;
                int y = idx / Width;
                float val = float.MaxValue;
                if (Area != null)
                {
                    Vector2 pt = Origin + new Vector2(x * Grain, y * Grain);
                    val = Area.Sample(pt);
                    if (val < 0)
                        return val;
                }
                short sd = StaticData.At(idx);
                foreach (var ob in Obstacles)
                {
                    sd = ob.SDF(x, y, sd);
                }
                return Mathf.Min(val, sd * StaticData.Scale);
            }
        }

        public SDFRuntimeScene(SDFScene data)
        {
            sceneData = data;
            StaticData = data.Data;
        }

        public void EnableObstacle(string name)
        {
            var obstacle = sceneData.Obstacles.Find(it => it.Name == name);
            if (obstacle != null && !Obstacles.Contains(obstacle))
            {
                Obstacles.Add(obstacle);
            }
        }

        public void DisableObstacle(string name)
        {
            int idx = Obstacles.FindIndex(it => it.Name == name);
            if (idx >= 0)
            {
                Obstacles.RemoveAt(idx);
            }
        }

        public LimitAreaShape SwitchLimitArea(string name)
        {
            if (Area != null && Area.Name != name)
            {
                Area = null;
            }
            Area = sceneData.Areas.Find(it => it.Name == name);
            return Area;
        }

        public float Sample(Vector2 pos)
        {
            pos = (pos - Origin) / StaticData.Grain;
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            float rx = pos.x - x;
            float ry = pos.y - y;
            //2 3
            //0 1
            float v0 = Get(x, y);
            float v1 = Get(x + 1, y);
            float v2 = Get(x, y + 1);;
            float v3 = Get(x + 1, y + 1);

            return (v0 * (1 - rx) + v1 * rx) * (1 - ry) + (v2 * (1 - rx) + v3 * rx) * ry;
        }

        private float Get(int x, int y)
        {
            int offset = 0;
            if (x < 0)
            {
                offset = Mathf.Max(offset, -x);
                x = 0;
            }
            if (x >= Width)
            {
                offset = Mathf.Max(offset, x - Width + 1);
                x = Width - 1;
            }
            if (y < 0)
            {
                offset = Mathf.Max(offset, -y);
                y = 0;
            }
            if (y >= Height)
            {
                offset = Mathf.Max(offset, y - Height + 1);
                y = Height - 1;
            }
            x = Mathf.Clamp(x, 0, StaticData.Width - 1);
            y = Mathf.Clamp(y, 0, StaticData.Height - 1);
            return this[x + y * StaticData.Width] - offset * StaticData.Grain;
        }
    }


}