using RayTracing.CalculationModel.Models;
using RayTracing.Web.Attributes;
using RayTracing.Web.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace RayTracing.Web.Models
{
    public class SurfaceDetails
    {
        [Display(Name = "Surface type")]
        public SurfaceType SurfaceType { get; set; }

        [Display(Name = "Surface properties")]
        public SurfacePropertyType SurfacePropertyType { get; set; }

        [Display(Name = "Interpolation type")]
        public SurfaceInterpolation SurfaceInterpolation { get; set; }

        [Display(Name = "Attenuation units")]
        public AttenUnits SurfaceAttenUnits { get; set; }

        /// <summary>
        ///     For non homogeneous surface
        /// </summary>
        [Display(Name = "Number of surface coordinates")]
        public int NumSurfaceCoords { get; set; }

        [Display(Name = "Range coordinates")]
        public double[] R { get; set; } = Array.Empty<double>();

        [Display(Name = "Range coordinates")]
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
                return string.Join(' ', R);
            }
        }

        [Display(Name = "Depth coordinates")]
        public double[] Z { get; set; } = Array.Empty<double>();

        [Display(Name = "Depth coordinates")]
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
                return string.Join(' ', Z);
            }
        }

        /// <summary>
        ///     Compressional speed
        /// </summary>
        [Display(Name = "Compressional speed")]
        public double[] Cp { get; set; } = Array.Empty<double>();

        [Display(Name = "Compressional speed")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string CpList
        {
            set
            {
                Cp = value.ToDoubleArray();
            }
            get
            {
                return string.Join(' ', Cp);
            }
        }

        /// <summary>
        ///     Shear speed
        /// </summary>
        [Display(Name = "Shear speed")]
        public double[] Cs { get; set; } = Array.Empty<double>();

        [Display(Name = "Shear speed")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string CsList
        {
            set
            {
                Cs = value.ToDoubleArray();
            }
            get
            {
                return string.Join(' ', Cs);
            }
        }

        /// <summary>
        ///     Density
        /// </summary>
        [Display(Name = "Density")]
        public double[] Rho { get; set; } = Array.Empty<double>();

        [Display(Name = "Density")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string RhoList
        {
            set
            {
                Rho = value.ToDoubleArray();
            }
            get
            {
                return string.Join(' ', Rho);
            }
        }

        /// <summary>
        ///     Compressional attenuation
        /// </summary>
        [Display(Name = "Compressional attenuation")]
        public double[] Ap { get; set; } = Array.Empty<double>();

        [Display(Name = "Compressional attenuation")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string ApList
        {
            set
            {
                Ap = value.ToDoubleArray();
            }
            get
            {
                return string.Join(' ', Ap);
            }
        }

        /// <summary>
        ///     Shear attenuation
        /// </summary>
        [Display(Name = "Shear attenuation")]
        public double[] As { get; set; } = Array.Empty<double>();

        [Display(Name = "Shear attenuation")]
        [Required]
        [NumericArray(ErrorMessage = "Please provide valid numerical values")]
        public string AsList
        {
            set
            {
                As = value.ToDoubleArray();
            }
            get
            {
                return string.Join(' ', As);
            }
        }
    }
}
