using RayTracing.CalculationModel.Models;
using System.ComponentModel.DataAnnotations;

namespace RayTracing.Web.Models
{
    public class AcousticProblemDescription
    {
        public double RayStep { get; set; }

        public double SourceR { get; set; }

        public double SourceZ { get; set; }

        public double Frequency { get; set; }

        public double RangeBoxStart { get; set; }

        public double RangeBoxEnd { get; set; }

        public bool CalculateThetas { get; set; }

        public int NumberOfThetas { get; set; }

        public double Theta0 { get; set; }

        public double ThetaN { get; set; }

        public double[] Thetas { get; set; }

        [Required]
        public SurfaceDetails Altimetry { get; set; }

        [Required]
        public SoundSpeedDetails SoundSpeed { get; set; }

        [Required]
        public SurfaceDetails Batimetry { get; set; }

        public CalculationType CalculationType { get; set; }

        public ArrayType HydrophoneArrayType { get; set; }

        public double[] HydrophoneRanges { get; set; }

        public double[] HydrophoneDepths { get; set; }

        public double Threshold { get; set; }
    }
}
