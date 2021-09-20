namespace RayTracing.CalculationModel.Models
{
    public class Object
    {
        public SurfaceType SurfaceType { get; set; }

        public AttenUnits SurfaceAttenUnits { get; set; }

        public double Cp { get; set; }

        public double Cs { get; set; }

        public double Rho { get; set; }

        public double Ap { get; set; }

        public double As { get; set; }

        public int NCoords { get; set; }

        public double[] R { get; set; }

        public double[] ZUp { get; set; }

        public double[] ZDown { get; set; }
    }
}
