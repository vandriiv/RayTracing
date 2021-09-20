using System.Numerics;

namespace RayTracing.CalculationModel.Models
{
    public class ArrivalDetails
    {
        public double Theta { get; set; }

        public double[] R { get; set; }

        public double[] Z { get; set; }

        public double[] Tau { get; set; }

        public Complex[] Amp { get; set; }

        public bool IReturns { get; set; }

        public int NSurRefl { get; set; }

        public int NBotRefl { get; set; }

        public int NObjRefl { get; set; }
    }
}
