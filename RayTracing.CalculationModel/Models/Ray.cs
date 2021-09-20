using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RayTracing.CalculationModel.Models
{
    public class Ray
    {
        public int NCoords { get; set; }

        public bool IKill { get; set; }    //indicates if ray has been "killed"

        public double Theta { get; set; }    //launching angle of the ray

        public double RMin { get; set; } //used to determine if a ray "turns back"

        public double RMax { get; set; }

        public bool IReturn { get; set; }    //indicates if a ray "turns back"

        public double[] R { get; set; }        //range of ray at index

        public double[] Z { get; set; }        //depth of ray at index

        public double[] C { get; set; }        //speed of sound at index

        public bool[] IRefl { get; set; }    //indicates if there is a reflection at a certain index of the ray coordinates.

        public int SRefl { get; set; }    //number of surface reflections

        public int BRefl { get; set; }    //number of bottom reflections

        public int ORefl { get; set; }    //number of object reflections

        public int NRefl { get; set; }    //number of total reflections

        public Complex[] Decay { get; set; }    //decay of ray

        public double[] Phase { get; set; }    //ray phase

        public double[] Tau { get; set; }      //acumulated travel time

        public double[] S { get; set; }        //acumulated distance travelled by the ray

        public double[] Ic { get; set; }       //see Chapter 3 of Traceo Manual

        public Vector[] BoundaryTg { get; set; } //"tbdry" a boundary's tangent vector

        public int[] BoundaryJ { get; set; }//"jbdry",  indicates at what boundary a ray is (-1 => above surface; 1 => below bottom)

        public int NRefrac { get; set; }  //"nrefr", number of refraction points

        public double[] RRefrac { get; set; }

        public double[] ZRefrac { get; set; }

        public double[] P { get; set; }        //used in solveDynamicEq

        public double[] Q { get; set; }        //used in solveDynamicEq

        public double[] Caustc { get; set; }   //used in solveDynamicEq

        public Complex[] Amp { get; set; }      //ray amplitude

        public void Init(int n)
        {
            NCoords = n;
            R = new double[n];
            Z = new double[n];
            C = new double[n];
            IRefl = new bool[n];
            Decay = new Complex[n];
            Phase = new double[n];
            Tau = new double[n];
            S = new double[n];
            Ic = new double[n];

            BoundaryTg = new Vector[n];
            for (var i = 0; i< n; i++)
            {
                BoundaryTg[i] = new Vector();
            }

            BoundaryJ = new int[n];
            RRefrac = new double[n];
            ZRefrac = new double[n];
            P = new double[n];
            Q = new double[n];
            Caustc = new double[n];
            Amp = new Complex[n];
        }

        public void DecreaseSize(int n)
        {
            if (R.Length < n)
            {
                return;
            }

            NCoords = n;
            R = R.Take(n).ToArray();
            Z = Z.Take(n).ToArray();
            C = C.Take(n).ToArray();
            IRefl = IRefl.Take(n).ToArray();
            Decay = Decay.Take(n).ToArray();
            Phase = Phase.Take(n).ToArray();
            Tau = Tau.Take(n).ToArray();
            S = S.Take(n).ToArray();
            Ic = Ic.Take(n).ToArray();
            BoundaryTg = BoundaryTg.Take(n).ToArray();
            BoundaryJ = BoundaryJ.Take(n).ToArray();
            RRefrac = RRefrac.Take(n).ToArray();
            ZRefrac = ZRefrac.Take(n).ToArray();
            P = P.Take(n).ToArray();
            Q = Q.Take(n).ToArray();
            Caustc = Caustc.Take(n).ToArray();
            Amp = Amp.Take(n).ToArray();
        }
    }
}
