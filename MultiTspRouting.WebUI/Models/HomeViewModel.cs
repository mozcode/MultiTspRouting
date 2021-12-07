using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MultiTspRouting.WebUI.Entities;

namespace MultiTspRouting.WebUI.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            GaEnvironments = new GaEnvironments();
            //TargetLat = "40,99970";
            //TargetLng = "28,82113";
            //TargetLat = "40,99896772312974";
            //TargetLng = "28,773122766616815";
            //TargetLat = "40,998915073714";
            //TargetLng = "28,773548741968";
            TargetLat = "41,0323128909";
            TargetLng = "28,8482325813";
            VehicleMaxCapacity = 7;
        }

        public GaEnvironments GaEnvironments { get; set; }

        public IFormFile CoordinateFile { get; set; }

        public List<Node> Nodes { get; set; }

        public List<List<Node>> PartialNodes { get; set; }

        public Route BestRoute { get; set; }

        public Node MapCenter { get; set; }

        public string TargetLat { get; set; }

        public string TargetLng { get; set; }

        public int MapZoom { get; set; }

        public Results Results { get; set; }

        public int VehicleMaxCapacity { get; set; }

        public List<CostViewModel> CostViewModels { get; set; }

        public double[,] DistanceMatrix { get; set; }
    }
}
