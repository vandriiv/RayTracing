namespace RayTracing.CalculationModel.Common
{
    static public class ArrayExtensions
    {
        public static T[] GetThirdDimension<T>(this T[,,] matrix, int firstD, int secondD)
        {
            var thirdDLength = matrix.GetLength(2);
            var result = new T[thirdDLength];
            for (var i = 0; i< thirdDLength; i++)
            {
                result[i] = matrix[firstD, secondD, i];
            }

            return result;
        }

        public static T[,] Transpose<T>(this T[,] matrix)
        {
            var rows = matrix.GetLength(0);
            var columns = matrix.GetLength(1);

            var result = new T[columns, rows];

            for (var c = 0; c < columns; c++)
            {
                for (var r = 0; r < rows; r++)
                {
                    result[c, r] = matrix[r, c];
                }
            }

            return result;
        }
    }
}
