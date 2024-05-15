using TrueSync;
using System.IO;

namespace TSDFNav
{
    [System.Serializable]
    public class SDFData : ISDF
    {
        private short[] data;
        private int width;
        private int height;
        private TFloat grain;
        private TFloat scale;
        private TVector2 origin;

        public int Width => width;
        public int Height => height;
        public TFloat Grain => grain;
        public TFloat Scale => scale;
        public TVector2 Origin => origin;
        public short[] Data => data;
        public short this[int x, int y]
        {
            get
            {
                if (x < TFloat.Zero || x >= width || y < TFloat.Zero || y >= height)
                    return short.MinValue;
                return data[x + y * width];
            }
        }

        public TFloat this[int idx]
        {
            get
            {
                if (idx < TFloat.Zero || idx >= data.Length)
                    return short.MinValue * scale;
                return data[idx] * scale;
            }
        }

        public short At(int idx)
        {
            return data[idx];
        }

        public TFloat Sample(TVector2 pos)
        {
            pos = (pos - Origin) / grain;
            int x = TMath.FloorToInt(pos.x);
            int y = TMath.FloorToInt(pos.y);
            int idx = x + y * width;
            TFloat rx = pos.x - x;
            TFloat ry = pos.y - y;
            //2 3
            //0 1
            TFloat v0 = this[idx];
            TFloat v1 = this[idx + 1];
            TFloat v2 = this[idx + width];
            TFloat v3 = this[idx + width + 1];

            return (v0 * (TFloat.One - rx) + v1 * rx) * (TFloat.One - ry) + (v2 * (TFloat.One - rx) + v3 * rx) * ry;
        }

        public TFloat Get(int x, int y)
        {
            return this[x + y * width];
        }

        public void Init(int width, int height, TFloat grain, TFloat scale, TVector2 origin, short[] data)
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
            grain = reader.ReadTFloat();
            scale = reader.ReadTFloat();
            origin = new TVector2(reader.ReadTFloat(), reader.ReadTFloat());
            int len = reader.ReadInt32();
            data = new short[len];
            for (int i = 0; i < len; ++i)
            {
                data[i] = reader.ReadInt16();
            }
        }
    }
}