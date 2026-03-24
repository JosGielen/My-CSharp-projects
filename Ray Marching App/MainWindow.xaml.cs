using GlmNet;
using JG_GL;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Ray_Marching_App
{
    public partial class MainWindow : Window
    {
        //Camera positioning
        private VertexGeometry canvas;
        string VertexShader = "";
        string FragmentShader = "";
        private bool rendering;
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
            zoom = 10.0f;
            mouseDist = new Point();
            //Set the Default Vertex and Fragment Shaders
            VertexShader = Environment.CurrentDirectory + "\\Default.vert";
            FragmentShader = Environment.CurrentDirectory + "\\Default.frag";
            scene1.SetShaders(VertexShader, FragmentShader);
            ShowShader(FragmentShader);
            //Create a rectangle in front of the camera to display the fragment shader code.
            CreateVertexGeometry("Rect.jgv");
            rendering = true;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            time += 1.0f / 60.0f;
            scene1.SetUniform1("Time", time);
            scene1.SetUniform1("Zoom", zoom);
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
            if (rendering && mouseDown)
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

        private void mnuLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog1.Multiselect = false;
            openFileDialog1.DefaultExt = ".*";
            openFileDialog1.Filter = "Fragment Shaders (*.frag)|*.frag";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog().Value)
            {
                FragmentShader = openFileDialog1.FileName;
                scene1.SetShaders(VertexShader, FragmentShader);
                zoom = 10.0f;
                mouseDist = new Point();
                ShowShader(FragmentShader);
                if (!rendering)
                {
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                    imgPlay.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\PauseIcon.jpg"));
                    rendering = true;
                }
            }
        }

        private void ShowShader(string shader)
        {
            using (StreamReader myStream = new StreamReader(shader))
            {
                txtShader.Text = myStream.ReadToEnd();
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (rendering)
            {
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                imgPlay.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\PlayIcon.jpg"));
                rendering = false;
            }
            else
            {
                try
                {
                    scene1.FragmentShader = txtShader.Text;
                    scene1.UpdateShaders();
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                    imgPlay.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\PauseIcon.jpg"));
                    rendering = true;
                }
                catch
                {
                    MessageBox.Show("Unable to load the Fragment Shader.", "Ray Tracing App Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog1.Filter = "Fragment Shader files (*.frag)|*.frag|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == true)
            {
                FragmentShader = saveFileDialog1.FileName;
                SaveFile();
            }
        }

        private void SaveFile()
        {
            //Write the data to the File
            StreamWriter outfile = null;
            try
            {
                outfile = new StreamWriter(FragmentShader);
                if (outfile != null)
                {
                    //Schrijf de Fragment Shader weg
                    outfile.WriteLine(txtShader.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot write the Shader to disk. Original error: " + ex.Message);
            }
            finally
            {
                if (outfile != null)
                {
                    outfile.Close();
                }
            }
        }
    }
}
