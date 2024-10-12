using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfNURBSCurve
{
    public class NURBS :NotifierBase
    {

        private ObservableCollection<RationalBSplinePoint> pWeightedPointSeries = new ObservableCollection<RationalBSplinePoint>();

        public ObservableCollection<RationalBSplinePoint> WeightedPointSeries
        {
            get { return pWeightedPointSeries; }
            set
            {
                SetProperty(ref pWeightedPointSeries, value);
            }
        }

        private bool pIsBSpline = true;

        public bool IsBSpline
        {
            get { return pIsBSpline; }
            set
            {
                SetProperty(ref pIsBSpline, value);
            }
        }


        /// <summary>
        /// This code is translated to C# from the original C++  code given on page 74-75 in "The NURBS Book" by Les Piegl and Wayne Tiller 
        /// </summary>
        /// <param name="i">Current control pont</param>
        /// <param name="p">The picewise polynomial degree</param>
        /// <param name="U">The knot vector</param>
        /// <param name="u">The value of the current curve point. Valid range from 0 <= u <=1 </param>
        /// <returns>N_{i,p}(u)</returns>
        private double Nip(int i, int p, double[] U, double u)
        {
            double[] N = new double[p + 1];
            double saved, temp;

            int m = U.Length - 1;
            if ((i == 0 && u == U[0]) || (i == (m - p - 1) && u == U[m]))
                return 1;

            if (u < U[i] || u >= U[i + p + 1])
                return 0;

            for (int j = 0; j <= p; j++)
            {
                if (u >= U[i + j] && u < U[i + j + 1])
                    N[j] = 1d;
                else
                    N[j] = 0d;
            }

            for (int k = 1; k <= p; k++)
            {
                if (N[0] == 0)
                    saved = 0d;
                else
                    saved = ((u - U[i]) * N[0]) / (U[i + k] - U[i]);

                for (int j = 0; j < p - k + 1; j++)
                {
                    double Uleft = U[i + j + 1];
                    double Uright = U[i + j + k + 1];

                    if (N[j + 1] == 0)
                    {
                        N[j] = saved;
                        saved = 0d;
                    }
                    else
                    {
                        temp = N[j + 1] / (Uright - Uleft);
                        N[j] = saved + (Uright - u) * temp;
                        saved = (u - Uleft) * temp;
                    }
                }
            }
            return N[0];
        }

        public PointCollection BSplineCurve(ObservableCollection<RationalBSplinePoint> Points, int Degree, double[] KnotVector, double StepSize)
        {
            PointCollection Result = new PointCollection();

            for (double i = 0; i < 1; i += StepSize)
            {
                if (this.IsBSpline)
                    Result.Add(BSplinePoint(Points, Degree, KnotVector, i));
                else
                    Result.Add(RationalBSplinePoint(Points, Degree, KnotVector, i));

            }

            if (!Result.Contains(Points[Points.Count - 1].MyPoint))
                Result.Add(Points[Points.Count - 1].MyPoint);

            return Result;
        }

        Point BSplinePoint(ObservableCollection<RationalBSplinePoint> Points, int degree, double[] KnotVector, double t)
        {

            double x, y;
            x = 0;
            y = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                double temp = Nip(i, degree, KnotVector, t);
                x += Points[i].MyPoint.X * temp;
                y += Points[i].MyPoint.Y * temp;
            }
            return new Point(x, y);
        }

        Point RationalBSplinePoint(ObservableCollection<RationalBSplinePoint> Points, int degree, double[] KnotVector, double t)
        {

            double x, y;
            x = 0;
            y = 0;
            double rationalWeight = 0d;

            for (int i = 0; i < Points.Count; i++)
            {
                double temp = Nip(i, degree, KnotVector, t)*Points[i].Weight;
                rationalWeight += temp;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                double temp = Nip(i, degree, KnotVector, t);
                x += Points[i].MyPoint.X*Points[i].Weight * temp/rationalWeight;
                y += Points[i].MyPoint.Y * Points[i].Weight * temp / rationalWeight;
            }
            return new Point(x, y);
        }


    }
}
