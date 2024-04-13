using TrueSync;
using System.Collections.Generic;
namespace TSDFNav
{
    public class SDFRuntimeScene : ISDF
    {
        protected SDFScene sceneData;
        protected SDFData StaticData;
        protected List<DynamicObstacle> Obstacles = new List<DynamicObstacle>();
        protected LimitAreaShape Area;

        public int Width => StaticData.Width;

        public int Height => StaticData.Height;

        public TFloat Grain => StaticData.Grain;

        public TFloat Scale => StaticData.Scale;

        public TVector2 Origin => StaticData.Origin;

        public LimitAreaShape LimitArea => Area;

        public TFloat this[int idx]
        {
            get
            {
                int x = idx % Width;
                int y = idx / Width;
                TFloat val = TFloat.MaxValue;
                if (Area != null)
                {
                    TVector2 pt = Origin + new TVector2(x * Grain, y * Grain);
                    val = Area.Sample(pt);
                    if (val < 0)
                        return val;
                }
                short sd = StaticData.At(idx);
                foreach (var ob in Obstacles)
                {
                    sd = ob.SDF(x, y, sd);
                }
                return TMath.Min(val, sd * StaticData.Scale);
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

        public TFloat Sample(TVector2 pos)
        {
            pos = (pos - Origin) / StaticData.Grain;
            int x = TMath.FloorToInt(pos.x);
            int y = TMath.FloorToInt(pos.y);
            TFloat rx = pos.x - x;
            TFloat ry = pos.y - y;
            //2 3
            //0 1
            TFloat v0 = Get(x, y);
            TFloat v1 = Get(x + 1, y);
            TFloat v2 = Get(x, y + 1);;
            TFloat v3 = Get(x + 1, y + 1);

            return (v0 * (TFloat.One - rx) + v1 * rx) * (TFloat.One - ry) + (v2 * (TFloat.One - rx) + v3 * rx) * ry;
        }

        private TFloat Get(int x, int y)
        {
            int offset = 0;
            if (x < 0)
            {
                offset = TMath.Max(offset, -x);
                x = 0;
            }
            if (x >= Width)
            {
                offset = TMath.Max(offset, x - Width + 1);
                x = Width - 1;
            }
            if (y < 0)
            {
                offset = TMath.Max(offset, -y);
                y = 0;
            }
            if (y >= Height)
            {
                offset = TMath.Max(offset, y - Height + 1);
                y = Height - 1;
            }
            x = TMath.Clamp(x, 0, StaticData.Width - 1);
            y = TMath.Clamp(y, 0, StaticData.Height - 1);
            return this[x + y * StaticData.Width] - offset * StaticData.Grain;
        }
    }


}