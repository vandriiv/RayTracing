using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum CalculationType
    {
        [Description("Calculate Ray Coordinates")]
        RayCoords,
        //[Description("Calculate All Ray Information")]
        //AllRayInfo,
        [Description("Calculate Eigenrays (Regula Falsi)")]
        EigenraysRegFalsi,
        [Description("Calculate Eigenrays (Proximity method)")]
        EigenraysProximity,
        [Description("Calculate Amplitudes and Delays (Regula falsi)")]
        AmpDelayRegFalsi,
        [Description("Calculate Amplitudes and Delays (Proximity method)")]
        AmpDelayProximity,
        [Description("Calculate Acoustic Pressure")]
        CohAcousticPressure,
        [Description("Calculate Transmission Loss")]
        CohTransmissionLoss,
        [Description("Calculate Particle Velocity")]
        PartVelocity,
        [Description("Calculate Acoustic Pressure And Particle Velocity")]
        CohAccousicPressurePartVelocity,
        [Description("Sound speed profile")]
        SoundSpeedProfile
    }
}
