using System;
using MultiTspRouting.WebUI.Class;
using MultiTspRouting.WebUI.Models;

namespace MultiTspRouting.WebUI.Entities
{
    public class Vehicle : ICloneable
    {
        public Vehicle(int nodeCount, int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            MinCapacity = nodeCount % maxCapacity;

            if (MinCapacity == 0)
            {
                MinCapacity = MaxCapacity;
                BreakCount = nodeCount / maxCapacity - 1;
            }
            else
            {
                BreakCount = (nodeCount - 1) / maxCapacity;
            }
        }

        public int MaxCapacity { get; set; }

        public int MinCapacity { get; set; }

        public int BreakCount { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}