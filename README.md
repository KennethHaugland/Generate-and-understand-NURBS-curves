<ul class="download">
	<li><a href="WpfNURBSCurve.zip">Download source code in C# - 85.9 KB</a></li>
</ul>

<p><img src="ProgramScreenshot.gif" style="width: 400px; height: 574px" /></p>

<h2>Introduction</h2>

<p>Every time I came across a situation where I needed to create a smooth curve from some points, one item always came up, NURBS. The tutorials online where almost always way too theoretical to my liking and I had a hard time figuring out my questions about them. In general, I wanted to know two tings about them. First, how would one create them, second, how you could manipulate and use&nbsp;them. I was hard pressed to find any simple tutorials with a working code, that also explained what was going on.</p>

<h2>Background</h2>

<p>To explain the basic concepts behind the B-Spline, I will return to the Bezier curve for a moment. If you haven&#39;t already seen my articles dealing with the Bezier curve, I urge you to read it (<a href="http://www.codeproject.com/Articles/747928/Spline-Interpolation-history-theory-and-implementa">Spline Interpolation - history, theory and implementation</a>).</p>

<p>Assume that I have two points, respectively <span class="math">\((x_0,y_0)\)</span> and <span class="math">\((x_1,y_1)\)</span> and want to connect these with a simple line, i.a. a Bezier curve of degree 1. To blend these two I also need a value t that tells me if I&#39;m close to the first point <span class="math">\((x_0,y_0)\)</span> and I call the corresponding&nbsp; <span class="math">\(t\)</span> value in that point for <span class="math">\(t_0\)</span> and call the <span class="math">t</span> value in point <span class="math">\((x_1,y_1)\)</span> for <span class="math">\(t_1\)</span>. I also demand that the value <span class="math">\(t_1 &gt; t_0\)</span> and that the resulting function is equal to zero if it is lower than <span class="math">\(t_0\)</span> and higher than <span class="math">\(t_1\)</span>. The most general way of writing the blending function for x and y is then:</p>

<div class="math">$y(t)_{\text{$x_0$ to $x_1$}} = \frac{t_1 - t}{t_1 - t_0} \cdot y_0 + \frac{t-t_0}{t_1 - t_0} \cdot y_1 \text{ for } t \in [ t_0, t_1 ]$</div>

<p>The common choice for start and endpoints of the <span class="math">\(t\)</span> is <span class="math">\(t_0 = 0\)</span> and <span class="math">\(t_1 = 1\)</span> giving a much more readable equation on the from:</p>

<div class="math">$y(t)_{\text{$x_0$ to $x_1$}} = (1-t) \cdot y_0 + t \cdot y_1 $</div>

<p>I will later need the general form with <span class="math">\(t_0\)</span> and <span class="math">\(t_1\)</span> values when I want to deal with the B-Spline blending functions. You should also be aware that adding the two functions always gives:</p>

<div class="math">$\frac{t_1 - t}{t_1 - t_0} + \frac{t-t_0}{t_1 - t_0} = 1$</div>

<p>In mathematics, this is called a convex combination of <span class="math">y_0</span> and <span class="math">y_1</span>.</p>

<p>At the first glance it might not seem that we have gained anything by this. But lets add another point called <span class="math">\((x_2,y_2)\)</span> and set up the blending formula between <span class="math">\((x_1,y_1)\)</span> and <span class="math">\((x_2,y_2)\)</span> :</p>

<div class="math">$y(t)_{\text{$x_1$ to $x_2$}} = (1-t) \cdot y_1+ t \cdot y_2$</div>

<p>I can now combine the two resulting equations in a new blending function:</p>

<div class="math">$y(t)_{\text{$x_0$ to $x_2$}} = (1-t) \cdot y(t)_{\text{$x_0$ to $x_1$}}&nbsp;+ t \cdot y(t)_{\text{$x_1$ to $x_2$}}$</div>

<p>These three points will now be combined into a second degree piecewise polynomial Bezier curve, and the formula above can be intuitively&nbsp;illustrated with the following image (from <a href="https://en.wikipedia.org/wiki/B%C3%A9zier_curve">Wikipedia</a>):</p>

<div><img src="B_zier_2_big.gif" style="width: 360px; height: 150px" /></div>

<div class="math">There is, however, no way of controlling what degree the piecewise polynomial would have. It is simply determined by the number of points in the curve.</div>

<h2>B-Spline</h2>

<p>The B-Spline allows you to modulate the degree of the curve, but in order to be able to do that, it is pretty clear that you need to be able to adjust when the t&#39;s are implemented. In fact, we actually need an own vector that will hold the <span class="math">\(t\)</span> values.&nbsp;</p>

