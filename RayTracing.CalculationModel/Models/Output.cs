using System.Numerics;

namespace RayTracing.CalculationModel.Models
{
    public class Output
    {
        public int NArrayR { get; set; }

        public int NArrayZ { get; set; }

        public CalculationType CalculationType { get; set; }

        public ArrayType ArrayType { get; set; }

        public double[] ArrayR { get; set; }

        public double[] ArrayZ { get; set; }

        public Complex[,] Pressure2D { get; set; }

        public Complex[,,] PressureH { get; set; }

        public Complex[,,] PressureV { get; set; }

        public double Dr { get; set; }

        public double Dz { get; set; }

        public double Miss { get; set; }
    }
}
