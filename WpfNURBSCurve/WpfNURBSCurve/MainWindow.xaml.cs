using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfNURBSCurve
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            lstPoints.ItemsSource = MyNurbs.WeightedPointSeries;
        }

        private NURBS MyNurbs = new NURBS();

        private void canDrawing_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Point p = new Point();
            p = e.MouseDevice.GetPosition(canDrawing);

            MyNurbs.WeightedPointSeries.Add(new RationalBSplinePoint(p, 1d));
            txtDegree.Text = (MyNurbs.WeightedPointSeries.Count - 1).ToString();

            double radius = 0;
            radius = 2.5;

            Ellipse c = new Ellipse();
            c.Fill = Brushes.Red;
            c.Width = radius * 2;
            c.Height = radius * 2;

            Canvas.SetLeft(c, p.X - c.Height / 2);
            Canvas.SetTop(c, p.Y - c.Width / 2);
            canDrawing.Children.Add(c);
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            int deg;
            if (int.TryParse(txtDegree.Text, out deg))
                PlotPoints(MyNurbs.BSplineCurve(MyNurbs.WeightedPointSeries, deg, CreateKnotVector(txtKnotVector.Text), 0.05));
        }

        private void PlotPoints(PointCollection p)
        {
            for (int i = 0; i <= p.Count - 2; i++)
            {
                Line ll = new Line();
                ll.Stroke = Brushes.Black;
                ll.StrokeThickness = 2;
                ll.X1 = p[i].X;
                ll.Y1 = p[i].Y;
                ll.X2 = p[i + 1].X;
                ll.Y2 = p[i + 1].Y;
                canDrawing.Children.Add(ll);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            MyNurbs.WeightedPointSeries.Clear();
            canDrawing.Children.Clear();
            txtDegree.Text = "0";
        }

        private void rbtnKnotType(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb.Content != null)
            {
                if (rb.Content.ToString() == "Uniform")
                {
                    int deg;
                    if (int.TryParse(txtDegree.Text, out deg))
                        CalcualteKnotVector(deg, MyNurbs.WeightedPointSeries.Count, (bool)rbtnUniform.IsChecked);
                }
                else
                {
                    int deg;
                    if (int.TryParse(txtDegree.Text, out deg))
                        CalcualteKnotVector(deg, MyNurbs.WeightedPointSeries.Count, (bool)rbtnUniform.IsChecked);
                }
            }
        }

        private void txtDegree_TextChanged(object sender, TextChangedEventArgs e)
        {
            int deg;
            if (int.TryParse(txtDegree.Text, out deg))
            {
                if (rbtnUniform != null)
                    CalcualteKnotVector(deg, MyNurbs.WeightedPointSeries.Count, (bool)rbtnUniform.IsChecked);

            }
                
        }

        private void CalcualteKnotVector(int Degree,int ContolPointCount, bool IsUniform)
        {
            if (Degree + 1 > ContolPointCount || ContolPointCount == 0)
                return;

            StringBuilder outText = new StringBuilder();


            int n = ContolPointCount;
            int m = n + Degree + 1;
            int divisor = m - 1 - 2 * Degree;

            if (IsUniform)
            {
                outText.Append("0");
                for (int i = 1; i < m; i++)
                {
                    if (i >= m - 1)
                        outText.Append(", 1");
                    else
                    {
                        int dividend = m-1;
                        outText.Append(", " + i.ToString() + "/" + dividend.ToString());
                    }
                }
            }
            else
            {
                outText.Append("0");
                for (int i = 1; i < m; i++)
                {
                    if (i <= Degree)
                        outText.Append(", 0");
                    else if (i >= m - Degree - 1)
                        outText.Append(", 1");
                    else
                    {
                        int dividend = i - Degree;
                        outText.Append(", " + dividend.ToString() + "/" + divisor.ToString());
                    }
                }
            }

            txtKnotVector.Text= outText.ToString();
        }

        double[] CreateKnotVector(string input)
        {
            string[] Items = input.Split(',');
            List<double> MyList = new List<double>();
            foreach (var item in Items)
            {
                if (item.Contains("/"))
                {
                    string[] DivisionString = item.Split('/');
                    double upper, lower;
                    if (double.TryParse(DivisionString[0], out upper) && double.TryParse(DivisionString[1], out lower))
                    {
                        MyList.Add(upper / lower);
                    }
                }
                else
                {
                    double test;
                    if (double.TryParse(item, out test))
                        MyList.Add(test);
                }
            }

            return MyList.ToArray();
        }

        private void rbtnBSpline_Checked(object sender, RoutedEventArgs e)
        {
            var rbtn = sender as RadioButton;

            if (rbtn.Content != null)
            {
                if (rbtn.Content.ToString() == "NURBS")
                {
                    MyNurbs.IsBSpline = false;
                }
                else
                {
                    MyNurbs.IsBSpline = true;
                }
            } 
        }
    }
}
