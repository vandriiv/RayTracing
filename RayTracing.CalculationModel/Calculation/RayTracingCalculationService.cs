using System;
using System.Numerics;
using RayTracing.CalculationModel.Common;
using RayTracing.CalculationModel.Models;
using Vector = RayTracing.CalculationModel.Models.Vector;

namespace RayTracing.CalculationModel.Calculation
{
    public class RayTracingCalculationService : IRayTracingCalculationService
    {
        public CalculationResult Calculate(Settings settings)
        {
            CalculationResult result;
            switch (settings.Output.CalculationType)
            {
                case CalculationType.RayCoords:
                    Console.WriteLine("Calculating ray coordinates [RCO].");
                    result = CalcRayCoords(settings);
                    break;

                case CalculationType.AllRayInfo:
                    Console.WriteLine("Calculating all ray information [ARI].");
                    result = CalcAllRayInfo(settings);
                    break;

                case CalculationType.EigenraysProximity:
                    Console.WriteLine("Calculating eigenrays by proximity method [EPR].");
                    result = СalcEigenrayPr(settings);
                    break;

                case CalculationType.EigenraysRegFalsi:
                    Console.WriteLine("Calculating eigenrays by Regula Falsi Method [ERF].");
                    result = CalcEigenrayRF(settings);
                    break;

                case CalculationType.AmpDelayProximity:
                    Console.WriteLine("Calculating amplitudes and delays by Proximity Method [ADP].");
                    result = CalcAmpDelPr(settings);
                    break;

                case CalculationType.AmpDelayRegFalsi:
                    Console.WriteLine("Calculating amplitudes and delays by Regula Falsi Method [ADR].");
                    result = CalcAmpDelRF(settings);
                    break;

                case CalculationType.CohAcousticPressure:
                    Console.WriteLine("Calculating coherent acoustic pressure [CPR].");
                    result = CalcCohAcoustPress(settings);
                    break;

                case CalculationType.CohTransmissionLoss:
                    Console.WriteLine("Calculating coherent transmission loss [CTL].");
                    result = CalcCohAcoustPress(settings);
                    result = CalcCohTransLoss(settings, result);
                    break;

                case CalculationType.PartVelocity:
                    Console.WriteLine("Calculating particle velocity [PVL].");
                    result = CalcCohAcoustPress(settings);
                    result = CalcParticleVel(settings, result);
                    break;

                case CalculationType.CohAccousicPressurePartVelocity:
                    Console.WriteLine("Calculating coherent acoustic pressure and particle velocity [PAV].");
                    result = CalcCohAcoustPress(settings);
                    result = CalcParticleVel(settings, result);
                    break;
                case CalculationType.SoundSpeedProfile:
                    Console.WriteLine("Calculating sound speed profile");
                    result = CalcSSP(settings);
                    break;
                default:
                    throw new CalculationException("Unknown output option.", nameof(settings.Output.CalculationType));
            }

            CalcSSP(settings);

            return result;
        }

        private int Bracket(int n, double[] x, double xi)
        {
            int ia = 0, im, ib = n-1;

            if ((xi < x[0]) || (xi > x[n - 1]))
            {
                throw new CalculationException("The index is out of bounds");
            }

            while (ib - ia > 1)
            {
                im = (ia + ib) / 2;
                if ((xi >= x[ia]) && (xi < x[im]))
                {
                    ib = im;
                }
                else
                {
                    ia = im;
                }
            }

            return ia;
        }

