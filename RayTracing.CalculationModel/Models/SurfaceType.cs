using System.ComponentModel;

namespace RayTracing.CalculationModel.Models
{
    public enum SurfaceType
    {
        [Description("Absorbent surface")]
        Absorvent,
        [Description("Elastic surface")]
        Elastic,
        [Description("Rigid surface")]
        Rigid,
        [Description("Vacuum over surface")]
        Vacuum
    }
}
