using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RayTracing.CalculationModel.Calculation;
using RayTracing.CalculationModel.Common;
using RayTracing.CalculationModel.Models;
using RayTracing.Web.Helpers;
using RayTracing.Web.Models;
using RayTracing.Web.Models.Mappers;
using RayTracing.Web.Models.Problems;
using RayTracing.Web.Models.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace RayTracing.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IRayTracingCalculationService _rayTracingCalculationService;
        private readonly IValidator<AcousticProblemDescription> _validator;
        private readonly CommonProblems _commonProblems;

        public IndexModel(
            IRayTracingCalculationService rayTracingCalculationService,
            IValidator<AcousticProblemDescription> validator,
            CommonProblems commonProblems)
        {
            _rayTracingCalculationService = rayTracingCalculationService;
            _validator = validator;
            _commonProblems = commonProblems;
        }

        public IEnumerable<NameIdModel> ArrayTypes { get; private set; }

        public IEnumerable<NameIdModel> AttenUnits { get; private set; }

        public IEnumerable<NameIdModel> CalculationTypes { get; private set; }

        public IEnumerable<NameIdModel> SoundSpeedClasses { get; private set; }

        public IEnumerable<NameIdModel> SoundSpeedDistributions { get; private set; }

        public IEnumerable<NameIdModel> SurfaceInterpolations { get; private set; }

        public IEnumerable<NameIdModel> SurfacePropetyTypes { get; private set; }

        public IEnumerable<NameIdModel> SurfaceTypes { get; private set; }

        [BindProperty]
        public AcousticProblemDescription AcousticProblem { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Problem { get; set; }

        [BindProperty]
        public IFormFile InputFile { get; set; }

        public IActionResult OnGet()
        {
            ArrayTypes = EnumUtils.GetValues<ArrayType>().Select(x => x.ToNameIdModel());
            AttenUnits = EnumUtils.GetValues<AttenUnits>().Select(x => x.ToNameIdModel());
            CalculationTypes = EnumUtils.GetValues<CalculationType>().Select(x => x.ToNameIdModel());
            SoundSpeedClasses = EnumUtils.GetValues<SoundSpeedClass>().Select(x => x.ToNameIdModel());
            SoundSpeedDistributions = EnumUtils.GetValues<SoundSpeedDistribution>().Select(x => x.ToNameIdModel());
            SurfaceInterpolations = EnumUtils.GetValues<SurfaceInterpolation>().Select(x => x.ToNameIdModel());
            SurfacePropetyTypes = EnumUtils.GetValues<SurfacePropertyType>().Select(x => x.ToNameIdModel());
            SurfaceTypes = EnumUtils.GetValues<SurfaceType>().Select(x => x.ToNameIdModel());

            if (TempData.ContainsKey(nameof(AcousticProblem)))
            {
                AcousticProblem = JsonConvert.DeserializeObject<AcousticProblemDescription>((string)TempData[nameof(AcousticProblem)]);
            }
            else if (AcousticProblem == null)
            {
                AcousticProblem = _commonProblems.GetAcousticProblem(Problem) ?? new AcousticProblemDescription();
            }

            return Page();
        }

        public IActionResult OnPostSubmit()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationErrors = _validator.Validate(AcousticProblem, nameof(AcousticProblem));
            if (validationErrors.Any())
            {
                ModelState.AddModelErrors(validationErrors);

                return BadRequest(ModelState);
            }

            try
            {
                var calulationResult = _rayTracingCalculationService.Calculate(AcousticProblem.ToCalculationModelInput());

                var mappedResult = MapCalculationResult(calulationResult, AcousticProblem);
                calulationResult = null;

                return new JsonResult(mappedResult);
            }
            catch(CalculationException calculationException)
            {
                Console.WriteLine(calculationException.StackTrace);
                return BadRequest(new { ErrorMessage = calculationException.Message });
            }
        }

        public IActionResult OnPostSerialize()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationErrors = _validator.Validate(AcousticProblem, nameof(AcousticProblem));
            if (validationErrors.Any())
            {
                ModelState.AddModelErrors(validationErrors);

                return BadRequest(ModelState);
            }

            return Content(JsonConvert.SerializeObject(AcousticProblem));
        }

        public async Task<IActionResult> OnPostFileUpload()
        {
            ModelState.Clear();

            if (InputFile == null || InputFile.Length == 0)
            {
                ModelState.AddModelError("", "File is not provided");
                return OnGet();
            }

            if (Path.GetExtension(InputFile.FileName).ToLower() != ".json")
            {
                ModelState.AddModelError("", "Only JSON files allowed");
                return OnGet();
            }

            using (var reader = new StreamReader(InputFile.OpenReadStream()))
            {
                var content = await reader.ReadToEndAsync();

                try
                {
                    var acousticProblem = JsonConvert.DeserializeObject<AcousticProblemDescription>(content);
                    TempData[nameof(AcousticProblem)] = JsonConvert.SerializeObject(acousticProblem);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "File content can't be converted to problem description.");
                    return OnGet();
                }
            }

            return RedirectToPage();
        }

        private object MapCalculationResult(CalculationResult calculationResult, AcousticProblemDescription acousticProblem)
        {
            return acousticProblem.CalculationType switch
            {
                CalculationType.RayCoords => new
                {
                    Rays = calculationResult.Rays.Select(r => new { r.R, r.Z }),
                    RangeStart = Math.Min(acousticProblem.Altimetry.R.Min(), acousticProblem.Batimetry.R.Min()),
                    RangeEnd = Math.Max(acousticProblem.Altimetry.R.Max(), acousticProblem.Batimetry.R.Max()),
                    DepthStart = Math.Min(acousticProblem.Altimetry.Z.Min(), acousticProblem.Batimetry.Z.Min()),
                    DepthEnd = Math.Max(acousticProblem.Altimetry.Z.Max(), acousticProblem.Batimetry.Z.Max()),
                    CalculationType = acousticProblem.CalculationType.ToString(),
                    SurfaceR = acousticProblem.Altimetry.R,
                    SurfaceZ = acousticProblem.Altimetry.Z,
                    BottomR = acousticProblem.Batimetry.R,
                    BottomZ = acousticProblem.Batimetry.Z,
                    SourceR = acousticProblem.SourceR,
                    SourceZ = acousticProblem.SourceZ
                },
                var x when (x == CalculationType.EigenraysProximity || x == CalculationType.EigenraysRegFalsi) => new
                {
                    Eigenrays = MapEigenreysResultToResponseObject(calculationResult.Eigenrays, calculationResult.HydrophoneR, calculationResult.HydrophoneZ),
                    HydrophoneRanges = acousticProblem.HydrophoneRanges,
                    HydrophoneDepths = acousticProblem.HydrophoneDepths,
                    RangeStart = Math.Min(acousticProblem.Altimetry.R.Min(), acousticProblem.Batimetry.R.Min()),
                    RangeEnd = Math.Max(acousticProblem.Altimetry.R.Max(), acousticProblem.Batimetry.R.Max()),
                    DepthStart = Math.Min(acousticProblem.Altimetry.Z.Min(), acousticProblem.Batimetry.Z.Min()),
                    DepthEnd = Math.Max(acousticProblem.Altimetry.Z.Max(), acousticProblem.Batimetry.Z.Max()),
                    CalculationType = acousticProblem.CalculationType.ToString(),
                    SurfaceR = acousticProblem.Altimetry.R,
                    SurfaceZ = acousticProblem.Altimetry.Z,
                    BottomR = acousticProblem.Batimetry.R,
                    BottomZ = acousticProblem.Batimetry.Z,
                    SourceR = acousticProblem.SourceR,
                    SourceZ = acousticProblem.SourceZ
                },
                CalculationType.CohTransmissionLoss => new
                {
                    ArrayType = acousticProblem.HydrophoneArrayType.ToString(),
                    TL = calculationResult.TL,
                    ArrayR = calculationResult.HydrophoneR,
                    ArrayZ = calculationResult.HydrophoneZ,
                    RangeStart = Math.Min(acousticProblem.Altimetry.R.Min(), acousticProblem.Batimetry.R.Min()),
                    RangeEnd = Math.Max(acousticProblem.Altimetry.R.Max(), acousticProblem.Batimetry.R.Max()),
                    DepthStart = Math.Min(acousticProblem.Altimetry.Z.Min(), acousticProblem.Batimetry.Z.Min()),
                    DepthEnd = Math.Max(acousticProblem.Altimetry.Z.Max(), acousticProblem.Batimetry.Z.Max()),
                    CalculationType = acousticProblem.CalculationType.ToString(),
                    SurfaceR = acousticProblem.Altimetry.R,
                    SurfaceZ = acousticProblem.Altimetry.Z,
                    BottomR = acousticProblem.Batimetry.R,
                    BottomZ = acousticProblem.Batimetry.Z,
                    SourceR = acousticProblem.SourceR,
                    SourceZ = acousticProblem.SourceZ
                },
                var x when (x == CalculationType.AmpDelayRegFalsi || x == CalculationType.AmpDelayProximity) => new
                {
                    Arrivals = MapArrivalsResultToResponseObject(calculationResult.Arrivals, calculationResult.HydrophoneR, calculationResult.HydrophoneZ),
                    HydrophoneRanges = calculationResult.HydrophoneR,
                    HydrophoneDepths = calculationResult.HydrophoneZ,
                    RangeStart = Math.Min(acousticProblem.Altimetry.R.Min(), acousticProblem.Batimetry.R.Min()),
                    RangeEnd = Math.Max(acousticProblem.Altimetry.R.Max(), acousticProblem.Batimetry.R.Max()),
                    DepthStart = Math.Min(acousticProblem.Altimetry.Z.Min(), acousticProblem.Batimetry.Z.Min()),
                    DepthEnd = Math.Max(acousticProblem.Altimetry.Z.Max(), acousticProblem.Batimetry.Z.Max()),
                    CalculationType = acousticProblem.CalculationType.ToString(),
                    SurfaceR = acousticProblem.Altimetry.R,
                    SurfaceZ = acousticProblem.Altimetry.Z,
                    BottomR = acousticProblem.Batimetry.R,
                    BottomZ = acousticProblem.Batimetry.Z,
                    SourceR = acousticProblem.SourceR,
                    SourceZ = acousticProblem.SourceZ
                },
                CalculationType.SoundSpeedProfile => new
                {
                    SSPC = calculationResult.SSPC,
                    SSPZ = calculationResult.SSPZ,
                    RangeStart = Math.Min(acousticProblem.Altimetry.R.Min(), acousticProblem.Batimetry.R.Min()),
                    RangeEnd = Math.Max(acousticProblem.Altimetry.R.Max(), acousticProblem.Batimetry.R.Max()),
                    DepthStart = Math.Min(acousticProblem.Altimetry.Z.Min(), acousticProblem.Batimetry.Z.Min()),
                    DepthEnd = Math.Max(acousticProblem.Altimetry.Z.Max(), acousticProblem.Batimetry.Z.Max()),
                    CalculationType = acousticProblem.CalculationType.ToString(),
                    SurfaceR = acousticProblem.Altimetry.R,
                    SurfaceZ = acousticProblem.Altimetry.Z,
                    BottomR = acousticProblem.Batimetry.R,
                    BottomZ = acousticProblem.Batimetry.Z,
                    SourceR = acousticProblem.SourceR,
                    SourceZ = acousticProblem.SourceZ
                },
                CalculationType.CohAcousticPressure => new
                {
                    ArrayType = acousticProblem.HydrophoneArrayType.ToString(),
                    Pressure = calculationResult.Pressure2D,
                    PressureTL = MapToTLRepresentation(calculationResult.Pressure2D),
                    ArrayR = calculationResult.HydrophoneR,
                    ArrayZ = calculationResult.HydrophoneZ,
                    RangeStart = Math.Min(acousticProblem.Altimetry.R.Min(), acousticProblem.Batimetry.R.Min()),
                    RangeEnd = Math.Max(acousticProblem.Altimetry.R.Max(), acousticProblem.Batimetry.R.Max()),
                    DepthStart = Math.Min(acousticProblem.Altimetry.Z.Min(), acousticProblem.Batimetry.Z.Min()),
                    DepthEnd = Math.Max(acousticProblem.Altimetry.Z.Max(), acousticProblem.Batimetry.Z.Max()),
                    CalculationType = acousticProblem.CalculationType.ToString(),
                    SurfaceR = acousticProblem.Altimetry.R,
                    SurfaceZ = acousticProblem.Altimetry.Z,
                    BottomR = acousticProblem.Batimetry.R,
                    BottomZ = acousticProblem.Batimetry.Z,
                    SourceR = acousticProblem.SourceR,
                    SourceZ = acousticProblem.SourceZ
                },
                CalculationType.PartVelocity => new
                {
                    ArrayType = acousticProblem.HydrophoneArrayType.ToString(),
                    U = calculationResult.U,
                    UTL = MapToTLRepresentation(calculationResult.U),
                    W = calculationResult.W,
                    WTL = MapToTLRepresentation(calculationResult.W),
                    ArrayR = calculationResult.HydrophoneR,
                    ArrayZ = calculationResult.HydrophoneZ,
                    RangeStart = Math.Min(acousticProblem.Altimetry.R.Min(), acousticProblem.Batimetry.R.Min()),
                    RangeEnd = Math.Max(acousticProblem.Altimetry.R.Max(), acousticProblem.Batimetry.R.Max()),
                    DepthStart = Math.Min(acousticProblem.Altimetry.Z.Min(), acousticProblem.Batimetry.Z.Min()),
                    DepthEnd = Math.Max(acousticProblem.Altimetry.Z.Max(), acousticProblem.Batimetry.Z.Max()),
                    CalculationType = acousticProblem.CalculationType.ToString(),
                    SurfaceR = acousticProblem.Altimetry.R,
                    SurfaceZ = acousticProblem.Altimetry.Z,
                    BottomR = acousticProblem.Batimetry.R,
                    BottomZ = acousticProblem.Batimetry.Z,
                    SourceR = acousticProblem.SourceR,
                    SourceZ = acousticProblem.SourceZ
                },
                CalculationType.CohAccousicPressurePartVelocity => new
                {
                    ArrayType = acousticProblem.HydrophoneArrayType.ToString(),
                    Pressure = calculationResult.Pressure2D,
                    PressureTL = MapToTLRepresentation(calculationResult.Pressure2D),
                    U = calculationResult.U,
                    UTL = MapToTLRepresentation(calculationResult.U),
                    W = calculationResult.W,
                    WTL = MapToTLRepresentation(calculationResult.W),
                    ArrayR = calculationResult.HydrophoneR,
                    ArrayZ = calculationResult.HydrophoneZ,
                    RangeStart = Math.Min(acousticProblem.Altimetry.R.Min(), acousticProblem.Batimetry.R.Min()),
                    RangeEnd = Math.Max(acousticProblem.Altimetry.R.Max(), acousticProblem.Batimetry.R.Max()),
                    DepthStart = Math.Min(acousticProblem.Altimetry.Z.Min(), acousticProblem.Batimetry.Z.Min()),
                    DepthEnd = Math.Max(acousticProblem.Altimetry.Z.Max(), acousticProblem.Batimetry.Z.Max()),
                    CalculationType = acousticProblem.CalculationType.ToString(),
                    SurfaceR = acousticProblem.Altimetry.R,
                    SurfaceZ = acousticProblem.Altimetry.Z,
                    BottomR = acousticProblem.Batimetry.R,
                    BottomZ = acousticProblem.Batimetry.Z,
                    SourceR = acousticProblem.SourceR,
                    SourceZ = acousticProblem.SourceZ
                },
                _ => new { }
            };
        }

        private object[,] MapEigenreysResultToResponseObject(Eigenrays[,] eigenrays, double[] hydrophoneR, double[] hydrophoneZ)
        {
            var rowsCount = eigenrays.GetLength(0);
            var colsCount = eigenrays.GetLength(1);

            var result = new object[rowsCount, colsCount];

            for (var i = 0; i < rowsCount; i++)
            {
                for (var j = 0; j < colsCount; j++)
                {
                    var eigenraysTmp = new List<object>(eigenrays[i, j].Eigenray.Count);
                    for (var k = 0; k < eigenrays[i, j].Eigenray.Count; k++)
                    {
                        eigenraysTmp.Add(new
                        {
                            eigenrays[i, j].Eigenray[k].R,
                            eigenrays[i, j].Eigenray[k].Z
                        });
                    }


                    result[i, j] = new
                    {
                        Rays = eigenraysTmp
                    };
                }
            }


            return result;
        }

        private object[,] MapArrivalsResultToResponseObject(Arrivals[,] arrivals, double[] hydrophoneR, double[] hydrophoneZ)
        {
            var rowsCount = arrivals.GetLength(0);
            var colsCount = arrivals.GetLength(1);

            var result = new object[rowsCount, colsCount];

            for (var i = 0; i < rowsCount; i++)
            {
                for (var j = 0; j < colsCount; j++)
                {
                    var arrivalsTmp = new List<object>(arrivals[i, j].Arrival.Count);
                    for (var k = 0; k < arrivals[i, j].Arrival.Count; k++)
                    {
                        arrivalsTmp.Add(new
                        {
                            Amp = Complex.Abs(arrivals[i, j].Arrival[k].Amp),
                            arrivals[i, j].Arrival[k].Tau
                        });
                    }

                    result[i, j] = new
                    {
                        Arrivals = arrivalsTmp
                    };
                }
            }

            return result;
        }

        private double[,] MapToTLRepresentation(Complex[,] source)
        {
            var rowsCount = source.GetLength(0);
            var colsCount = source.GetLength(1);

            var result = new double[rowsCount, colsCount];
            for (var i = 0; i < rowsCount; i++)
            {
                for (var j = 0; j < colsCount; j++)
                {
                    result[i, j] = -20 * Math.Log10(Complex.Abs(source[i, j]));
                }
            }

            return result;
        }
    }
}
