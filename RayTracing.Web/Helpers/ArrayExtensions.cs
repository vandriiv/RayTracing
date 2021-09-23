using System;
using System.Text;

namespace RayTracing.Web.Helpers
{
    public static class ArrayExtensions
    {
        public static string ToSpaceSeparatedString<T> (this T[] arr)
        {
            return string.Join(' ', arr);
        }

        public static string ToSpaceSeparatedString<T>(this T[,] arr)
        {
            var rowsCount = arr.GetLength(0);
            var colsCount = arr.GetLength(1);

            var stringBuilder = new StringBuilder();
            for (var i = 0; i < rowsCount; i++)
            {
                for (var j = 0; j < colsCount; j++)
                {
                    stringBuilder.Append(arr[i, j]);

                    if (j != colsCount - 1)
                    {
                        stringBuilder.Append(' ');
                    }
                }

                if (i != rowsCount - 1)
                {
                    stringBuilder.Append(Environment.NewLine);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
