using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using RayTracing.Web.Helpers;

namespace RayTracing.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NumericMatrixAttribute : ValidationAttribute, IClientModelValidator
    {
        public bool AllowEmpty { get; set; } = false;

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-numericmatrix", ErrorMessage);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var stringValue = (string)value;

            return AllowEmpty && string.IsNullOrEmpty(stringValue) || stringValue.IsValidNumericMatrix()
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }

        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }

            attributes.Add(key, value);
            return true;
        }
    }
}
