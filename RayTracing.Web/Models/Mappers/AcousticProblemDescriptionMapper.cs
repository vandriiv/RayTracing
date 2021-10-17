using RayTracing.CalculationModel.Models;

namespace RayTracing.Web.Models.Mappers
{
    public static class AcousticProblemDescriptionMapper
    {
        public static Settings ToCalculationModelInput(this AcousticProblemDescription problemDescription)
        {
            var settings = new Settings();

            settings.Source.Ds = problemDescription.RayStep;
            settings.Source.Rx = problemDescription.SourceR;
            settings.Source.Zx = problemDescription.SourceZ;
            settings.Source.Freqx = problemDescription.Frequency;
            settings.Source.Rbox1 = problemDescription.RangeBoxStart;
            settings.Source.Rbox2 = problemDescription.RangeBoxEnd;
            settings.Source.NThetas = problemDescription.NumberOfThetas;
            settings.Source.Thetas = problemDescription.Thetas;
            settings.Source.DTheta = GetThetasStep(problemDescription.Thetas);

            settings.Altimetry.Ap = problemDescription.Altimetry.Ap;
            settings.Altimetry.As = problemDescription.Altimetry.As;
            settings.Altimetry.Cp = problemDescription.Altimetry.Cp;
            settings.Altimetry.Cs = problemDescription.Altimetry.Cs;
            settings.Altimetry.NumSurfaceCoords = problemDescription.Altimetry.NumSurfaceCoords;
            settings.Altimetry.R = problemDescription.Altimetry.R;
            settings.Altimetry.Rho = problemDescription.Altimetry.Rho;
            settings.Altimetry.SurfaceAttenUnits = problemDescription.Altimetry.SurfaceAttenUnits;
            settings.Altimetry.SurfaceInterpolation = problemDescription.Altimetry.SurfaceInterpolation;
            settings.Altimetry.SurfacePropertyType = problemDescription.Altimetry.SurfacePropertyType;
            settings.Altimetry.SurfaceType = problemDescription.Altimetry.SurfaceType;
            settings.Altimetry.Z = problemDescription.Altimetry.Z;

            settings.Batimetry.Ap = problemDescription.Batimetry.Ap;
            settings.Batimetry.As = problemDescription.Batimetry.As;
            settings.Batimetry.Cp = problemDescription.Batimetry.Cp;
            settings.Batimetry.Cs = problemDescription.Batimetry.Cs;
            settings.Batimetry.NumSurfaceCoords = problemDescription.Batimetry.NumSurfaceCoords;
            settings.Batimetry.R = problemDescription.Batimetry.R;
            settings.Batimetry.Rho = problemDescription.Batimetry.Rho;
            settings.Batimetry.SurfaceAttenUnits = problemDescription.Batimetry.SurfaceAttenUnits;
            settings.Batimetry.SurfaceInterpolation = problemDescription.Batimetry.SurfaceInterpolation;
            settings.Batimetry.SurfacePropertyType = problemDescription.Batimetry.SurfacePropertyType;
            settings.Batimetry.SurfaceType = problemDescription.Batimetry.SurfaceType;
            settings.Batimetry.Z = problemDescription.Batimetry.Z;

            settings.SoundSpeed.C1D = problemDescription.SoundSpeed.C1D;
            settings.SoundSpeed.C2D = problemDescription.SoundSpeed.C2D;
            settings.SoundSpeed.CClass = problemDescription.SoundSpeed.SoundSpeedClass;
            settings.SoundSpeed.CDist = problemDescription.SoundSpeed.SoundSpeedDistribution;
            settings.SoundSpeed.Nr = problemDescription.SoundSpeed.NumberOfPointsInRange;
            settings.SoundSpeed.Nz = problemDescription.SoundSpeed.NumberOfPointsInDepth;
            settings.SoundSpeed.R = problemDescription.SoundSpeed.R;
            settings.SoundSpeed.Z = problemDescription.SoundSpeed.Z;

            settings.Options.KillBackscatteredRays = false;
            settings.Options.NSSPPoints = problemDescription.NumberOfSSPPoints <=0 ? 100 : problemDescription.NumberOfSSPPoints;

            settings.Output.CalculationType = problemDescription.CalculationType;
            settings.Output.ArrayType = problemDescription.HydrophoneArrayType;
            settings.Output.NArrayR = problemDescription.NumberOfHydrophonesAlongRange;
            settings.Output.NArrayZ = problemDescription.NumberOfHydrophonesAlongDepth;
            settings.Output.ArrayR = problemDescription.HydrophoneRanges;
            settings.Output.ArrayZ = problemDescription.HydrophoneDepths;
            settings.Output.Miss = problemDescription.Threshold;

            return settings;
        }

        private static double GetThetasStep(double[] thetas)
        {
            if (thetas.Length <= 1)
            {
                return 0;
            }
            else
            {
                return thetas[1] - thetas[0];
            }
        }
    }
}
