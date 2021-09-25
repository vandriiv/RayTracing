using System.Collections.Generic;

namespace RayTracing.Web.Models.Validators
{
    public interface IValidator<T>
    {
        IReadOnlyDictionary<string, List<string>> Validate(T model, string modelFieldName = "");
    }
}
