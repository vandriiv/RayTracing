using System;
using System.Numerics;

namespace RayTracing.CalculationModel.Models
{
    public class EigenrayDetails
    {
        public double Theta { get; set; }

        public double[] R { get; set; } = Array.Empty<double>();

        public double[] Z { get; set; } = Array.Empty<double>();

        public double[] Tau { get; set; } = Array.Empty<double>();

        public Complex[] Amp { get; set; } = Array.Empty<Complex>();

        public bool IReturns { get; set; }

        public int NSurRefl { get; set; }

        public int NBotRefl { get; set; }

        public int NObjRefl { get; set; }

        public int NRefrac { get; set; }

        public double[] RefracR { get; set; } = Array.Empty<double>();

        public double[] RefracZ { get; set; } = Array.Empty<double>();
    }
}
