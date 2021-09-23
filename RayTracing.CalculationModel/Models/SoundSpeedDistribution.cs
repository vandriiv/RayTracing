using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum SoundSpeedDistribution
    {
        [Description("'c(z,z)' sound speed profile c = c(z)")]
        Profile,
        [Description("'c(r,z)' sound speed field c = c(r, z)")]
        Field
    }
}
