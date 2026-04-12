using Microsoft.Win32;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OpenTK_WPF
{
    public partial class MainWindow : Window
    {
        private string VertexShaderFile = "";
        private string VertexShaderCode;
        private string FragmentShaderFile = "";
        private string FragmentShaderCode;
        private float[] vertices;
        private uint[] indices;
        private Shader shader;
        private bool updateShaders;
        private int ProgramHandle = 0;
        private int VertexArrayHandle = 0;
        private int VertexBufferHandle = 0;
        private int ElementBufferHandle = 0;
        private bool rendering;
        private float time;
        private bool mouseDown = false;
        private Point mousePrevPos;
        private Point mouseDist;
        private float zoom;
        private bool useTextures = false;
        private List<string> textureFiles;
        private List<Texture> textures;
        private bool useCubeMap = false;
        private List<string> CubeMapFiles;
        private Cubemap cubemap;
        private DateTime LastRenderTime;
        private int Framecounter;

        public MainWindow()
        {
            InitializeComponent();
            OpenTkControl.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            time = 0;
            zoom = 1.0f;
            mouseDist = new Point();
            CubeMapFiles = new List<string>();
            textureFiles = new List<string>();
            textures = new List<Texture>();
            updateShaders = true;
            //Load the Default Vertex and Fragment Shaders
            VertexShaderFile = Environment.CurrentDirectory + "\\Default.vert";
            FragmentShaderFile = Environment.CurrentDirectory + "\\Default.frag";
            //LoadShaders(VertexShaderFile, FragmentShaderFile);
            //Set-up the OpenTK GLWpfControl
            Initialize();
            //Show the fragment shader in the Textbox
            ShowFragmentShader(FragmentShaderFile);
            OpenTkControl.Render += OpenTkControl_OnRender;
            rendering = true;
        }

        private void Initialize()
        {
            //Specify the vertex positions and texture coördinates to fill the entire viewport
            vertices =
            [
             1.0f,  1.0f, 0.0f, 1.0f, 1.0f, // top right
             1.0f, -1.0f, 0.0f, 1.0f, 0.0f, // bottom right
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom left
            -1.0f,  1.0f, 0.0f, 0.0f, 1.0f  // top left
            ];
            indices =
            [
                0, 1, 3,
                1, 2, 3
            ];
            //Set the background color
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            //Create the VertexArray
            VertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayHandle);
            //Create the VertexBuffer
            VertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            //Create the ElementBuffer (= IndexBuffer)
            ElementBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            //Create the ShadeProgram
            if (updateShaders)
            {
                LoadShaders(VertexShaderFile, FragmentShaderFile);
            }
            shader = new Shader(VertexShaderCode, FragmentShaderCode);
            shader.Use();
            //Set the Vertex and texturecoördinate positions
            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        }

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            //Show FPS
            Framecounter += 1;
            if (Framecounter == 100) //Show FPS average over 100 frames
            {
                double fps = (int)(100000 / (DateTime.Now - LastRenderTime).TotalMilliseconds);
                Title = " FPS = " + fps.ToString();
                LastRenderTime = DateTime.Now;
                Framecounter = 0;
            }
            time += 1.0f / 60.0f;
            shader.SetFloat("Time", time);
            shader.SetFloat("Zoom", zoom);
            shader.SetVector3("Mouse", new Vector3((float)mouseDist.X, (float)mouseDist.Y, 0.0f));
            shader.SetVector3("Resolution", new Vector3((float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight, 0.0f));
            //GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindVertexArray(VertexArrayHandle);
            int offset = 0;
            if (useCubeMap && cubemap != null) 
            {
                cubemap.Use(TextureUnit.Texture0);
                offset = 1; 
            }
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].Use(TextureUnit.Texture0 + i + offset);
            }
            shader.Use();
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnLoad();
        }

        private void UnLoad()
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferHandle);
            GL.UseProgram(0);
            GL.DeleteProgram(ProgramHandle);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (rendering)
            {
                OpenTkControl.Render -= OpenTkControl_OnRender;
                imgPlay.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\PlayIcon.jpg"));
                UnLoad();
                rendering = false;
            }
            else
            {
                try
                {
                    FragmentShaderCode = txtShader.Text;
                    updateShaders = false;
                    Initialize();
                    LoadTextures();
                    OpenTkControl.Render += OpenTkControl_OnRender;
                    imgPlay.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\PauseIcon.jpg"));
                    rendering = true;
                }
                catch
                {
                    MessageBox.Show("Unable to load the Fragment Shader.", "Ray Tracing App Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void LoadShaders(string vertexShaderFile, string fragmentShaderFile)
        {
            try
            {
                using (StreamReader reader = new StreamReader(vertexShaderFile))
                {
                    VertexShaderCode = reader.ReadToEnd();
                }
                using (StreamReader reader = new StreamReader(fragmentShaderFile))
                {
                    FragmentShaderCode = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("LoadShaders was unable to load the shaders : " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region "Mouse Events"

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
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
            if (zoom < 0.1f) { zoom = 0.1f; }
            if (zoom > 200.0f) { zoom = 200.0f; }
        }

        #endregion


        #region "Menu Events"

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
                FragmentShaderFile = openFileDialog1.FileName;
                lblFileName.Content = Path.GetFileName(FragmentShaderFile);
                updateShaders = true;
                UnLoad();
                Initialize();
                ShowFragmentShader(FragmentShaderFile);
                zoom = 10.0f;
                mouseDist = new Point();
                if (!rendering)
                {
                    OpenTkControl.Render += OpenTkControl_OnRender;
                    imgPlay.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\PauseIcon.jpg"));
                    rendering = true;
                }
            }
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(FragmentShaderFile);
        }

        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog1.Filter = "Fragment Shader files (*.frag)|*.frag|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == true)
            {
                FragmentShaderFile = saveFileDialog1.FileName;
                SaveFile(FragmentShaderFile);
                lblFileName.Content = Path.GetFileName(FragmentShaderFile);
            }
        }

        private void mnuLoadTexture_Click(object sender, RoutedEventArgs e)
        {
            //Add the texturefile the textureFiles List
            string filename = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select the Texture image";
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog1.Multiselect = false;
            openFileDialog1.DefaultExt = ".*";
            openFileDialog1.Filter = "Windows Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|TIFF (*.tiff)|*.tiff|PNG (*.png)|*.png";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog().Value)
            {
                filename = openFileDialog1.FileName;
                textureFiles.Add(filename);
                int index = textureFiles.Count;
                //Get each line of text of the Fragment shader
                List<string> list = new List<string>();
                for (int i = 0; i < txtShader.LineCount; i++)
                {
                    list.Add(txtShader.GetLineText(i));
                }
                //Add a new Texture line after the existing Texture lines
                StringBuilder sb = new StringBuilder();
                bool insert = true;
                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].Contains("//Texture:") && insert == true)
                    {
                        sb.AppendLine("//Texture:" + filename);
                        insert = false;
                    }
                    sb.Append(list[i]);
                }
                txtShader.Text = sb.ToString();
                //AddTexture(textureFiles.Count, filename);
            }
        }

        /// <summary>
        /// Adds a cubemap (overwrites any existing cubemap data in the Fragment Shader)
        /// </summary>
        private void mnuLoadCubeMap_Click(object sender, RoutedEventArgs e)
        {
            //Select the 6 texture files that make up the CubeMap
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select the 6 images of the CubeMap";
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog1.Multiselect = true;
            openFileDialog1.DefaultExt = ".*";
            openFileDialog1.Filter = "Windows Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|TIFF (*.tiff)|*.tiff|PNG (*.png)|*.png";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog().Value && openFileDialog1.FileNames.Length == 6)
            {
                CubeMapFiles.Clear();
                CubeMapFiles.AddRange(openFileDialog1.FileNames);
                //Remove any existing Cubemap lines
                List<string> list = new List<string>();
                for (int i = 0; i < txtShader.LineCount; i++)
                {
                    list.Add(txtShader.GetLineText(i));
                }
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].Contains("//Cubemap:"))
                    {
                        sb.Append(list[i]);
                    }
                }
                txtShader.Text = sb.ToString();
                //Add a new line for each image file at the beginning of the fragment shader
                sb = new StringBuilder();
                for (int i = 0; i < CubeMapFiles.Count; i++)
                {
                    sb.AppendLine("//Cubemap:" + CubeMapFiles[i]);
                }
                useCubeMap = true;
                sb.Append(txtShader.Text);
                txtShader.Text = sb.ToString();
                LoadCubeMap();
            }
        }

        private void mnuUnLoadCubeMap_Click(object sender, RoutedEventArgs e)
        {
            //Remove any existing Cubemap lines
            CubeMapFiles.Clear();
            List<string> list = new List<string>();
            for (int i = 0; i < txtShader.LineCount; i++)
            {
                list.Add(txtShader.GetLineText(i));
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].Contains("//Cubemap:"))
                {
                    sb.Append(list[i]);
                }
            }
            txtShader.Text = sb.ToString();
            LoadCubeMap();
        }

        /// <summary>
        /// Save the generated scene to an image file
        /// </summary>
        private void mnuSaveImage_Click(object sender, RoutedEventArgs e)
        {
            Rect R = new Rect(OpenTkControl.Margin.Left, OpenTkControl.Margin.Top, OpenTkControl.ActualWidth, OpenTkControl.ActualHeight);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)R.Right, (int)R.Bottom, 96.0, 96.0, System.Windows.Media.PixelFormats.Default);
            rtb.Render(OpenTkControl);
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            BitmapEncoder MyEncoder = new BmpBitmapEncoder();
            saveFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog1.Filter = "Windows Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|TIFF (*.tiff)|*.tiff|PNG (*.png)|*.png";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == true)
            {
                try
                {
                    switch (saveFileDialog1.FilterIndex)
                    {
                        case 1:
                            MyEncoder = new BmpBitmapEncoder();
                            break;
                        case 2:
                            MyEncoder = new JpegBitmapEncoder();
                            break;
                        case 3:
                            MyEncoder = new GifBitmapEncoder();
                            break;
                        case 4:
                            MyEncoder = new TiffBitmapEncoder();
                            break;
                        case 5:
                            MyEncoder = new PngBitmapEncoder();
                            break;
                    }
                    MyEncoder.Frames.Add(BitmapFrame.Create(rtb));
                    // Create an instance of StreamWriter to write the Image to the file.
                    FileStream sw = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                    MyEncoder.Save(sw);
                }
                catch
                {
                    MessageBox.Show("The Image could not be saved.", "ProgramName error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        #endregion


        #region "Utilities"

        /// <summary>
        /// Shows the FragmentShader in the TextBox and loads a Cubemap and texture files that are specified
        /// </summary>
        /// <param name="fileName">The FragmentShader file</param>
        private void ShowFragmentShader(string fileName)
        {
            using (StreamReader myStream = new StreamReader(fileName))
            {
                txtShader.Text = myStream.ReadToEnd();
            }
            lblFileName.Content = Path.GetFileName(fileName);
            //Check for cubemap and textures needed
            string line = "";
            textureFiles.Clear();
            CubeMapFiles.Clear();
            useCubeMap = false;
            useTextures = false;
            using (StreamReader myStream = new StreamReader(fileName))
            {
                while (!myStream.EndOfStream)
                {
                    line = myStream.ReadLine();
                    if (line.StartsWith("//Cubemap:"))
                    {
                        CubeMapFiles.Add(line.Substring(10));
                        useCubeMap = true;
                    }
                    if (line.StartsWith("//Texture:"))
                    {
                        textureFiles.Add(line.Substring(10));
                        useTextures = true;
                    }
                }
            }
            if (useCubeMap) { LoadCubeMap(); }
            if (useTextures) { LoadTextures(); }
        }

        /// <summary>
        /// Adds a cubemap (overwrites any existing cubemap data in the Fragment Shader)
        /// </summary>
        private void LoadCubeMap()
        {
            cubemap = Cubemap.LoadFromFiles(CubeMapFiles);
            cubemap.Use(TextureUnit.Texture0);
            shader.SetInt("cubemap", 0);
        }

        /// <summary>
        /// Adds all textures specified in the Fragment shader
        /// </summary>
        private void LoadTextures()
        {
            textures.Clear();
            for (int i = 0; i < textureFiles.Count; i++)
            {
                AddTexture(i, textureFiles[i]);
            }
        }

        /// <summary>
        /// Adds a single texture
        /// </summary>
        /// <param name="index"></param>
        /// <param name="filename"></param>
        private void AddTexture(int index, string filename)
        {
            int i = index;
            if (useCubeMap) { index++; }
            //Load the textures.
            Texture tex1 = Texture.LoadFromFile(filename);
            textures.Add(tex1);
            tex1.Use(TextureUnit.Texture0 + index);
            //setup the samplers in the shader to use the right textures.
            shader.SetInt("texture" + i.ToString(), index);
        }

        /// <summary>
        /// Save the Fragment shader to a file.
        /// </summary>
        /// <param name="filename"></param>
        private void SaveFile(string filename)
        {
            //Write the data to the File
            StreamWriter outfile = null;
            try
            {
                outfile = new StreamWriter(filename);
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

        #endregion
    }
}