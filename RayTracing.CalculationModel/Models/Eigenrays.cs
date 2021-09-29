using System.Collections.Generic;

namespace RayTracing.CalculationModel.Models
{
    public class Eigenrays
    {
        public int NEigenrays { get; set; }

        public IList<EigenrayDetails> Eigenray { get; set; } = new List<EigenrayDetails>();
    }
}
