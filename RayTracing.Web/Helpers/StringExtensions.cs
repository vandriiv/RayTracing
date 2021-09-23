using System;
using System.Linq;

namespace RayTracing.Web.Helpers
{
    public static class StringExtensions
    {
        public static double[] ToDoubleArray(this string value)
        {
            return value
                    .Split(' ')
                    .Select(str =>
                    {
                        bool success = double.TryParse(str, out double value);
                        return new { Value = value, Success = success };
                    })
                    .Where(pair => pair.Success)
                    .Select(pair => pair.Value)
                    .ToArray();
        }

        public static bool IsValidNumericArray(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Split(' ').All(v => double.TryParse(v, out double result));
        }

        public static double [,] ToDoubleMatrix(this string value)
        {
            var rows = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var minColsCount = rows.Min(r => r.ToDoubleArray().Length);

            var result = new double[rows.Length, minColsCount];
            for (var i = 0; i < rows.Length; i++)
            {
                var singleRow = rows[i].ToDoubleArray();
                for (var j = 0; j < singleRow.Length; j++)
                {
                    result[i,j] = singleRow[j];
                }
            }

            return result;
        }

        public static bool IsValidNumericMatrix(this string value)
        {
            var rows = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return rows.All(v => v.IsValidNumericArray());
        }
    }
}
