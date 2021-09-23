using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum CalculationType
    {
        [Description("Calculate Ray Coordinates")]
        RayCoords,
        [Description("Calculate All Ray Information")]
        AllRayInfo,
        [Description("Calculate Eigenrays (use Regula Falsi)")]
        EigenraysRegFalsi,
        [Description("Calculate Eigenrays (use Proximity method)")]
        EigenraysProximity,
        [Description("Calculate Amplitudes and Delays (use Regula falsi)")]
        AmpDelayRegFalsi,
        [Description("Calculate Amplitudes and Delays (use Proximity method)")]
        AmpDelayProximity,
        [Description("Calculate Coherent acoustic PRessure")]
        CohAcousticPressure,
        [Description("Calculate Coherent Transmission Loss")]
        CohTransmissionLoss,
        [Description("Calculate coherent Particle VeLocity")]
        PartVelocity,
        [Description("Calculate Coherent acoustic Pressure And Particle velocity")]
        CohAccousicPressurePartVelocity
    }
}