<p>We would also have to place some restrictions on the knot vector, called <span class="math">\(U\)</span>, namely that the number of elements, <span class="math">\(m\)</span>, arranged as&nbsp;<span class="math">\({t_0}, \cdots , t_i , \cdots, t_{m-1}\)</span>&nbsp;and the sequence must satisfy <span class="math">\(t_i \le t_{i+1}\)</span>. In addition, I have also chosen&nbsp;the interval&nbsp;to be <span class="math">\(t_0 = 0\)</span> and <span class="math">\(t_{m-1}=1\)</span>. The knot span is half open meaning that the interval consists of <span class="math">\([u_i, u_{i+1})\)</span> , outside this interval, as it would be if the two values are equal, the associated knot span would also be 0. Furthermore, the number of knots <span class="math">\(m\)</span>, the degree <span class="math">\(p\)</span> and the&nbsp;number of control points <span class="math">\(n\)</span> are connected by the formula <span class="math">\(n = m - p -1\)</span>. &nbsp;</p>

<p>In order to calculate the i-th B-spline function of degree p using the Cox-de Boor formula, one starts with the&nbsp;B-spline of 0 degrees:</p>

<div class="math">$N_{i,0}(t) = \begin{cases} 1 &amp; \text{if $t_i \le t \lt t_{i+1}$} \\[2ex] 0 &amp; \text{otherwise} \end{cases}$</div>

<p>And a recursive formula that connects the lower degree with the higher degree B-Spline function is as follows:</p>

<div class="math">$N_{i,p}(t)=\frac{t - t_i}{t_{i+p}-t_i}N_{i, p-1}(t) + \frac{t_{i+p+1}-t}{t_{i+p+1}-t_{i+1}}N_{i+1,p-1}$</div>

<p>The code for <span class="math">\(N_{i,p}(t)\)</span> is then possible to construct:</p>

