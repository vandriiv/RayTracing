using RayTracing.CalculationModel.Models;

namespace RayTracing.Web.Models
{
    public class SurfaceDetails
    {
        public SurfaceType SurfaceType { get; set; }

        public SurfacePropertyType SurfacePropertyType { get; set; }

        public double[] R { get; set; }

        public double[] Z { get; set; }

        /// <summary>
        ///     Compressional speed
        /// </summary>
        public double[] Cp { get; set; }

        /// <summary>
        ///     Shear speed
        /// </summary>
        public double[] Cs { get; set; }

        /// <summary>
        ///     Density
        /// </summary>
        public double[] Rho { get; set; }

        /// <summary>
        ///     Compressional attenuation
        /// </summary>
        public double[] Ap { get; set; }

        /// <summary>
        ///     Shear attenuation
        /// </summary>
        public double[] As { get; set; }

        public SurfaceInterpolation SurfaceInterpolation { get; set; }

        public AttenUnits SurfaceAttenUnits { get; set; }

        /// <summary>
        ///     For non homogeneous surface
        /// </summary>
        public int NumSurfaceCoords { get; set; }
    }
}
