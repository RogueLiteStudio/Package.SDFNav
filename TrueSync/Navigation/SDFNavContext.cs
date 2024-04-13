using System.Collections.Generic;

namespace TSDFNav
{
    public class SDFNavContext
    {
        public SDFRuntimeScene SDFMap;
        public PathFinder PathFinder;

        //GC优化用临时数据，每次使用的时候清理下
        public MoveDirectionRange MoveBlock = new MoveDirectionRange();
        public List<NeighborAgentInfo> Neighbors = new List<NeighborAgentInfo>();//

        public void Init(SDFRuntimeScene data)
        {
            SDFMap = data;
            PathFinder = new JPSPathFinder(data);
        }

        public void Clear()
        {
            MoveBlock.Clear();
            Neighbors.Clear();
        }

        public void AddNeighbor(NeighborAgentInfo neighbor)
        {
            int insertIdx = Neighbors.Count;
            for (int i = 0; i < Neighbors.Count; ++i)
            {
                if (Neighbors[i].Distance > neighbor.Distance)
                {
                    insertIdx = i;
                    break;
                }
            }
            Neighbors.Insert(insertIdx, neighbor);
        }
    }
}
