namespace RayTracing.CalculationModel.Models
{
    public class Settings
    {
        public Source Source { get; set; }

        public Interface Altimetry { get; set; }

        public SoundSpeed SoundSpeed { get; set; }

        public Objects Objects { get; set; }

        public Output Output { get; set; }

        public Options Options { get; set; }

        public Interface Batimetry { get; set; }

        public Settings()
        {
            Source = new Source();
            Altimetry = new Interface();
            Batimetry = new Interface();
            Options = new Options();
            Objects = new Objects();
            SoundSpeed = new SoundSpeed();
            Output = new Output();
        }
    }
}
