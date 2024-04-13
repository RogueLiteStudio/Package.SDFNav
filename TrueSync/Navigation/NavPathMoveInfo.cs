using System.Collections.Generic;
using TrueSync;

namespace TSDFNav
{
    public class NavPathMoveInfo
    {
        public List<TVector2> Path = new List<TVector2>();//反向路径
        public TVector2 LastMoveDirection;//上一次的移动方向
        public TFloat LastAdjustAngle;//上一次移动调整的角度

        public bool HasFinished=>Path.Count == 0;

        public bool HasAdjustAngle => LastAdjustAngle < NavigationUtil.Epsilon || LastAdjustAngle > NavigationUtil.Epsilon;

        public TVector2 NextPonit()
        {
            if (Path.Count > 0)
                return Path[Path.Count - 1];
            return TVector2.zero;
        }

        public void RemoveLastPoint()
        {
            int count = Path.Count;
            if (count > 0)
            {
                Path.RemoveAt(count - 1);
            }
        }

        public void Clear()
        {
            Path.Clear();
            LastMoveDirection = TVector2.zero;
            LastAdjustAngle = TFloat.Zero;
        }
    }
}
