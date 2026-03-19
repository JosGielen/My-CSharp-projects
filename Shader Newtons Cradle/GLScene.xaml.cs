using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using GlmNet;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.Shaders;
using System.Collections.Generic;

namespace Shader_Newtons_Cradle
{
    public partial class GLScene : UserControl
    {
        public const uint positionAttribute = 0;
        public const uint normalAttribute = 1;
        public const uint colorAttribute = 2;
        public const uint textureAttribute = 3;
        //OpenGL data
        private OpenGL my_gl;
        private Dictionary<uint, String> my_AttributeLocations;
        private string my_VertexShader;
        private string my_FragmentShader;
        private ShaderProgram my_Shader;
        private bool my_UpdateShader;
        private mat4 viewMatrix;
        private mat4 projectionMatrix;
        //Scene data
        public bool SceneIsLoaded;
        private Color my_Background;
        private readonly List<GLGeometry> my_Geometries;
        private bool my_GenerateGeometries;
        private GLCamera my_Camera;
        //Camera Mouse control
        private bool MousebuttonDown;
        private readonly double MouseSensitivity;
        private Point MouseStartPos;
        private Point MouseEndPos;
        //Camera positioning
        private Vector3D my_CamStartPos;
        private Vector3D my_CamStartTarget;
        private Vector3D my_CamStartUpDir; 

