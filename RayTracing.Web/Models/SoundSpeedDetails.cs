using RayTracing.CalculationModel.Models;
using RayTracing.Web.Attributes;
using RayTracing.Web.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace RayTracing.Web.Models
{
    public class SoundSpeedDetails
    {
        [Display(Name = "Type of sound speed distribution")]
        public SoundSpeedDistribution SoundSpeedDistribution { get; set; }

        [Display(Name = "Class of sound speed")]
        public SoundSpeedClass SoundSpeedClass { get; set; }

        [Display(Name = "Number of points in range")]
        public int NumberOfPointsInRange { get; set; }

        [Display(Name = "Number of points in depth")]
        public int NumberOfPointsInDepth { get; set; }

        [Display(Name = "Range points")]
        public double[] R { get; set; } = Array.Empty<double>();

        [Display(Name = "Range points")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string RList
        {
            set
            {
                R = value.ToDoubleArray();
            }
            get
            {
                return R.ToSpaceSeparatedString();
            }
        }

        [Display(Name = "Depth points")]
        public double[] Z { get; set; } = Array.Empty<double>();

        [Display(Name = "Depth points")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string ZList
        {
            set
            {
                Z = value.ToDoubleArray();
            }
            get
            {
                return Z.ToSpaceSeparatedString();
            }
        }

        /// <summary>
        ///     Sound speed at (z0)
        /// </summary>
        public double[] C1D { get; set; } = Array.Empty<double>();

        [Display(Name = "Sound speed at (z0)")]
        [NumericArray(AllowEmpty = true, ErrorMessage = "Please provide valid numerical values")]
        public string C1DList
        {
            set
            {
                C1D = value.ToDoubleArray();
            }
            get
            {
                return C1D.ToSpaceSeparatedString();
            }
        }

        /// <summary>
        ///     Sound speed at (r0, z0)
        /// </summary>
        [Display(Name = "Sound speed at (r0,z0)")]
        public double[,] C2D { get; set; } = new double[0, 0];

        [Display(Name = "Sound speed at (r0,z0)")]
        [NumericMatrix(AllowEmpty = true, ErrorMessage = "Please provide valid numerical values")]
        public string C2DList
        {
            set
            {
                C2D = value.ToDoubleMatrix();
            }
            get
            {
                return C2D.ToSpaceSeparatedString();
            }
        }
    }
}
