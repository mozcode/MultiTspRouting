namespace MultiTspRouting.WebUI.Entities
{
    public class Node
    {
        public Node(int number = 0, double latitude = 0, double longitude = 0)
        {
            No = number;
            Lat = latitude;
            Lng = longitude;
        }

        public int No { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
