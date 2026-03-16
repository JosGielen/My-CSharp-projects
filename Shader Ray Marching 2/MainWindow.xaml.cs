using GlmNet;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Shader_Ray_Marching
{
    public partial class MainWindow : Window
    {
        //Camera positioning
        private VertexGeometry canvas;
        private float time;
        private bool mouseDown = false;
        private Point mousePrevPos;
        private Point mouseDist;
        
        public MainWindow()
        {
            InitializeComponent();
            canvas = new VertexGeometry();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Create a rectangle in front of the camera to display the fragment shader code.
            CreateVertexGeometry("Rect.jgv");
            time = 0;
            mouseDist = new Point();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            scene1.SetUniform3("u_resolution", new vec3((float)scene1.ActualWidth, (float)scene1.ActualHeight, 0.0f)); 
            time += 1.0f / 60.0f;
            scene1.SetUniform1("time", time);
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
                scene1.SetUniform3("mouse", new vec3((float)(mouseDist.X), (float)(mouseDist.Y), 0.0f));
                mousePrevPos = pt;
            }
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mouseDown = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                scene1.SetUniform3("u_resolution", new vec3((float)scene1.ActualWidth, (float)scene1.ActualHeight, 0.0f));
            }
        }
    }
}
