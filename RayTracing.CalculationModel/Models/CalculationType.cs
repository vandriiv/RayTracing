using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum CalculationType
    {
        [Description("Calculate Ray Coordinates")]
        RayCoords,
        [Description("Calculate All Ray Information")]
        AllRayInfo,
        [Description("Calculate Eigenrays (Regula Falsi)")]
        EigenraysRegFalsi,
        [Description("Calculate Eigenrays (Proximity method)")]
        EigenraysProximity,
        [Description("Calculate Amplitudes and Delays (Regula falsi)")]
        AmpDelayRegFalsi,
        [Description("Calculate Amplitudes and Delays (Proximity method)")]
        AmpDelayProximity,
        [Description("Calculate Coherent acoustic Pressure")]
        CohAcousticPressure,
        [Description("Calculate Coherent Transmission Loss")]
        CohTransmissionLoss,
        [Description("Calculate coherent Particle VeLocity")]
        PartVelocity,
        [Description("Calculate Coherent acoustic pressure And particle velocity")]
        CohAccousicPressurePartVelocity,
        [Description("Sound speed profile")]
        SoundSpeedProfile
    }
}