<pre lang="cs">
        /// &lt;summary&gt;
        /// This code is translated to C# from the original C++ code given on page 74-75 in &quot;The NURBS Book&quot; by Les Piegl and Wayne Tiller 
        /// &lt;/summary&gt;
        /// &lt;param name=&quot;i&quot;&gt;Current control pont&lt;/param&gt;
        /// &lt;param name=&quot;p&quot;&gt;The picewise polynomial degree&lt;/param&gt;
        /// &lt;param name=&quot;U&quot;&gt;The knot vector&lt;/param&gt;
        /// &lt;param name=&quot;u&quot;&gt;The value of the current curve point. Valid range from 0 &lt;= u &lt;=1 &lt;/param&gt;
        /// &lt;returns&gt;N_{i,p}(u)&lt;/returns&gt;
        private double Nip(int i, int p, double[] U, double u)
        {
            double[] N = new double[p + 1];
            double saved, temp;

            int m = U.Length - 1;
            if ((i == 0 &amp;&amp; u == U[0]) || (i == (m - p - 1) &amp;&amp; u == U[m]))
                return 1;

            if (u &lt; U[i] || u &gt;= U[i + p + 1])
                return 0;

            for (int j = 0; j &lt;= p; j++)
            {
                if (u &gt;= U[i + j] &amp;&amp; u &lt; U[i + j + 1])
                    N[j] = 1d;
                else
                    N[j] = 0d;
            }

            for (int k = 1; k &lt;= p; k++)
            {
                if (N[0] == 0)
                    saved = 0d;
                else
                    saved = ((u - U[i]) * N[0]) / (U[i + k] - U[i]);

                for (int j = 0; j &lt; p - k + 1; j++)
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
        }</pre>

<p>There are some special cases in the beginning and a precheck on what knot vectors that isnt 0 makes the code look a bit worse than it really is. The second for loop is where the recursive Cox-de Boor formula is implemented.</p>

<p>It is also possible to create a B-Spline function for any derivative of the B-Spline. I will not give the code here though, but it can be found in The NURBS book, where the code for Nip is from too.</p>

<p>Assuming that we have a valid knot vector and degree on the B-Spline function it is easy to generate the curve by calcualting for all t from 0 to 1:</p>

<pre lang="cs">
        Point BSplinePoint(PointCollection Points, int degree, double[] KnotVector, double t)
        {

            double x, y;
            x = 0;
            y = 0;
            for (int i = 0; i &lt; Points.Count; i++)
            {
                double temp = Nip(i, degree, KnotVector, t);
                x += Points[i].X * temp;
                y += Points[i].Y * temp;
            }
            return new Point(x, y);
        }</pre>

<h2>Creating a B-Spline knot vector</h2>

<p>The highest degree <span class="math">\(p\)</span> you could have on a B-Spline curve, is actually a Bezier curve, a higher curve would not have enough control points to generate it. The knot vector for the Bezier spline is:</p>

<div class="math">$U = \{ \underbrace{0, \ldots , 0}_{p+1} , \underbrace{1, \ldots , 1}_{p+1} \}$</div>

<p>Any lower degree, that passes trough the start point and endpoint where they are clamped (or open, sadly the terminology in literature varies), will have the general form:</p>

<div class="math">$U = \{ \underbrace{0, \ldots , 0}_{p+1} , u_{p+1}, \ldots , u_{m-p-1}, \underbrace{1, \ldots , 1}_{p+1} \}$</div>

<p>There are many different ways that you can create a B-Spline with different coefficients in the knot vector, the only requirement is that the next value is either equal or higher than the previous one. When the knot vector is clamped it is called nonperiodic or non-uniform B-Spline, the reason beeing that the number of control point spans it includes in making the spline, will vary from 1 to the selected degree as illustrated below:</p>

<p><img src="nonunifrom.gif" style="width: 500px; height: 488px" /></p>

<p>There are two methods provided to generate a knot vector in the program. One has a&nbsp;uniform internal knot span that is clamped in the end and beginning, and the other has a uniform knot span for all points.&nbsp;</p>

<pre lang="cs">
   private void CalcualteKnotVector(int Degree,int ContolPointCount, bool IsUniform)
        {
            if (Degree + 1 &gt; ContolPointCount || ContolPointCount == 0)
                return;

            StringBuilder outText = new StringBuilder();

            int n = ContolPointCount;
            int m = n + Degree + 1;
            int divisor = m - 1 - 2 * Degree;

            if (IsUniform)
            {
                outText.Append(&quot;0&quot;);
                for (int i = 1; i &lt; m; i++)
                {
                    if (i &gt;= m - 1)
                        outText.Append(&quot;, 1&quot;);
                    else
                    {
                        int dividend = m-1;
                        outText.Append(&quot;, &quot; + i.ToString() + &quot;/&quot; + dividend.ToString());
                    }
                }
            }
            else
            {
                outText.Append(&quot;0&quot;);
                for (int i = 1; i &lt; m; i++)
                {
                    if (i &lt;= Degree)
                        outText.Append(&quot;, 0&quot;);
                    else if (i &gt;= m - Degree - 1)
                        outText.Append(&quot;, 1&quot;);
                    else
                    {
                        int dividend = i - Degree;
                        outText.Append(&quot;, &quot; + dividend.ToString() + &quot;/&quot; + divisor.ToString());
                    }
                }
            }

            txtKnotVector.Text= outText.ToString();
        }</pre>

<p>The series these produce can of cource be altered to anything you&#39;d like, but having this as a startingpoint help me very much in the beginning. You should also note that you can start with negative numbers and end on numbers higher then 1 in the knot vector, typically useful for creating polygon chapes.</p>

<h2>Non Uniform Rational B-Splines (NURBS) curve</h2>

<p>Once you have understood the B-Spline curve the Rational B-Spline is really simple. Each control point have a weight assigned to it, and if the weight is equal to all points, you will have the standard B-Spline curve. This gives you control of the curve around individual points without changing the degree or the controlpoint posisions, thus make the manipulation very intuitive from a human perspective. The code is really simple:</p>

<pre lang="cs">
        Point RationalBSplinePoint(ObservableCollection&lt;RationalBSplinePoint&gt; Points, int degree, double[] KnotVector, double t)
        {

            double x, y;
            x = 0;
            y = 0;
            double rationalWeight = 0d;

            for (int i = 0; i &lt; Points.Count; i++)
            {
                double temp = Nip(i, degree, KnotVector, t)*Points[i].Weight;
                rationalWeight += temp;
            }

            for (int i = 0; i &lt; Points.Count; i++)
            {
                double temp = Nip(i, degree, KnotVector, t);
                x += Points[i].MyPoint.X*Points[i].Weight * temp/rationalWeight;
                y += Points[i].MyPoint.Y * Points[i].Weight * temp / rationalWeight;
            }
            return new Point(x, y);
        }</pre>

<h2>Referances</h2>

<p>The main resources&nbsp;for this article was:</p>

<ul>
	<li>The NURBS book, 2nd Edition, by Les Piegl and Wayne Tiller</li>
	<li>A practical guide to Splines, by Carl de Boor</li>
</ul>

<p>There are also some minor referances to other sites in the article where I have taken some of the images from.</p>

<p>&nbsp;</p>

<p>&nbsp;</p>
