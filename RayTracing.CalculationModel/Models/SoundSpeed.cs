namespace RayTracing.CalculationModel.Models
{
    public class SoundSpeed
    {
        public SoundSpeedDistribution CDist { get; set; }

        public SoundSpeedClass CClass { get; set; }

        public int Nr { get; set; }

        public int Nz { get; set; }

        public double[] R { get; set; }

        public double[] Z { get; set; }

        public double[] C1D { get; set; }

        public double[,] C2D { get; set; }
    }
}
