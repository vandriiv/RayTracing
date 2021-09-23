using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum SurfacePropertyType
    {
        [Description("Homogeneous surface")]
        Homogeneous,
        [Description("Non-homogeneous surface")]
        NonHomogeneous
    }
}
