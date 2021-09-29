using RayTracing.CalculationModel.Models;
using System.Collections.Generic;

namespace RayTracing.Web.Models.Problems
{
    public class CommonProblems
    {
        private readonly IDictionary<string, AcousticProblemDescription> _acousticProblems;

        public CommonProblems()
        {
            _acousticProblems = new Dictionary<string, AcousticProblemDescription>();

            _acousticProblems.TryAdd(nameof(PekerisFlat).ToLower(), PekerisFlat());
        }

        public IReadOnlyList<(string Name, string Key)> AvailableProblems()
        {
            return new List<(string Name, string Key)>
            {
                ("Pekeris flat waveguide", nameof(PekerisFlat).ToLower())
            }
            .AsReadOnly();
        }

        public AcousticProblemDescription GetAcousticProblem(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            if (_acousticProblems.TryGetValue(key, out var description))
            {
                return description;
            }

            return null;
        }

        private AcousticProblemDescription PekerisFlat()
        {
            var description = new AcousticProblemDescription
            {
                RayStep = 1.1,
                SourceR = 0,
                SourceZ = 25,
                RangeBoxStart = -1,
                RangeBoxEnd = 1101,
                Frequency = 717,
                NumberOfThetas = 6,
                Thetas = new double[] { -30, -15, -5, 5, 15, 30 }
            };

            description.Altimetry.SurfaceType = SurfaceType.Vacuum;
            description.Altimetry.SurfacePropertyType = SurfacePropertyType.Homogeneous;
            description.Altimetry.SurfaceInterpolation = SurfaceInterpolation.Flat;
            description.Altimetry.SurfaceAttenUnits = AttenUnits.dBperLambda;
            description.Altimetry.NumSurfaceCoords = 2;
            description.Altimetry.Cp = new double[1] { 0 };
            description.Altimetry.Cs = new double[1] { 0 };
            description.Altimetry.Rho = new double[1] { 0 };
            description.Altimetry.Ap = new double[1] { 0 };
            description.Altimetry.As = new double[1] { 0 };

            description.Altimetry.R = new double[] { -2, 1102 };
            description.Altimetry.Z = new double[] { 0, 0 };

            description.SoundSpeed.SoundSpeedDistribution = SoundSpeedDistribution.Profile;
            description.SoundSpeed.SoundSpeedClass = SoundSpeedClass.Isovelocity;
            description.SoundSpeed.NumberOfPointsInRange = 1;
            description.SoundSpeed.NumberOfPointsInDepth = 2;

            description.SoundSpeed.Z = new double[2] { 0, 100 };
            description.SoundSpeed.C1D = new double[2] { 1500, 1500 };

            description.Batimetry.SurfaceType = SurfaceType.Elastic;
            description.Batimetry.SurfacePropertyType = SurfacePropertyType.Homogeneous;
            description.Batimetry.SurfaceInterpolation = SurfaceInterpolation.Flat;
            description.Batimetry.SurfaceAttenUnits = AttenUnits.dBperLambda;
            description.Batimetry.NumSurfaceCoords = 2;
            description.Batimetry.Cp = new double[1] { 1700 };
            description.Batimetry.Cs = new double[1] { 0 };
            description.Batimetry.Rho = new double[1] { 1.7 };
            description.Batimetry.Ap = new double[1] { 0.7 };
            description.Batimetry.As = new double[1] { 0 };

            description.Batimetry.R = new double[] { -2, 1102 };
            description.Batimetry.Z = new double[] { 100, 100 };

            description.HydrophoneArrayType = ArrayType.Vertical;
            description.NumberOfHydrophonesAlongRange = 1;
            description.NumberOfHydrophonesAlogDepth = 1;

            description.HydrophoneRanges = new double[] { 1000 };
            description.HydrophoneDepths = new double[] { 75 };

            description.CalculationType = CalculationType.RayCoords;
            description.Threshold = 1;

            return description;
        }
    }
}
