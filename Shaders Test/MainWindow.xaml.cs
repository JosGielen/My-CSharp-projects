using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Shaders_Test
{
    public partial class MainWindow : Window
    {
        //Camera positioning
        private VertexGeometry canvas;
        private float time;

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
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
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
    }
}
