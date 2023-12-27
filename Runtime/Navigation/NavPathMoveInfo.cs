using System.Collections.Generic;
using UnityEngine;

namespace SDFNav
{
    public class NavPathMoveInfo
    {
        public List<Vector2> Path = new List<Vector2>();//反向路径
        public Vector2 LastMoveDirection;//上一次的移动方向
        public float LastAdjustAngle;//上一次移动调整的角度

        public bool HasFinished=>Path.Count == 0;

        public bool HasAdjustAngle => LastAdjustAngle < NavigationUtil.Epsilon || LastAdjustAngle > NavigationUtil.Epsilon;

        public Vector2 NextPonit()
        {
            if (Path.Count > 0)
                return Path[Path.Count - 1];
            return Vector2.zero;
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
            LastMoveDirection = Vector2.zero;
            LastAdjustAngle = 0;
        }
    }
}
