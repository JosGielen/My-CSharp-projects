using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Media.Media3D;
using JG_GL;
using System.Windows.Controls;
using System.IO;
using System.Collections.Generic;

namespace STL_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool App_Loaded = false;
        private GLGeometry my_Geo;
        //Camera positioning
        private Vector3D CamStartPos = new Vector3D(5.0, 5.0, 10.0);
        private Vector3D CamStartTarget = new Vector3D(0.0, 0.0, 0.0);
        private Vector3D CamUpDir = new Vector3D(0.0, 1.0, 0.0);
        //FPS data
        private DateTime LastRenderTime;
        private int Framecounter;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Initialize FPS counter
            LastRenderTime = DateTime.Now;
            Framecounter = 0;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            //Set the scene lights
            Scene1.Lights.Clear();
            GLLight l1 = new GLLight(LightType.DirectionalLight)
            {
                Position = new Vector3D(0.0, 0.0, 0.0),
                Direction = new Vector3D(-1.0, -2.0, -3.0),
                Ambient = Color.FromRgb(255, 255, 255),
                Diffuse = Color.FromRgb(255, 255, 255),
                Specular = Color.FromRgb(255, 255, 255)
            };
            Scene1.AddLight(l1);
            GLLight l2 = new GLLight(LightType.PointLight)
            {
                Position = new Vector3D(0.0, 8.0, 0.0),
                Ambient = Color.FromRgb(150, 150, 150),
                Diffuse = Color.FromRgb(200, 200, 200),
                Specular = Color.FromRgb(255, 255, 255),
                Linear = 0.1,
                Quadratic = 0.0,
                Constant = 0.01
            };
            Scene1.AddLight(l2);
            GLLight l3 = new GLLight(LightType.SpotLight)
            {
                Position = new Vector3D(-4.0, 4.0, -4.0),
                Direction = new Vector3D(4.0, -2.0, 4.0),
                Ambient = Color.FromRgb(150, 150, 150),
                Diffuse = Color.FromRgb(255, 255, 255),
                Specular = Color.FromRgb(255, 255, 255),
                CutOff = 10.0,
                OuterCutOff = 20.0,
                Linear = 0.05,
                Quadratic = 0.01,
                Constant = 0.0
            };
            Scene1.AddLight(l3);
            Scene1.ShowAxes = true;
            Scene1.ShowGrid = false;
            Scene1.Axes.X_Axis.Length = 5;
            Scene1.Axes.Y_Axis.Length = 5;
            Scene1.Axes.Z_Axis.Length = 5;
            Scene1.Axes.LabelSize = 0.2;
            Scene1.Axes.ArrowSize = 0.2;
            Scene1.Grid.MaxSize = 5;
            Scene1.Grid.Interval = 0.5;
            //Scene1.Camera = new FreeFlyingCamera();
            Scene1.Camera.Position = CamStartPos;
            Scene1.Camera.TargetPosition = CamStartTarget;
            Scene1.Camera.UpDirection = CamUpDir;
            SetGeneralSceneParameters();
            App_Loaded = true;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (!App_Loaded) return;
            //Show FPS
            Framecounter += 1;
            if (Framecounter == 100)
            {
                double fps = (int)(100000 / (DateTime.Now - LastRenderTime).TotalMilliseconds);
                if (my_Geo != null)
                {
                    Title = "Vertex Count = " + my_Geo.Vertices.Length.ToString() + "  :  FPS = " + fps.ToString();
                }
                LastRenderTime = DateTime.Now;
                Framecounter = 0;
            }
            //Update the scene
            foreach (GLGeometry geo in Scene1.Geometries)
            {
                geo.Update();
            }
            //Render the scene.
            Scene1.Render();
        }


        private void MnuOpenSTL_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //Show an OpenFile dialog
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog1.Filter = "STL Files (*.stl)|*.stl|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == true)
            {
                CreateSTLGeometry(openFileDialog1.FileName);
            }
        }

        private void MnuOpenVertex_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //Show an OpenFile dialog
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog1.Filter = "JG_Vertex Files (*.jgv)|*.jgv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == true)
            {
                CreateVertexGeometry(openFileDialog1.FileName);
            }
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void CreateSTLGeometry(string stlfile)
        {
            //Create the Scene Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new STL_ImportGeometry(1, stlfile)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void CreateVertexGeometry(string jgvfile)
        {
            //Create the Scene Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new JG_VertexGeometry(1, jgvfile)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void CmbGeometryDrawMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!App_Loaded) return;
            switch (CmbGeometryDrawMode.SelectedIndex)
            {
                case 0:
                    my_Geo.DrawMode = DrawMode.Fill;
                    break;
                case 1:
                    my_Geo.DrawMode = DrawMode.Lines;
                    break;
                case 2:
                    my_Geo.DrawMode = DrawMode.Points;
                    break;
            }
        }

        private void SetGeneralGeoParameters()
        {
            switch (my_Geo.DrawMode)
            {
                case DrawMode.Fill:
                    CmbGeometryDrawMode.SelectedIndex = 0;
                    break;
                case DrawMode.Lines:
                    CmbGeometryDrawMode.SelectedIndex = 1;
                    break;
                case DrawMode.Points:
                    CmbGeometryDrawMode.SelectedIndex = 2;
                    break;
            }
            TxtGeometryPosX.Text = my_Geo.Position.X.ToString();
            TxtGeometryPosY.Text = my_Geo.Position.Y.ToString();
            TxtGeometryPosZ.Text = my_Geo.Position.Z.ToString();
            TxtGeometryInitRotX.Text = my_Geo.InitialRotationAxis.X.ToString();
            TxtGeometryInitRotY.Text = my_Geo.InitialRotationAxis.Y.ToString();
            TxtGeometryInitRotZ.Text = my_Geo.InitialRotationAxis.Z.ToString();
            TxtGeometryRotAxisX.Text = my_Geo.RotationAxis.X.ToString();
            TxtGeometryRotAxisY.Text = my_Geo.RotationAxis.Y.ToString();
            TxtGeometryRotAxisZ.Text = my_Geo.RotationAxis.Z.ToString();
            TxtGeometryRotSpeed.Text = my_Geo.RotationSpeed.ToString();
            TxtGeometryAmbientR.Text = my_Geo.AmbientMaterial.R.ToString();
            TxtGeometryAmbientG.Text = my_Geo.AmbientMaterial.G.ToString();
            TxtGeometryAmbientB.Text = my_Geo.AmbientMaterial.B.ToString();
            TxtGeometryDiffuseR.Text = my_Geo.DiffuseMaterial.R.ToString();
            TxtGeometryDiffuseG.Text = my_Geo.DiffuseMaterial.G.ToString();
            TxtGeometryDiffuseB.Text = my_Geo.DiffuseMaterial.B.ToString();
            TxtGeometrySpecularR.Text = my_Geo.SpecularMaterial.R.ToString();
            TxtGeometrySpecularG.Text = my_Geo.SpecularMaterial.G.ToString();
            TxtGeometrySpecularB.Text = my_Geo.SpecularMaterial.B.ToString();
            TxtGeometryShininess.Text = my_Geo.Shininess.ToString();
            CbGeometryMaterialUse.IsChecked = my_Geo.UseMaterial;
            TxtGeometryTexFile.Text = my_Geo.TextureFile;
            TxtGeometryTexScaleX.Text = my_Geo.TextureScaleX.ToString();
            TxtGeometryTexScaleY.Text = my_Geo.TextureScaleY.ToString();
            CbGeometryTextureUse.IsChecked = my_Geo.UseTexture;
            TxtGeometryPalette.Text = my_Geo.PaletteFile;
            CbGeometryVertColUse.IsChecked = my_Geo.UseVertexColors;
            TxtGeometryVertColInt.Text = my_Geo.VertexColorIntensity.ToString();
        }

        private void SetGeneralSceneParameters()
        {
            CBShowAxes.IsChecked = Scene1.ShowAxes;
            CBShowGrid.IsChecked = Scene1.ShowGrid;
            CBShowXYGrid.IsChecked = Scene1.ShowGrid && Scene1.Grid.ShowXY_Xdirection;
            CBShowXZGrid.IsChecked = Scene1.ShowGrid && Scene1.Grid.ShowXZ_XDirection;
            CBShowYZGrid.IsChecked = Scene1.ShowGrid && Scene1.Grid.ShowYZ_YDirection;
            TxtAxesLength.Text = Scene1.Axes.X_Axis.Length.ToString();
            TxtLabelSize.Text = Scene1.Axes.LabelSize.ToString();
            TxtArrowSize.Text = Scene1.Axes.ArrowSize.ToString();
            TxtGridSize.Text = Scene1.Grid.MaxSize.ToString();
            TxtGridInterval.Text = Scene1.Grid.Interval.ToString();
            CBDirLight.IsChecked = Scene1.Lights[0].SwitchedOn;
            CBPointLight.IsChecked = Scene1.Lights[1].SwitchedOn;
            CBSpotLight.IsChecked = Scene1.Lights[2].SwitchedOn;
        }

        private void TxtGeometryPos_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                my_Geo.Position = new Vector3D(double.Parse(TxtGeometryPosX.Text), double.Parse(TxtGeometryPosY.Text), double.Parse(TxtGeometryPosZ.Text));
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGeometryInitRot_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                double X = double.Parse(TxtGeometryInitRotX.Text);
                double Y = double.Parse(TxtGeometryInitRotY.Text);
                double Z = double.Parse(TxtGeometryInitRotZ.Text);
                if (X + Y + Z != 0)
                {
                    my_Geo.InitialRotationAxis = new Vector3D(X, Y, Z);
                    my_Geo.GenerateGeometry(Scene1);
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGeometryRotAxis_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                double X = double.Parse(TxtGeometryRotAxisX.Text);
                double Y = double.Parse(TxtGeometryRotAxisY.Text);
                double Z = double.Parse(TxtGeometryRotAxisZ.Text);
                if (X + Y + Z != 0)
                {
                    my_Geo.RotationAxis = new Vector3D(X, Y, Z);
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGeometryRotSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                my_Geo.RotationSpeed = double.Parse(TxtGeometryRotSpeed.Text);
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGeometryAmbient_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                my_Geo.AmbientMaterial = Color.FromRgb(byte.Parse(TxtGeometryAmbientR.Text), byte.Parse(TxtGeometryAmbientG.Text), byte.Parse(TxtGeometryAmbientB.Text));
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGeometryDiffuse_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                my_Geo.DiffuseMaterial = Color.FromRgb(byte.Parse(TxtGeometryDiffuseR.Text), byte.Parse(TxtGeometryDiffuseG.Text), byte.Parse(TxtGeometryDiffuseB.Text));
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGeometrySpecular_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                my_Geo.SpecularMaterial = Color.FromRgb(byte.Parse(TxtGeometrySpecularR.Text), byte.Parse(TxtGeometrySpecularG.Text), byte.Parse(TxtGeometrySpecularB.Text));
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGeometryShininess_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                my_Geo.Shininess = double.Parse(TxtGeometryShininess.Text);
            }
            catch
            {
                //Do nothing
            }
        }

        private void CbGeometryMaterialUse_Click(object sender, RoutedEventArgs e)
        {
            if (!App_Loaded) return;
            if (CbGeometryMaterialUse.IsChecked.Value)
            {
                my_Geo.UseMaterial = true;
            }
            else
            {
                my_Geo.UseMaterial = false;
            }
        }

        private void BtnGeometryTexBrowse_Click(object sender, RoutedEventArgs e)
        {
            //Select a Texture file (any image file)
            OpenFileDialog OFD = new OpenFileDialog()
            {
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "Windows Bitmap (*.bmp,*.dib)|*.bmp;*.dib|JPEG (*.jpg,*.jpeg,*.jfif,*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe|TIFF (*.tif,tiff)|*.tif;*.tiff|PNG (*.png)|*.png|GIF (*.gif)| *.gif|All Image files (*.*)|*.*",
                FilterIndex = 6,
                RestoreDirectory = true
            };
            if (OFD.ShowDialog() == true)
            {
                TxtGeometryTexFile.Text = Path.GetFileName(OFD.FileName);
                my_Geo.TextureFile = OFD.FileName;
                my_Geo.UseTexture = true;
                CbGeometryTextureUse.IsChecked = true;
            }
        }

        private void CbGeometryTextureUse_Click(object sender, RoutedEventArgs e)
        {
            if (!App_Loaded) return;
            if (CbGeometryTextureUse.IsChecked.Value)
            {
                my_Geo.UseTexture = true;
            }
            else
            {
                my_Geo.UseTexture = false;
            }
        }

        private void TxtGeometryTexScaleX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                my_Geo.TextureScaleX = double.Parse(TxtGeometryTexScaleX.Text);
                my_Geo.GenerateGeometry(Scene1);
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGeometryTexScaleY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                my_Geo.TextureScaleY = double.Parse(TxtGeometryTexScaleY.Text);
                my_Geo.GenerateGeometry(Scene1);
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnGeometryPaletteBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog()
            {
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "Palette files (*.cpl)|*.cpl",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            if (OFD.ShowDialog() == true)
            {
                TxtGeometryPalette.Text = Path.GetFileName(OFD.FileName);
                Vector3D V = my_Geo.GetVertexLayout();
                int colorcount = 1;
                try
                {
                    if (V.X > 0) colorcount *= (int)V.X;
                    if (V.Y > 0) colorcount *= (int)V.Y;
                    if (V.Z > 0) colorcount *= (int)V.Z;
                    if (colorcount < 0) colorcount = (int)(V.X + V.Y + V.Z);
                }
                catch
                {
                    colorcount = (int)(V.X + V.Y + V.Z);
                }
                CbGeometryVertColUse.IsChecked = true;
                my_Geo.SetVertexColors(OFD.FileName, colorcount);
                my_Geo.GenerateGeometry(Scene1);
            }
        }

        private void CbGeometryVertColUse_Click(object sender, RoutedEventArgs e)
        {
            if (CbGeometryVertColUse.IsChecked.Value)
            {
                my_Geo.UseVertexColors = true;
            }
            else
            {
                my_Geo.UseVertexColors = false;
            }
            my_Geo.GenerateGeometry(Scene1);
        }

        private void TxtGeometryVertColInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                my_Geo.VertexColorIntensity = double.Parse(TxtGeometryVertColInt.Text);
                my_Geo.GenerateGeometry(Scene1);
            }
            catch
            {
                //Do nothing
            }
        }

        private void CBShowAxes_Click(object sender, RoutedEventArgs e)
        {
            if (!App_Loaded) return;
            if (CBShowAxes.IsChecked.Value)
            {
                Scene1.ShowAxes = true;
            }
            else
            {
                Scene1.ShowAxes = false;
            }
            Scene1.GenerateGeometries();
        }

        private void CBShowGrid_Click(object sender, RoutedEventArgs e)
        {
            if (!App_Loaded) return;
            if (CBShowGrid.IsChecked.Value)
            {
                Scene1.ShowGrid = true;
                CBShowXYGrid.IsChecked = Scene1.Grid.ShowXY_Xdirection;
                CBShowXZGrid.IsChecked = Scene1.Grid.ShowXZ_XDirection;
                CBShowYZGrid.IsChecked = Scene1.Grid.ShowYZ_YDirection;
            }
            else
            {
                Scene1.ShowGrid = false;
                CBShowXYGrid.IsChecked = false;
                CBShowXZGrid.IsChecked = false;
                CBShowYZGrid.IsChecked = false;
            }
            Scene1.GenerateGeometries();
        }

        private void CBShowXYGrid_Click(object sender, RoutedEventArgs e)
        {
            if (!App_Loaded) return;
            if (CBShowXYGrid.IsChecked.Value)
            {
                Scene1.Grid.ShowXY_Xdirection = true;
                Scene1.Grid.ShowXY_YDirection = true;
            }
            else
            {
                Scene1.Grid.ShowXY_Xdirection = false;
                Scene1.Grid.ShowXY_YDirection = false;
            }
            Scene1.GenerateGeometries();
        }

        private void CBShowXZGrid_Click(object sender, RoutedEventArgs e)
        {
            if (!App_Loaded) return;
            if (CBShowXZGrid.IsChecked.Value)
            {
                Scene1.Grid.ShowXZ_XDirection = true;
                Scene1.Grid.ShowXZ_ZDirection = true;
            }
            else
            {
                Scene1.Grid.ShowXZ_XDirection = false;
                Scene1.Grid.ShowXZ_ZDirection = false;
            }
            Scene1.GenerateGeometries();
        }

        private void CBShowYZGrid_Click(object sender, RoutedEventArgs e)
        {
            if (!App_Loaded) return;
            if (CBShowYZGrid.IsChecked.Value)
            {
                Scene1.Grid.ShowYZ_YDirection = true;
                Scene1.Grid.ShowYZ_ZDirection = true;
            }
            else
            {
                Scene1.Grid.ShowYZ_YDirection = false;
                Scene1.Grid.ShowYZ_ZDirection = false;
            }
            Scene1.GenerateGeometries();
        }

        private void TxtAxesLength_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                Scene1.Axes.X_Axis.Length = double.Parse(TxtAxesLength.Text);
                Scene1.Axes.Y_Axis.Length = double.Parse(TxtAxesLength.Text);
                Scene1.Axes.Z_Axis.Length = double.Parse(TxtAxesLength.Text);
                Scene1.GenerateGeometries();
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtLabelSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                Scene1.Axes.LabelSize = double.Parse(TxtLabelSize.Text);
                Scene1.GenerateGeometries();
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtArrowSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                Scene1.Axes.ArrowSize = double.Parse(TxtArrowSize.Text);
                Scene1.GenerateGeometries();
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGridSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                Scene1.Grid.MaxSize = double.Parse(TxtGridSize.Text);
                Scene1.GenerateGeometries();
            }
            catch
            {
                //Do nothing
            }
        }

        private void TxtGridInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App_Loaded) return;
            try
            {
                Scene1.Grid.Interval = double.Parse(TxtGridInterval.Text);
                Scene1.GenerateGeometries();
            }
            catch
            {
                //Do nothing
            }
        }

        private void CBDirLight_Click(object sender, RoutedEventArgs e)
        {
            if (CBDirLight.IsChecked.Value)
            {
                Scene1.Lights[0].SwitchedOn = true;
            }
            else
            {
                Scene1.Lights[0].SwitchedOn = false;
            }
            Scene1.UpdateLights();
        }

        private void CBPointLight_Click(object sender, RoutedEventArgs e)
        {
            if (CBPointLight.IsChecked.Value)
            {
                Scene1.Lights[1].SwitchedOn = true;
            }
            else
            {
                Scene1.Lights[1].SwitchedOn = false;
            }
            Scene1.UpdateLights();
        }

        private void CBSpotLight_Click(object sender, RoutedEventArgs e)
        {
            if (CBSpotLight.IsChecked.Value)
            {
                Scene1.Lights[2].SwitchedOn = true;
            }
            else
            {
                Scene1.Lights[2].SwitchedOn = false;
            }
            Scene1.UpdateLights();
        }

        private void MnuShowBox_Click(object sender, RoutedEventArgs e)
        {
            //Create the Box Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new BoxGeometry()
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowCone_Click(object sender, RoutedEventArgs e)
        {
            //Create the Cone Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new ConeGeometry(0.0,3.0,5.0,20,40)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowCyl_Click(object sender, RoutedEventArgs e)
        {
            //Create the Cylinder Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new CylinderGeometry(2.0, 5.0)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShow3DLine_Click(object sender, RoutedEventArgs e)
        {
            //Create the CylinderLine Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new CylinderLineGeometry(0.1)
            {
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill,
                StartPt = new Vector3D(2.0,2.0,2.0),
                EndPt = new Vector3D(0.0,-3.0, -1.0)
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowDodec_Click(object sender, RoutedEventArgs e)
        {
            //Create the Dodecahedron Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new DodecahedronGeometry(2.5)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowEllips_Click(object sender, RoutedEventArgs e)
        {
            //Create the Ellipsoid Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new EllipsoidGeometry(3.0, 2.0, 1.5, 32, 32)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowIcosa_Click(object sender, RoutedEventArgs e)
        {
            //Create the Icosahedron Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new IcosahedronGeometry(2.0)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowLine_Click(object sender, RoutedEventArgs e)
        {
            //Create the Line Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new JG_GL.LineGeometry(2.0,3.0,1.0,-1.0,0.0,-2.0)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowMesh_Click(object sender, RoutedEventArgs e)
        {
            //Create the Mesh Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new MeshGeometry(50, 50, 0.1)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Lines
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowPLine_Click(object sender, RoutedEventArgs e)
        {
            //Create the PolyLine Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new PolyLineGeometry(3.0, 6.0, 3.0, true)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowPLineRot_Click(object sender, RoutedEventArgs e)
        {
            List<Point> my_Points = new List<Point>();
            my_Points.Add(new Point(2, 0));
            my_Points.Add(new Point(3.0, 1.0));
            my_Points.Add(new Point(4.0, 2.0));
            my_Points.Add(new Point(5.0, 3.0));
            my_Points.Add(new Point(5.0, 4.0));
            my_Points.Add(new Point(4.0, 5.0));
            my_Points.Add(new Point(3.0, 6.0));
            my_Points.Add(new Point(2.0, 7.0));
            my_Points.Add(new Point(2.0, 8.0));
            my_Points.Add(new Point(1.0, 8.0));
            my_Points.Add(new Point(1.0, 5.0));
            //Create the PolyLineRotation Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new PolylineRotationGeometry(4.0, 6.0, 2.0, my_Points, 64, 3)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 100);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 50);
            my_Geo.SpecularMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.Shininess = 15.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowText_Click(object sender, RoutedEventArgs e)
        {
            //Create the Text Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new TextGeometry("Hello World")
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill,
                FontSize = 28,
                Width = 5.5,
                ForeColor = Colors.Lime,
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(0, 0, 0);
            my_Geo.AmbientMaterial = Color.FromRgb(0, 0, 0);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowTorus_Click(object sender, RoutedEventArgs e)
        {
            //Create the Torus Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new TorusGeometry(1.5, 4.0, 64, 64)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 100, 100);
            my_Geo.AmbientMaterial = Color.FromRgb(150, 50, 50);
            my_Geo.SpecularMaterial = Color.FromRgb(150, 150, 150);
            my_Geo.Shininess = 25.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowTreFoil_Click(object sender, RoutedEventArgs e)
        {
            //Create the TreFoilKnot Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new TrefoilKnotGeometry(128, 128)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(100, 200, 100);
            my_Geo.AmbientMaterial = Color.FromRgb(50, 100, 50);
            my_Geo.SpecularMaterial = Color.FromRgb(150, 150, 150);
            my_Geo.Shininess = 15.0;
            my_Geo.UseMaterial = false;
            my_Geo.TextureFile = Environment.CurrentDirectory + "\\Textures\\Rope.jpg";
            my_Geo.UseTexture = true;
            my_Geo.TextureScaleX = 3.0;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowTube_Click(object sender, RoutedEventArgs e)
        {
            //Create the Tube Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new TubeGeometry(0.5,2.0,3.5,16,32)
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }

        private void MnuShowUserDef_Click(object sender, RoutedEventArgs e)
        {
            //Create the User Defined Geometry:
            App_Loaded = false;
            Scene1.ClearGeometries();
            my_Geo = new UserGeometry()
            {
                Position = new Vector3D(0, 0, 0),
                InitialRotationAxis = new Vector3D(0, 0, 0),
                RotationAxis = new Vector3D(0, 1, 0),
                RotationSpeed = 0.0,
                TextureScaleX = 1,
                TextureScaleY = 1,
                LineWidth = 1,
                PointSize = 2.0,
                DrawMode = DrawMode.Fill
            };
            my_Geo.DiffuseMaterial = Color.FromRgb(200, 200, 200);
            my_Geo.AmbientMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.SpecularMaterial = Color.FromRgb(100, 100, 100);
            my_Geo.Shininess = 5.0;
            my_Geo.UseMaterial = true;
            Scene1.AddGeometry(my_Geo);
            SetGeneralGeoParameters();
            App_Loaded = true;
        }
    }
}
