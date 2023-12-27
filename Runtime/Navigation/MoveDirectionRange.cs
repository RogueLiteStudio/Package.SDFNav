using System.Collections.Generic;
using UnityEngine;
namespace SDFNav
{
    public class MoveDirectionRange
    {
        struct Range
        {
            public float Min;
            public float Max;
        }

        private List<Range> LeftRange = new List<Range>();
        private List<Range> RightRange = new List<Range>();
        public bool IsEmpty=>LeftRange.Count == 0 && RightRange.Count == 0;
        public void Clear()
        {
            LeftRange.Clear();
            RightRange.Clear();
        }

        public float GetMinOffsetAngle()
        {
            float rightAngle = GetRightMinAngle();
            if (rightAngle == 0)
                return rightAngle;
            float leftAngle = GetLeftMinAngle();
            if (leftAngle > rightAngle)
                return rightAngle;
            return -leftAngle;
        }

        public float GetLeftMinAngle()
        {
            float angle = 0;
            foreach (var range in LeftRange)
            {
                if (range.Min > angle)
                    break;
                angle = range.Max + 5;
            }
            return angle;
        }

        public float GetRightMinAngle()
        {
            float angle = 0;
            foreach (var range in RightRange)
            {
                if (range.Min > angle)
                    break;
                angle = range.Max + 5;
            }
            return angle;
        }


        public void AddAngle(float angle, float offset)
        {
            float min = angle - offset;
            float max = angle + offset;
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

        private void AddToRange(float min, float max, List<Range> ranges)
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
                r.Min = Mathf.Min(min, r.Min);
                r.Max = Mathf.Max(max, r.Max);
                ranges[i] = r;
                return;
            }
            ranges.Insert(insertIdx, new Range { Min = min, Max = max });
        }
    }
}