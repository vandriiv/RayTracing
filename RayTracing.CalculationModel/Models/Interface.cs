namespace RayTracing.CalculationModel.Models
{
    public class Interface
    {
        public SurfaceType SurfaceType { get; set; }

        public SurfacePropertyType SurfacePropertyType { get; set; }

        public double[] R { get; set; }                      //"rati(n)"             |

        public double[] Z { get; set; }                      //"zati(n)"              }

        public double[] Cp { get; set; }                     //"cpati",  compressional speed

        public double[] Cs { get; set; }                     //"csati",  shear speed


        public double[] Rho { get; set; }                    //"rhoati", density

        public double[] Ap { get; set; }                     //"apati",  compressional attenuation

        public double[] As { get; set; }                     //"asati"   shear attenuation

        public SurfaceInterpolation SurfaceInterpolation { get; set; }

        public AttenUnits SurfaceAttenUnits { get; set; }

        public int NumSurfaceCoords { get; set; }
    }
}
