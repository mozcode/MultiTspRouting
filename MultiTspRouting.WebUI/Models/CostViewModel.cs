namespace MultiTspRouting.WebUI.Models
{
    public class CostViewModel
    {
        public CostViewModel(int iterationNumber, double totalCost)
        {
            IterationNumber = iterationNumber;
            TotalCost = totalCost;
        }

        public int IterationNumber { get; set; }

        public double TotalCost { get; set; }
    }
}
