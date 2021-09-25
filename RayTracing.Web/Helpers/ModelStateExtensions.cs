using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace RayTracing.Web.Helpers
{
    public static class ModelStateExtensions
    {
        public static void AddModelErrors(
            this ModelStateDictionary modelState,
            IReadOnlyDictionary<string, List<string>> validationErrors)
        {
            foreach (var resultError in validationErrors)
            {
                foreach (var value in resultError.Value)
                {
                    modelState.AddModelError(resultError.Key, value);
                }
            }
        }

        public static void AddModelErrors(
            this ModelStateDictionary modelState,
            IEnumerable<string> errors)
        {
            foreach (var resultError in errors)
            {
                modelState.AddModelError(string.Empty, resultError);
            }
        }
    }
}
