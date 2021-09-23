using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RayTracing.CalculationModel.Models;
using RayTracing.Web.Helpers;
using RayTracing.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace RayTracing.Web.Pages
{
    public class IndexModel : PageModel
    {

        public IEnumerable<NameIdModel> ArrayTypes { get; private set; }

        public IEnumerable<NameIdModel> AttenUnits { get; private set; }

        public IEnumerable<NameIdModel> CalculationTypes { get; private set; }

        public IEnumerable<NameIdModel> SoundSpeedClasses { get; private set; }

        public IEnumerable<NameIdModel> SoundSpeedDistributions { get; private set; }

        public IEnumerable<NameIdModel> SurfaceInterpolations { get; private set; }

        public IEnumerable<NameIdModel> SurfacePropetyTypes { get; private set; }

        public IEnumerable<NameIdModel> SurfaceTypes { get; private set; }

        [BindProperty]
        public AcousticProblemDescription AcousticProblem { get; set; } = new AcousticProblemDescription();

        public IndexModel()
        {
        }

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

            return Page();
        }

        public IActionResult OnPost()
        {
            return Page();
        }
    }
}
