using System.Collections.Generic;

namespace RayTracing.Web.Models.Validators
{
    public interface IValidator<T>
    {
        IEnumerable<string> Validate(T model);
    }
}
