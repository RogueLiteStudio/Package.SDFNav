using System.Collections.Generic;
using System.IO;

namespace TSDFNav
{
    [System.Serializable]
    public class SDFScene
    {
        public SDFData Data = new SDFData();
        public List<DynamicObstacle> Obstacles = new List<DynamicObstacle>();
        public List<LimitAreaShape> Areas = new List<LimitAreaShape>();
        public HeightMapData HeightMap = new HeightMapData();

        public void Write(BinaryWriter writer)
        {
            Data.Write(writer);
            writer.Write((short)Obstacles.Count);
            foreach (var ob in Obstacles)
            {
                ob.Write(writer);
            }
            writer.Write((short)Areas.Count);
            foreach (var area in Areas)
            {
                area.Write(writer);
            }
            HeightMap.Write(writer);
        }
        public void Read(BinaryReader reader)
        {
            Data.Read(reader);
            if (reader.PeekChar() < 0)
                return;
            int len = reader.ReadInt16();
            Obstacles.Capacity = len;
            for (int i=0; i<len; ++i)
            {
                DynamicObstacle ob = new DynamicObstacle();
                ob.Read(reader);
                Obstacles.Add(ob);
            }
            if (IsEnd(reader))
                return;
            len = reader.ReadInt16();
            Areas.Capacity = len;
            for (int i = 0; i < len; ++i)
            {
                LimitAreaShape area = new LimitAreaShape();
                area.Read(reader);
                Areas.Add(area);
            }
            if (IsEnd(reader))
                return;
            HeightMap.Read(reader);
        }

        public static bool IsEnd(BinaryReader reader)
        {
            return reader.BaseStream.Position == reader.BaseStream.Length;
        }

        public void Read(byte[] data)
        {
            using(MemoryStream memory = new MemoryStream(data))
            {
                using(BinaryReader reader = new BinaryReader(memory))
                {
                    Read(reader);
                }
            }
        }
    }
}