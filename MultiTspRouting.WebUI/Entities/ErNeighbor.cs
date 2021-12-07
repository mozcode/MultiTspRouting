using System.Collections.Generic;
using System.Linq;

namespace MultiTspRouting.WebUI.Entities
{
    public class ErNeighbor
    {
        public ErNeighbor()
        {
            NeighborCount = 4;
            CommonCount = 0;
            Neighbors = new List<int>();
        }

        public ErNeighbor(int indexValue)
        {
            IndexValue = indexValue;
            NeighborCount = 4;
            CommonCount = 0;
            Neighbors = new List<int>();
        }

        public int IndexValue { get; set; }

        public int CommonCount { get; set; }

        public int NeighborCount { get; set; }

        public List<int> Neighbors { get; set; }

        public void AddNeighborIfNotExistInFirst2Index(int neighborValue)
        {
            Neighbors.Add(neighborValue);

            for (var index = 0; index < 2; index++)
            {
                if (Neighbors[index] == neighborValue)
                {
                    CommonCount++;
                }
            }
        }

        public void DeleteNeighbor(int neighborValue)
        {
            int neighborCount = Neighbors.Count(x => x == neighborValue);

            if (neighborCount > 0)
            {
                NeighborCount--;

                if (neighborCount > 1)
                {
                    CommonCount--;
                    NeighborCount--;
                }

                Neighbors.RemoveAll(x => x == neighborValue);
            }
        }
    }
}
