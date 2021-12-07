using System.Collections.Generic;
using System.Linq;

namespace MultiTspRouting.WebUI.Entities
{
    public class Population
    {
        public Population(List<Route> routes)
        {
            Routes = routes.OrderBy(x => x.BiggestCost).ToList();

            BestCost = Routes[0].BiggestCost;
        }

        public double BestCost { get; set; }

        public List<Route> Routes { get; set; }
    }
}