        private CalculationResult СalcEigenrayPr(Settings settings)
        {
            double junkDouble = 0;
            double zRay = 0, tauRay = 0;
            Complex junkComplex = new Complex(), ampRay = new Complex();
            int nRet = 0;
            int[] iRet = new int[51];
            int maxNumEigenrays = 0;

            Eigenrays[,] eigenrays = new Eigenrays[settings.Output.NArrayR, settings.Output.NArrayZ];

            var _result = new CalculationResult
            {
                Thethas = settings.Source.Thetas,
                HydrophoneR = settings.Output.ArrayR,
                HydrophoneZ = settings.Output.ArrayZ
            };


            var rays = new Ray[settings.Source.NThetas];
            for (var i = 0; i < settings.Source.NThetas; i++)
            {
                rays[i] = new Ray();

                double thetai = -settings.Source.Thetas[i] * Math.PI / 180.0;
                rays[i].Theta = thetai;
                double ctheta = Math.Abs(Math.Cos(thetai));
                if (ctheta > 1.0e-7)
                {
                    SolveEikonalEq(settings, rays[i]);
                    SolveDynamicEq(settings, rays[i]);
                    for (var j = 0; j < settings.Output.NArrayR; j++)
                    {
                        double rHyd = settings.Output.ArrayR[j];

                        if ((rHyd >= rays[i].RMin) && (rHyd <= rays[i].RMax))
                        {
                            double zHyd;
                            double dz;
                            if (rays[i].IReturn == false)
                            {
                                int iHyd = Bracket(rays[i].NCoords, rays[i].R, rHyd);
                                IntLinear1D(rays[i].R, rays[i].Z, rHyd, ref zRay, ref junkDouble, iHyd);
                                for (var jj = 0; jj < settings.Output.NArrayZ; jj++)
                                {
                                    zHyd = settings.Output.ArrayZ[jj];
                                    dz = Math.Abs(zRay - zHyd);
                                    eigenrays[j, jj] ??= new Eigenrays();
                                    if (dz < settings.Output.Miss)
                                    {
                                        IntLinear1D(rays[i].R, rays[i].Tau, rHyd, ref tauRay, ref junkDouble, iHyd);
                                        IntComplexLinear1D(rays[i].R, rays[i].Amp, rHyd, ref ampRay, ref junkComplex, iHyd);
                                        rays[i].R[iHyd + 1] = rHyd;
                                        rays[i].Z[iHyd + 1] = zRay;
                                        rays[i].Tau[iHyd + 1] = tauRay;
                                        rays[i].Amp[iHyd + 1] = ampRay;

                                        var eigenrayDetails = new EigenrayDetails
                                        {
                                            Theta = rays[i].Theta,
                                            R = rays[i].R[..(iHyd + 2)],
                                            Z = rays[i].Z[..(iHyd + 2)],
                                            Tau = rays[i].Tau[..(iHyd + 2)],
                                            Amp = rays[i].Amp[..(iHyd + 2)],
                                            IReturns = rays[i].IReturn,
                                            NSurRefl = rays[i].SRefl,
                                            NBotRefl = rays[i].BRefl,
                                            NObjRefl = rays[i].ORefl,
                                            NRefrac = rays[i].NRefrac
                                        };

                                        if (rays[i].NRefrac > 0)
                                        {
                                            eigenrayDetails.RefracR = rays[i].RRefrac;
                                            eigenrayDetails.RefracZ = rays[i].ZRefrac;
                                        }

                                        eigenrays[j, jj].Eigenray.Add(eigenrayDetails);

                                        eigenrays[j, jj].NEigenrays += 1;
                                        maxNumEigenrays = Math.Max(eigenrays[j, jj].NEigenrays, maxNumEigenrays);
                                    }
                                }
                            }
                            else
                            {
                                EBracket(rays[i].NCoords, rays[i].R, rHyd, ref nRet, iRet);
                                for (var l = 0; l < nRet; l++)
                                {
                                    IntLinear1D(rays[i].R, rays[i].Z, rHyd, ref zRay, ref junkDouble, iRet[l]);
                                    for (var jj = 0; jj < settings.Output.NArrayZ; jj++)
                                    {
                                        zHyd = settings.Output.ArrayZ[jj];
                                        dz = Math.Abs(zRay - zHyd);
                                        eigenrays[j, jj] ??= new Eigenrays();
                                        if (dz < settings.Output.Miss)
                                        {
                                            IntLinear1D(rays[i].R, rays[i].Tau, rHyd, ref tauRay, ref junkDouble, iRet[l]);
                                            IntComplexLinear1D(rays[i].R, rays[i].Amp, (Complex)rHyd, ref ampRay, ref junkComplex, iRet[l]);
                                            rays[i].R[iRet[l] + 1] = rHyd;
                                            rays[i].Z[iRet[l] + 1] = zRay;
                                            rays[i].Tau[iRet[l] + 1] = tauRay;
                                            rays[i].Amp[iRet[l] + 1] = ampRay;

                                            var eigenrayDetails = new EigenrayDetails
                                            {
                                                Theta = rays[i].Theta,
                                                R = rays[i].R[..(iRet[l] + 1)],
                                                Z = rays[i].Z[..(iRet[l] + 1)],
                                                Tau = rays[i].Tau[..(iRet[l] + 1)],
                                                Amp = rays[i].Amp[..(iRet[l] + 1)],
                                                IReturns = rays[i].IReturn,
                                                NSurRefl = rays[i].SRefl,
                                                NBotRefl = rays[i].BRefl,
                                                NObjRefl = rays[i].ORefl,
                                                NRefrac = rays[i].NRefrac
                                            };

                                            if (rays[i].NRefrac > 0)
                                            {
                                                eigenrayDetails.RefracR = rays[i].RRefrac;
                                                eigenrayDetails.RefracZ = rays[i].ZRefrac;
                                            }

                                            eigenrays[j, jj].NEigenrays += 1;
                                            eigenrays[j, jj].Eigenray.Add(eigenrayDetails);

                                            maxNumEigenrays = Math.Max(eigenrays[j, jj].NEigenrays, maxNumEigenrays);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            _result.Rays = rays;
            _result.MaxNumEigenrays = maxNumEigenrays;
            _result.Eigenrays = eigenrays;

            return _result;
        }

        private void IntLinear1D(double[] x, double[] f, double xi, ref double fi, ref double fxi, int offset = 0)
        {
            fxi = (f[1 + offset] - f[0 + offset]) / (x[1 + offset] - x[0 + offset]);
            fi = f[0 + offset] + (xi - x[0 + offset]) * fxi;
        }

        private CalculationResult CalcEigenrayRF(Settings settings)
        {
            double thetai, ctheta;
            int i, j, k, l, nRays;
            int nPossibleEigenRays;
            double zRay = 0;
            double junkDouble = 0;
            int nTrial;
            double theta0 = 0, f0;
            double fl, fr, prod;
            double[] thetaL;
            double[] thetaR;
            Ray tempRay = new Ray();
            double[] thetas;
            double[,] depths;
            double[] dz;

            Eigenrays[,] eigenrays = new Eigenrays[settings.Output.NArrayR, settings.Output.NArrayZ];

            var _result = new CalculationResult
            {
                Thethas = settings.Source.Thetas,
                HydrophoneZ = settings.Output.ArrayZ,
                HydrophoneR = settings.Output.ArrayR
            };

            var rays = new Ray[settings.Source.NThetas];
            thetas = new double[settings.Source.NThetas];
            depths = new double[settings.Source.NThetas, settings.Output.NArrayR];

            nRays = 0;
            double rHyd;
            for (i = 0; i < settings.Source.NThetas; i++)
            {
                rays[i] = new Ray();

                thetai = -settings.Source.Thetas[i] * Math.PI / 180.0;
                rays[i].Theta = thetai;
                ctheta = Math.Abs(Math.Cos(thetai));
                if (ctheta > 1.0e-7)
                {
                    thetas[nRays] = thetai;
                    SolveEikonalEq(settings, rays[i]);
                    SolveDynamicEq(settings, rays[i]);

                    if (rays[i].IReturn == true)
                    {
                        throw new CalculationException("Returning eigenrays can only be determined by Proximity");
                    }
                    for (j = 0; j < settings.Output.NArrayR; j++)
                    {
                        rHyd = settings.Output.ArrayR[j];
                        if ((rHyd >= rays[i].RMin) && (rHyd <= rays[i].RMax))
                        {
                            int iHyd = Bracket(rays[i].NCoords, rays[i].R, rHyd);
                            IntLinear1D(rays[i].R, rays[i].Z, rHyd, ref zRay, ref junkDouble, iHyd);
                            depths[nRays, j] = zRay;
                        }
                        else
                        {
                            depths[nRays, j] = double.NaN;
                        }
                    }
                    nRays++;
                }
            }
            dz = new double[nRays];
            thetaL = new double[nRays];
            thetaR = new double[nRays];
            var maxNumEigenrays = 0;
            for (i = 0; i < settings.Output.NArrayR; i++)
            {
                rHyd = settings.Output.ArrayR[i];
                for (j = 0; j < settings.Output.NArrayZ; j++)
                {
                    double zHyd = settings.Output.ArrayZ[j];
                    for (k = 0; k < nRays; k++)
                    {
                        dz[k] = zHyd - depths[k,i];
                    }
                    nPossibleEigenRays = 0;
                    for (k = 0; k < nRays - 1; k++)
                    {
                        fl = dz[k];
                        fr = dz[k + 1];
                        prod = fl * fr;

                        if (double.IsNaN(depths[k,i]) == false &&
                            double.IsNaN(depths[k + 1,i]) == false)
                        {
                            if ((fl == 0.0) && (fr != 0.0))
                            {
                                thetaL[nPossibleEigenRays] = thetas[k];
                                thetaR[nPossibleEigenRays] = thetas[k + 1];
                                nPossibleEigenRays++;

                            }
                            else if ((fr == 0.0) && (fl != 0.0))
                            {
                                thetaL[nPossibleEigenRays] = thetas[k];
                                thetaR[nPossibleEigenRays] = thetas[k + 1];
                                nPossibleEigenRays++;

                            }
                            else if (prod < 0.0)
                            {
                                thetaL[nPossibleEigenRays] = thetas[k];
                                thetaR[nPossibleEigenRays] = thetas[k + 1];
                                nPossibleEigenRays++;
                            }
                        }
                        if (nPossibleEigenRays > nRays)
                        {
                            throw new CalculationException("Unexpected error. Number of possible eigenrays exceeds number of calculated rays");
                        }
                    }

                    eigenrays[i, j] = new Eigenrays();
                    int nFoundEigenRays = 0;
                    for (l = 0; l < nPossibleEigenRays; l++)
                    {
                        settings.Source.Rbox2 = rHyd + 1;
                        tempRay.Theta = thetaL[l];
                        SolveEikonalEq(settings, tempRay);
                        fl = tempRay.Z[tempRay.NCoords - 1] - zHyd;
                        tempRay.Init(tempRay.NCoords);
                        tempRay.Theta = thetaR[l];
                        SolveEikonalEq(settings, tempRay);
                        fr = tempRay.Z[tempRay.NCoords - 1] - zHyd;
                        tempRay.Init(tempRay.NCoords);

                        bool success;
                        if (Math.Abs(fl) <= settings.Output.Miss)
                        {
                            theta0 = thetaL[l];
                            nFoundEigenRays++;
                            success = true;

                        }
                        else if (Math.Abs(fr) <= settings.Output.Miss)
                        {
                            theta0 = thetaR[l];
                            nFoundEigenRays++;
                            success = true;
                        }
                        else
                        {
                            nTrial = 0;
                            success = false;
                            while (success == false)
                            {
                                nTrial++;

                                if (nTrial > 21)
                                {
                                    break;
                                }

                                theta0 = thetaR[l] - fr * (thetaL[l] - thetaR[l]) / (fl - fr);
                                tempRay.Theta = theta0;
                                SolveEikonalEq(settings, tempRay);
                                f0 = tempRay.Z[tempRay.NCoords - 1] - zHyd;
                                tempRay.Init(tempRay.NCoords);
                                if (Math.Abs(f0) < settings.Output.Miss)
                                {
                                    success = true;
                                    nFoundEigenRays++;
                                    break;
                                }
                                else
                                {
                                    prod = fl * f0;

                                    if (prod < 0.0)
                                    {
                                        thetaR[l] = theta0;
                                        fr = f0;
                                    }
                                    else
                                    {
                                        thetaL[l] = theta0;
                                        fl = f0;
                                    }
                                }
                            }
                        }
                        if (success == true)
                        {
                            tempRay.Theta = theta0;
                            SolveEikonalEq(settings, tempRay);
                            SolveDynamicEq(settings, tempRay);

                            var eigenrayDetails = new EigenrayDetails
                            {
                                Theta = tempRay.Theta,
                                R = tempRay.R,
                                Z = tempRay.Z,
                                Tau = tempRay.Tau,
                                Amp = tempRay.Amp,
                                IReturns = tempRay.IReturn,
                                NSurRefl = tempRay.SRefl,
                                NBotRefl = tempRay.BRefl,
                                NObjRefl = tempRay.ORefl,
                                NRefrac = tempRay.NRefrac
                            };

                            if (tempRay.NRefrac > 0)
                            {
                                eigenrayDetails.RefracR = tempRay.RRefrac;
                                eigenrayDetails.RefracZ = tempRay.ZRefrac;
                            }

                            eigenrays[i, j].NEigenrays += 1;
                            eigenrays[i, j].Eigenray.Add(eigenrayDetails);

                            maxNumEigenrays = Math.Max(eigenrays[i, j].NEigenrays, maxNumEigenrays);
                        }
                    }
                }
            }
            _result.MaxNumEigenrays = maxNumEigenrays;
            _result.Eigenrays = eigenrays;
            _result.Rays = rays;

            return _result;
        }
        private void IntBarycCubic1D(double[] x, double[] f, double xi, ref double fi, ref double fxi, ref double fxxi, int offset = 0)
        {
            double[] a = new double[3],
            px = new double[3],
            sx = new double[3],
            qx = new double[3];

            px[0] = (x[1 + offset] - x[0 + offset]) * (x[1 + offset] - x[2 + offset]) * (x[1 + offset] - x[3 + offset]);
            px[1] = (x[2 + offset] - x[0 + offset]) * (x[2 + offset] - x[1 + offset]) * (x[2 + offset] - x[3 + offset]);
            px[2] = (x[3 + offset] - x[0 + offset]) * (x[3 + offset] - x[1 + offset]) * (x[3 + offset] - x[2 + offset]);

            for (var i = 0; i < 3; i++)
            {
                a[i] = (f[i + 1 + offset] - f[0 + offset]) / px[i];
            }

            px[0] = (xi - x[0 + offset]) * (xi - x[2 + offset]) * (xi - x[3 + offset]);
            px[1] = (xi - x[0 + offset]) * (xi - x[1 + offset]) * (xi - x[3 + offset]);
            px[2] = (xi - x[0 + offset]) * (xi - x[1 + offset]) * (xi - x[2 + offset]);

            sx[0] = (xi - x[0 + offset]) * (xi - x[2 + offset]) + (xi - x[0 + offset]) * (xi - x[3 + offset]) + (xi - x[2 + offset]) * (xi - x[3 + offset]);
            sx[1] = (xi - x[0 + offset]) * (xi - x[1 + offset]) + (xi - x[0 + offset]) * (xi - x[3 + offset]) + (xi - x[1 + offset]) * (xi - x[3 + offset]);
            sx[2] = (xi - x[0 + offset]) * (xi - x[1 + offset]) + (xi - x[0 + offset]) * (xi - x[2 + offset]) + (xi - x[1 + offset]) * (xi - x[2 + offset]);

            qx[0] = 2 * (3 * xi - x[0 + offset] - x[2 + offset] - x[3 + offset]);
            qx[1] = 2 * (3 * xi - x[0 + offset] - x[1 + offset] - x[3 + offset]);
            qx[2] = 2 * (3 * xi - x[0 + offset] - x[1 + offset] - x[2 + offset]);

            fi = f[0 + offset];
            fxi = 0.0;
            fxxi = 0.0;

            for (var i = 0; i < 3; i++)
            {
                fi += a[i] * px[i];
                fxi += a[i] * sx[i];
                fxxi += a[i] * qx[i];
            }
        }

        private double Thorpe(double freq)
        {
            var fxf = Math.Pow(freq / 1000, 2);
            var alpha = 0.0033 + 0.11 * fxf / (1 + fxf) + 44 * fxf / (4100 + fxf) + 0.0003 * fxf;
            alpha /= 8685.8896;

            return alpha;
        }

        private double DotProduct(Vector u, Vector v) => u.R * v.R + u.Z * v.Z;

        private double SpecularReflection(Vector normal, Vector tauI, Vector tauR)
        {
            var c = DotProduct(normal, tauI);

            tauR.R = tauI.R - 2 * c * normal.R;
            tauR.Z = tauI.Z - 2 * c * normal.Z;

            var theta = Math.Cos(DotProduct(normal, tauR));

            return theta;
        }

        private Complex BoundaryReflectionCoeff(double rho1, double rho2, double cp1, double cp2, double cs2, double ap, double As, double theta)
        {
            var log10e = Math.Log10(Math.E);

            var tilap = ap / (40.0 * Math.PI * log10e);
            var tilas = As / (40.0 * Math.PI * log10e);

            var tilcp2 = cp2 * (1.0 - Complex.ImaginaryOne * tilap) / (1 + tilap * tilap);
            var tilcs2 = cs2 * (1.0 - Complex.ImaginaryOne * tilas) / (1 + tilas * tilas);

            var a1 = rho2 / rho1;
            var a2 = tilcp2 / cp1;
            var a3 = tilcs2 / cp1;
            var a4 = a3 * Math.Sin(theta);
            var a5 = 2.0 * a4 * a4;
            var a6 = a2 * Math.Sin(theta);
            var a7 = 2.0 * a5 - a5 * a5;

            var d = a1 * (a2 * (1.0 - a7) / Complex.Sqrt(1.0 - a6 * a6) + a3 * a7 / Complex.Sqrt(1.0 - 0.5 * a5));

            Complex refl = (d * Math.Cos(theta) - 1.0) / (d * Math.Cos(theta) + 1.0);
            return refl;
        }

        private double ConvertUnits(double aIn, double lambda, double freq, AttenUnits units)
        {
            const double c1 = 8.68588963806504;

            return units switch
            {
                AttenUnits.dBperkHz => aIn * lambda * freq * 1.0e-3,
                AttenUnits.dBperMeter => aIn * lambda,
                AttenUnits.dBperNeper => aIn * lambda * c1,
                AttenUnits.qFactor => aIn == 0 ? 0 : c1 * Math.PI / aIn,
                AttenUnits.dBperLambda => aIn,
                _ =>
                throw new CalculationException("Invalid attenuation units", nameof(units))
            };
        }

        private void IntBarycParab2D(double[] x, double[] y, double[,] f, double xi, double yi,
            ref double fi, ref double fxi, ref double fyi, ref double fxxi, ref double fyyi, ref double fxyi)
        {
            double[] px = new double[3];
            double[] py = new double[3];
            double[] sx = new double[3];
            double[] sy = new double[3];
            double[,] a = new double[3, 3];

            px[0] = (x[0] - x[1]) * (x[0] - x[2]);
            px[1] = (x[1] - x[0]) * (x[1] - x[2]);
            px[2] = (x[2] - x[0]) * (x[2] - x[1]);

            py[0] = (y[0] - y[1]) * (y[0] - y[2]);
            py[1] = (y[1] - y[0]) * (y[1] - y[2]);
            py[2] = (y[2] - y[0]) * (y[2] - y[1]);

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    a[i, j] = f[i,j] / (px[j] * py[i]);
                }
            }

            px[0] = (xi - x[1]) * (xi - x[2]);
            px[1] = (xi - x[0]) * (xi - x[2]);
            px[2] = (xi - x[0]) * (xi - x[1]);

            py[0] = (yi - y[1]) * (yi - y[2]);
            py[1] = (yi - y[0]) * (yi - y[2]);
            py[2] = (yi - y[0]) * (yi - y[1]);

            sx[0] = 2.0 * xi - x[1] - x[2];
            sx[1] = 2.0 * xi - x[0] - x[2];
            sx[2] = 2.0 * xi - x[0] - x[1];

            sy[0] = 2.0 * yi - y[1] - y[2];
            sy[1] = 2.0 * yi - y[0] - y[2];
            sy[2] = 2.0 * yi - y[0] - y[1];

            fi = 0;
            fxi = 0;
            fyi = 0;
            fxxi = 0;
            fyyi = 0;
            fxyi = 0;

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    fi += a[i, j] * px[j] * py[i];
                    fxi += a[i, j] * sx[j] * py[i];
                    fyi += a[i, j] * px[j] * sy[i];
                    fxxi += a[i, j] * 2 * py[i];
                    fyyi += a[i, j] * 2 * px[j];
                    fxyi += a[i, j] * sx[j] * sy[i];
                }
            }
        }

        private void IntComplexBarycParab1D(double[] x, Complex[] f, Complex xi, ref Complex fi,
            ref Complex fxi, ref Complex fxxi)
        {
            Complex px1 = (x[1] - x[0]) * (x[1] - x[2]);
            Complex px2 = (x[2] - x[0]) * (x[2] - x[1]);

            var a1 = (f[1] - f[0]) / px1;
            var a2 = (f[2] - f[0]) / px2;

            px1 = (xi - x[0]) * (xi - x[2]);
            px2 = (xi - x[0]) * (xi - x[1]);

            var sx1 = 2.0 * xi - x[0] - x[2];
            var sx2 = 2.0 * xi - x[0] - x[1];

            fi = f[0] + a1 * px1 + a2 * px2;
            fxi = a1 * sx1 + a2 * sx2;
            fxxi = a1 * 2.0 + a2 * 2.0;
        }

        private void IntComplexLinear1D(double[] x, Complex[] f, Complex xi, ref Complex fi, ref Complex fxi, int offset = 0)
        {
            fxi = (f[1 + offset] - f[0 + offset]) / (x[1 + offset] - x[0 + offset]);
            fi = f[0 + offset] + (xi - x[0 + offset]) * fxi;
        }

        private double[] LinearSpaced(int n, double xMin, double xMax)
        {
            var x = new double[n];
            var dx = (xMax - xMin) / (n - 1);
            for (var i = 0; i < n; i++)
            {
                x[i] = xMin + dx * i;
            }

            return x;
        }

        private double ReflectionCorr(int iTop,
            Vector sigma,
            Vector tauB,
            Vector gradC,
            double ci)
        {
            Vector nBdy = new Vector();
            Vector sigmaN = new Vector();

            iTop = -iTop;
            nBdy.R = -tauB.Z;
            nBdy.Z = tauB.R;
            if (iTop == 1)
            {
                nBdy.R = -nBdy.R;
                nBdy.R = -nBdy.Z;
            }

            sigmaN.R = -sigma.Z;
            sigmaN.Z = sigma.Z;

            var tg = DotProduct(sigma, tauB);
            var th = DotProduct(sigma, nBdy);
            var cn = DotProduct(gradC, sigmaN);
            var cs = DotProduct(gradC, sigma);

            var rm = tg / th;
            if (iTop == 1)
            {
                cn = -cn;
            }

            return rm * (4 * cn - 2 * rm * cs) / ci;
        }

        private void LineLineIntersec(Point p1,
            Point p2,
            Point q1,
            Point q2,
            ref int i,
            Point isect)
        {
            i = 0;
            var distance = (p1.R - p2.R) * (q1.Z - q2.Z) - (p1.Z - p2.Z) * (q1.R - q2.R);

            if (Math.Abs(distance) > 1e-16)
            {
                i = 1;

                isect.R = ((p1.R * p2.Z - p1.Z * p2.R) * (q1.R - q2.R) - (p1.R - p2.R) * (q1.R * q2.Z - q1.Z * q2.R)) / distance;
                isect.Z = ((p1.R * p2.Z - p1.Z * p2.R) * (q1.Z - q2.Z) - (p1.Z - p2.Z) * (q1.R * q2.Z - q1.Z * q2.R)) / distance;
            }
        }

        private void BoundaryInterpolationExplicit(int NumSurfaceCoords,
            double[] r,
            double[] z,
            SurfaceInterpolation surfaceInterpolation,
            double ri,
            ref double zi,
            Vector taub,
            Vector normal)
        {

            double zri = 0;
            double zrri = 0;
            int i;
            switch (surfaceInterpolation)
            {
                case SurfaceInterpolation.Flat:
                    IntLinear1D(r, z, ri, ref zi, ref zri);
                    break;

                case SurfaceInterpolation.Sloped:
                    IntLinear1D(r, z, ri, ref zi, ref zri);
                    break;

                case SurfaceInterpolation.Linear:
                    i = Bracket(NumSurfaceCoords, r, ri);
                    IntLinear1D(r, z, ri, ref zi, ref zri, i);
                    break;

                case SurfaceInterpolation.Cubic:
                    if (ri <= r[1])
                    {
                        i = 1;
                    }
                    else if (ri >= r[NumSurfaceCoords - 2])
                    {
                        i = NumSurfaceCoords - 3;
                    }
                    else
                    {
                        i = Bracket(NumSurfaceCoords, r, ri);
                    }
                    IntBarycCubic1D(r, z, ri, ref zi, ref zri, ref zrri, i - 1);
                    break;
            }

            taub.R = Math.Cos(Math.Atan(zri));
            taub.Z = Math.Sin(Math.Atan(zri));
            if (Math.Abs(taub.R) == 1.0)
            {
                taub.Z = 0;
            }
            else if (Math.Abs(taub.Z) == 1.0)
            {
                taub.R = 0;
            }

            normal.R = -taub.Z;
            normal.Z = taub.R;
        }

        private void CsValues(
            Settings settings, double ri, double zi, ref double ci, ref double cc, ref double si, ref double cri,
            ref double czi, Vector slowness, ref double crri, ref double czzi, ref double crzi)
        {
            double k, a, eta, root, root32, root52;

            const double epsilon = 7.4e-3f;
            const double bmunk = 1300.0;
            const double bmunk2 = bmunk * bmunk;

            switch (settings.SoundSpeed.CDist)
            {
                case SoundSpeedDistribution.Profile:
                    cri = 0;
                    crri = 0;
                    crzi = 0;
                    switch (settings.SoundSpeed.CClass)
                    {
                        case SoundSpeedClass.Isovelocity:
                            ci = settings.SoundSpeed.C1D[0];
                            czi = 0;
                            czzi = 0;
                            break;

                        case SoundSpeedClass.Linear:
                            k = (settings.SoundSpeed.C1D[1] - settings.SoundSpeed.C1D[0]) / (settings.SoundSpeed.Z[1] - settings.SoundSpeed.Z[0]);
                            ci = settings.SoundSpeed.C1D[0] + k * (zi - settings.SoundSpeed.Z[0]);
                            czi = k;
                            czzi = 0;
                            break;

                        case SoundSpeedClass.Parabolic:
                            k = (settings.SoundSpeed.C1D[1] - settings.SoundSpeed.C1D[0]) / Math.Pow(settings.SoundSpeed.Z[1] - settings.SoundSpeed.Z[0], 2);
                            ci = settings.SoundSpeed.C1D[0] + k * Math.Pow(zi - settings.SoundSpeed.Z[0], 2);
                            czi = 2 * k * (zi - settings.SoundSpeed.Z[0]);
                            czzi = 2 * k;
                            break;

                        case SoundSpeedClass.Exponential:
                            k = Math.Log(settings.SoundSpeed.C1D[0] / settings.SoundSpeed.C1D[1]) / (settings.SoundSpeed.Z[1] - settings.SoundSpeed.Z[0]);
                            ci = settings.SoundSpeed.C1D[0] * Math.Exp(-k * (zi - settings.SoundSpeed.Z[0]));
                            czi = -k * ci;
                            czzi = k * k * ci;
                            break;

                        case SoundSpeedClass.N2Linear:
                            k = (Math.Pow(settings.SoundSpeed.C1D[0] / settings.SoundSpeed.C1D[1], 2) - 1) / (settings.SoundSpeed.Z[1] - settings.SoundSpeed.Z[0]);
                            root = Math.Sqrt(1 + k * (zi - settings.SoundSpeed.Z[0]));
                            root32 = Math.Pow(root, 3 / 2);
                            root52 = Math.Pow(root, 5 / 2);
                            ci = settings.SoundSpeed.C1D[0] / Math.Sqrt(1 + k * (zi - settings.SoundSpeed.Z[0]));
                            czi = -k * settings.SoundSpeed.C1D[0] / (2 * root32);
                            czzi = 3 * k * k * settings.SoundSpeed.C1D[0] / (4 * root52);
                            break;

                        case SoundSpeedClass.InvSquare:
                            a = Math.Pow((settings.SoundSpeed.C1D[1] / settings.SoundSpeed.C1D[0]) - 1, 2);
                            root = Math.Sqrt(a / (1 - a));
                            k = root / (settings.SoundSpeed.Z[1] - settings.SoundSpeed.Z[0]);
                            root = Math.Sqrt(1 + Math.Pow(k * (zi - settings.SoundSpeed.Z[0]), 2));
                            root32 = Math.Pow(root, 3 / 2);
                            root52 = Math.Pow(root, 5 / 2);
                            ci = settings.SoundSpeed.C1D[0] * (1 + k * (zi - settings.SoundSpeed.Z[0]) / root);
                            czi = settings.SoundSpeed.C1D[0] * k / root32;
                            czzi = -3 * settings.SoundSpeed.C1D[0] * Math.Pow(k, 3) / root52;
                            break;

                        case SoundSpeedClass.Munk:
                            eta = 2 * (zi - settings.SoundSpeed.Z[0]) / bmunk;
                            ci = settings.SoundSpeed.C1D[0] * (1 + epsilon * (eta + Math.Exp(-eta) - 1));
                            czi = 2 * epsilon * settings.SoundSpeed.C1D[0] * (1 - Math.Exp(-eta)) / bmunk;
                            czzi = 4 * epsilon * settings.SoundSpeed.C1D[0] * Math.Exp(-eta) / bmunk2;
                            break;

                        case SoundSpeedClass.Tabulated:
                            CValues1D(settings.SoundSpeed.Nz, settings.SoundSpeed.Z, settings.SoundSpeed.C1D, zi, ref ci, ref czi, ref czzi);
                            break;

                        default:
                            throw new CalculationException("Unknown SSP class", nameof(settings.SoundSpeed.CClass));
                    }
                    break;
                case SoundSpeedDistribution.Field:
                    CValues2D(settings.SoundSpeed.Nr, settings.SoundSpeed.Nz, settings.SoundSpeed.R, settings.SoundSpeed.Z, settings.SoundSpeed.C2D, ri, zi, ref ci, ref cri, ref czi, ref crri, ref czzi, ref crzi);
                    break;

                default:
                    throw new CalculationException("Unknown sound speed distribution", nameof(settings.SoundSpeed.CDist));
            }

            cc = Math.Pow(ci, 2);
            si = 1.0 / ci;
            slowness.R = -cri / cc;
            slowness.Z = -czi / cc;
        }

        private void CValues2D(
            int nx, int ny, double[] xTable, double[] yTable, double[,] cTable, double xi, double yi,
            ref double ci, ref double cxi, ref double cyi, ref double cxxi, ref double cyyi, ref double cxyi)
        {
            int i;
            if (xi <= xTable[1])
            {
                i = 0;
            }
            else if (xi >= xTable[nx - 2])
            {
                i = nx - 3;
            }
            else
            {
                i = Bracket(nx, xTable, xi);
            }

            int j;
            if (yi <= yTable[1])
            {
                j = 0;
            }
            else if (yi >= yTable[ny - 2])
            {
                j = ny - 3;
            }
            else
            {
                j = Bracket(ny, yTable, yi);
            }

            double[,] tempDouble2D = new double[3, 3];

            for (var a = 0; a < 3; a++)
            {
                for (var b = 0; b < 3; b++)
                {
                    tempDouble2D[a, b] = cTable[j + a,i + b];
                }
            }

            IntBarycParab2D(xTable, yTable, tempDouble2D, xi, yi, ref ci, ref cxi, ref cyi, ref cxxi, ref cyyi, ref cxyi);
        }

        private void CValues1D(int n, double[] xTable, double[] cTable, double xi, ref double ci,
            ref double cxi, ref double cxxi)
        {
            if (xi >= xTable[1] && xi < xTable[n - 2])
            {
                int i = Bracket(n, xTable, xi);
                IntBarycCubic1D(xTable, cTable, xi, ref ci, ref cxi, ref cxxi, i - 1);

            }
            else if (xi < xTable[1])
            {
                IntLinear1D(xTable, cTable, xi, ref ci, ref cxi);
                cxxi = 0.0;

            }
            else if (xi >= xTable[n - 2])
            {
                IntLinear1D(xTable, cTable, xi, ref ci, ref cxi, n - 2);
                cxxi = 0.0;
            }
            else
            {
                throw new CalculationException("Interpolation error");
            }
        }

        public CalculationResult CalcSSP(Settings settings)
        {
            var nPoints = settings.Options.NSSPPoints;
            var c = new double[nPoints];
            double cc = 0,
            sigmaI = 0,
            cri = 0,
            czi = 0,
            crri = 0,
            czzi = 0,
            crzi = 0;

            var slowness = new Vector();
            var depths = LinearSpaced(nPoints, settings.SoundSpeed.Z[0], settings.SoundSpeed.Z[settings.SoundSpeed.Nz - 1]);
            for (var i = 0; i < nPoints; i++)
            {
                var ci = c[i];
                CsValues(
                settings, settings.Source.Rbox1, depths[i], ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);

                c[i] = ci;
            }

            return new CalculationResult
            {
                SSPZ = depths,
                SSPC = c
            };
        }

        private void RKF45(Settings settings, ref double dsi, double[] yOld, double[] fOld, double[] yNew, double[] fNew, ref double ds4, ref double ds5)
        {
            double[] k1 = new double[4];
            double[] k2 = new double[4];
            double[] k3 = new double[4];
            double[] k4 = new double[4];
            double[] k5 = new double[4];
            double[] k6 = new double[4];
            double[] yk = new double[4];
            double[] yrk4 = new double[4];
            double[] yrk5 = new double[4];
            double ci = 0;
            double cc = 0;
            double sigmaI = 0;
            double cri = 0;
            double czi = 0;
            double crri = 0;
            double czzi = 0;
            double crzi = 0;
            Vector es = new Vector();
            Vector slowness = new Vector();
            const double A1 = 25.0 / 216.0; ;
            const double A3 = 1408.0 / 2565.0;
            const double A4 = 2197.0 / 4101.0;
            const double A5 = -1.0 / 5.0;
            const double B1 = 16.0 / 135.0;
            const double B3 = 6656.0 / 12825.0;
            const double B4 = 28561.0 / 56430.0;
            const double B5 = -9.0 / 50.0;
            const double B6 = 2.0 / 55.0;

            double ri = yOld[0];
            double zi = yOld[1];
            CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);

            for (var j = 0; j < 4; j++)
            {
                k1[j] = fOld[j];
                yk[j] = yOld[j] + 0.25 * dsi * k1[j];
            }
            ri = yk[0];
            zi = yk[1];
            double sigmaR = yk[2];
            double sigmaZ = yk[3];
            sigmaI = Math.Sqrt(Math.Pow(sigmaR, 2) + Math.Pow(sigmaZ, 2));

            es.R = sigmaR / sigmaI;
            es.Z = sigmaZ / sigmaI;
            CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);
            k2[0] = es.R;
            k2[1] = es.Z;
            k2[2] = slowness.R;
            k2[3] = slowness.Z;
            for (var j = 0; j < 4; j++)
            {
                yk[j] = yOld[j] + dsi * (3.0 / 32.0 * k1[j] + 9.0 / 32.0 * k2[j]);
            }
            ri = yk[0];
            zi = yk[1];
            sigmaR = yk[2];
            sigmaZ = yk[3];
            sigmaI = Math.Sqrt(Math.Pow(sigmaR, 2) + Math.Pow(sigmaZ, 2));
            es.R = sigmaR / sigmaI;
            es.Z = sigmaZ / sigmaI;
            CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);
            k3[0] = es.R;
            k3[1] = es.Z;
            k3[2] = slowness.R;
            k3[3] = slowness.Z;
            for (var j = 0; j < 4; j++)
            {
                yk[j] = yOld[j] + dsi * (1932.0 / 2197.0 * k1[j] - 7200.0 / 2197.0 * k2[j] + 7296.0 / 2197.0 * k3[j]);
            }
            ri = yk[0];
            zi = yk[1];
            sigmaR = yk[2];
            sigmaZ = yk[3];
            sigmaI = Math.Sqrt(Math.Pow(sigmaR, 2) + Math.Pow(sigmaZ, 2));
            es.R = sigmaR / sigmaI;
            es.Z = sigmaZ / sigmaI;
            CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);
            k4[0] = es.R;
            k4[1] = es.Z;
            k4[2] = slowness.R;
            k4[3] = slowness.Z;
            for (var j = 0; j < 4; j++)
            {
                yk[j] = yOld[j] + dsi * (439.0 / 216.0 * k1[j] - 8.0 * k2[j] + 3680.0 / 513.0 * k3[j] - 845.0 / 4104 * k4[j]);
            }
            ri = yk[0];
            zi = yk[1];
            sigmaR = yk[2];
            sigmaZ = yk[3];
            sigmaI = Math.Sqrt(Math.Pow(sigmaR, 2) + Math.Pow(sigmaZ, 2));
            es.R = sigmaR / sigmaI;
            es.Z = sigmaZ / sigmaI;
            CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);
            k5[0] = es.R;
            k5[1] = es.Z;
            k5[2] = slowness.R;
            k5[3] = slowness.Z;
            for (var j = 0; j < 4; j++)
            {
                yk[j] = yOld[j] + dsi * (2.0 * k2[j] - 8.0 / 27.0 * k1[j] + 3544.0 / 2565.0 * k3[j] + 1859.0 / 4104 * k4[j] - 11.0 / 40.0 * k5[j]);
            }
            ri = yk[0];
            zi = yk[1];
            sigmaR = yk[2];
            sigmaZ = yk[3];
            sigmaI = Math.Sqrt(Math.Pow(sigmaR, 2) + Math.Pow(sigmaZ, 2));
            es.R = sigmaR / sigmaI;
            es.Z = sigmaZ / sigmaI;
            CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);
            k6[0] = es.R;
            k6[1] = es.Z;
            k6[2] = slowness.R;
            k6[3] = slowness.Z;

