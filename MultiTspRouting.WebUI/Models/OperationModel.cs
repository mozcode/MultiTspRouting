using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MultiTspRouting.WebUI.Entities;

namespace MultiTspRouting.WebUI.Models
{
    public class OperationModel
    {
        public GaEnvironments GaEnvironments { get; set; }

        public int[] NodeArray { get; set; }
    }
}
