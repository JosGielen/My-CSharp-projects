using JG_GL;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3D_Sierpinski
{

    public partial class MainWindow : Window
    {
        private bool App_Loaded = false;
        private PolyLineGeometry my_Polyline;
        private List<Vector3D> points = new List<Vector3D>();
        private Vector3D CurrentPt;
        private Vector3D MidPt;
        private readonly Random Rnd = new Random();
        //Camera positioning
        private Vector3D CamStartPos = new Vector3D(0.0, 0.0, 60.0);
        private Vector3D CamStartTarget = new Vector3D(0.0, 0.0, 0.0);
        private Vector3D CamUpDir = new Vector3D(0.0, 1.0, 0.0);
        //Parameters
        private int PointsNum = 3;
        private double StepPercentage = 50;
        private bool useColor = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            my_Polyline = new PolyLineGeometry(1.0, 1.0, 1.0, false)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                DrawMode = DrawMode.Points,
                PointSize = 1.0,
                LineWidth = 1.0,
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.002,
                AmbientMaterial = Colors.LightGreen
            };
            Scene1.ShowAxes = false;
            Scene1.ShowGrid = false;
            Scene1.Camera.Position = CamStartPos;
            Scene1.Camera.TargetPosition = CamStartTarget;
            Scene1.Camera.UpDirection = CamUpDir;
            Vector3D p;
            MidPt = new Vector3D(0.0, 0.0, 0.0);
            points = new List<Vector3D>();
            double dX = 20.0 * Math.Cos(Math.PI / 6);
            double dZ = 20.0 * Math.Cos(Math.PI / 3);
            //Back
            p = new Vector3D(0.0, -12.5, -20.0);
            points.Add(p);
            my_Polyline.Points.Add(p);
            //Left
            p = new Vector3D(-dX, -12.5, dZ);
            points.Add(p);
            my_Polyline.Points.Add(p);
            //Right
            p = new Vector3D(dX, -12.5, dZ);
            points.Add(p);
            my_Polyline.Points.Add(p);
            //Top
            p = new Vector3D(0.0, 12.5, 0.0);
            points.Add(p);
            my_Polyline.Points.Add(p);
            Scene1.AddGeometry(my_Polyline);
            CurrentPt = new Vector3D(40 * Rnd.NextDouble() - 20,  40 * Rnd.NextDouble() - 20, 40 * Rnd.NextDouble() - 20);
            CompositionTarget.Rendering += CompositionTarget_Rendering; ;
            App_Loaded = true;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (!App_Loaded) return;
            //Create the 3D Sierpinski using the Chaos Game
            Vector3D nextPt;
            int index;
            if (my_Polyline.Points.Count < 30000)
            {
                for (int I = 0; I < 100; I++)
                {
                    index = Rnd.Next(points.Count);
                    nextPt = points[index];
                    nextPt = Lerp3D(CurrentPt, nextPt, StepPercentage / 100);
                    my_Polyline.Points.Add(nextPt);
                    CurrentPt = nextPt; 
                }
                my_Polyline.GenerateGeometry(Scene1);
            }
            my_Polyline.Update();
            Scene1.Render();
        }

        private Vector3D Lerp3D(Vector3D V1, Vector3D V2, double Percentage)
        {
            Vector3D result = new Vector3D();
            result.X = V1.X + StepPercentage / 100 * (V2.X - V1.X);
            result.Y = V1.Y + StepPercentage / 100 * (V2.Y - V1.Y);
            result.Z = V1.Z + StepPercentage / 100 * (V2.Z - V1.Z);
            return result;
        }
    }
}