            for (var j = 0; j < 4; j++)
            {
                yrk4[j] = yOld[j] + dsi * (A1 * k1[j] + A3 * k3[j] + A4 * k4[j] + A5 * k5[j]);
                yrk5[j] = yOld[j] + dsi * (B1 * k1[j] + B3 * k3[j] + B4 * k4[j] + B5 * k5[j] + B6 * k6[j]);
                yNew[j] = yrk5[j];
            }
            ri = yNew[0];
            zi = yNew[1];
            sigmaR = yNew[2];
            sigmaZ = yNew[3];
            double dr = yrk4[0] - yOld[0];
            double dz = yrk4[1] - yOld[1];
            ds4 = Math.Sqrt(dr * dr + dz * dz);

            dr = yrk5[0] - yOld[0];
            dz = yrk5[1] - yOld[1];
            ds5 = Math.Sqrt(dr * dr + dz * dz);
            sigmaI = Math.Sqrt(Math.Pow(sigmaR, 2) + Math.Pow(sigmaZ, 2));
            es.R = sigmaR / sigmaI;
            es.Z = sigmaZ / sigmaI;
            CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);
            fNew[0] = es.R;
            fNew[1] = es.Z;
            fNew[2] = slowness.R;
            fNew[3] = slowness.Z;
        }

        private void BoundaryInterpolation(
            Interface @interface,
            double ri,
            ref double zi,
            Vector taub,
            Vector normal)
        {
            BoundaryInterpolationExplicit(
                @interface.NumSurfaceCoords,
                @interface.R,
                @interface.Z,
                @interface.SurfaceInterpolation,
                ri,
                ref zi,
                taub,
                normal);
        }

        private void SolveEikonalEq(Settings settings, Ray ray)
        {
            double cx = 0,
            ci = 0,
            cc = 0,
            sigmaI = 0,
            cri = 0,
            czi = 0,
            crri = 0,
            czzi = 0,
            crzi = 0;
            Complex reflCoeff = new Complex(), reflDecay;
            Vector es = new Vector();
            Vector slowness = new Vector();
            Vector junkVector = new Vector();
            Vector normal = new Vector();
            Vector tauB = new Vector();
            Vector tauR = new Vector();
            double[] yOld = new double[4];
            double[] fOld = new double[4];
            double[] yNew = new double[4];
            double[] fNew = new double[4];
            double dsi = 0,
            ds4 = 0,
            ds5 = 0;
            double altInterpolatedZ = 0,
            batInterpolatedZ = 0;
            Point pointA = new Point(),
            pointB = new Point(),
            pointIsect = new Point();
            double rho2 = 0,
            cp2 = 0,
            cs2 = 0,
            ap = 0,
            _as = 0;
            double dr;
            double ziDown = 0,
            ziUp = 0;
            int initialMemorySize = (int)Math.Ceiling(Math.Abs((settings.Source.Rbox2 - settings.Source.Rbox1) / settings.Source.Ds)) * 20;
            ray.Init(initialMemorySize);
            double rho1 = 1.0;
            ray.IKill = false;
            int sRefl = 0;
            int bRefl = 0;
            int oRefl = 0;
            int jRefl = 0;
            int ibdry = 0;
            ray.IRefl[0] = false;

            ray.IReturn = false;
            reflDecay = 1.0 + 0.0 * Complex.ImaginaryOne;
            ray.Decay[0] = reflDecay;
            ray.Phase[0] = 0.0;

            ray.R[0] = settings.Source.Rx;
            ray.RMin = ray.R[0];
            ray.RMax = ray.R[0];
            ray.Z[0] = settings.Source.Zx;

            es.R = Math.Cos(ray.Theta);
            es.Z = Math.Sin(ray.Theta);

            CsValues(
            settings, settings.Source.Rx, settings.Source.Zx, ref cx, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);

            double sigmaR = sigmaI * es.R;
            double sigmaZ = sigmaI * es.Z;

            ray.C[0] = cx;
            ray.Tau[0] = 0;
            ray.S[0] = 0;
            ray.Ic[0] = 0;
            yOld[0] = settings.Source.Rx;
            yOld[1] = settings.Source.Zx;
            yOld[2] = sigmaR;
            yOld[3] = sigmaZ;

            fOld[0] = es.R;
            fOld[1] = es.Z;
            fOld[2] = slowness.R;
            fOld[3] = slowness.Z;
            int i = 0;
            double dz;
            while ((ray.IKill == false) && (ray.R[i] < settings.Source.Rbox2) && (ray.R[i] > settings.Source.Rbox1))
            {
                dsi = settings.Source.Ds;
                double stepError = 1;
                int numRungeKutta = 0;

                while (stepError > 0.1)
                {
                    if (numRungeKutta > 100)
                    {
                        throw new CalculationException("Runge-Kutta integration: failure in step convergence");
                    }
                    RKF45(settings, ref dsi, yOld, fOld, yNew, fNew, ref ds4, ref ds5);

                    numRungeKutta++;
                    stepError = Math.Abs(ds4 - ds5) / (0.5 * (ds4 + ds5));
                    dsi *= 0.5;
                }

                es.R = fNew[0];
                es.Z = fNew[1];
                double ri = yNew[0];
                double zi = yNew[1];
                if ((ri > settings.Altimetry.R[0]) && (ri < settings.Altimetry.R[settings.Altimetry.NumSurfaceCoords - 1]) && (ri > settings.Batimetry.R[0]) && (ri < settings.Batimetry.R[settings.Batimetry.NumSurfaceCoords - 1]))
                {
                    BoundaryInterpolation(settings.Altimetry, ri, ref altInterpolatedZ, junkVector, normal);
                    BoundaryInterpolation(settings.Batimetry, ri, ref batInterpolatedZ, junkVector, normal);
                }
                else
                {
                    ray.IKill = true;
                }
                double thetaRefl;
                double lambda;
                double tempDouble;
                if ((ray.IKill == false) && (zi <= altInterpolatedZ || zi >= batInterpolatedZ))
                {
                    pointA.R = yOld[0];
                    pointA.Z = yOld[1];
                    pointB.R = yNew[0];
                    pointB.Z = yNew[1];
                    if (zi <= altInterpolatedZ)
                    {
                        RayBoundaryIntersection(settings.Altimetry, pointA, pointB, pointIsect);
                        ri = pointIsect.R;
                        zi = pointIsect.Z;
                        BoundaryInterpolation(settings.Altimetry, ri, ref altInterpolatedZ, tauB, normal);
                        ibdry = -1;
                        sRefl++;
                        jRefl = 1;

                        thetaRefl = SpecularReflection(normal, es, tauR);
                        if (ri + settings.Source.Ds * tauR.R < settings.Source.Rbox1 || ri + settings.Source.Ds * tauR.R > settings.Source.Rbox2)
                        {
                            ray.IKill = true;
                        }
                        else
                        {
                            BoundaryInterpolation(settings.Altimetry, ri + settings.Source.Ds * tauR.R, ref altInterpolatedZ, tauB, normal);
                            if ((zi + settings.Source.Ds * tauR.Z) < altInterpolatedZ)
                            {
                                ray.IKill = true;
                            }
                        }
                        switch (settings.Altimetry.SurfaceType)
                        {

                            case SurfaceType.Absorvent:
                                reflCoeff = 0.0 + 0.0 * Complex.ImaginaryOne;
                                ray.IKill = true;
                                break;

                            case SurfaceType.Rigid:
                                reflCoeff = 1.0 + 0.0 * Complex.ImaginaryOne;
                                break;

                            case SurfaceType.Vacuum:
                                reflCoeff = -1.0 + 0.0 * Complex.ImaginaryOne;
                                break;

                            case SurfaceType.Elastic:
                                switch (settings.Altimetry.SurfacePropertyType)
                                {

                                    case SurfacePropertyType.Homogeneous:
                                        rho2 = settings.Altimetry.Rho[0];
                                        cp2 = settings.Altimetry.Cp[0];
                                        cs2 = settings.Altimetry.Cs[0];
                                        ap = settings.Altimetry.Ap[0];
                                        _as = settings.Altimetry.As[0];
                                        lambda = cp2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(ap, lambda, settings.Source.Freqx, settings.Altimetry.SurfaceAttenUnits);
                                        ap = tempDouble;
                                        lambda = cs2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(_as, lambda, settings.Source.Freqx, settings.Altimetry.SurfaceAttenUnits);
                                        _as = tempDouble;
                                        reflCoeff = BoundaryReflectionCoeff(rho1, rho2, ci, cp2, cs2, ap, _as, thetaRefl);
                                        break;

                                    case SurfacePropertyType.NonHomogeneous:
                                        BoundaryInterpolationExplicit(settings.Altimetry.NumSurfaceCoords, settings.Altimetry.R, settings.Altimetry.Rho, settings.Altimetry.SurfaceInterpolation, ri, ref rho2, junkVector, junkVector);
                                        BoundaryInterpolationExplicit(settings.Altimetry.NumSurfaceCoords, settings.Altimetry.R, settings.Altimetry.Cp, settings.Altimetry.SurfaceInterpolation, ri, ref cp2, junkVector, junkVector);
                                        BoundaryInterpolationExplicit(settings.Altimetry.NumSurfaceCoords, settings.Altimetry.R, settings.Altimetry.Cs, settings.Altimetry.SurfaceInterpolation, ri, ref cs2, junkVector, junkVector);
                                        BoundaryInterpolationExplicit(settings.Altimetry.NumSurfaceCoords, settings.Altimetry.R, settings.Altimetry.Ap, settings.Altimetry.SurfaceInterpolation, ri, ref ap, junkVector, junkVector);
                                        BoundaryInterpolationExplicit(settings.Altimetry.NumSurfaceCoords, settings.Altimetry.R, settings.Altimetry.As, settings.Altimetry.SurfaceInterpolation, ri, ref _as, junkVector, junkVector);
                                        lambda = cp2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(ap, lambda, settings.Source.Freqx, settings.Altimetry.SurfaceAttenUnits);
                                        ap = tempDouble;
                                        lambda = cs2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(_as, lambda, settings.Source.Freqx, settings.Altimetry.SurfaceAttenUnits);
                                        _as = tempDouble;
                                        reflCoeff = BoundaryReflectionCoeff(rho1, rho2, ci, cp2, cs2, ap, _as, thetaRefl);
                                        break;
                                    default:
                                        throw new CalculationException("Unknown surface properties", nameof(settings.Altimetry.SurfacePropertyType));
                                }
                                break;
                            default:
                                throw new CalculationException("Unknown surface type", nameof(settings.Altimetry.SurfaceType));
                        }
                        reflDecay *= reflCoeff;

                        if (Complex.Abs(reflDecay) < 1.0e-15)
                        {
                            ray.IKill = true;
                        }
                    }
                    else if (zi >= batInterpolatedZ)
                    {
                        RayBoundaryIntersection(settings.Batimetry, pointA, pointB, pointIsect);
                        ri = pointIsect.R;
                        zi = pointIsect.Z;

                        BoundaryInterpolation(settings.Batimetry, ri, ref batInterpolatedZ, tauB, normal);
                        normal.R = -normal.R;
                        normal.Z = -normal.Z;

                        ibdry = 1;
                        bRefl++;
                        jRefl = 1;

                        thetaRefl = SpecularReflection(normal, es, tauR);
                        if (ri + settings.Source.Ds * tauR.R < settings.Source.Rbox1 || ri + settings.Source.Ds * tauR.R > settings.Source.Rbox2)
                        {
                            ray.IKill = true;
                        }
                        else
                        {
                            BoundaryInterpolation(settings.Batimetry, ri + settings.Source.Ds * tauR.R, ref batInterpolatedZ, tauB, normal);
                            if ((zi + settings.Source.Ds * tauR.Z) > batInterpolatedZ)
                            {
                                ray.IKill = true;
                            }
                        }

                        switch (settings.Batimetry.SurfaceType)
                        {

                            case SurfaceType.Absorvent:
                                reflCoeff = 0.0 + 0.0 * Complex.ImaginaryOne;
                                ray.IKill = true;
                                break;

                            case SurfaceType.Rigid:
                                reflCoeff = 1.0 + 0.0 * Complex.ImaginaryOne;
                                break;

                            case SurfaceType.Vacuum:
                                reflCoeff = -1.0 + 0.0 * Complex.ImaginaryOne;
                                break;

                            case SurfaceType.Elastic:
                                switch (settings.Batimetry.SurfacePropertyType)
                                {

                                    case SurfacePropertyType.Homogeneous:
                                        rho2 = settings.Batimetry.Rho[0];
                                        cp2 = settings.Batimetry.Cp[0];
                                        cs2 = settings.Batimetry.Cs[0];
                                        ap = settings.Batimetry.Ap[0];
                                        _as = settings.Batimetry.As[0];
                                        lambda = cp2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(ap, lambda, settings.Source.Freqx, settings.Batimetry.SurfaceAttenUnits);
                                        ap = tempDouble;
                                        lambda = cs2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(_as, lambda, settings.Source.Freqx, settings.Batimetry.SurfaceAttenUnits);
                                        _as = tempDouble;
                                        reflCoeff = BoundaryReflectionCoeff(rho1, rho2, ci, cp2, cs2, ap, _as, thetaRefl);
                                        break;

                                    case SurfacePropertyType.NonHomogeneous:
                                        BoundaryInterpolationExplicit(settings.Batimetry.NumSurfaceCoords, settings.Batimetry.R, settings.Batimetry.Rho, settings.Batimetry.SurfaceInterpolation, ri, ref rho2, junkVector, junkVector);
                                        BoundaryInterpolationExplicit(settings.Batimetry.NumSurfaceCoords, settings.Batimetry.R, settings.Batimetry.Cp, settings.Batimetry.SurfaceInterpolation, ri, ref cp2, junkVector, junkVector);
                                        BoundaryInterpolationExplicit(settings.Batimetry.NumSurfaceCoords, settings.Batimetry.R, settings.Batimetry.Cs, settings.Batimetry.SurfaceInterpolation, ri, ref cs2, junkVector, junkVector);
                                        BoundaryInterpolationExplicit(settings.Batimetry.NumSurfaceCoords, settings.Batimetry.R, settings.Batimetry.Ap, settings.Batimetry.SurfaceInterpolation, ri, ref ap, junkVector, junkVector);
                                        BoundaryInterpolationExplicit(settings.Batimetry.NumSurfaceCoords, settings.Batimetry.R, settings.Batimetry.As, settings.Batimetry.SurfaceInterpolation, ri, ref _as, junkVector, junkVector);
                                        lambda = cp2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(ap, lambda, settings.Source.Freqx, settings.Batimetry.SurfaceAttenUnits);
                                        ap = tempDouble;
                                        lambda = cs2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(_as, lambda, settings.Source.Freqx, settings.Batimetry.SurfaceAttenUnits);
                                        _as = tempDouble;
                                        reflCoeff = BoundaryReflectionCoeff(rho1, rho2, ci, cp2, cs2, ap, _as, thetaRefl);
                                        break;
                                    default:
                                        throw new CalculationException("Unknown surface properties", nameof(settings.Batimetry.SurfacePropertyType));
                                }
                                break;
                            default:
                                throw new CalculationException("Unknown surface type", nameof(settings.Batimetry.SurfaceType));
                        }

                        reflDecay *= reflCoeff;
                        if (Complex.Abs(reflDecay) < 1.0e-15)
                        {
                            ray.IKill = true;
                        }
                    }

                    ri = pointIsect.R;
                    zi = pointIsect.Z;
                    CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);
                    yNew[0] = ri;
                    yNew[1] = zi;
                    yNew[2] = sigmaI * tauR.R;
                    yNew[3] = sigmaI * tauR.Z;

                    fNew[0] = tauR.R;
                    fNew[1] = tauR.Z;
                    fNew[2] = slowness.R;
                    fNew[3] = slowness.Z;
                }

                int j;
                if (settings.Objects.NumObjects > 0)
                {
                    for (j = 0; j < settings.Objects.NumObjects; j++)
                    {
                        int nObjCoords = settings.Objects.ObjectsList[j].NCoords;

                        if ((ri >= settings.Objects.ObjectsList[j].R[0]) && (ri < settings.Objects.ObjectsList[j].R[nObjCoords - 1]))
                        {

                            if (settings.Objects.ObjectsList[j].ZDown[0] != settings.Objects.ObjectsList[j].ZUp[0])
                            {
                                throw new CalculationException("Lower and upper object boundaries do not start at the same depth");
                            }
                            BoundaryInterpolationExplicit(nObjCoords, settings.Objects.ObjectsList[j].R, settings.Objects.ObjectsList[j].ZDown, settings.Objects.SurfaceInterpolation, ri, ref ziDown, junkVector, normal);
                            BoundaryInterpolationExplicit(nObjCoords, settings.Objects.ObjectsList[j].R, settings.Objects.ObjectsList[j].ZUp, settings.Objects.SurfaceInterpolation, ri, ref ziUp, junkVector, normal);
                            if ((yNew[1] >= ziDown) && (yNew[1] <= ziUp))
                            {
                                pointA.R = yOld[0];
                                pointA.Z = yOld[1];
                                pointB.R = yNew[0];
                                pointB.Z = yNew[1];
                                if (yOld[0] < yNew[0] && yOld[0] >= settings.Objects.ObjectsList[j].R[0] && yNew[0] <= settings.Objects.ObjectsList[j].R[nObjCoords - 1])
                                {

                                    BoundaryInterpolationExplicit(nObjCoords, settings.Objects.ObjectsList[j].R, settings.Objects.ObjectsList[j].ZUp, settings.Objects.SurfaceInterpolation, yOld[0], ref ziUp, junkVector, normal);
                                    BoundaryInterpolationExplicit(nObjCoords, settings.Objects.ObjectsList[j].R, settings.Objects.ObjectsList[j].ZDown, settings.Objects.SurfaceInterpolation, yOld[0], ref ziDown, junkVector, normal);
                                    if (yOld[1] < ziDown)
                                    {
                                        RayObjectIntersection(settings.Objects, j, -1, pointA, pointB, pointIsect);
                                        ibdry = -1;
                                    }
                                    else
                                    {
                                        RayObjectIntersection(settings.Objects, j, 1, pointA, pointB, pointIsect);
                                        ibdry = 1;
                                    }
                                }
                                else if (yOld[0] < settings.Objects.ObjectsList[j].R[0] && yNew[0] < settings.Objects.ObjectsList[j].R[nObjCoords - 1])
                                {
                                    pointA.Z = pointB.Z - (pointB.Z - pointA.Z) / (pointB.R - pointA.R) * (pointB.R - settings.Objects.ObjectsList[j].R[0]);
                                    pointA.R = settings.Objects.ObjectsList[j].R[0];
                                    if (pointA.Z < settings.Objects.ObjectsList[j].ZUp[0])
                                    {
                                        RayObjectIntersection(settings.Objects, j, -1, pointA, pointB, pointIsect);
                                        ibdry = -1;
                                    }
                                    else
                                    {
                                        RayObjectIntersection(settings.Objects, j, 1, pointA, pointB, pointIsect);
                                        ibdry = 1;
                                    }
                                }
                                else if (yOld[0] > yNew[0] && yOld[0] <= settings.Objects.ObjectsList[j].R[nObjCoords - 1] && yNew[0] >= settings.Objects.ObjectsList[j].R[0])
                                {

                                    BoundaryInterpolationExplicit(nObjCoords, settings.Objects.ObjectsList[j].R, settings.Objects.ObjectsList[j].ZUp, settings.Objects.SurfaceInterpolation, yOld[0], ref ziUp, junkVector, normal);
                                    BoundaryInterpolationExplicit(nObjCoords, settings.Objects.ObjectsList[j].R, settings.Objects.ObjectsList[j].ZDown, settings.Objects.SurfaceInterpolation, yOld[0], ref ziDown, junkVector, normal);
                                    if (yOld[1] < ziDown)
                                    {
                                        RayObjectIntersection(settings.Objects, j, -1, pointA, pointB, pointIsect);
                                        ibdry = -1;
                                    }
                                    else
                                    {
                                        RayObjectIntersection(settings.Objects, j, 1, pointA, pointB, pointIsect);
                                        ibdry = 1;
                                    }
                                }
                                else if (yOld[0] > settings.Objects.ObjectsList[j].R[nObjCoords - 1] && yNew[0] >= settings.Objects.ObjectsList[j].R[0])
                                {
                                    pointA.Z = pointB.Z - (pointB.Z - pointA.Z) / (pointB.R - pointA.R) * (pointB.R - settings.Objects.ObjectsList[j].R[nObjCoords - 1]);
                                    pointA.R = settings.Objects.ObjectsList[j].R[nObjCoords - 1];
                                    if (pointA.Z < settings.Objects.ObjectsList[j].ZUp[nObjCoords - 1])
                                    {
                                        RayObjectIntersection(settings.Objects, j, -1, pointA, pointB, pointIsect);
                                        ibdry = -1;
                                    }
                                    else
                                    {
                                        RayObjectIntersection(settings.Objects, j, 1, pointA, pointB, pointIsect);
                                        ibdry = 1;
                                    }
                                }
                                else
                                {
                                    throw new CalculationException("Object reflection case: ray beginning neither behind or between object box.Check object coordinates");
                                }

                                ri = pointIsect.R;
                                zi = pointIsect.Z;
                                if (ibdry == -1)
                                {
                                    BoundaryInterpolationExplicit(nObjCoords, settings.Objects.ObjectsList[j].R, settings.Objects.ObjectsList[j].ZDown, settings.Objects.SurfaceInterpolation, ri, ref ziDown, tauB, normal);
                                    normal.R = -normal.R;
                                    normal.Z = -normal.Z;
                                }
                                else if (ibdry == 1)
                                {
                                    BoundaryInterpolationExplicit(nObjCoords, settings.Objects.ObjectsList[j].R, settings.Objects.ObjectsList[j].ZUp, settings.Objects.SurfaceInterpolation, ri, ref ziUp, tauB, normal);
                                }
                                else
                                {
                                    throw new CalculationException("Object reflection case: ray neither being reflected on down or up faces.Check object coordinates");
                                }
                                oRefl++;
                                jRefl = 1;
                                thetaRefl = SpecularReflection(normal, es, tauR);

                                switch (settings.Objects.ObjectsList[j].SurfaceType)
                                {
                                    case SurfaceType.Absorvent:
                                        reflCoeff = 0.0 + 0.0 * Complex.ImaginaryOne;
                                        ray.IKill = true;
                                        break;

                                    case SurfaceType.Rigid:
                                        reflCoeff = 1.0 + 0.0 * Complex.ImaginaryOne;
                                        break;

                                    case SurfaceType.Vacuum:
                                        reflCoeff = -1.0 + 0.0 * Complex.ImaginaryOne;
                                        break;

                                    case SurfaceType.Elastic:
                                        rho2 = settings.Objects.ObjectsList[j].Rho;
                                        cp2 = settings.Objects.ObjectsList[j].Cp;
                                        cs2 = settings.Objects.ObjectsList[j].Cs;
                                        ap = settings.Objects.ObjectsList[j].Ap;
                                        _as = settings.Objects.ObjectsList[j].As;
                                        lambda = cp2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(ap, lambda, settings.Source.Freqx, settings.Objects.ObjectsList[j].SurfaceAttenUnits);
                                        ap = tempDouble;
                                        lambda = cs2 / settings.Source.Freqx;
                                        tempDouble = ConvertUnits(_as, lambda, settings.Source.Freqx, settings.Objects.ObjectsList[j].SurfaceAttenUnits);
                                        _as = tempDouble;
                                        reflCoeff = BoundaryReflectionCoeff(rho1, rho2, ci, cp2, cs2, ap, _as, thetaRefl);
                                        break;

                                    default:
                                        throw new CalculationException("Unknown object boundary type", nameof(Models.Object.SurfaceType));
                                }

                                reflDecay *= reflCoeff;
                                if (Complex.Abs(reflDecay) < 1.0e-15)
                                {
                                    ray.IKill = true;
                                }
                                es.R = tauR.R;
                                es.Z = tauR.Z;
                                CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);

                                yNew[0] = ri;
                                yNew[1] = zi;
                                yNew[2] = sigmaI * es.R;
                                yNew[3] = sigmaI * es.Z;

                                fNew[0] = es.R;
                                fNew[1] = es.Z;
                                fNew[2] = slowness.R;
                                fNew[3] = slowness.Z;
                            }
                        }
                    }
                }
                ri = yNew[0];
                zi = yNew[1];
                ray.R[i + 1] = yNew[0];
                ray.Z[i + 1] = yNew[1];

                es.R = fNew[0];
                es.Z = fNew[1];
                CsValues(settings, ri, zi, ref ci, ref cc, ref sigmaI, ref cri, ref czi, slowness, ref crri, ref czzi, ref crzi);

                dr = ray.R[i + 1] - ray.R[i];
                dz = ray.Z[i + 1] - ray.Z[i];

                dsi = Math.Sqrt(dr * dr + dz * dz);

                ray.Tau[i + 1] = ray.Tau[i] + dsi / ci;
                ray.C[i + 1] = ci;
                ray.S[i + 1] = ray.S[i] + dsi;
                ray.Ic[i + 1] = ray.Ic[i] + dsi * ray.C[i + 1];

                ray.IRefl[i + 1] = jRefl != 0;
                ray.BoundaryJ[i + 1] = ibdry;

                ray.BoundaryTg[i + 1].R = tauB.R;
                ray.BoundaryTg[i + 1].Z = tauB.Z;
                if (jRefl == 1)
                {
                    ray.Phase[i + 1] = ray.Phase[i] - Math.Atan2(reflCoeff.Imaginary, reflCoeff.Real);
                }
                else
                {
                    ray.Phase[i + 1] = ray.Phase[i];
                }

                jRefl = 0;
                ibdry = 0;
                tauB.R = 0.0;
                tauB.Z = 0.0;
                ray.Decay[i + 1] = reflDecay;

                for (j = 0; j < 4; j++)
                {
                    yOld[j] = yNew[j];
                    fOld[j] = fNew[j];
                }

                i++;

                if (i > ray.NCoords - 1)
                {
                    throw new CalculationException("Ray step too small, number of points in ray coordinates exceeds allocated memory");
                }
            }
            ray.NCoords = i + 1;
            ray.SRefl = sRefl;
            ray.BRefl = bRefl;
            ray.ORefl = oRefl;
            ray.NRefl = sRefl + bRefl + oRefl;
            dr = ray.R[ray.NCoords - 1] - ray.R[ray.NCoords - 2];
            dz = ray.Z[ray.NCoords - 1] - ray.Z[ray.NCoords - 2];
            double dIc = ray.Ic[ray.NCoords - 1] - ray.Ic[ray.NCoords - 2];
            double dTau = ray.Tau[ray.NCoords - 1] - ray.Tau[ray.NCoords - 2];

            if (ray.R[ray.NCoords - 1] > settings.Source.Rbox2)
            {
                ray.Z[ray.NCoords - 1] = ray.Z[ray.NCoords - 2] + (settings.Source.Rbox2 - ray.R[ray.NCoords - 2]) * dz / dr;
                ray.Ic[ray.NCoords - 1] = ray.Ic[ray.NCoords - 2] + (settings.Source.Rbox2 - ray.R[ray.NCoords - 2]) * dIc / dr;
                ray.Tau[ray.NCoords - 1] = ray.Tau[ray.NCoords - 2] + (settings.Source.Rbox2 - ray.R[ray.NCoords - 2]) * dTau / dr;
                ray.R[ray.NCoords - 1] = settings.Source.Rbox2;

                ray.DecreaseSize(ray.NCoords);
            }
            else if (ray.R[ray.NCoords - 1] < settings.Source.Rbox1)
            {
                ray.Z[ray.NCoords - 1] = ray.Z[ray.NCoords - 2] + (settings.Source.Rbox1 - ray.R[ray.NCoords - 2]) * dz / dr;
                ray.Ic[ray.NCoords - 1] = ray.Ic[ray.NCoords - 2] + (settings.Source.Rbox1 - ray.R[ray.NCoords - 2]) * dIc / dr;
                ray.R[ray.NCoords - 1] = settings.Source.Rbox1;

                ray.DecreaseSize(ray.NCoords);
            }

            ray.NRefrac = 0;
            for (i = 1; i < ray.NCoords - 2; i++)
            {
                ray.RMin = Math.Min(ray.RMin, ray.R[i]);
                ray.RMax = Math.Max(ray.RMax, ray.R[i]);
                double prod = (ray.Z[i + 1] - ray.Z[i]) * (ray.Z[i] - ray.Z[i - 1]);

                if ((prod < 0) && (ray.IRefl[i] != true))
                {
                    ray.RRefrac[ray.NRefrac] = ray.R[i];
                    ray.ZRefrac[ray.NRefrac] = ray.Z[i];
                    ray.NRefrac++;
                }

                prod = (ray.R[i + 1] - ray.R[i]) * (ray.R[i] - ray.R[i - 1]);
                if (prod < 0)
                {
                    ray.IReturn = true;
                    if (settings.Options.KillBackscatteredRays)
                    {
                        ray.NCoords = i + 1;
                        ray.IReturn = false;
                        settings.Options.NBackscatteredRays += 1;
                        break;
                    }
                }
            }
            ray.RMin = Math.Min(ray.RMin, ray.R[ray.NCoords - 1]);
            ray.RMax = Math.Max(ray.RMax, ray.R[ray.NCoords - 1]);
        }

        private void RayBoundaryIntersection(Interface @interface, Point a, Point b, Point isect)
        {
            int i = 0;
            var dz = new double[101];
            Point q1 = new Point();
            Point q2 = new Point();
            Vector taub = new Vector();
            Vector normal = new Vector();

            switch (@interface.SurfaceInterpolation)
            {
                case SurfaceInterpolation.Flat:
                    isect.Z = @interface.Z[0];
                    isect.R = (b.R - a.R) / (b.Z - a.Z) * (isect.Z - a.Z) + a.R;
                    break;

                case SurfaceInterpolation.Sloped:
                    q1.R = @interface.R[0];
                    q1.Z = @interface.Z[0];
                    q2.R = @interface.R[1];
                    q2.Z = @interface.Z[1];
                    LineLineIntersec(a, b, q1, q2, ref i, isect);
                    break;

                default:
                    int n = 101;
                    var rl = LinearSpaced(n, a.R, b.R);
                    var zl = LinearSpaced(n, a.Z, b.Z);
                    var zi = isect.Z;
                    BoundaryInterpolation(@interface, rl[0], ref zi, taub, normal);
                    isect.Z = zi;
                    dz[0] = zl[0] - isect.Z;
                    for (i = 1; i < n; i++)
                    {
                        zi = isect.Z;
                        BoundaryInterpolation(@interface, rl[i], ref zi, taub, normal);
                        isect.Z = zi;
                        dz[i] = zl[i] - isect.Z;
                        if ((dz[0] * dz[i]) <= 0)
                        {
                            break;
                        }
                    }
                    isect.R = rl[i - 1] - dz[i - 1] * (rl[i] - rl[i - 1]) / (dz[i] - dz[i - 1]);
                    isect.Z = (isect.R - a.R) / (b.R - a.R) * (b.Z - a.Z) + a.Z;
                    if (Math.Abs(isect.Z) < 1.0e-12)
                    {
                        isect.Z = 0.0;
                    }
                    break;
            }
        }

        private void RayObjectIntersection(Objects objects, int j, int boundary, Point a, Point b, Point isect)
        {
            Interface tempInterface = new Interface
            {
                NumSurfaceCoords = objects.ObjectsList[j].NCoords,
                R = objects.ObjectsList[j].R,
                Z = boundary switch
                {
                    -1 => objects.ObjectsList[j].ZDown,
                    1 => objects.ObjectsList[j].ZUp,
                    _ => throw new CalculationException("RayObjectIntersection: unknown boundary"),
                },
                SurfaceInterpolation = objects.SurfaceInterpolation
            };
            RayBoundaryIntersection(tempInterface, a, b, isect);
        }

        private void SolveDynamicEq(Settings settings, Ray ray)
        {
            int ibdry;
            double alpha;
            double cii = 0, cxc = 0, sigmaI = 0, crri = 0, czzi = 0, crzi = 0;
            double crriNext = 0.0, czziNext = 0.0, crziNext = 0.0;
            Vector slowness = new Vector();
            Vector gradC = new Vector();
            Vector nGradC = new Vector();
            Vector dGradC = new Vector();
            double dr, dz, dsi;
            Vector es = new Vector(), sigma = new Vector();
            double drdn, dzdn;
            double cnn;
            Vector sigmaN = new Vector();
            double cnj, csj, rm, rn;
            Vector tauB = new Vector();
            double prod;
            int i;
            ray.P[0] = 1;
            ray.Q[0] = 0;
            ray.Caustc[0] = 0;
            alpha = Thorpe(settings.Source.Freqx);
            double ri = ray.R[0];
            double zi = ray.Z[0];
            var tempR = gradC.R;
            var tempZ = gradC.Z;
            CsValues(settings, ri, zi, ref cii, ref cxc, ref sigmaI, ref tempR, ref tempZ, slowness, ref crri, ref czzi, ref crzi);
            gradC.R = tempR;
            gradC.Z = tempZ;
            for (i = 0; i < ray.NCoords - 2; i++)
            {
                gradC.R = nGradC.R;
                gradC.Z = nGradC.Z;
                crri = crriNext;
                czzi = czziNext;
                crzi = crziNext;
                ri = ray.R[i + 1];
                zi = ray.Z[i + 1];
                tempR = nGradC.R;
                tempZ = nGradC.Z;
                CsValues(settings, ri, zi, ref cii, ref cxc, ref sigmaI, ref tempR, ref tempZ, slowness, ref crriNext, ref czziNext, ref crziNext);
                nGradC.R = tempR;
                nGradC.Z = tempZ;

                dGradC.R = nGradC.R - gradC.R;
                dGradC.Z = nGradC.Z - gradC.Z;
                dr = ray.R[i + 1] - ray.R[i];
                dz = ray.Z[i + 1] - ray.Z[i];

                dsi = Math.Sqrt(dr * dr + dz * dz);
                es.R = dr / dsi;
                es.Z = dz / dsi;

                sigma.R = sigmaI * es.R;
                sigma.Z = sigmaI * es.Z;

                drdn = -es.Z;
                dzdn = es.R;
                cnn = (drdn * drdn) * crri + 2 * drdn * dzdn * crzi + (dzdn * dzdn) * czzi;
                double ci = ray.C[i];
                cxc = ci * ci;

                if (ray.IRefl[i + 1] == false)
                {
                    ray.P[i + 1] = ray.P[i] - ray.Q[i] * (cnn / cxc) * dsi;
                    ray.Q[i + 1] = ray.Q[i] + ray.P[i] * ci * dsi;
                    sigmaN.R = -sigma.Z;
                    sigmaN.Z = sigma.R;
                    cnj = DotProduct(dGradC, sigmaN);
                    csj = DotProduct(dGradC, sigma);

                    if (sigma.Z != 0)
                    {
                        rm = sigma.R / sigma.Z;
                        rn = -(rm * (2 * cnj - rm * csj) / cii);
                        ray.P[i + 1] = ray.P[i] + ray.Q[i] * rn;
                    }
                }
                else
                {
                    ibdry = ray.BoundaryJ[i + 1];
                    tauB.R = ray.BoundaryTg[i + 1].R;
                    tauB.Z = ray.BoundaryTg[i + 1].Z;

                    rn = ReflectionCorr(ibdry, sigma, tauB, gradC, ci);
                    ray.P[i + 1] = ray.P[i] + ray.Q[i] * rn;
                    ray.Q[i + 1] = ray.Q[i];
                }

                prod = ray.Q[i] * ray.Q[i + 1];
                if ((prod <= 0) && (ray.Q[i] != 0))
                {
                    ray.Caustc[i + 1] = ray.Caustc[i] + Math.PI / 2.0;
                }
                else
                {
                    ray.Caustc[i + 1] = ray.Caustc[i];
                }
            }
            for (i = 1; i < ray.NCoords; i++)
            {
                var ap_aq = (Complex)(ray.C[0] * Math.Cos(ray.Theta) * ray.C[i] / (ray.Ic[i] * ray.Q[i]));
                ray.Amp[i] = Complex.Sqrt(ap_aq) * ray.Decay[i] * Math.Exp(-alpha * ray.S[i]);
            }
            ray.Amp[0] = double.NaN;
        }

        private CalculationResult CalcParticleVel(Settings settings, CalculationResult result = null)
        {
            double rHyd, zHyd;
            double[] xp = new double[3];
            double dr, dz;
            Complex junkComplex = new Complex(), dP_dRi = new Complex(), dP_dZi = new Complex();
            dr = settings.Output.Dr;
            dz = settings.Output.Dz;

            var _result = result ?? new CalculationResult();

            int dimR;
            int dimZ;
            switch (settings.Output.ArrayType)
            {
                case ArrayType.Horizontal:
                    dimR = settings.Output.NArrayR;
                    dimZ = 1;
                    break;

                case ArrayType.Vertical:
                    dimR = 1;
                    dimZ = settings.Output.NArrayZ;
                    break;

                case ArrayType.Linear:
                    dimR = 1;
                    dimZ = settings.Output.NArrayZ;
                    break;

                case ArrayType.Rectangular:
                    dimR = settings.Output.NArrayR;
                    dimZ = settings.Output.NArrayZ;
                    break;

                default:
                    throw new CalculationException("Unknown array type", nameof(settings.Output.ArrayType));
            }

            Complex[,] dP_dR2D = new Complex[dimR, dimZ];
            Complex[,] dP_dZ2D = new Complex[dimR, dimZ];

            switch (settings.Output.ArrayType)
            {
                case ArrayType.Horizontal:
                case ArrayType.Vertical:
                case ArrayType.Rectangular:
                    for (var j = 0; j < settings.Output.NArrayR; j++)
                    {
                        rHyd = settings.Output.ArrayR[j];

                        for (var k = 0; k < settings.Output.NArrayZ; k++)
                        {
                            zHyd = settings.Output.ArrayZ[k];

                            xp[0] = rHyd - dr;
                            xp[1] = rHyd;
                            xp[2] = rHyd + dr;

                            IntComplexBarycParab1D(xp, settings.Output.PressureH.GetThirdDimension(j, k), rHyd, ref junkComplex, ref dP_dRi, ref junkComplex);

                            dP_dR2D[j,k] = -Complex.ImaginaryOne * dP_dRi;

                            xp[0] = zHyd - dz;
                            xp[1] = zHyd;
                            xp[2] = zHyd + dz;

                            IntComplexBarycParab1D(xp, settings.Output.PressureV.GetThirdDimension(j, k), zHyd, ref junkComplex, ref dP_dZi, ref junkComplex);

                            dP_dZ2D[j,k] = Complex.ImaginaryOne * dP_dZi;
                        }
                    }
                    break;
                case ArrayType.Linear:
                    for (var j = 0; j < settings.Output.NArrayR; j++)
                    {
                        rHyd = settings.Output.ArrayR[j];
                        zHyd = settings.Output.ArrayZ[j];

                        xp[0] = rHyd - dr;
                        xp[1] = rHyd;
                        xp[2] = rHyd + dr;

                        IntComplexBarycParab1D(xp, settings.Output.PressureH.GetThirdDimension(0, j), rHyd, ref junkComplex, ref dP_dRi, ref junkComplex);

                        dP_dR2D[0,j] = -Complex.ImaginaryOne * dP_dRi;

                        xp[0] = zHyd - dz;
                        xp[1] = zHyd;
                        xp[2] = zHyd + dz;

                        IntComplexBarycParab1D(xp, settings.Output.PressureV.GetThirdDimension(0, j), zHyd, ref junkComplex, ref dP_dZi, ref junkComplex);

                        dP_dZ2D[0,j] = Complex.ImaginaryOne * dP_dZi;
                    }
                    break;
            }

            switch (settings.Output.ArrayType)
            {
                case ArrayType.Horizontal:
                case ArrayType.Rectangular:
                case ArrayType.Vertical:
                    _result.U = dP_dR2D.Transpose();
                    _result.W = dP_dZ2D.Transpose();
                    break;
                case ArrayType.Linear:
                    _result.U = (Complex[,])dP_dR2D.Clone();
                    _result.W = (Complex[,])dP_dZ2D.Clone();
                    break;
            }

            return _result;
        }

        private CalculationResult CalcRayCoords(Settings settings)
        {
            Ray[] rays = new Ray[settings.Source.NThetas];
            for (var i = 0; i < settings.Source.NThetas; i++)
            {
                rays[i] = new Ray();

                double thetai = -settings.Source.Thetas[i] * Math.PI / 180.0;
                rays[i].Theta = thetai;
                double ctheta = Math.Abs(Math.Cos(thetai));
                if (ctheta > 1.0e-7)
                {
                    SolveEikonalEq(settings, rays[i]);
                }
            }
            return new CalculationResult
            {
                Thethas = settings.Source.Thetas,
                Rays = rays
            };
        }

        private CalculationResult CalcCohTransLoss(Settings settings, CalculationResult result = null)
        {
            var _result = result ?? new CalculationResult();
            switch (settings.Output.ArrayType)
            {
                case ArrayType.Rectangular:
                case ArrayType.Horizontal:
                case ArrayType.Vertical:
                    var tl2D = new double?[settings.Output.NArrayR, settings.Output.NArrayZ];
                    for (var i = 0; i < settings.Output.NArrayR; i++)
                    {
                        for (var j = 0; j < settings.Output.NArrayZ; j++)
                        {
                            var temp = -20.0 * Math.Log10(Complex.Abs(settings.Output.Pressure2D[i, j]));
                            tl2D[i, j] = double.IsFinite(temp) ? temp : (double?)null;
                        }
                    }
                    _result.TL2D = tl2D.Transpose();
                    break;

                case ArrayType.Linear:
                    int dim = Math.Max(settings.Output.NArrayR, settings.Output.NArrayZ);
                    var tl = new double[dim];

                    for (var j = 0; j < dim; j++)
                    {
                        tl[j] = -20.0 * Math.Log10(Complex.Abs(settings.Output.Pressure2D[0, j]));
                    }

                    _result.TL = tl;
                    break;
            }

            return _result;
        }

        private void GetRayParameters(Ray ray, int iHyd, double q0, double rHyd, ref double dzdr, ref double tauRay, ref double zRay, ref Complex ampRay, ref double qRay, ref double width)
        {
            Complex junkComplex = new Complex();
            double junkDouble = 0;
            if (ray.IRefl[iHyd + 1] == true)
            {
                iHyd--;
            }

            IntLinear1D(ray.R, ray.Z, rHyd, ref zRay, ref dzdr, iHyd);
            IntLinear1D(ray.R, ray.Tau, rHyd, ref tauRay, ref junkDouble, iHyd);
            IntComplexLinear1D(ray.R, ray.Amp, rHyd, ref ampRay, ref junkComplex, iHyd);
            IntLinear1D(ray.R, ray.Q, rHyd, ref qRay, ref junkDouble, iHyd);

            double theta = Math.Atan(dzdr);
            width = Math.Max(Math.Abs(ray.Q[iHyd]), Math.Abs(ray.Q[iHyd + 1]));
            width /= ((q0) * Math.Cos(theta));
        }

        private void GetRayPressure(Settings settings, Ray ray, int iHyd, double q0, double rHyd, double zHyd, ref Complex pressure)
        {
            double dzdr = 0, tauRay = 0, zRay = 0, qRay = 0, width = 0;
            Complex ampRay = new Complex();

            GetRayParameters(ray, iHyd, q0, rHyd, ref dzdr, ref tauRay, ref zRay, ref ampRay, ref qRay, ref width);
            GetRayPressureExplicit(settings, ray, iHyd, zHyd, tauRay, zRay, dzdr, ampRay, width, ref pressure);
        }


        private void GetRayPressureExplicit(
            Settings settings, Ray ray, int iHyd, double zHyd, double tauRay, double zRay, double dzdr, Complex ampRay,
            double width, ref Complex pressure)
        {
            double omega, theta;
            Vector es = new Vector();
            Vector e1 = new Vector();
            Vector deltaR = new Vector();
            double dr, dz, n, sRay;
            double delay;
            double phi;

            omega = 2 * Math.PI * settings.Source.Freqx;
            theta = Math.Atan(dzdr);

            es.R = Math.Cos(theta);
            es.Z = Math.Sin(theta);
            e1.R = -es.Z;
            e1.Z = es.R;
            deltaR.R = 0.0;
            deltaR.Z = zHyd - zRay;
            dr = ray.R[iHyd + 1] - ray.R[iHyd];
            dz = ray.Z[iHyd + 1] - ray.Z[iHyd];

            n = DotProduct(deltaR, e1);
            sRay = DotProduct(deltaR, es);

            n = Math.Abs(n);
            sRay /= Math.Sqrt(dr * dr + dz * dz);

            delay = tauRay + sRay * (ray.Tau[iHyd + 1] - ray.Tau[iHyd]);
            if (n < width)
            {
                phi = (width - n) / width;
                pressure = phi * Complex.Abs(ampRay) * Complex.Exp(-Complex.ImaginaryOne * (omega * delay - ray.Phase[iHyd] - ray.Caustc[iHyd]));
            }
            else
            {
                pressure = 0.0 + 0.0 * Complex.ImaginaryOne;
            }
        }

        private int PressureStar(Settings settings, Ray ray, double rHyd, double zHyd, double q0, Complex[] pressure_H, Complex[] pressure_V)
        {
            double dzdr = 0, tauRay = 0, zRay = 0, qRay = 0, width = 0;
            Complex ampRay = new Complex();
            double rLeft = rHyd - settings.Output.Dr;
            double rRight = rHyd + settings.Output.Dr;
            double zBottom = zHyd - settings.Output.Dz;
            double zTop = zHyd + settings.Output.Dz;
            if (rLeft < settings.Source.Rbox1 ||
                rRight >= settings.Source.Rbox2)
            {
                return 0;
            }

            int iHyd = Bracket(ray.NCoords, ray.R, rLeft);
            if (iHyd != 0)
            {
                if (iHyd < ray.NCoords - 1)
                {
                    GetRayParameters(ray, iHyd, q0, rLeft, ref dzdr, ref tauRay, ref zRay, ref ampRay, ref qRay, ref width);
                    var tempPH = pressure_H[0];
                    GetRayPressureExplicit(settings, ray, iHyd, zHyd, tauRay, zRay, dzdr, ampRay, width, ref tempPH);
                    pressure_H[0] = tempPH;
                }
            }
            else
            {
                return 0;
            }


            iHyd = Bracket(ray.NCoords, ray.R, rHyd);
            if (iHyd != 0)
            {
                if (iHyd < ray.NCoords - 1)
                {
                    GetRayParameters(ray, iHyd, q0, rHyd, ref dzdr, ref tauRay, ref zRay, ref ampRay, ref qRay, ref width);
                    var tempV0 = pressure_V[0];
                    var tempV1 = pressure_V[1];
                    var tempV2 = pressure_V[2];
                    GetRayPressureExplicit(settings, ray, iHyd, zTop, tauRay, zRay, dzdr, ampRay, width, ref tempV0);
                    GetRayPressureExplicit(settings, ray, iHyd, zHyd, tauRay, zRay, dzdr, ampRay, width, ref tempV1);
                    GetRayPressureExplicit(settings, ray, iHyd, zBottom, tauRay, zRay, dzdr, ampRay, width, ref tempV2);
                    pressure_V[0] = tempV0;
                    pressure_V[1] = tempV1;
                    pressure_V[2] = tempV2;

                    pressure_H[1] = pressure_V[1];
                }
            }
            else
            {
                return 0;
            }

            iHyd = Bracket(ray.NCoords, ray.R, rRight);

            if (iHyd != 0)
            {
                if (iHyd < ray.NCoords - 1)
                {
                    GetRayParameters(ray, iHyd, q0, rRight, ref dzdr, ref tauRay, ref zRay, ref ampRay, ref qRay, ref width);
                    var tempPH2 = pressure_H[2];
                    GetRayPressureExplicit(settings, ray, iHyd, zHyd, tauRay, zRay, dzdr, ampRay, width, ref tempPH2);
                    pressure_H[2] = tempPH2;
                }
            }
            else
            {
                return 0;
            }
            return 1;
        }

        private void EBracket(int n, double[] x, double xi, ref int nb, int[] ib)
        {
            int i;
            double a, b;

            ib[0] = 0;
            nb = 0;

            for (i = 0; i < n - 2; i++)
            {
                a = Math.Min(x[i], x[i + 1]);
                b = Math.Max(x[i], x[i + 1]);
                if ((xi >= a) && (xi < b) && (nb < 50))
                {
                    ib[nb] = i;
                    nb++;
                }
            }
        }

        private int PressureMStar(Settings settings, Ray ray, double rHyd, double zHyd, double q0, Complex[] pressure_H, Complex[] pressure_V)
        {
            int i, jj;
            double dzdr = 0, tauRay = 0, zRay = 0, qRay = 0, width = 0;
            Complex ampRay = new Complex();
            int nRet = 0;
            int[] iRet = new int[51];
            Complex [] tempPressure = new Complex[3];
            double rLeft = rHyd - settings.Output.Dr;
            double rRight = rHyd + settings.Output.Dr;
            double zBottom = zHyd - settings.Output.Dz;
            double zTop = zHyd + settings.Output.Dz;
            if (rLeft < settings.Source.Rbox1 ||
                rRight >= settings.Source.Rbox2)
            {
                return 0;
            }
            for (i = 0; i < 3; i++)
            {
                pressure_H[i] = 0;
                pressure_V[i] = 0;
            }

            EBracket(ray.NCoords, ray.R, rLeft, ref nRet, iRet);
            for (jj = 0; jj < nRet; jj++)
            {
                GetRayParameters(ray, iRet[jj], q0, rLeft, ref dzdr, ref tauRay, ref zRay, ref ampRay, ref qRay, ref width);
                GetRayPressureExplicit(settings, ray, iRet[jj], zHyd, tauRay, zRay, dzdr, ampRay, width, ref tempPressure[0]);
                pressure_H[0] += tempPressure[0];
            }

            EBracket(ray.NCoords, ray.R, rHyd, ref nRet, iRet);
            for (jj = 0; jj < nRet; jj++)
            {
                GetRayParameters(ray, iRet[jj], q0, rHyd, ref dzdr, ref tauRay, ref zRay, ref ampRay, ref qRay, ref width);

                GetRayPressureExplicit(settings, ray, iRet[jj], zTop, tauRay, zRay, dzdr, ampRay, width, ref tempPressure[0]);
                GetRayPressureExplicit(settings, ray, iRet[jj], zHyd, tauRay, zRay, dzdr, ampRay, width, ref tempPressure[1]);
                GetRayPressureExplicit(settings, ray, iRet[jj], zBottom, tauRay, zRay, dzdr, ampRay, width, ref tempPressure[2]);
                pressure_V[0] += tempPressure[0];
                pressure_V[1] += tempPressure[1];
                pressure_H[1] += tempPressure[1];
                pressure_V[2] += tempPressure[2];
            }

            EBracket(ray.NCoords, ray.R, rRight, ref nRet, iRet);

            for (jj = 0; jj < nRet; jj++)
            {
                GetRayParameters(ray, iRet[jj], q0, rRight, ref dzdr, ref tauRay, ref zRay, ref ampRay, ref qRay, ref width);
                GetRayPressureExplicit(settings, ray, iRet[jj], zHyd, tauRay, zRay, dzdr, ampRay, width, ref tempPressure[2]);
                pressure_H[2] += tempPressure[2];
            }
            
            return 1;
        }

        private CalculationResult CalcAllRayInfo(Settings settings)
        {
            var rays = new Ray[settings.Source.NThetas];
            for (var i = 0; i < settings.Source.NThetas; i++)
            {
                rays[i] = new Ray();

                double thetai = -settings.Source.Thetas[i] * Math.PI / 180.0;
                rays[i].Theta = thetai;
                double ctheta = Math.Abs(Math.Cos(thetai));
                if (ctheta > 1.0e-7)
                {
                    SolveEikonalEq(settings, rays[i]);
                    SolveDynamicEq(settings, rays[i]);
                }
            }

            return new CalculationResult
            {
                Thethas = settings.Source.Thetas,
                Rays = rays
            };
        }

        private CalculationResult CalcAmpDelPr(Settings settings)
        {
            double junkDouble = 0;
            int maxNumArrivals = 0;
            double zRay = 0, tauRay = 0;
            Complex junkComplex = new Complex(), ampRay = new Complex();
            int nRet = 0;
            int[] iRet = new int[51];
            Ray[] rays = new Ray[settings.Source.NThetas];

            Arrivals[,] arrivals = new Arrivals[settings.Output.NArrayR, settings.Output.NArrayZ];
            for (var i = 0; i < settings.Output.NArrayR; i++)
            {
                for (var j = 0; j < settings.Output.NArrayZ; j++)
                {
                    arrivals[i, j] = new Arrivals();
                }
            }

            var _result = new CalculationResult
            {
                Thethas = settings.Source.Thetas,
                HydrophoneR = settings.Output.ArrayR,
                HydrophoneZ = settings.Output.ArrayZ,
                SourceZ = settings.Source.Zx
            };
            for (var i = 0; i < settings.Source.NThetas; i++)
            {
                rays[i] = new Ray();

                double thetai = -settings.Source.Thetas[i] * Math.PI / 180.0;
                rays[i].Theta = thetai;
                double ctheta = Math.Abs(Math.Cos(thetai));
                if (ctheta > 1.0e-7)
                {
                    SolveEikonalEq(settings, rays[i]);
                    SolveDynamicEq(settings, rays[i]);
                    for (var j = 0; j < settings.Output.NArrayR; j++)
                    {
                        double rHyd = settings.Output.ArrayR[j];

                        if ((rHyd >= rays[i].RMin) && (rHyd <= rays[i].RMax))
                        {
                            double zHyd;
                            double dz;
                            if (rays[i].IReturn == false)
                            {
                                int iHyd = Bracket(rays[i].NCoords, rays[i].R, rHyd);
                                IntLinear1D(rays[i].R, rays[i].Z, rHyd, ref zRay, ref junkDouble, iHyd);
                                for (var jj = 0; jj < settings.Output.NArrayZ; jj++)
                                {
                                    zHyd = settings.Output.ArrayZ[jj];
                                    dz = Math.Abs(zRay - zHyd);
                                    if (dz < settings.Output.Miss)
                                    {
                                        IntLinear1D(rays[i].R, rays[i].Tau, rHyd, ref tauRay, ref junkDouble, iHyd);
                                        IntComplexLinear1D(rays[i].R, rays[i].Amp, rHyd, ref ampRay, ref junkComplex, iHyd);

                                        arrivals[j, jj].Arrival.Add(new ArrivalDetails
                                        {
                                            Theta = settings.Source.Thetas[i],
                                            R = rHyd,
                                            Z = zRay,
                                            Tau = tauRay,
                                            Amp = ampRay,
                                            IReturns = rays[i].IReturn,
                                            NSurRefl = rays[i].SRefl,
                                            NBotRefl = rays[i].BRefl,
                                            NObjRefl = rays[i].ORefl
                                        });
                                        arrivals[j,jj].NArrivals += 1;
                                        maxNumArrivals = Math.Max(arrivals[j,jj].NArrivals, maxNumArrivals);
                                    }
                                }
                            }
                            else
                            {
                                EBracket(rays[i].NCoords, rays[i].R, rHyd, ref nRet, iRet);
                                int l;
                                for (l = 0; l < nRet; l++)
                                {
                                    IntLinear1D(rays[i].R, rays[i].Z, rHyd, ref zRay, ref junkDouble, iRet[l]);
                                    for (var jj = 0; jj < settings.Output.NArrayZ; jj++)
                                    {
                                        zHyd = settings.Output.ArrayZ[jj];
                                        dz = Math.Abs(zRay - zHyd);

                                        arrivals[j, jj] = new Arrivals();
                                        if (dz < settings.Output.Miss)
                                        {
                                            IntLinear1D(rays[i].R, rays[i].Tau, rHyd, ref tauRay, ref junkDouble, iRet[l]);
                                            IntComplexLinear1D(rays[i].R, rays[i].Amp, (Complex)rHyd, ref ampRay, ref junkComplex, iRet[l]);

                                            arrivals[j, jj].Arrival.Add(new ArrivalDetails
                                            {
                                                Theta = settings.Source.Thetas[i],
                                                R = rHyd,
                                                Z = zRay,
                                                Tau = tauRay,
                                                Amp = ampRay,
                                                IReturns = rays[i].IReturn,
                                                NSurRefl = rays[i].SRefl,
                                                NBotRefl = rays[i].BRefl,
                                                NObjRefl = rays[i].ORefl
                                            });

                                            arrivals[j,jj].NArrivals += 1;
                                            maxNumArrivals = Math.Max(arrivals[j,jj].NArrivals, maxNumArrivals);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            _result.MaxNumArrivals = maxNumArrivals;
            _result.Arrivals = arrivals;

            return _result;
        }

        private CalculationResult CalcAmpDelRF(Settings settings)
        {
            double thetai, ctheta;
            int nRays;
            int nPossibleArrivals;
            double zRay = 0, zHyd, rHyd;
            double junkDouble = 0;
            int maxNumArrivals = 0;
            int nTrial;
            double theta0 = 0, f0;
            double fl, fr, prod;
            var tempRay = new Ray();
            Ray[] rays = new Ray[settings.Source.NThetas];

            Arrivals[,] arrivals = new Arrivals[settings.Output.NArrayR, settings.Output.NArrayZ];

            var _result = new CalculationResult
            {
                Thethas = settings.Source.Thetas,
                HydrophoneR = settings.Output.ArrayR,
                HydrophoneZ = settings.Output.ArrayZ,
                SourceZ = settings.Source.Zx
            };

            nRays = 0;

            var thetas = new double[settings.Source.NThetas];
            var depths = new double[settings.Source.NThetas, settings.Output.NArrayR];

            for (var i = 0; i < settings.Source.NThetas; i++)
            {
                rays[i] = new Ray();

                thetai = -settings.Source.Thetas[i] * Math.PI / 180.0;
                rays[i].Theta = thetai;
                ctheta = Math.Abs(Math.Cos(thetai));
                if (ctheta > 1.0e-7)
                {
                    thetas[nRays] = thetai;
                    SolveEikonalEq(settings, rays[i]);
                    SolveDynamicEq(settings, rays[i]);

                    if (rays[i].IReturn == true)
                    {
                        throw new CalculationException("Returning eigenrays can only be determined by Proximity");
                    }
                    for (var j = 0; j < settings.Output.NArrayR; j++)
                    {
                        rHyd = settings.Output.ArrayR[j];
                        if ((rHyd >= rays[i].RMin) && (rHyd <= rays[i].RMax))
                        {
                            int iHyd = Bracket(rays[i].NCoords, rays[i].R, rHyd);
                            IntLinear1D(rays[i].R, rays[i].Z, rHyd, ref zRay, ref junkDouble, iHyd);
                            depths[nRays,j] = zRay;
                        }
                        else
                        {
                            depths[nRays,j] = double.NaN;
                        }
                    }
                    nRays++;
                }
            }

            var thetaL = new double[nRays];
            var thetaR = new double[nRays];
            var dz = new double[nRays];

            for (var i = 0; i < settings.Output.NArrayR; i++)
            {
                rHyd = settings.Output.ArrayR[i];
                for (var j = 0; j < settings.Output.NArrayZ; j++)
                {
                    zHyd = settings.Output.ArrayZ[j];
                    for (var k = 0; k < nRays; k++)
                    {
                        dz[k] = zHyd - depths[k,i];
                    }

                    nPossibleArrivals = 0;
                    for (var k = 0; k < nRays - 1; k++)
                    {
                        fl = dz[k];
                        fr = dz[k + 1];
                        prod = fl * fr;

                        if (double.IsNaN(depths[k,i]) == false &&
                            double.IsNaN(depths[k + 1,i]) == false)
                        {

                            if ((fl == 0.0) && (fr != 0.0))
                            {
                                thetaL[nPossibleArrivals] = thetas[k];
                                thetaR[nPossibleArrivals] = thetas[k + 1];
                                nPossibleArrivals++;

                            }
                            else if ((fr == 0.0) && (fl != 0.0))
                            {
                                thetaL[nPossibleArrivals] = thetas[k];
                                thetaR[nPossibleArrivals] = thetas[k + 1];
                                nPossibleArrivals++;

                            }
                            else if (prod < 0.0)
                            {
                                thetaL[nPossibleArrivals] = thetas[k];
                                thetaR[nPossibleArrivals] = thetas[k + 1];
                                nPossibleArrivals++;
                            }
                        }
                        if (nPossibleArrivals > nRays)
                        {
                            throw new CalculationException("Unexpecte error. Number of possible eigenrays exceeds number of calculated rays");
                        }
                    }

                    arrivals[i, j] = new Arrivals();
                    int nFoundArrivals = 0;
                    for (var l = 0; l < nPossibleArrivals; l++)
                    {
                        settings.Source.Rbox2 = rHyd + 1;
                        tempRay.Theta = thetaL[l];
                        SolveEikonalEq(settings, tempRay);
                        fl = tempRay.Z[tempRay.NCoords - 1] - zHyd;
                        tempRay.Init(settings.Source.NThetas);
                        tempRay.Theta = thetaR[l];
                        SolveEikonalEq(settings, tempRay);
                        fr = tempRay.Z[tempRay.NCoords - 1] - zHyd;
                        tempRay.Init(settings.Source.NThetas);

                        bool success;
                        if (Math.Abs(fl) <= settings.Output.Miss)
                        {
                            theta0 = thetaL[l];
                            nFoundArrivals++;
                            success = true;

                        }
                        else if (Math.Abs(fr) <= settings.Output.Miss)
                        {
                            theta0 = thetaR[l];
                            nFoundArrivals++;
                            success = true;
                        }
                        else
                        {
                            nTrial = 0;
                            success = false;
                            while (success == false)
                            {
                                nTrial++;

                                if (nTrial > 21)
                                {
                                    break;
                                }

                                theta0 = thetaR[l] - fr * (thetaL[l] - thetaR[l]) / (fl - fr);
                                tempRay.Theta = theta0;
                                SolveEikonalEq(settings, tempRay);
                                f0 = tempRay.Z[tempRay.NCoords - 1] - zHyd;
                                tempRay.Init(0);
                                if (Math.Abs(f0) < settings.Output.Miss)
                                {
                                    success = true;
                                    nFoundArrivals++;
                                    break;
                                }
                                else
                                {
                                    prod = fl * f0;

                                    if (prod < 0.0)
                                    {
                                        thetaR[l] = theta0;
                                        fr = f0;
                                    }
                                    else
                                    {
                                        thetaL[l] = theta0;
                                        fl = f0;
                                    }
                                }
                            }
                        }
                        if (success == true)
                        {
                            tempRay.Theta = theta0;
                            SolveEikonalEq(settings, tempRay);
                            SolveDynamicEq(settings, tempRay);

                            arrivals[i, j].Arrival.Add(new ArrivalDetails
                            {
                                Theta = settings.Source.Thetas[i],
                                R = tempRay.R[tempRay.NCoords - 1],
                                Z = tempRay.Z[tempRay.NCoords - 1],
                                Tau = tempRay.Tau[tempRay.NCoords - 1],
                                Amp = tempRay.Amp[tempRay.NCoords - 2],
                                IReturns = tempRay.IReturn,
                                NSurRefl = tempRay.SRefl,
                                NBotRefl = tempRay.BRefl,
                                NObjRefl = tempRay.ORefl
                            });

                            arrivals[i,j].NArrivals += 1;
                            maxNumArrivals = Math.Max(arrivals[i,j].NArrivals, maxNumArrivals);
                        }
                    }
                }
            }
            _result.MaxNumArrivals = maxNumArrivals;
            _result.Arrivals = arrivals;
            _result.Rays = rays;

            return _result;
        }

        private CalculationResult CalcCohAcoustPress(Settings settings)
        {
            var rays = new Ray[settings.Source.NThetas];
            double ctheta, thetai, cx = 0, q0;
            double junkDouble = 0;
            Vector junkVector = new Vector();
            double rHyd, zHyd;
            Complex pressure = new Complex();
            var pressure_H = new Complex[3];
            var pressure_V = new Complex[3];
            int nRet = 0;
            int[] iRet = new int[51];
            int dimR;
            int dimZ;
            switch (settings.Output.ArrayType)
            {
                case ArrayType.Horizontal:
                    dimR = settings.Output.NArrayR;
                    dimZ = 1;
                    break;

                case ArrayType.Vertical:
                    dimR = 1;
                    dimZ = settings.Output.NArrayZ;
                    break;

                case ArrayType.Linear:
                    dimR = settings.Output.NArrayR;
                    dimZ = settings.Output.NArrayZ;
                    break;

                case ArrayType.Rectangular:
                    dimR = settings.Output.NArrayR;
                    dimZ = settings.Output.NArrayZ;
                    break;

                default:
                    throw new CalculationException("Unknown array type", nameof(settings.Output.ArrayType));
            }

            var _result = new CalculationResult
            {
                Thethas = settings.Source.Thetas,
                HydrophoneR = settings.Output.ArrayR,
                HydrophoneZ = settings.Output.ArrayZ
            };

            CsValues(settings, settings.Source.Rx, settings.Source.Zx, ref cx,
                ref junkDouble, ref junkDouble, ref junkDouble, ref junkDouble,
                junkVector, ref junkDouble, ref junkDouble, ref junkDouble);

            q0 = cx / (Math.PI * settings.Source.DTheta / 180.0);

            if (settings.Output.CalculationType == CalculationType.PartVelocity ||
                settings.Output.CalculationType == CalculationType.CohAccousicPressurePartVelocity)
            {
                var lambda = cx / settings.Source.Freqx;
                double dr = lambda / 10;
                double dz = lambda / 10;

                for (var i = 1; i < settings.Output.NArrayR; i++)
                {
                    dr = Math.Min(Math.Abs(settings.Output.ArrayR[i] - settings.Output.ArrayR[i - 1]), dr);
                }
                for (var i = 1; i < settings.Output.NArrayZ; i++)
                {
                    dz = Math.Min(Math.Abs(settings.Output.ArrayZ[i] - settings.Output.ArrayZ[i - 1]), dz);
                }

                settings.Output.Dr = dr;
                settings.Output.Dz = dz;
                settings.Output.PressureH = new Complex[dimR,dimZ,3];
                settings.Output.PressureV = new Complex[dimR, dimZ, 3];
            }
            if (settings.Output.CalculationType == CalculationType.CohAcousticPressure ||
                settings.Output.CalculationType == CalculationType.CohTransmissionLoss ||
                settings.Output.CalculationType == CalculationType.CohAccousicPressurePartVelocity)
            {
                settings.Output.Pressure2D = new Complex[dimR, dimZ];
            }
            for (var i = 0; i < settings.Source.NThetas; i++)
            {
                rays[i] = new Ray();

                thetai = -settings.Source.Thetas[i] * Math.PI / 180.0;
                rays[i].Theta = thetai;
                ctheta = Math.Abs(Math.Cos(thetai));
                if (ctheta > 1.0e-7)
                {
                    SolveEikonalEq(settings, rays[i]);
                    SolveDynamicEq(settings, rays[i]);

                    switch (settings.Output.CalculationType)
                    {
                        case CalculationType.PartVelocity:
                        case CalculationType.CohAccousicPressurePartVelocity:
                            switch (settings.Output.ArrayType)
                            {
                                case ArrayType.Horizontal:
                                case ArrayType.Vertical:
                                case ArrayType.Rectangular:

                                    for (var j = 0; j < dimR; j++)
                                    {
                                        rHyd = settings.Output.ArrayR[j];

                                        if (rHyd >= rays[i].RMin && rHyd < rays[i].RMax)
                                        {
                                            if (rays[i].IReturn == false)
                                            { 
                                                for (var k = 0; k < dimZ; k++)
                                                {
                                                    zHyd = settings.Output.ArrayZ[k];

                                                    if (PressureStar(settings, rays[i], rHyd, zHyd, q0, pressure_H, pressure_V) != 0)
                                                    {
                                                        for (var l = 0; l < 3; l++)
                                                        {
                                                            settings.Output.PressureH[j,k,l] += pressure_H[l];
                                                            settings.Output.PressureV[j,k,l] += pressure_V[l];
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                for (var k = 0; k < dimZ; k++)
                                                {
                                                    zHyd = settings.Output.ArrayZ[k];
                                                    if (PressureMStar(settings, rays[i], rHyd, zHyd, q0, pressure_H, pressure_V) != 0)
                                                    {
                                                        for (var l = 0; l < 3; l++)
                                                        {
                                                            settings.Output.PressureH[j,k,l] += pressure_H[l];
                                                            settings.Output.PressureV[j,k,l] += pressure_V[l];
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("pressureMStar returned false => at least one of the pressure contribution points is outside rBox");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Hydrophone not within range of ray coordinates");
                                        }
                                    }
                                    break;
                                case ArrayType.Linear:
                                    for (var j = 0; j < dimR; j++)
                                    {
                                        rHyd = settings.Output.ArrayR[j];
                                        if (rHyd >= rays[i].RMin && rHyd < rays[i].RMax)
                                        {
                                            zHyd = settings.Output.ArrayZ[j];

                                            if (rays[i].IReturn == false)
                                            {
                                                if (PressureStar(settings, rays[i], rHyd, zHyd, q0, pressure_H, pressure_V) != 0)
                                                {
                                                    for (var l = 0; l < 3; l++)
                                                    {
                                                        settings.Output.PressureH[0,j,l] += pressure_H[l];
                                                        settings.Output.PressureV[0,j,l] += pressure_V[l];
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (PressureMStar(settings, rays[i], rHyd, zHyd, q0, pressure_H, pressure_V) != 0)
                                                {
                                                    for (var l = 0; l < 3; l++)
                                                    {
                                                        settings.Output.PressureH[0,j,l] += pressure_H[l];
                                                        settings.Output.PressureV[0,j,l] += pressure_V[l];
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    throw new CalculationException("Unknown array type", nameof(settings.Output.ArrayType));
                            }
                            break;

                        case CalculationType.CohAcousticPressure:
                        case CalculationType.CohTransmissionLoss:
                            int iHyd;
                            switch (settings.Output.ArrayType)
                            {
                                case ArrayType.Linear:

                                    for (var j = 0; j < dimZ; j++)
                                    {
                                        rHyd = settings.Output.ArrayR[j];
                                        zHyd = settings.Output.ArrayZ[j];

                                        if (rHyd >= rays[i].RMin && rHyd < rays[i].RMax)
                                        {
                                            if (rays[i].IReturn == false)
                                            {
                                                iHyd = Bracket(rays[i].NCoords, rays[i].R, rHyd);
                                                GetRayPressure(settings, rays[i], iHyd, q0, rHyd, zHyd, ref pressure);
                                                settings.Output.Pressure2D[0, j] += pressure;

                                            }
                                            else
                                            {
                                                EBracket(rays[i].NCoords, rays[i].R, rHyd, ref nRet, iRet);

                                                for (var jj = 0; jj < nRet; jj++)
                                                {
                                                    for (var k = 0; k < dimZ; k++)
                                                    {
                                                        zHyd = settings.Output.ArrayZ[k];
                                                        GetRayPressure(settings, rays[i], iRet[jj], q0, rHyd, zHyd, ref pressure);
                                                        settings.Output.Pressure2D[0, j] += pressure;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;

                                case ArrayType.Horizontal:
                                case ArrayType.Vertical:
                                case ArrayType.Rectangular:
                                    for (var j = 0; j < dimR; j++)
                                    {
                                        rHyd = settings.Output.ArrayR[j];
                                        if (rHyd >= rays[i].RMin && rHyd < rays[i].RMax)
                                        {
                                            if (rays[i].IReturn == false)
                                            {
                                                iHyd = Bracket(rays[i].NCoords, rays[i].R, rHyd);
                                                for (var k = 0; k < dimZ; k++)
                                                {

                                                    zHyd = settings.Output.ArrayZ[k];
                                                    GetRayPressure(settings, rays[i], iHyd, q0, rHyd, zHyd, ref pressure);

                                                    settings.Output.Pressure2D[j, k] += pressure;
                                                }
                                            }
                                            else
                                            {
                                                EBracket(rays[i].NCoords, rays[i].R, rHyd, ref nRet, iRet);
                                                for (var k = 0; k < dimZ; k++)
                                                {
                                                    zHyd = settings.Output.ArrayZ[k];

                                                    for (var jj = 0; jj < nRet; jj++)
                                                    {
                                                        GetRayPressure(settings, rays[i], iRet[jj], q0, rHyd, zHyd, ref pressure);
                                                        settings.Output.Pressure2D[j, k] += pressure;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    throw new CalculationException("Unknown array type", nameof(settings.Output.ArrayType));
                            }
                            break;
                        default:
                            throw new CalculationException("Unknown output type", nameof(settings.Output.CalculationType));
                    }
                }
            }
            if (settings.Output.CalculationType == CalculationType.CohAcousticPressure ||
                settings.Output.CalculationType == CalculationType.CohAccousicPressurePartVelocity)
            {
                if (settings.Output.CalculationType == CalculationType.CohAccousicPressurePartVelocity)
                {
                    for (var j = 0; j < dimR; j++)
                    {
                        for (var i = 0; i < dimZ; i++)
                        {
                            settings.Output.Pressure2D[j,i] = settings.Output.PressureH[j,i,1];
                        }
                    }
                }
                switch (settings.Output.ArrayType)
                {
                    case ArrayType.Linear:
                        _result.Pressure2D = settings.Output.Pressure2D;
                        break;
                    case ArrayType.Vertical:
                    case ArrayType.Horizontal:
                    case ArrayType.Rectangular:
                        _result.Pressure2D = settings.Output.Pressure2D.Transpose();
                        break;
                }
            }

            return _result;
        }
    }
}