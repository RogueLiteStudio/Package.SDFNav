using UnityEngine;
using System.IO;

namespace SDFNav
{
    [System.Serializable]
    public class SDFData : ISDF
    {
        private short[] data;
        private int width;
        private int height;
        private float grain;
        private float scale;
        private Vector2 origin;

        public int Width => width;
        public int Height => height;
        public float Grain => grain;
        public float Scale => scale;
        public Vector2 Origin => origin;
        public short this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                    return short.MinValue;
                return data[x + y * width];
            }
        }

        public float this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= data.Length)
                    return short.MinValue * scale;
                return data[idx] * scale;
            }
        }

        public short At(int idx)
        {
            return data[idx];
        }

        public float Sample(Vector2 pos)
        {
            pos = (pos - Origin) / grain;
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int idx = x + y * width;
            float rx = pos.x - x;
            float ry = pos.y - y;
            //2 3
            //0 1
            float v0 = this[idx];
            float v1 = this[idx + 1];
            float v2 = this[idx + width];
            float v3 = this[idx + width + 1];

            return (v0 * (1 - rx) + v1 * rx) * (1 - ry) + (v2 * (1 - rx) + v3 * rx) * ry;
        }

        public float Get(int x, int y)
        {
            return this[x + y * width];
        }

        public void Init(int width, int height, float grain, float scale, Vector2 origin, short[] data)
        {
            this.width = width;
            this.height = height;
            this.grain = grain;
            this.scale = scale;
            this.origin = origin;
            this.data = new short[width * height];
            data.CopyTo(this.data, 0);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(width);
            writer.Write(height);
            writer.Write(grain);
            writer.Write(scale);
            writer.Write(origin.x);
            writer.Write(origin.y);
            writer.Write(data.Length);
            for (int i = 0; i < data.Length; ++i)
            {
                writer.Write(data[i]);
            }
        }

        public void Read(BinaryReader reader)
        {
            width = reader.ReadInt32();
            height = reader.ReadInt32();
            grain = reader.ReadSingle();
            scale = reader.ReadSingle();
            origin = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            int len = reader.ReadInt32();
            data = new short[len];
            for (int i = 0; i < len; ++i)
            {
                data[i] = reader.ReadInt16();
            }
        }
    }
}