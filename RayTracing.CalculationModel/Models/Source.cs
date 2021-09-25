namespace RayTracing.CalculationModel.Models
{
    public class Source
    {
        public double Ds { get; set; }             //ray step

        public double Rx { get; set; }          //source coords

        public double Zx { get; set; }

        public double Rbox1 { get; set; }   //the box that limits the range of the rays

        public double Rbox2 { get; set; }

        public double Freqx { get; set; }          //source frequency

        public int NThetas { get; set; }        //number of launching angles

        public double DTheta { get; set; }         //the increment between launching angles

        public double[] Thetas { get; set; }         //the array that will actually contain the launching angles
    }
}
