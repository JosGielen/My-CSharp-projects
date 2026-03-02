using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ImageTools
{
    public partial class SettingForm
    {
        //All settings are public
        public Brush ThumbBackground;
        public Brush ThumbSelectedBackground;
        public int ThumbFontSize;
        public int ThumbWidth;
        public int ThumbHeight;
        public int ThumbSpacing;
        public bool ThumbRoundedCorners;
        public string ImageFileName;
        public string StartFolder;
        public int ImageFormatIndex;
        public Brush ToolColor;
        public Brush ToolTextColor;
        public int ToolLineThickness;
        public Brush ToolSelectedColor;
        public Brush ToolSelectedTextcolor;
        public int ToolSelectedLineThickness;
        public int ToolFontSize;
        public int ToolDecimals;
        public Brush HandleColor;
        public int HandleAlpha;
        public int HandleSize;
        public Brush HandleSelectedColor;
        public int HandleSelectedAlpha;
        public int HandleSelectedSize;
        //private variables for internal use
        private string my_Inifile;
        private MainWindow my_Parent;
        private List<Brush> My_Brushes = new List<Brush>();

        public SettingForm(MainWindow parent)
        {
            // This call is required by the designer.
            InitializeComponent();
            my_Parent = parent;
            //Load the start-up settings (can be used without showing the form)
            my_Inifile = Environment.CurrentDirectory + "\\ImageTools.ini";
            try
            {
                LoadSettings(my_Inifile);
            }
            catch
            {
                //No ini file or wrong data format. ;
                DefaultSettings();
                SaveSettings(my_Inifile);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //This event occurs when Show() is first used on the form
            Init();
            WriteSettings();
        }

        private void Init()
        {
            //Initialize the controls on the form
            Type BrushesType = typeof(Brushes);
            BrushConverter bc = new BrushConverter();
            CmbThumbBackground.Items.Clear();
            CmbSelectBackground.Items.Clear();
            CmbToolColor.Items.Clear();
            CmbToolTextColor.Items.Clear();
            CmbSelectColor.Items.Clear();
            CmbSelectTextColor.Items.Clear();
            CmbHandleColor.Items.Clear();
            CmbSelectHandleColor.Items.Clear();
            My_Brushes.Clear();
            foreach (System.Reflection.PropertyInfo propinfo in BrushesType.GetProperties())
            {
                if (propinfo.PropertyType == typeof(SolidColorBrush))
                {
                    CmbThumbBackground.Items.Add(propinfo.Name);
                    CmbSelectBackground.Items.Add(propinfo.Name);
                    CmbToolColor.Items.Add(propinfo.Name);
                    CmbToolTextColor.Items.Add(propinfo.Name);
                    CmbSelectColor.Items.Add(propinfo.Name);
                    CmbSelectTextColor.Items.Add(propinfo.Name);
                    CmbHandleColor.Items.Add(propinfo.Name);
                    CmbSelectHandleColor.Items.Add(propinfo.Name);
                    My_Brushes.Add((Brush)bc.ConvertFromString(propinfo.Name));
                }
            }
        }

        #region "Buttons"

        private void BtnSEMStart_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Title = "Select the StartFolder.";
            OFD.DefaultDirectory = "C:\\";
            if (OFD.ShowDialog() == true)
            {
                StartFolder = Path.GetDirectoryName(OFD.FileName);
                TxtSEMStart.Text = StartFolder;
            }
        }

        private void BtnWidthUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThumbWidth = int.Parse(TxtWidth.Text);
                ThumbWidth += 1;
                TxtWidth.Text = ThumbWidth.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnWidthDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThumbWidth = int.Parse(TxtWidth.Text);
                if (ThumbWidth > 1)
                {
                    ThumbWidth -= 1;
                    TxtWidth.Text = ThumbWidth.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnHeightUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThumbHeight = int.Parse(TxtHeight.Text);
                ThumbHeight += 1;
                TxtHeight.Text = ThumbHeight.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnHeightDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThumbHeight = int.Parse(TxtHeight.Text);
                if (ThumbHeight > 1)
                {
                    ThumbHeight -= 1;
                    TxtHeight.Text = ThumbHeight.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnFontSizeUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThumbFontSize = int.Parse(TxtFontSize.Text);
                ThumbFontSize += 1;
                TxtFontSize.Text = ThumbFontSize.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnFontSizeDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThumbFontSize = int.Parse(TxtFontSize.Text);
                if (ThumbFontSize > 5)
                {
                    ThumbFontSize -= 1;
                    TxtFontSize.Text = ThumbFontSize.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnSpacingUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThumbSpacing = int.Parse(TxtSpacing.Text);
                ThumbSpacing += 1;
                TxtSpacing.Text = ThumbSpacing.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnSpacingDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThumbSpacing = int.Parse(TxtSpacing.Text);
                if (ThumbSpacing > 0)
                {
                    ThumbSpacing -= 1;
                    TxtSpacing.Text = ThumbSpacing.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnLinethicknessUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolLineThickness = int.Parse(TxtLineThickness.Text);
                ToolLineThickness += 1;
                TxtLineThickness.Text = ToolLineThickness.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnLineThicknessDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolLineThickness = int.Parse(TxtLineThickness.Text);
                if (ToolLineThickness > 1)
                {
                    ToolLineThickness -= 1;
                    TxtLineThickness.Text = ToolLineThickness.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnSelectLinethicknessUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolSelectedLineThickness = int.Parse(TxtSelectLineThickness.Text);
                ToolSelectedLineThickness += 1;
                TxtSelectLineThickness.Text = ToolSelectedLineThickness.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnSelectLineThicknessDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolSelectedLineThickness = int.Parse(TxtSelectLineThickness.Text);
                if (ToolSelectedLineThickness > 1)
                {
                    ToolSelectedLineThickness -= 1;
                    TxtSelectLineThickness.Text = ToolSelectedLineThickness.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnDecimalsUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolDecimals = int.Parse(TxtDecimals.Text);
                ToolDecimals += 1;
                TxtDecimals.Text = ToolDecimals.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnDecimalsDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolDecimals = int.Parse(TxtDecimals.Text);
                if (ToolDecimals > 0)
                {
                    ToolDecimals -= 1;
                    TxtDecimals.Text = ToolDecimals.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnToolFontSizeUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolFontSize = int.Parse(TxtToolFontSize.Text);
                ToolFontSize += 1;
                TxtToolFontSize.Text = ToolFontSize.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnToolFontSizeDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolFontSize = int.Parse(TxtToolFontSize.Text);
                if (ToolFontSize > 5)
                {
                    ToolFontSize -= 1;
                    TxtToolFontSize.Text = ToolFontSize.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnHandleAlphaUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HandleAlpha = int.Parse(TxtHandleAlpha.Text);
                HandleAlpha += 10;
                TxtHandleAlpha.Text = HandleAlpha.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnHandleAlphaDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HandleAlpha = int.Parse(TxtHandleAlpha.Text);
                if (HandleAlpha >= 10)
                {
                    HandleAlpha -= 10;
                    TxtHandleAlpha.Text = HandleAlpha.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnHandleSizeUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HandleSize = int.Parse(TxtHandleSize.Text);
                HandleSize += 1;
                TxtHandleSize.Text = HandleSize.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnHandleSizeDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HandleSize = int.Parse(TxtHandleSize.Text);
                if (HandleSize > 1)
                {
                    HandleSize -= 1;
                    TxtHandleSize.Text = HandleSize.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnSelectHandleAlphaUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HandleSelectedAlpha = int.Parse(TxtSelectHandleAlpha.Text);
                HandleSelectedAlpha += 10;
                TxtSelectHandleAlpha.Text = HandleSelectedAlpha.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnSelectHandleAlphaDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HandleSelectedAlpha = int.Parse(TxtSelectHandleAlpha.Text);
                if (HandleSelectedAlpha >= 10)
                {
                    HandleSelectedAlpha -= 10;
                    TxtSelectHandleAlpha.Text = HandleSelectedAlpha.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnSelectHandleSizeUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HandleSelectedSize = int.Parse(TxtSelectHandleSize.Text);
                HandleSelectedSize += 1;
                TxtSelectHandleSize.Text = HandleSelectedSize.ToString();
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnSelectHandleSizeDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HandleSelectedSize = int.Parse(TxtSelectHandleSize.Text);
                if (HandleSelectedSize > 1)
                {
                    HandleSelectedSize -= 1;
                    TxtSelectHandleSize.Text = HandleSelectedSize.ToString();
                }
            }
            catch
            {
                //Do nothing
            }
        }

        #endregion

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            //Apply the current settings in the application but keep the settingform open.
            ReadSettings();
            my_Parent.Update();
        }

        private void BtnDefaults_Click(object sender, RoutedEventArgs e)
        {
            //Show the default settings in the settingform
            DefaultSettings();
            WriteSettings();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            //Apply the current settings in the application and hide the settingform.
            ReadSettings();
            my_Parent.Update();
            Hide();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Hide the settingform.
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Prevent closing the form so the settings remain accessible
            Hide();
            e.Cancel = true;
        }

        private void DefaultSettings()
        {
            //Set the default settings
            ThumbBackground = Brushes.White;
            ThumbSelectedBackground = Brushes.Blue;
            ThumbFontSize = 12;
            ThumbWidth = 150;
            ThumbHeight = 200;
            ThumbSpacing = 5;
            ThumbRoundedCorners = true;
            StartFolder = "C:\\";
            ImageFormatIndex = 2;
            ToolColor = Brushes.Red;
            ToolTextColor = Brushes.White;
            ToolLineThickness = 1;
            ToolSelectedColor = Brushes.Yellow;
            ToolSelectedTextcolor = Brushes.Black;
            ToolSelectedLineThickness = 1;
            ToolFontSize = 18;
            ToolDecimals = 1;
            HandleColor = Brushes.White;
            HandleAlpha = 50;
            HandleSize = 12;
            HandleSelectedColor = Brushes.Green;
            HandleSelectedAlpha = 50;
            HandleSelectedSize = 18;
            ImageFileName = "";
        }

        private void ReadSettings()
        {
            //Read the values in the settingform to the setting variables
            ThumbBackground = My_Brushes[CmbThumbBackground.SelectedIndex];
            ThumbSelectedBackground = My_Brushes[CmbSelectBackground.SelectedIndex];
            ToolColor = My_Brushes[CmbToolColor.SelectedIndex];
            ToolTextColor = My_Brushes[CmbToolTextColor.SelectedIndex];
            ToolSelectedColor = My_Brushes[CmbSelectColor.SelectedIndex];
            ToolSelectedTextcolor = My_Brushes[CmbSelectTextColor.SelectedIndex];
            HandleColor = My_Brushes[CmbHandleColor.SelectedIndex];
            HandleSelectedColor = My_Brushes[CmbSelectHandleColor.SelectedIndex];
            ThumbFontSize = int.Parse(TxtFontSize.Text);
            ThumbWidth = int.Parse(TxtWidth.Text);
            ThumbHeight = int.Parse(TxtHeight.Text);
            ThumbSpacing = int.Parse(TxtSpacing.Text);
            ThumbRoundedCorners = CbRoundedCorners.IsChecked.Value;
            StartFolder = TxtSEMStart.Text;
            ImageFormatIndex = CmbImageFormat.SelectedIndex + 1;
            ToolLineThickness = int.Parse(TxtLineThickness.Text);
            ToolSelectedLineThickness = int.Parse(TxtSelectLineThickness.Text);
            ToolFontSize = int.Parse(TxtToolFontSize.Text);
            ToolDecimals = int.Parse(TxtDecimals.Text);
            HandleAlpha = int.Parse(TxtHandleAlpha.Text);
            HandleSize = int.Parse(TxtHandleSize.Text);
            HandleSelectedAlpha = int.Parse(TxtSelectHandleAlpha.Text);
            HandleSelectedSize = int.Parse(TxtSelectHandleSize.Text);
        }

        private void WriteSettings()
        {
            //Show the setting variables in the settingform
            int index = 0;
            for (int I = 0; I < My_Brushes.Count; I++)
            {
                if (My_Brushes[I].ToString() == ThumbBackground.ToString())
                {
                    CmbThumbBackground.SelectedIndex = I;
                }
                if (My_Brushes[I].ToString() == ThumbSelectedBackground.ToString())
                {
                    CmbSelectBackground.SelectedIndex = I;
                }
                if (My_Brushes[I].ToString() == ToolColor.ToString())
                {
                    CmbToolColor.SelectedIndex = I;
                }
                if (My_Brushes[I].ToString() == ToolTextColor.ToString())
                {
                    CmbToolTextColor.SelectedIndex = I;
                }
                if (My_Brushes[I].ToString() == ToolSelectedColor.ToString())
                {
                    CmbSelectColor.SelectedIndex = I;
                }
                if (My_Brushes[I].ToString() == ToolSelectedTextcolor.ToString())
                {
                    CmbSelectTextColor.SelectedIndex = I;
                }
                if (My_Brushes[I].ToString() == HandleColor.ToString())
                {
                    CmbHandleColor.SelectedIndex = I;
                }
                if (My_Brushes[I].ToString() == HandleSelectedColor.ToString())
                {
                    CmbSelectHandleColor.SelectedIndex = I;
                }
            }
            TxtFontSize.Text = ThumbFontSize.ToString();
            TxtWidth.Text = ThumbWidth.ToString();
            TxtHeight.Text = ThumbHeight.ToString();
            TxtSpacing.Text = ThumbSpacing.ToString();
            CbRoundedCorners.IsChecked = ThumbRoundedCorners;
            TxtSEMStart.Text = StartFolder;
            CmbImageFormat.SelectedIndex = ImageFormatIndex - 1;
            TxtLineThickness.Text = ToolLineThickness.ToString();
            TxtSelectLineThickness.Text = ToolSelectedLineThickness.ToString();
            TxtToolFontSize.Text = ToolFontSize.ToString();
            TxtDecimals.Text = ToolDecimals.ToString();
            TxtHandleAlpha.Text = HandleAlpha.ToString();
            TxtHandleSize.Text = HandleSize.ToString();
            TxtSelectHandleAlpha.Text = HandleSelectedAlpha.ToString();
            TxtSelectHandleSize.Text = HandleSelectedSize.ToString();
        }

        private void MnuLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            //Show an OpenFile dialog
            OFD.InitialDirectory = Path.GetDirectoryName(my_Inifile);
            OFD.Filter = "Ini Files (*.ini)|*.ini";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;
            if (OFD.ShowDialog() == true)
            {
                my_Inifile = OFD.FileName;
                try
                {
                    LoadSettings(my_Inifile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot load the ImageTools settings!\nOriginal error: " + ex.Message, "SEMView error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MnuSave_Click(object sender, RoutedEventArgs e)
        {
            ReadSettings();
            SaveSettings(my_Inifile);
        }

        private void MnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.InitialDirectory = Path.GetDirectoryName(my_Inifile);
            SFD.Filter = "Ini Files (*.ini)|*.ini";
            SFD.FilterIndex = 1;
            SFD.RestoreDirectory = true;
            if (SFD.ShowDialog() == true)
            {
                my_Inifile = SFD.FileName;
                ReadSettings();
                SaveSettings(my_Inifile);
            }
        }

        private void MnuClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void MnuUpdate_Click(object sender, RoutedEventArgs e)
        {
            WriteSettings();
        }

        public void SaveSettings()
        {
            SaveSettings(my_Inifile);
        }

        private void SaveSettings(string filename)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(filename);
                //Write the data to the file
                sw.WriteLine(ThumbBackground.ToString());
                sw.WriteLine(ThumbSelectedBackground.ToString());
                sw.WriteLine(ThumbFontSize.ToString());
                sw.WriteLine(ThumbWidth.ToString());
                sw.WriteLine(ThumbHeight.ToString());
                sw.WriteLine(ThumbSpacing.ToString());
                sw.WriteLine(ThumbRoundedCorners.ToString());
                sw.WriteLine(StartFolder.ToString());
                sw.WriteLine(ImageFormatIndex.ToString());
                sw.WriteLine(ToolColor.ToString());
                sw.WriteLine(ToolTextColor.ToString());
                sw.WriteLine(ToolLineThickness.ToString());
                sw.WriteLine(ToolSelectedColor.ToString());
                sw.WriteLine(ToolSelectedTextcolor.ToString());
                sw.WriteLine(ToolSelectedLineThickness.ToString());
                sw.WriteLine(ToolFontSize.ToString());
                sw.WriteLine(ToolDecimals.ToString());
                sw.WriteLine(HandleColor.ToString());
                sw.WriteLine(HandleAlpha.ToString());
                sw.WriteLine(HandleSize.ToString());
                sw.WriteLine(HandleSelectedColor.ToString());
                sw.WriteLine(HandleSelectedAlpha.ToString());
                sw.WriteLine(HandleSelectedSize.ToString());
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Cannot save the ImageTools settings!\nOriginal error: " + Ex.Message, "SEMView error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if ((sw != null))
                {
                    sw.Close();
                }
            }
        }

        private void LoadSettings(string filename)
        {
            BrushConverter bc = new BrushConverter();
            StreamReader sr = null;
            sr = new StreamReader(filename);
            ThumbBackground = (Brush)bc.ConvertFromString(sr.ReadLine());
            ThumbSelectedBackground = (Brush)bc.ConvertFromString(sr.ReadLine());
            ThumbFontSize = int.Parse(sr.ReadLine());
            ThumbWidth = int.Parse(sr.ReadLine());
            ThumbHeight = int.Parse(sr.ReadLine());
            ThumbSpacing = int.Parse(sr.ReadLine());
            ThumbRoundedCorners = bool.Parse(sr.ReadLine());
            StartFolder = sr.ReadLine();
            ImageFormatIndex = int.Parse(sr.ReadLine());
            ToolColor = (Brush)bc.ConvertFromString(sr.ReadLine());
            ToolTextColor = (Brush)bc.ConvertFromString(sr.ReadLine());
            ToolLineThickness = int.Parse(sr.ReadLine());
            ToolSelectedColor = (Brush)bc.ConvertFromString(sr.ReadLine());
            ToolSelectedTextcolor = (Brush)bc.ConvertFromString(sr.ReadLine());
            ToolSelectedLineThickness = int.Parse(sr.ReadLine());
            ToolFontSize = int.Parse(sr.ReadLine());
            ToolDecimals = int.Parse(sr.ReadLine());
            HandleColor = (Brush)bc.ConvertFromString(sr.ReadLine());
            HandleAlpha = int.Parse(sr.ReadLine());
            HandleSize = int.Parse(sr.ReadLine());
            HandleSelectedColor = (Brush)bc.ConvertFromString(sr.ReadLine());
            HandleSelectedAlpha = int.Parse(sr.ReadLine());
            HandleSelectedSize = int.Parse(sr.ReadLine());
            ImageFileName = "";
            if ((sr != null))
            {
                sr.Close();
            }
        }
    }
}
