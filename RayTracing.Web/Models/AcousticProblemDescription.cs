using RayTracing.CalculationModel.Models;
using RayTracing.Web.Attributes;
using RayTracing.Web.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace RayTracing.Web.Models
{
    public class AcousticProblemDescription
    {
        [Display(Name = "Number of SSP points")]
        public int NumberOfSSPPoints { get; set; }

        [Display(Name = "Ray step")]
        public double RayStep { get; set; }

        [Display(Name = "Source coordinate (r)")]
        public double SourceR { get; set; }

        [Display(Name = "Source coordinate (z)")]
        public double SourceZ { get; set; }

        [Display(Name = "Source frequency")]
        public double Frequency { get; set; }

        [Display(Name = "Range box (start)")]
        public double RangeBoxStart { get; set; }

        [Display(Name = "Range box (end)")]
        public double RangeBoxEnd { get; set; }

        [Display(Name = "Number of launching angles")]
        public int NumberOfThetas { get; set; }

        [Display(Name = "First launching angle")]
        public double Theta0 { get; set; }

        [Display(Name = "Last launching angle")]
        public double ThetaN { get; set; }

        [Display(Name = "Launching angles")]
        public double[] Thetas { get; set; } = Array.Empty<double>();

        [Display(Name = "Launching angles")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string ThetasList
        {
            set
            {
                Thetas = value.ToDoubleArray();
            }
            get
            {
                return Thetas.ToSpaceSeparatedString();
            }
        }

        [Required]
        public SurfaceDetails Altimetry { get; set; } = new SurfaceDetails();

        [Required]
        public SoundSpeedDetails SoundSpeed { get; set; } = new SoundSpeedDetails();

        [Required]
        public SurfaceDetails Batimetry { get; set; } = new SurfaceDetails();

        [Display(Name = "Number of hydrophones along range")]
        public int NumberOfHydrophonesAlongRange { get; set; }

        [Display(Name = "Number of hydrophones along depth")]
        public int NumberOfHydrophonesAlongDepth { get; set; }

        [Display(Name = "Array type")]
        public ArrayType HydrophoneArrayType { get; set; }

        [Display(Name = "Hydrophones ranges")]
        public double[] HydrophoneRanges { get; set; } = Array.Empty<double>();

        [Display(Name = "Hydrophones ranges")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string HydrophoneRangesList
        {
            set
            {
                HydrophoneRanges = value.ToDoubleArray();
            }
            get
            {
                return HydrophoneRanges.ToSpaceSeparatedString();
            }
        }

        [Display(Name = "Hydrophones depths")]
        public double[] HydrophoneDepths { get; set; } = Array.Empty<double>();

        [Display(Name = "Hydrophones depths")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string HydrophoneDepthsList
        {
            set
            {
                HydrophoneDepths = value.ToDoubleArray();
            }
            get
            {
                return HydrophoneDepths.ToSpaceSeparatedString();
            }
        }

        [Display(Name = "Calculation type")]
        public CalculationType CalculationType { get; set; }

        [Display(Name = "Threshold (Eigenray parameter)")]
        public double Threshold { get; set; }
    }
}
