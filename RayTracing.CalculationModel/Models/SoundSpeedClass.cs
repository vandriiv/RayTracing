using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum SoundSpeedClass
    {
        [Description("Isovelocity")]
        Isovelocity,
        [Description("Linear")]
        Linear,
        [Description("Parabolic")]
        Parabolic,
        [Description("Exponential")]
        Exponential,
        [Description("n^2 linear")]
        N2Linear,
        [Description("Inverse-square gradien")]
        InvSquare,
        [Description("Munk")]
        Munk,
        [Description("Tabulated")]
        Tabulated
    }
}
