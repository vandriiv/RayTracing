using System;

namespace RayTracing.CalculationModel.Common
{
    public class CalculationException : Exception
    {
        public string ParamName { get; }

        public CalculationException(string message) : base(message)
        {
        }

        public CalculationException(string message, string paramName) : base(message)
        {
            ParamName = paramName;
        }
    }
}
