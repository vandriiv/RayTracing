namespace RayTracing.CalculationModel.Models
{
    public class Options
    {
        public bool KillBackscatteredRays { get; set; }  //command line switch 

        public int NBackscatteredRays { get; set; }//a counter for the number of rays truncated due to the --killBackscatteredRays switch

        public string CaseTitle { get; set; }

        public bool SaveSSP { get; set; }

        public int NSSPPoints { get; set; }
    }
}
