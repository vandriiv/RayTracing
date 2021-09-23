using System;
using System.Collections.Generic;

namespace RayTracing.Web.Models.Validators
{
    public class AcousticProblemDescriptionValidator : IValidator<AcousticProblemDescription>
    {
        public IEnumerable<string> Validate(AcousticProblemDescription model)
        {
            var validationErrors = new List<string>();

            return validationErrors;
        }
    }
}
