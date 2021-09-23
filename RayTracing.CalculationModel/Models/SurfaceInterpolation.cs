using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum SurfaceInterpolation
    {
        [Description("Flat surface")]
        Flat,
        [Description("Surface with a slope")]
        Sloped,
        [Description("Piecewise linear interpolation")]
        Linear, //2p
        [Description("Piecewise cubic interpolation")]
        Cubic //4p
    }
}
