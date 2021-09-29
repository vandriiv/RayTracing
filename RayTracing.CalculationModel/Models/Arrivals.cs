using System.Collections.Generic;

namespace RayTracing.CalculationModel.Models
{
    public class Arrivals
    {
        public int NArrivals { get; set; }

        public IList<ArrivalDetails> Arrival { get; set; } = new List<ArrivalDetails>();
    }
}
