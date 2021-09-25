using RayTracing.CalculationModel.Models;

namespace RayTracing.Web.Models.Mappers
{
    public static class AcousticProblemDescriptionMapper
    {
        public static Settings ToCalculationModelInput(this AcousticProblemDescription problemDescription)
        {
            return new Settings();
        }
    }
}
