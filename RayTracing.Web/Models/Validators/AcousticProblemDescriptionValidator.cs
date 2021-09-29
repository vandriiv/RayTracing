using System.Collections.Generic;

namespace RayTracing.Web.Models.Validators
{
    public class AcousticProblemDescriptionValidator : IValidator<AcousticProblemDescription>
    {
        public IReadOnlyDictionary<string, List<string>> Validate(AcousticProblemDescription model, string modelFieldName = "")
        {
            var template = modelFieldName + ".{0}";
            var nestedFieldTemplate = modelFieldName + ".{0}.{1}";

            var validationErrors = new Dictionary<string, List<string>>();

            if (model.Frequency <= 0)
            {
                validationErrors.Add(string.Format(template, nameof(model.Frequency)), new List<string> { "Frequency should be > 0" });
            }

            if (model.NumberOfThetas <= 0)
            {
                validationErrors.Add(string.Format(template, nameof(model.NumberOfThetas)), new List<string> { "Number of launching angles should be > 0" });
            }

            if (model.Thetas.Length != model.NumberOfThetas)
            {
                validationErrors.Add(string.Format(template, nameof(model.Thetas)), new List<string> { "Launching angles count != provided number of launching angles" });
            }

            if (model.Altimetry.NumSurfaceCoords <= 0)
            {
                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.NumSurfaceCoords)), new List<string> { "Number of surface coordinates should be > 0" });
            }

            if (model.Altimetry.R.Length != model.Altimetry.NumSurfaceCoords)
            {
                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.R)), new List<string> { "Range coordinates count != number of surface coordinates" });
            }

            if (model.Altimetry.Z.Length != model.Altimetry.NumSurfaceCoords)
            {
                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.Z)), new List<string> { "Depth coordinates count != number of surface coordinates" });
            }

            if (model.Altimetry.SurfacePropertyType == CalculationModel.Models.SurfacePropertyType.Homogeneous)
            {
                if (model.Altimetry.Cp.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.Cp)), new List<string> { "Compressional speed count for Homogeneous surface should be 1" });
                }

                if(model.Altimetry.Cs.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.Cs)), new List<string> { "Shear speed count for Homogeneous surface should be 1" });
                }

                if(model.Altimetry.Ap.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.Ap)), new List<string> { "Compressional attenuation count for Homogeneous surface should be 1" });
                }

                if (model.Altimetry.As.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.As)), new List<string> { "Shear attenuation count for Homogeneous surface should be 1" });
                }

                if (model.Altimetry.Rho.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.Rho)), new List<string> { "Density count for Homogeneous surface should be 1" });
                }
            }
            else
            {
                if (model.Altimetry.Cp.Length != model.Altimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.Cp)), new List<string> { "Compressional speed count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }

                if (model.Altimetry.Cs.Length != model.Altimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.Cs)), new List<string> { "Shear speed count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }

                if (model.Altimetry.Ap.Length != model.Altimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.Ap)), new List<string> { "Compressional attenuation count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }

                if (model.Altimetry.As.Length != model.Altimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.As)), new List<string> { "Shear attenuation count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }

                if (model.Altimetry.Rho.Length != model.Altimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Altimetry), nameof(model.Altimetry.Rho)), new List<string> { "Density count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }
            }

            if (model.SoundSpeed.NumberOfPointsInRange <= 0 && model.SoundSpeed.SoundSpeedDistribution != CalculationModel.Models.SoundSpeedDistribution.Profile)
            {
                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.NumberOfPointsInRange)), new List<string> { "Number of points in range should be > 0" });
            }

            if (model.SoundSpeed.NumberOfPointsInDepth <= 0)
            {
                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.NumberOfPointsInDepth)), new List<string> { "Number of points in depth should be > 0" });
            }

            if (model.SoundSpeed.SoundSpeedDistribution == CalculationModel.Models.SoundSpeedDistribution.Profile)
            {
                if (model.SoundSpeed.SoundSpeedClass != CalculationModel.Models.SoundSpeedClass.Tabulated)
                {
                    if (model.SoundSpeed.Z.Length != 2)
                    {
                        validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.Z)), new List<string> { "For non-tabulated sound speed class only required ( z0(0),c0(0) ) and ( z0(1), c0(1) )" });
                    }

                    if (model.SoundSpeed.C1D.Length != 2)
                    {
                        validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.C1D)), new List<string> { "For non-tabulated sound speed class only required ( z0(0),c0(0) ) and ( z0(1), c0(1) )" });
                    }

                    if (model.SoundSpeed.SoundSpeedClass != CalculationModel.Models.SoundSpeedClass.Munk &&
                        model.SoundSpeed.SoundSpeedClass != CalculationModel.Models.SoundSpeedClass.Isovelocity)
                    {
                        if (model.SoundSpeed.Z.Length > 1 && model.SoundSpeed.Z[0] == model.SoundSpeed.Z[1])
                        {
                            if (validationErrors.TryGetValue(nameof(model.SoundSpeed.Z), out var zErrors))
                            {
                                zErrors.Add("z[1] == z[0] only valid for Isovelocity and Munk Options");
                            }
                            else
                            {
                                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.Z)), new List<string> { "z[1] == z[0] only valid for Isovelocity and Munk Options" });
                            }
                        }

                        if (model.SoundSpeed.C1D.Length > 1 && model.SoundSpeed.C1D[0] == model.SoundSpeed.C1D[1])
                        {
                            if (validationErrors.TryGetValue(nameof(model.SoundSpeed.C1D), out var c1DErrors))
                            {
                                c1DErrors.Add("c[1] == c[0] only valid for Isovelocity and Munk Options");
                            }
                            else
                            {
                                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.C1D)), new List<string> { "c[1] == c[0] only valid for Isovelocity and Munk Options" });
                            }
                        }
                    }
                }
                else
                {
                    if (model.SoundSpeed.Z.Length != model.SoundSpeed.NumberOfPointsInDepth)
                    {
                        validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.Z)), new List<string> { "Depth points count should be equals to provided number of points in depth" });
                    }

                    if (model.SoundSpeed.C1D.Length != model.SoundSpeed.NumberOfPointsInDepth)
                    {
                        validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.C1D)), new List<string> { "Sound speed count should be equals to provided number of points in depth" });
                    }
                }
            }
            else
            {
                if (model.SoundSpeed.SoundSpeedClass != CalculationModel.Models.SoundSpeedClass.Tabulated)
                {
                    validationErrors.Add(
                        string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.SoundSpeedDistribution)),
                        new List<string> { "Tabulated sound speed class is only possible for 'Field' distribution" });
                }
                else
                {
                    if (model.SoundSpeed.R.Length != model.SoundSpeed.NumberOfPointsInRange)
                    {
                        validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.R)), new List<string> { "Range points count should be equals to provided number of points in depth" });
                    }
                    
                    if (model.SoundSpeed.Z.Length != model.SoundSpeed.NumberOfPointsInDepth)
                    {
                        validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.Z)), new List<string> { "Depth points count should be equals to provided number of points in depth" });
                    }
                    else if (model.SoundSpeed.C2D.GetLength(0) != model.SoundSpeed.NumberOfPointsInDepth ||
                        model.SoundSpeed.C2D.GetLength(1) != model.SoundSpeed.NumberOfPointsInRange)
                    {
                        validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.SoundSpeed), nameof(model.SoundSpeed.Z)), new List<string> { "Sound speed matrix is not valid" });
                    }
                }
            }

            if (model.Batimetry.NumSurfaceCoords <= 0)
            {
                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.NumSurfaceCoords)), new List<string> { "Number of surface coordinates should be > 0" });
            }

            if (model.Batimetry.R.Length != model.Batimetry.NumSurfaceCoords)
            {
                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.R)), new List<string> { "Range coordinates count != number of surface coordinates" });
            }

            if (model.Batimetry.Z.Length != model.Batimetry.NumSurfaceCoords)
            {
                validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.Z)), new List<string> { "Depth coordinates count != number of surface coordinates" });
            }

            if (model.Batimetry.SurfacePropertyType == CalculationModel.Models.SurfacePropertyType.Homogeneous)
            {
                if (model.Batimetry.Cp.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.Cp)), new List<string> { "Compressional speed count for Homogeneous surface should be 1" });
                }

                if (model.Batimetry.Cs.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.Cs)), new List<string> { "Shear speed count for Homogeneous surface should be 1" });
                }

                if (model.Batimetry.Ap.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.Ap)), new List<string> { "Compressional attenuation count for Homogeneous surface should be 1" });
                }

                if (model.Batimetry.As.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.As)), new List<string> { "Shear attenuation count for Homogeneous surface should be 1" });
                }

                if (model.Batimetry.Rho.Length != 1)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.Rho)), new List<string> { "Density count for Homogeneous surface should be 1" });
                }
            }
            else
            {
                if (model.Batimetry.Cp.Length != model.Batimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.Cp)), new List<string> { "Compressional speed count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }

                if (model.Batimetry.Cs.Length != model.Batimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.Cs)), new List<string> { "Shear speed count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }

                if (model.Batimetry.Ap.Length != model.Batimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.Ap)), new List<string> { "Compressional attenuation count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }

                if (model.Batimetry.As.Length != model.Batimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.As)), new List<string> { "Shear attenuation count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }

                if (model.Batimetry.Rho.Length != model.Batimetry.NumSurfaceCoords)
                {
                    validationErrors.Add(string.Format(nestedFieldTemplate, nameof(model.Batimetry), nameof(model.Batimetry.Rho)), new List<string> { "Density count for Non-homogeneous surface should be equals to provided number of surface coordinates" });
                }
            }

            return validationErrors;
        }
    }
}

