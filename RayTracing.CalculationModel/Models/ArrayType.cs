using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum ArrayType
    {
        [Description("Rectangular")]
        Rectangular,
        [Description("Horizontal")]
        Horizontal,
        [Description("Vertical")]
        Vertical,
        [Description("Linear")]
        Linear
    }
}
