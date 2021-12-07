using System;
using System.Diagnostics;
using System.Linq;
using MultiTspRouting.WebUI.Class;

namespace MultiTspRouting.WebUI.Entities
{
    public class Route
    {
        public Route(int[] nodeArray, int breakCount)
        {
            BiggestCost = 0;
            NodeArray = nodeArray;
            PartialCosts = new double[breakCount + 1];

            CalculateNodeArrayCost(breakCount);
        }

        public int[] NodeArray { get; set; }

        public double BiggestCost { get; set; }

        public double[] PartialCosts { get; set; }

        void CalculateNodeArrayCost(int breakCount)
        {
            void SetBiggestCost(double newCost)
            {
                if (BiggestCost < newCost)
                {
                    BiggestCost = newCost;
                }
            }

            int j = 0;
            int i = 0;

            for (; i <= breakCount; i++)
            {
                int partLength = Tools.GetNodePartLengthByPartNumber(NodeArray, i + 1, breakCount);

                PartialCosts[i] = Tools.SumDistanceOfNodeArrayInGivenInterval(NodeArray, j, partLength + j - 1);

                j += partLength;

                SetBiggestCost(PartialCosts[i]);
            }
        }
    }
}