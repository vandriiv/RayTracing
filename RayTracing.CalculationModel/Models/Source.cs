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

        public bool AutocalculateThetas { get; set; }

        public int NThetas { get; set; }        //number of launching angles

        public double Theta0 { get; set; } //first and last launching angle

        public double ThetaN { get; set; }

        public double DTheta { get; set; }         //the increment between launching angles

        public double[] Thetas { get; set; }         //the array that will actually contain the launching angles

        public void CalculateThetas()
        {
            if (!AutocalculateThetas)
            {
                return;
            }

            if (NThetas == 1)
            {
                Thetas[0] = Theta0;
                DTheta = 0;
            }
            else if (NThetas == 2)
            {
                Thetas[0] = Theta0;
                Thetas[1] = ThetaN;
                DTheta = ThetaN - Theta0;
            }
            else
            {
                Thetas[0] = Theta0;
                Thetas[NThetas - 1] = ThetaN;
                DTheta = (ThetaN - Theta0) / ((double)NThetas - 1);

                for (var i = 1; i <= NThetas - 2; i++)
                {
                    Thetas[i] = Theta0 + DTheta * (i);
                }
            }
        }
    }
}