        public GLScene()
        {
            InitializeComponent();
            my_UpdateShader = false;
            my_GenerateGeometries = false;
            my_Geometries = new List<GLGeometry>();
            my_Background = Colors.Black;
            //Set the default camera
            my_CamStartPos = new Vector3D(0.0, 0.0, 1.25);
            my_CamStartTarget = new Vector3D(0.0, 0.0, 0.0);
            my_CamStartUpDir = new Vector3D(0.0, 1.0, 0.0);
            my_Camera = new FixedCamera(my_CamStartPos, my_CamStartTarget, my_CamStartUpDir);
            MousebuttonDown = false;
            MouseSensitivity = 0.5;
            SceneIsLoaded = false;
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.Parse(OpenGLCtrl.OpenGL.Version.Substring(0,3)) < 4.0)
                {
                    OpenGLCtrl.OpenGLVersion = SharpGL.Version.OpenGLVersion.OpenGL4_0;
                }
                if (OpenGLCtrl.RenderContextType != RenderContextType.FBO )
                {
                    OpenGLCtrl.RenderContextType = RenderContextType.FBO;
                }
                OpenGLCtrl.FrameRate = 60;
                OpenGLCtrl.OnApplyTemplate();
                //Set the default OpenGLControl handlers to prevent SharpGL error messages.
                OpenGLCtrl.OpenGLInitialized += GLControl_OpenGLInitialized;
                OpenGLCtrl.OpenGLDraw += GLControl_OpenGLDraw;
                OpenGLCtrl.Resized += GLControl_Resized;
            }
            catch (Exception ex)
            {
                MessageBox.Show("WPF.OpenGLControl settings caused exception: " + ex.Message, "GLScene Error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            my_gl = OpenGLCtrl.OpenGL;
            //Specify the attribute locations
           my_AttributeLocations = new Dictionary<uint, string>
            {
                {positionAttribute, "Position" },
                {normalAttribute, "Normal" },
                {colorAttribute, "Color" },
                {textureAttribute, "Texture" }
            };
            //Check version before creating shaders.
            //TODO: This could allow to switch to Immediate mode??
            if (double.Parse(my_gl.Version.Substring(0,3)) < 2.0)
            {
                throw new Exception("OpenGL version " + double.Parse(my_gl.Version.Substring(0, 3)) + " does not allow use of shaders.");
            }
            //Create the shaderProgram.
            my_Shader = new ShaderProgram();
            if (!my_UpdateShader) //No Shaders were defined so use the default shaders
            {
                LoadDefaultShaders();
                UpdateShaders();
            }
            CreateProjectionMatrix(0.45F, ActualWidth, ActualHeight, 1, 300);
            //Set the scene background color.
            if (Background != null)
            {
                my_Background = (Color)ColorConverter.ConvertFromString(Background.ToString());
                my_gl.ClearColor(my_Background.ScR, my_Background.ScG, my_Background.ScB, my_Background.ScA);
            }
            SceneIsLoaded = true;
        }

        #region "Properties"

        public OpenGL GL
        {
            get { return my_gl; }
        }

        public ShaderProgram Shader
        {
            get { return my_Shader; } 
        }

        public List<GLGeometry> Geometries
        {
            get { return my_Geometries; }
        }

        public Dictionary<uint, string> AttributeLocations
        {
            get { return my_AttributeLocations; }
        }

        public GLCamera Camera
        {
            get { return my_Camera; }
            set 
            {
                my_Camera = value;
                my_CamStartPos = value.Position;
                my_CamStartTarget = value.TargetPosition;
                my_CamStartUpDir = value.UpDirection;
            }
        }

        public string VertexShader
        {
            get { return my_VertexShader; }
        }

        public string FragmentShader
        {
            get { return my_FragmentShader; }
        }

        public Vector3D CameraDefaultPosition
        {
            get { return my_CamStartPos; }
            set { my_CamStartPos = value; }
        }

        public Vector3D CameraDefaultTarget
        {
            get { return my_CamStartTarget; }
            set { my_CamStartTarget = value; }
        }

        public Vector3D CameraDefaultUpDirection
        {
            get { return my_CamStartUpDir; }
            set { my_CamStartUpDir = value; }
        }

        public void SetUniform1(string name, float value)
        {
            my_Shader.SetUniform1(my_gl, name, value);
        }

        public void SetUniform3(string name, vec3 value)
        {
            my_Shader.SetUniform3(my_gl, name, value.x, value.y, value.z);
        }

        public void SetUniformMatrix3(string name, mat3 value)
        {
            my_Shader.SetUniformMatrix3(my_gl, name, value.to_array());
        }

        public void SetUniformMatrix4(string name, mat4 value)
        {
            my_Shader.SetUniformMatrix4(my_gl, name, value.to_array());
        }

        #endregion 


        #region "Camera Control"

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseStartPos = e.GetPosition(OpenGLCtrl);
            MousebuttonDown = true;
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (MousebuttonDown )
            {
                MouseEndPos = e.GetPosition(OpenGLCtrl);
                switch(Camera.Type )
                {
                    case CameraType.Fixed:
                    {
                        //Do nothing
                        break;
                    }
                    case CameraType.ParentControlled:
                    {
                        //Must be implemented in the Parent window.
                        break;
                    }
                    case CameraType.FreeFlying:
                    {
                        Camera.Horizontal(0.5 * MouseSensitivity * (MouseStartPos - MouseEndPos).X);
                        Camera.Vertical(0.5 * MouseSensitivity * (MouseStartPos - MouseEndPos).Y);
                        break;
                    }
                    case CameraType.Trackball:
                    {
                        Camera.Horizontal(MouseSensitivity * (MouseEndPos - MouseStartPos).X);
                        Camera.Vertical(MouseSensitivity * (MouseStartPos - MouseEndPos).Y);
                        break;
                    }
                }
                MouseStartPos = MouseEndPos; 
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MousebuttonDown = false;
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double amount = 0.02 * MouseSensitivity * e.Delta;
            switch (Camera.Type)
            {
                case CameraType.Fixed:
                {
                    //Do nothing
                    break;
                }
                case CameraType.ParentControlled:
                {
                    //Must be implemented in the Parent window.
                    break;
                }
                case CameraType.FreeFlying:
                {
                    Camera.Forward(amount);
                    break;
                }
                case CameraType.Trackball:
                {
                    Camera.Forward(amount);
                    break;
                }
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (Camera.Type)
            {
                case CameraType.Fixed:
                {
                    //Do nothing
                    break;
                }
                case CameraType.ParentControlled:
                {
                    //Must be implemented in the Parent window.
                    break;
                }
                case CameraType.FreeFlying:
                {
                    switch(e.Key )
                    {
                        case Key.Up:
                        {
                            Camera.Forward(Camera.MoveSpeed);
                            break;
                        }
                        case Key.Down:
                        {
                            Camera.Forward(-1 * Camera.MoveSpeed);
                            break;
                        }
                        case Key.Right:
                        {
                            Camera.Horizontal(-1 * Camera.MoveSpeed);
                            break;
                        }
                        case Key.Left:
                        {
                            Camera.Horizontal(Camera.MoveSpeed);
                            break;
                        }
                    }
                    break;
                }
                case CameraType.Trackball:
                {
                    switch (e.Key)
                    {
                        case Key.Up:
                        {
                            Camera.Vertical(Camera.MoveSpeed);
                            break;
                        }
                        case Key.Down:
                        {
                            Camera.Vertical(-1 * Camera.MoveSpeed);
                            break;
                        }
                        case Key.Right:
                        {
                            Camera.Horizontal( Camera.MoveSpeed);
                            break;
                        }
                        case Key.Left:
                        {
                            Camera.Horizontal(-1 * Camera.MoveSpeed);
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            switch(e.Key )
            {
                case Key.Escape:
                {
                    Camera.Position = my_CamStartPos;
                    Camera.TargetPosition = my_CamStartTarget;
                    Camera.UpDirection = my_CamStartUpDir;
                    break;
                }
            }
        }

        #endregion

        #region "Scene Modification"

        /// <summary>
        /// Adds a GLGeometry to the Scene.
        /// <para>All the geometries in the scene are re-created at the next render pass.</para>
        /// </summary>
        /// <param name="geo">A Geometry derived from GLGeometry.</param>
        public void AddGeometry(GLGeometry geo)
        {
            my_Geometries.Add(geo);
            my_GenerateGeometries = true;
        }

        /// <summary>
        /// Creates the actual geometry objects that are in the scene.
        /// <para>This is called automatic at the next render pass after adding a geometry to the scene.</para>
        /// </summary>
        public void GenerateGeometries()
        {
            //Set the scene background color.
            if (Background != null)
            {
                my_Background = (Color)ColorConverter.ConvertFromString(Background.ToString());
            }
            my_gl.ClearColor(my_Background.ScR, my_Background.ScG, my_Background.ScB, my_Background.ScA);
            Window parentWindow = Window.GetWindow(this);
            parentWindow.PreviewKeyDown += UserControl_KeyDown;
            parentWindow.PreviewKeyUp += UserControl_KeyUp;
            foreach(GLGeometry geo in my_Geometries )
            {
                geo.GenerateGeometry(this);
            }
        }

        /// <summary>
        /// Removes all Geometries from the scene (except the axes if they are set).
        /// </summary>
        public void ClearGeometries()
        {
            my_Geometries.Clear();
        }

        /// <summary>
        /// Loads the Shader files into memmory.
        /// <para>The ProgramShader is created at the next render pass.</para>
        /// This replaces the default Shaders used by the GLScene.
        /// </summary>
        /// <param name="vertexShaderFile">Path of the Vertex Shader file.</param>
        /// <param name="fragmentShaderFile">Path of the Fragment Shader file.</param>
        public void SetShaders(string vertexShaderFile, string fragmentShaderFile)
        {
            try
            {
                my_VertexShader = LoadTextFile(vertexShaderFile);
                my_FragmentShader = LoadTextFile(fragmentShaderFile);
                my_UpdateShader = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SetShaders was unable to load the shaders : " + ex.Message, "GLScene Error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Creates the ShaderProgram
        /// <para>This is called automatic at the next render pass after setting Shaders.</para>
        /// </summary>
        public void UpdateShaders()
        {
            if (double.Parse(my_gl.Version.Substring(0, 3)) < 2.0)
            {
                throw new Exception("OpenGL version " + double.Parse(my_gl.Version.Substring(0, 3)) + " does not allow use of shaders.");
            }
            //Create the shaderProgram.
            try
            {
                my_Shader = new ShaderProgram();
                my_Shader.Create(my_gl, my_VertexShader, my_FragmentShader, my_AttributeLocations);
                my_Shader.Bind(my_gl);
                my_UpdateShader = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to update the shaders : " + ex.Message, "GLScene Error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Renders the scene. This must be called from the Parent window in a loop.
        /// </summary>
        public void Render()
        {
            //Clear the buffers
            my_gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);
            if (my_GenerateGeometries)
            {
                GenerateGeometries();
                my_GenerateGeometries = false;
            }
            if (my_UpdateShader)
            {
                UpdateShaders();
                my_UpdateShader = false;
            }
            //Create the view matrix each render pass to allow camera movement.
            viewMatrix = my_Camera.GetViewMatrix();
            my_Shader.SetUniform3(my_gl, "viewPos", my_Camera.X, my_Camera.Y, my_Camera.Z);
            my_Shader.SetUniformMatrix4(my_gl, "View", viewMatrix.to_array());
            my_Shader.SetUniformMatrix4(my_gl, "Projection", projectionMatrix.to_array());
            //Draw the Geometry
            foreach( GLGeometry geo in my_Geometries)
            {
                geo.Draw(my_Shader);
            }
        }

        #endregion

        #region "Utilities"

        private void CreateProjectionMatrix(double fov, double width, double height, double near, double far)
        {
            float H = (float)(fov * height / width);
            projectionMatrix = glm.pfrustum((float)(-fov), (float)fov, -H, H, (float)near, (float)far);
        }

        private void LoadDefaultShaders()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                StreamReader reader = new StreamReader(Environment.CurrentDirectory + "\\PerPixel.vert");
                my_VertexShader = reader.ReadToEnd();
                reader.Close();
                reader = new StreamReader(Environment.CurrentDirectory + "\\PerPixel.frag");
                my_FragmentShader = reader.ReadToEnd();
                reader.Close();
                my_UpdateShader = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load the Default Shaders : " + ex.Message, "GLScene Error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string LoadTextFile(string textFileName)
        {
            string result;
            StreamReader reader = new StreamReader(textFileName);
            result = reader.ReadToEnd();
            reader.Close();
            return result;
        }

        private void GLControl_OpenGLInitialized(Object sender , OpenGLEventArgs args )
        {
            //Does nothing. Used to prevent SharpGL error messages.
        }

        private void GLControl_OpenGLDraw(Object sender, OpenGLEventArgs args)
        {
            //Does nothing. Used to prevent SharpGL error messages.
        }

        private void GLControl_Resized(Object sender, OpenGLEventArgs args)
        {
            CreateProjectionMatrix(0.45F, ActualWidth, ActualHeight, 1, 300);
        }

        #endregion 
    }
}
