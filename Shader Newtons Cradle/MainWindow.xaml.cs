using GlmNet;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Shader_Newtons_Cradle
{
    public partial class MainWindow : Window
    {
        //Camera positioning
        private VertexGeometry canvas;
        private float time;
        private bool mouseDown = false;
        private Point mousePrevPos;
        private Point mouseDist;
        private float zoom;

        public MainWindow()
        {
            InitializeComponent();
            canvas = new VertexGeometry();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            time = 0;
            zoom = 25.0f;
            mouseDist = new Point();
            //Create a rectangle in front of the camera to display the fragment shader code.
            CreateVertexGeometry("Rect.jgv");
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            time += 1.0f / 60.0f;
            scene1.SetUniform1("Time", time);
            scene1.SetUniform1("Zoom", zoom);
            scene1.SetUniform3("Resolution", new vec3((float)scene1.ActualWidth, (float)scene1.ActualHeight, 0.0f));
            scene1.SetUniform3("Mouse", new vec3((float)(mouseDist.X), (float)(mouseDist.Y), 0.0f));
            //Render the scene.
            scene1.Render();
        }

        private void CreateVertexGeometry(string jgvfile)
        {
            //Create the Scene Geometry:
            scene1.ClearGeometries();
            canvas = new VertexGeometry(1.0, jgvfile)
            {
                Position = new Vector3D(0.0, 0.0, 0.0),
                DrawMode = DrawMode.Fill
            };
            scene1.AddGeometry(canvas);
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mousePrevPos = e.GetPosition(this);
            mouseDown = true;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseDown)
            {
                Point pt = e.GetPosition(this);
                mouseDist += pt - mousePrevPos;
                mousePrevPos = pt;
            }
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mouseDown = false;
        }

        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                zoom *= 1.05f;
            }
            else
            {
                zoom *= 0.95f;
            }
            if (zoom < 1) { zoom = 1; }
            if (zoom > 200) { zoom = 200; }
        }
    }
}
