using TrueSync;
namespace TSDFNav
{
    [System.Serializable]
    public class HeightMapData
    {
        public int Width;
        public int Height;
        public TFloat Grain;
        public TVector2 Origin;
        public TFloat Min;
        public TFloat Max;
        public byte[] Data;//如果没有高度差，则为空
        public override string ToString()
        {
            return $"W:{Width},H:{Height},P:{Origin},Range:{Min} -> {Max}";
        }
        public TFloat Sample(TVector2 pos)
        {
            if (Data == null)
                return Min;
            pos = (pos - Origin) / Grain;
            int x = TMath.FloorToInt(pos.x);
            int y = TMath.FloorToInt(pos.y);
            int idx = x + y * Width;
            TFloat rx = pos.x - x;
            TFloat ry = pos.y - y;
            
            //2 3
            //0 1
            TFloat v0 = Data[idx];
            TFloat v1 = Data[idx + 1];
            TFloat v2 = Data[idx + Width];
            TFloat v3 = Data[idx + Width + 1];
            TFloat v = (v0 * (TFloat.One - rx) + v1 * rx) * (TFloat.One - ry) + (v2 * (TFloat.one - rx) + v3 * rx) * ry;
            return (v/255) *(Max - Min) + Min;
        }
        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(Grain);
            writer.Write(Origin.x);
            writer.Write(Origin.y);
            writer.Write(Min);
            writer.Write(Max);
            if (Data != null)
            {
                writer.Write(Data.Length);
                writer.Write(Data);
            }
            else
            {
                writer.Write(0);
            }
        }

        public void Read(System.IO.BinaryReader reader)
        {
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Grain = reader.ReadTFloat();
            Origin = new TVector2(reader.ReadTFloat(), reader.ReadTFloat());
            Min = reader.ReadTFloat();
            Max = reader.ReadTFloat();
            int len = reader.ReadInt32();
            if (len > 0)
            {
                Data = reader.ReadBytes(len);
            }
            else
            {
                Data = null;
            }
        }
    }
}