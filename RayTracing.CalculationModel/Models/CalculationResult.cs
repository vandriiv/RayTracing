using System.Numerics;

namespace RayTracing.CalculationModel.Models
{
    public class CalculationResult
    {
        public int MaxNumArrivals { get; set; }

        public int MaxNumEigenrays { get; set; }

        public double[] SSPC { get; set; }

        public double[] SSPZ { get; set; }

        public uint NBackscatteredRays { get; set; }

        public Ray[] Rays { get; set; }

        public double[] Thethas { get; set; }

        public Complex[,] W { get; set; }

        public Complex[,] U { get; set; }

        public Eigenrays[,] Eigenrays { get; set; }

        public double[] HydrophoneZ { get; set; }

        public double[] HydrophoneR { get; set; }

        public double[] TL { get; set; }

        public double[,] TL2D { get; set; }

        public Complex[,] Pressure2D { get; set; }

        public Arrivals[,] Arrivals { get; set; }

        public double SourceZ { get; set; }
    }
}
