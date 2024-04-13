using System.Collections.Generic;
using TrueSync;
namespace TSDFNav
{
    public class MoveDirectionRange
    {
        struct Range
        {
            public TFloat Min;
            public TFloat Max;
        }

        private List<Range> LeftRange = new List<Range>();
        private List<Range> RightRange = new List<Range>();
        public bool IsEmpty=>LeftRange.Count == 0 && RightRange.Count == 0;
        public void Clear()
        {
            LeftRange.Clear();
            RightRange.Clear();
        }

        public TFloat GetMinOffsetAngle()
        {
            TFloat rightAngle = GetRightMinAngle();
            if (rightAngle == 0)
                return rightAngle;
            TFloat leftAngle = GetLeftMinAngle();
            if (leftAngle > rightAngle)
                return rightAngle;
            return -leftAngle;
        }

        public TFloat GetLeftMinAngle()
        {
            TFloat angle = 0;
            foreach (var range in LeftRange)
            {
                if (range.Min > angle)
                    break;
                angle = range.Max + 5;
            }
            return angle;
        }

        public TFloat GetRightMinAngle()
        {
            TFloat angle = 0;
            foreach (var range in RightRange)
            {
                if (range.Min > angle)
                    break;
                angle = range.Max + 5;
            }
            return angle;
        }


        public void AddAngle(TFloat angle, TFloat offset)
        {
            TFloat min = angle - offset;
            TFloat max = angle + offset;
            if (min < -180)
            {
                AddToRange(360 + min, 180, RightRange);
                min = -180;
            }
            if (max > 180)
            {
                AddToRange(360 - max, 180, LeftRange);
                max = 180;
            }
            if (min >= 0)
            {
                AddToRange(min, max, RightRange);
            }
            else if (max <= 0)
            {
                AddToRange(-max, -min, LeftRange);
            }
            else
            {
                AddToRange(0, -min, LeftRange);
                AddToRange(0, max, RightRange);
            }
        }

        private void AddToRange(TFloat min, TFloat max, List<Range> ranges)
        {
            int insertIdx = 0;
            for (int i = 0; i < ranges.Count; ++i)
            {
                var r = ranges[i];
                if (r.Min > max)
                {
                    insertIdx = i;
                    break;
                }
                if (min > r.Max)
                {
                    insertIdx++;
                    continue;
                }
                r.Min = TMath.Min(min, r.Min);
                r.Max = TMath.Max(max, r.Max);
                ranges[i] = r;
                return;
            }
            ranges.Insert(insertIdx, new Range { Min = min, Max = max });
        }
    }
}