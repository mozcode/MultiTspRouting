namespace MultiTspRouting.WebUI.Entities
{
    public class GaEnvironments
    {
        public GaEnvironments()
        {
            PopulationSize = 500;
            Elitism = 1;
            MutationRate = 0.02;
            TwoOptRate = 0.02;
            IterationNumber = 500;
            IterationTerminationPercent = 50;
            RandomIntegerCount = 3;
        }

        public int PopulationSize { get; set; }

        public int Elitism { get; set; }

        public double MutationRate { get; set; }

        public double TwoOptRate { get; set; }

        public int IterationNumber { get; set; }

        public int IterationTerminationPercent { get; set; }

        public int RandomIntegerCount { get; set; }
    }
}