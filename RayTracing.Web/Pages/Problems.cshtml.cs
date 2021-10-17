using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RayTracing.Web.Models.Problems;

namespace RayTracing.Web.Pages
{
    public class ProblemsModel : PageModel
    {
        private readonly CommonProblems _commonProblems;

        public ProblemsModel(CommonProblems commonProblems)
        {
            _commonProblems = commonProblems;
        }

        public IReadOnlyList<(string Name, string Key)> Problems { get; set; }

        public void OnGet()
        {
            Problems = _commonProblems.AvailableProblems();
        }
    }
}
