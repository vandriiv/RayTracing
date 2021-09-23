using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum AttenUnits
    {
        [Description("dB/kHz")]
        dBperkHz,
        [Description("dB/meter")]
        dBperMeter,
        [Description("dB/neper")]
        dBperNeper,
        [Description("Q factor")]
        qFactor,
        [Description("dB/λ")]
        dBperLambda
    }
}
