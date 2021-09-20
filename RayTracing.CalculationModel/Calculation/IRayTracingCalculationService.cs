using RayTracing.CalculationModel.Models;

namespace RayTracing.CalculationModel.Calculation
{
    interface IRayTracingCalculationService
    {
        CalculationResult Calculate(Settings settings);

        CalculationResult CalcSSP(Settings settings);
    }
}
