using TrueSync;
using System.IO;

namespace TSDFNav
{
    public enum AreaShpeType
    {
        Circle,
        Box,
    }
    [System.Serializable]
    public class LimitAreaShape
    {
        public string Name;
        public AreaShpeType Shape;
        public TVector2 Center;
        public TVector2 Size;
        public TVector2 Rotation;//{cos, sin}
        
        public TFloat Sample(TVector2 pt)
        {
            switch (Shape)
            {
                case AreaShpeType.Circle:
                    return Size.x*TFloat.Half - TVector2.Distance(pt, Center);
                case AreaShpeType.Box:
                    pt -= Center;
                    pt = new TVector2(Rotation.x * pt.x - Rotation.y * pt.y, Rotation.y * pt.x + Rotation.x * pt.y);
                    pt.x = TMath.Abs(pt.x);
                    pt.y = TMath.Abs(pt.y);
                    TVector2 d = pt - Size * TFloat.Half;
                    return -(TVector2.Max(d, TVector2.zero).magnitude + TMath.Min(TMath.Max(d.x, d.y), TFloat.Zero));
            }
            return TFloat.MaxValue;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write((byte)Shape);
            writer.Write(Center.x);
            writer.Write(Center.y);
            writer.Write(Size.x);
            if (Shape == AreaShpeType.Box)
            {
                writer.Write(Size.y);
                writer.Write(Rotation.x);
                writer.Write(Rotation.y);
            }
        }

        public void Read(BinaryReader reader)
        {
            Name = reader.ReadString();
            Shape = (AreaShpeType)reader.ReadByte();
            Center.x = reader.ReadTFloat();
            Center.y = reader.ReadTFloat();
            Size.x = reader.ReadTFloat();
            if (Shape == AreaShpeType.Box)
            {
                Size.y = reader.ReadTFloat();
                Rotation.x = reader.ReadTFloat();
                Rotation.y = reader.ReadTFloat();
            }
        }
    }
}