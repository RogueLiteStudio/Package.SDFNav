using System.IO;

namespace SDFNav
{
    [System.Serializable]
    public class DynamicObstacle
    {
        public string Name;
        public int Width;
        public int Height;
        public int X;
        public int Y;
        public short[] Data;
        public short this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return short.MaxValue;
                return Data[x + y * Width];
            }
        }

        public bool TryGet(int x, int y, out short val)
        {
            x -= X;
            y -= Y;
            val = 0;
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;
            val = Data[x + y * Width];
            return true;
        }

        public short SDF(int x, int y, short sd)
        {
            x -= X;
            y -= Y;
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return sd;
            short val = Data[x + y * Width];
            return val < sd ? val : sd;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Data.Length);
            for (int i = 0; i < Data.Length; ++i)
            {
                writer.Write(Data[i]);
            }
        }

        public void Read(BinaryReader reader)
        {
            Name = reader.ReadString();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
            int len = reader.ReadInt32();
            Data = new short[len];
            for (int i = 0; i < len; ++i)
            {
                Data[i] = reader.ReadInt16();
            }
        }
    }


}