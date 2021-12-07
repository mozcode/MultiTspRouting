namespace MultiTspRouting.WebUI.Models
{
    public class Results
    {
        public int TotalIteration { get; set; }

        public double BiggestCost { get; set; }

        public int LastBestIteration { get; set; }

        public string ExecutionTime { get; set; }
    }
}
