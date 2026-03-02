using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Fluid_Sim
{
    ///Copyright 2022 Matthias Müller - Ten Minute Physics, 
    ///www.youtube.com/c/TenMinutePhysics
    ///www.matthiasMueller.info/tenMinutePhysics
    ///MIT License
    ///Converted to C# and Modified by Jos Gielen 2026
    public partial class MainWindow : Window
    {
        private double simHeight;
        private double canvasScale;
        private double simWidth;
        private int numX;
        private int numY;
        private double h;
        public int frameNr;
        public bool paused;
        private bool mouseDown = false;
        private WriteableBitmap bitmap;
        //Simulation parameters
        public int sceneNr;
        public double gravity;
        public double dt;
        public int numIters;
        public double overRelaxation;
        public double obstacleX;
        public double obstacleY;
        public double obstacleRadius;
        public int resolution;
        public bool showObstacle;
        public bool showPressure;
        public bool showSmoke;
        public bool showStreamlines;
        public Fluid fluid;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            simHeight = 1.1;
            canvasScale = canvas1.ActualHeight / simHeight;
            simWidth = canvas1.ActualWidth / canvasScale;
            bitmap = new WriteableBitmap((int)canvas1.ActualWidth, (int)canvas1.ActualHeight, 96, 96, PixelFormats.Rgb24, null);
            sceneNr = 1;
            gravity = 0.0;
            showObstacle = true;
            showPressure = false;
            showSmoke = true;
            obstacleRadius = 0.17;
            overRelaxation = 1.9;
            dt = 1.0 / 60.0;
            resolution = 100;
            numIters = 80;
            cbPressure.IsChecked = false;
            cbSmoke.IsChecked = true;
            setupScene(simWidth, simHeight);
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public void setupScene(double width, double height)
        {
            double domainHeight = 1.0;
            double domainWidth = domainHeight / height * width;
            h = domainWidth / resolution;
            numX = (int)Math.Floor(width / h);
            numY = (int)Math.Floor(height / h);
            double density = 1000.0;
            fluid = new Fluid(density, numX, numY, h);
            if (sceneNr == 0) // tank
            {
                for (int i = 0; i < numX; i++)
                {
                    for (int j = 0; j < numY; j++)
                    {
                            
                        if (i == 0 || i == numX - 1 || j == 0)
                        {
                            fluid.s[i, j] = 0.0; // solid
                        }
                        else
                        {
                            fluid.s[i, j] = 1.0;// fluid
                        }
                    }
                }
            }
            else if (sceneNr == 1 || sceneNr == 3)  // vortex shedding
            {
                var inVel = 2.0;
                for (var i = 0; i < numX; i++)
                {
                    for (var j = 0; j < numY; j++)
                    {
                        if (i == 0 || j == 0 || j == numY - 1)
                        {
                            fluid.s[i, j] = 0.0;  // solid
                        }
                        else
                        {
                            fluid.s[i, j] = 1.0;  // fluid
                        }
                        if (i == 1)
                        {
                            fluid.u[i, j] = inVel;
                        }
                    }
                }
                //Smoke generation in the middle over 10% of the height
                double pipeH = 0.1 * numY;
                int minJ = (int)Math.Floor(0.5 * numY - 0.5 * pipeH);
                int maxJ = (int)Math.Floor(0.5 * numY + 0.5 * pipeH);
                for (var j = minJ; j < maxJ; j++)
                {
                    fluid.m[0, j] = 0.0;
                }
                if (showObstacle) { setObstacle(0.5, 0.54, true); }
            }
        }

        public void setObstacle(double x, double y, bool reset)
        {
            double vx = 0.0;
            double vy = 0.0;
            if (!reset)
            {
                vx = (x - obstacleX) / dt;
                vy = (y - obstacleY) / dt;
            }
            obstacleX = x;
            obstacleY = y;
            double r = obstacleRadius;
            for (int i = 1; i < numX - 2; i++)
            {
                for (int j = 1; j < numY - 2; j++)
                {
                    fluid.s[i, j] = 1.0;
                    double dx = (i + 0.5) * h - x;
                    double dy = (j + 0.5) * h - y;
                    if (dx * dx + dy * dy < r * r)
                    {
                        fluid.s[i, j] = 0.0;
                        if (sceneNr == 2)
                        {
                            fluid.m[i, j] = 0.5 + 0.5 * Math.Sin(0.1 * frameNr);
                        }
                        else
                        {
                            fluid.m[i, j] = 1.0;
                        }
                        fluid.u[i, j] = vx;
                        fluid.u[i + 1, j] = vx;
                        fluid.v[i, j] = vy;
                        fluid.v[i, j + 1] = vy;
                    }
                }
            }
            showObstacle = true;
        }


        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            simulate();
            draw();
            Dispatcher.Invoke(Wait, DispatcherPriority.ApplicationIdle);
        }

        private void Wait()
        {
            Thread.Sleep(10);
        }

        private void simulate()
        {
            if (!paused)
            {
                fluid.simulate(dt, gravity, numIters, overRelaxation);
            }
            frameNr++;
        }

        private Color getSciColor(double val, double minVal, double maxVal)
        {
            val = Math.Min(Math.Max(val, minVal), maxVal - 0.0001);
            double d = maxVal - minVal;
            val = d == 0.0 ? 0.5 : (val - minVal) / d;
            double m = 0.25;
            int num = (int)Math.Floor(val / m);
            double s = (val - num * m) / m;
            double r = 0;
            double g = 0;
            double b = 0;
            switch (num)
            {
                case 0: r = 0.0; g = s; b = 1.0; break;
                case 1: r = 0.0; g = 1.0; b = 1.0 - s; break;
                case 2: r = s; g = 1.0; b = 0.0; break;
                case 3: r = 1.0; g = 1.0 - s; b = 0.0; break;
            }
            return Color.FromRgb((byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
        }

        private void draw()
        {
            canvas1.Children.Clear();
            //double cellScale = 1.0; //1.1;
            double minP = fluid.p[0,0];
            double maxP = fluid.p[0,0];
            for (var i = 0; i < numX; i++)
            {
                for (int j = 0; j< numY; j++)
                {
                    minP = Math.Min(minP, fluid.p[i,j]);
                    maxP = Math.Max(maxP, fluid.p[i,j]);
                }
            }
            Color color = Colors.White;
            for (int i = 0; i < numX; i++)
            {
                for (int j = 0; j < numY; j++)
                {
                    if (showPressure)
                    {
                        double p = fluid.p[i , j];
                        double s = fluid.m[i , j];
                        color = getSciColor(p, minP, maxP);
                        if (showSmoke)
                        {
                            color.R = (byte)Math.Max(0.0, color.R - 255 * s);
                            color.G = (byte)Math.Max(0.0, color.G - 255 * s);
                            color.B = (byte)Math.Max(0.0, color.B - 255 * s);
                        }
                    }
                    else if (showSmoke)
                    {
                        double s = fluid.m[i , j];
                        color.R = (byte)(255 * s);
                        color.G = (byte)(255 * s);
                        color.B = (byte)(255 * s);
                        if (sceneNr == 2) { color = getSciColor(s, 0.0, 1.0); }
                    }
                    else if (fluid.s[i , j] == 0.0)
                    {
                        color.R = 0;
                        color.G = 0;
                        color.B = 0;
                    }
                    int x = (int)Math.Floor(cX(i * h));
                    int y = (int)Math.Floor(cY((j + 1) * h));
                    //int cx = (int)Math.Floor(canvasScale * cellScale * h) + 1;
                    //int cy = (int)Math.Floor(canvasScale * cellScale * h) + 1;
                    int cx = (int)Math.Floor(canvasScale * h) + 1;
                    int cy = (int)Math.Floor(canvasScale * h) + 1;

                    SetPixelArea(bitmap, x, y, cx, cy, color);
                }
            }
            canvas1.Background = new ImageBrush(bitmap);

            if (showStreamlines)
            {
                int numSegs = 15;
                Polyline poly;
                for (var i = 1; i < numX - 1; i += 5)
                {
                    for (int j = 1; j < numY - 1; j += 5)
                    {
                        double x = (i + 0.5) * h;
                        double y = (j + 0.5) * h;
                        poly = new Polyline()
                        {
                            Stroke = Brushes.DarkGray,
                            StrokeThickness = 1.0,
                        };
                        poly.Points.Add(new Point(cX(x), cY(y)));
                        for (int k = 0; k < numSegs; k++)
                        {
                            double u = fluid.sampleField(x, y, 0);
                            double v = fluid.sampleField(x, y, 1);
                            double l = Math.Sqrt(u * u + v * v);
                            x += u * 0.01;
                            y += v * 0.01;
                            if (x > numX * fluid.h)
                            {
                                break;
                            }
                            poly.Points.Add(new Point(cX(x), cY(y)));
                        }
                        canvas1.Children.Add(poly);
                    }
                }
            }

            if (showObstacle)
            {
                double r = obstacleRadius + h;
                Ellipse el = new Ellipse()
                {
                    Width = 2 * canvasScale * r,
                    Height = 2 * canvasScale * r,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.0
                };
                if (showPressure)
                {
                    el.Fill = Brushes.Blue;
                }
                else
                {
                    el.Fill = Brushes.LightGray;
                }
                el.SetValue(Canvas.LeftProperty, cX(obstacleX - r));
                el.SetValue(Canvas.TopProperty, cY(obstacleY + r));
                canvas1.Children.Add(el);
            }
            if (showPressure)
            {
                Label lb = new Label()
                {
                    FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Arial"),
                    FontSize = 16,
                    Foreground = Brushes.Red,
                    Content = "pressure: " + minP.ToString("F0") + " - " + maxP.ToString("F0") + " N/m"
                };
                lb.SetValue(Canvas.LeftProperty, 10.0);
                lb.SetValue(Canvas.TagProperty, 35.0);
                canvas1.Children.Add(lb);
            }
        }

        //OverWrite an area of width w by height h pixels at location X,Y with Color c 
        public void SetPixelArea(WriteableBitmap WB, int X, int Y, int w, int h, Color c)
        {
            //For PixelFormats.Rgb24
            byte[] pixelData = new byte[3 * w * h];
            for (int i = 0; i < pixelData.Length; i += 3)
            {
                pixelData[i + 0] = c.R;
                pixelData[i + 1] = c.G;
                pixelData[i + 2] = c.B;
            }
            if (X + w > WB.PixelWidth) { return; }
            if (X < 0) { return; }
            if (Y + h > WB.PixelHeight) { return; }
            if (Y < 0) { return; }
            WB.WritePixels(new Int32Rect(X, Y, w, h), pixelData, 3 * w, 0);
        }

        private double cX(double x)
        {
            return x * canvasScale;
        }

        private double cY(double y)
        {
            return canvas1.ActualHeight - y * canvasScale;
        }

        private void btnWind_Click(object sender, RoutedEventArgs e)
        {
            sceneNr = 1;
            gravity = 0.0;
            showObstacle = true;
            showPressure = false;
            showSmoke = true;
            dt = 1.0 / 60.0;
            obstacleRadius = 0.17;
            overRelaxation = 1.9;
            dt = 1.0 / 60.0;
            resolution = 100;
            numIters = 80;
            cbPressure.IsChecked = false;
            cbSmoke.IsChecked = true;
            setupScene(simWidth, simHeight);
        }

        private void btnTank_Click(object sender, RoutedEventArgs e)
        {
            sceneNr = 0;
            gravity = -9.81;
            overRelaxation = 1.9;
            showObstacle = true;
            showPressure = true;
            showSmoke = false;
            dt = 1.0 / 200.0;
            obstacleRadius = 0.17;
            resolution = 100;
            numIters = 40;
            cbPressure.IsChecked = true;
            cbSmoke.IsChecked = false;
            setupScene(simWidth, simHeight);
        }

        private void btnPaint_Click(object sender, RoutedEventArgs e)
        {
            sceneNr = 2;
            gravity = 0.0;
            overRelaxation = 1.0;
            showObstacle = true;
            showPressure = false;
            showSmoke = true;
            obstacleRadius = 0.1;
            numIters = 80;
            resolution = 100;
            cbPressure.IsChecked = false;
            cbSmoke.IsChecked = true;
            setupScene(simWidth, simHeight);
        }

        private void cbStream_Click(object sender, RoutedEventArgs e)
        {
            showStreamlines = cbStream.IsChecked.Value;
        }

        private void cbPressure_Click(object sender, RoutedEventArgs e)
        {
            showPressure = cbPressure.IsChecked.Value;
        }

        private void cbSmoke_Click(object sender, RoutedEventArgs e)
        {
            showSmoke = cbSmoke.IsChecked.Value;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.P: paused = !paused; break;
                case Key.M: paused = false; simulate(); paused = true; break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(canvas1);
            double x = pt.X / canvasScale;
            double y = (canvas1.ActualHeight - pt.Y) / canvasScale;
            setObstacle(x, y, true);
            mouseDown = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Point pt = e.GetPosition(canvas1);
                double x = pt.X / canvasScale;
                double y = (canvas1.ActualHeight - pt.Y) / canvasScale;
                setObstacle(x, y, false);
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
        }
    }
}