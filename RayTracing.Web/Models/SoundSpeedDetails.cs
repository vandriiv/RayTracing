using RayTracing.CalculationModel.Models;

namespace RayTracing.Web.Models
{
    public class SoundSpeedDetails
    {
        public SoundSpeedDistribution SoundSpeedDistribution { get; set; }

        public SoundSpeedClass SoundSpeedClass { get; set; }

        public int NumberOfPointsInRange { get; set; }

        public int NumberOfPointsInDepth { get; set; }

        public double[] R { get; set; }

        public double[] Z { get; set; }

        /// <summary>
        ///     Sound speed at (z0)
        /// </summary>
        public double[] C1D { get; set; }

        /// <summary>
        ///     Sound speed at (r0, z0)
        /// </summary>
        public double[,] C2D { get; set; }
    }
}
