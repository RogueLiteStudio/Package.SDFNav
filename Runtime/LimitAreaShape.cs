using UnityEngine;
using System.IO;

namespace SDFNav
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
        public Vector2 Center;
        public Vector2 Size;
        public Vector2 Rotation;//{cos, sin}
        
        public float Sample(Vector2 pt)
        {
            switch (Shape)
            {
                case AreaShpeType.Circle:
                    return Size.x*0.5f - Vector2.Distance(pt, Center);
                case AreaShpeType.Box:
                    pt -= Center;
                    pt = new Vector2(Rotation.x * pt.x - Rotation.y * pt.y, Rotation.y * pt.x + Rotation.x * pt.y);
                    pt.x = Mathf.Abs(pt.x);
                    pt.y = Mathf.Abs(pt.y);
                    Vector2 d = pt - Size * 0.5f;
                    return -(Vector2.Max(d, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0));
            }
            return float.MaxValue;
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
            Center.x = reader.ReadSingle();
            Center.y = reader.ReadSingle();
            Size.x = reader.ReadSingle();
            if (Shape == AreaShpeType.Box)
            {
                Size.y = reader.ReadSingle();
                Rotation.x = reader.ReadSingle();
                Rotation.y = reader.ReadSingle();
            }
        }
    }
}