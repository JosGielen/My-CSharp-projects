using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ColorGradient_Dialog;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;

namespace Fractally
{
    public partial class MainWindow : Window
    {
        private delegate void RenderDelegate(int Y);
        private delegate void StatusDelegate(double v);
        private PixelFormat pf = PixelFormats.Rgb24;
        private int Stride = 0;
        private byte[] pixelData;
        private ImageBrush imgbrush = new ImageBrush();
        private string my_PaletteFile = "";
        private string my_GradientFile = "";
        private List<Color> Colors = new List<Color>();
        private ColorGradientDialog CG = null;
        private double[,] Iters;
        private double Xmin = 0D;
        private double Xmax = 0D;
        private double Ymin = 0D;
        private double Ymax = 0D;
        private double X1 = 0D;
        private double Y1 = 0D;
        private double X2 = 0D;
        private double Y2 = 0D;
        private double CalcRatio = 0.0;
        private int FracType = 0;
        private double ConstRe = 0.2;
        private double ConstIm = 0.565;
        private int Zmax = 100;     //Default Bail-out value
        private int Nmax = 500;     //Default Max number of iterations
        private double Colormultiplier = 1;
        private double ColorStartIndex = 0;
        private int VeldWidth = 0;
        private int VeldHeight = 0;
        private int MaxColIndex = 0;
        private bool Started = false;
        private bool CanResize = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region "Window Events"

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LBFracTypes.Items.Add("Mandelbrot");
            LBFracTypes.Items.Add("Julia");
            TxtBailout.Text = Zmax.ToString();
            TxtMaxIter.Text = Nmax.ToString();
            TxtConstRe.Text = ConstRe.ToString();
            TxtConstIm.Text = ConstIm.ToString();
            LblColorMag.Content = "Color Magnifier : " + Colormultiplier.ToString();
            LblColorScroll.Content = "Color Scroll : " + ColorStartIndex.ToString();
            //Set the initial Canvas Size
            VeldWidth = 700;
            VeldHeight = 600;
            ResizeWindow(VeldWidth, VeldHeight);
            ResetFractalData(VeldWidth, VeldHeight);
            CalcRatio = (double)VeldWidth / VeldHeight;
            //Default type = Mandelbrot;
            LBFracTypes.SelectedIndex = 0;
            FracType = 0;
            //Initial window = (-2.5,-1.5) - (1,1.5);
            Xmin = -2.5;
            Ymin = -1.5;
            Xmax = 1;
            Ymax = 1.5;
            X1 = Xmin;
            X2 = Xmax;
            Y1 = Ymin;
            Y2 = Ymax;
            //Set the initial color palette
            my_PaletteFile = Environment.CurrentDirectory + "\\default.cpl";
            my_GradientFile = Environment.CurrentDirectory + "\\default.cgr";
            OpenPalette(my_PaletteFile);
            imgbrush.ImageSource = BitmapSource.Create(VeldWidth, VeldHeight, 96, 96, pf, null, pixelData, Stride);
            imgbrush.Stretch = Stretch.UniformToFill;
            Canvas1.Background = imgbrush;
            //Set the rubberband properties
            Rband.AspectRatio = (double)VeldWidth / VeldHeight;
            Rband.Stroke = Brushes.Yellow;
            Rband.BoxFillOpacity = 0.3;
            Rband.BoxFillColor = Brushes.LightGray;
            Rband.CornerSize = 5;
            Rband.IsEnabled = false;
            CanResize = true;
            Title = "Fractally";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rband.Mouse_Down(e.GetPosition(Canvas1));
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Rband.Mouse_Move(e.GetPosition(Canvas1));
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rband.Mouse_Up(e.GetPosition(Canvas1));
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            double W = Canvas1.ActualWidth;
            double H = Canvas1.ActualHeight;
            double X1N;
            double X2N;
            double Y1N;
            double Y2N;
            if (Rband.IsEnabled & Rband.IsDrawn)
            {
                X1N = X1 + Rband.TopLeftCorner.X * (X2 - X1) / W;
                X2N = X1 + Rband.BottomRightCorner.X * (X2 - X1) / W;
                Y1N = Y1 + Rband.TopLeftCorner.Y * (Y2 - Y1) / H;
                Y2N = Y1 + Rband.BottomRightCorner.Y * (Y2 - Y1) / H;
                if (3.5 / (X2N - X1N) > 10000000000000.0)
                {
                    if (MessageBox.Show("The requested zoom exceeds the calculation accuracy./nDo you wish to proceed?", "Fractally Information", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No) return;
                }
                X1 = X1N;
                X2 = X2N;
                Y1 = Y1N;
                Y2 = Y2N;
                Rband.Clear();
                KeepRatio();
                BtnCalc_Click(this, new RoutedEventArgs());
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (CanResize)
            {
                //Fit the Canvas into the new window size
                Canvas1.Width = e.NewSize.Width - 168;
                Canvas1.Height = e.NewSize.Height - 94;
                VeldWidth = (int)(e.NewSize.Width - 168);
                VeldHeight = (int)(e.NewSize.Height - 94);
                KeepRatio();
                Rband.AspectRatio = (double)VeldWidth / VeldHeight;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        #endregion

        #region "Menu"

        private void MnuOpenFractal_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            StreamReader myStream;
            double newX1 = 0;
            double newX2 = 0;
            double newY1 = 0;
            double newY2 = 0;
            bool check = false;
            OFD.InitialDirectory = Environment.CurrentDirectory;
            OFD.Filter = "Fractals (*.frc)|*.frc";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;
            if (OFD.ShowDialog().Value)
            {
                try
                {
                    myStream = new StreamReader(OFD.FileName);
                    if (myStream != null)
                    {
                        // Lees de Instelling data uit de file
                        VeldWidth = int.Parse(myStream.ReadLine());
                        VeldHeight = int.Parse(myStream.ReadLine());
                        my_PaletteFile = myStream.ReadLine();
                        newX1 = double.Parse(myStream.ReadLine());
                        newX2 = double.Parse(myStream.ReadLine());
                        newY1 = double.Parse(myStream.ReadLine());
                        newY2 = double.Parse(myStream.ReadLine());
                        FracType = int.Parse(myStream.ReadLine());
                        ConstRe = double.Parse(myStream.ReadLine());
                        ConstIm = double.Parse(myStream.ReadLine());
                        Colormultiplier = double.Parse(myStream.ReadLine());
                        ColorStartIndex = double.Parse(myStream.ReadLine());
                        Zmax = int.Parse(myStream.ReadLine());
                        Nmax = int.Parse(myStream.ReadLine());
                        check = bool.Parse(myStream.ReadLine());
                        //Pas de Window en array afmetingen aan
                        ResizeWindow(VeldWidth, VeldHeight);
                        ResetFractalData(VeldWidth, VeldHeight);
                        //Lees de Fractal data
                        for (int I = 0; I < VeldWidth; I++)
                        {
                            for (int J = 0; J < VeldHeight; J++)
                            {
                                Iters[I, J] = double.Parse(myStream.ReadLine());
                            }
                        }
                    }
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Cannot read the Fractal data. Original error: " + Ex.Message, "Fractally error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //Check of myStream wel open is want er kan een exception geweest zijn.
                if (myStream != null)
                {
                    myStream.Close();
                }
                //Laad de palette kleuren
                OpenPalette(my_PaletteFile);
                //Zet de controls op de nieuwe waarden
                TxtBailout.Text = Zmax.ToString();
                TxtMaxIter.Text = Nmax.ToString();
                LBFracTypes.SelectedIndex = FracType;
                TxtConstRe.Text = ConstRe.ToString();
                TxtConstIm.Text = ConstIm.ToString();
                SldColorMag.Value = Colormultiplier;
                SldColorScroll.Value = ColorStartIndex;
                CBSmooth.IsChecked = check;
                X1 = newX1;
                X2 = newX2;
                Y1 = newY1;
                Y2 = newY2;
                //Onthoud de berekende waarden
                CalcRatio = VeldWidth / VeldHeight;
                //Teken de fractal
                UpdateColors();
                //Update the progressbar
                PBStatus.Value = 100;
            }
            Rband.IsEnabled = true;
        }

        private void MnuOpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.InitialDirectory = Environment.CurrentDirectory;
            OFD.Filter = "Bitmap (*.bmp)|*.bmp|JPEG format (*.jpg)|*.jpg";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;
            if (OFD.ShowDialog() == true)
            {
                BitmapImage bmp = new BitmapImage(new Uri(OFD.FileName));
                ResizeWindow(bmp.PixelWidth, bmp.PixelHeight);
                imgbrush.ImageSource = bmp;
                //Update the progressbar
                PBStatus.Value = 0;
            }
            Rband.Clear();
            Rband.IsEnabled = false;
        }

        private void MnuSavefractal_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            StreamWriter myStream = null;
            SFD.InitialDirectory = Environment.CurrentDirectory;
            SFD.Filter = "Fractals (*.frc)|*.frc";
            SFD.FilterIndex = 1;
            SFD.RestoreDirectory = true;
            if (SFD.ShowDialog() == true)
            {
                try
                {
                    myStream = new StreamWriter(SFD.FileName);
                    if (myStream != null)
                    {
                        //Write the fractal data to the file
                        myStream.WriteLine(VeldWidth);
                        myStream.WriteLine(VeldHeight);
                        myStream.WriteLine(my_PaletteFile);
                        myStream.WriteLine(X1);
                        myStream.WriteLine(X2);
                        myStream.WriteLine(Y1);
                        myStream.WriteLine(Y2);
                        myStream.WriteLine(FracType);
                        myStream.WriteLine(ConstRe);
                        myStream.WriteLine(ConstIm);
                        myStream.WriteLine(Colormultiplier);
                        myStream.WriteLine(ColorStartIndex);
                        myStream.WriteLine(Zmax);
                        myStream.WriteLine(Nmax);
                        myStream.WriteLine(CBSmooth.IsChecked);
                        for (int I = 0; I < VeldWidth; I++)
                        {
                            for (int J = 0; J < VeldHeight; J++)
                            {
                                myStream.WriteLine(Iters[I, J]);
                            }
                        }
                        //Update the progressbar
                        PBStatus.Value = 100;
                    }
                }
                catch (Exception Ex)
                {
                    //Update the progressbar
                    PBStatus.Value = 0;
                    MessageBox.Show("Cannot save the Fractal data. Original error: " + Ex.Message, "Fractally error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // Check this again, since we need to make sure we didn't throw an exception on open.
                if (myStream != null)
                {
                    myStream.Close();
                }
            }
        }

        private void MnuSave1024_Click(object sender, RoutedEventArgs e)
        {
            CalculateToFile(1024, 768);
        }

        private void MnuSave1280_Click(object sender, RoutedEventArgs e)
        {
            CalculateToFile(1280, 1024);
        }

        private void MnuSave1600_Click(object sender, RoutedEventArgs e)
        {
            CalculateToFile(1600, 1200);
        }

        private void MnuSave1366_Click(object sender, RoutedEventArgs e)
        {
            CalculateToFile(1366, 768);
        }

        private void MnuSave1920_Click(object sender, RoutedEventArgs e)
        {
            CalculateToFile(1920, 1080);
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MnuCopyImage_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bmp = BitmapSource.Create(VeldWidth, VeldHeight, 96, 96, pf, null, pixelData, Stride);
            Clipboard.SetImage(bmp);
        }

        private void MnuPasteImage_Click(object sender, RoutedEventArgs e)
        {
            imgbrush.ImageSource = Clipboard.GetImage();
            Rband.Clear();
            Rband.IsEnabled = false;
        }

        private void MnuClear_Click(object sender, RoutedEventArgs e)
        {
            imgbrush.ImageSource = null;
            Rband.Clear();
            Rband.IsEnabled = false;
        }

        private void MnuOpenPalette_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog1.Filter = "Color Palettes (*.cpl)|*.cpl";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == true)
            {
                my_PaletteFile = openFileDialog1.FileName;
                OpenPalette(my_PaletteFile);
                UpdateColors();
            }
        }

        private void MnuEditPalette_Click(object sender, RoutedEventArgs e)
        {
            CG = new ColorGradientDialog(my_GradientFile);
            CG.ShowDialog();
            if (CG.DialogResult == true )
            {
                UpdateColors(CG.ColorList);
            }
        }

        private void MnuNewPalette_Click(object sender, RoutedEventArgs e)
        {
            CG = new ColorGradientDialog();
            CG.ShowDialog();
        }

        public void UpdateColors(List<Color> colorList)
        {
            Colors = colorList;
            UpdateColors();
        }

        private void MnuCalculate_Click(object sender, RoutedEventArgs e)
        {
            BtnCalc_Click(sender, e);
        }

        private void MnuResetScale_Click(object sender, RoutedEventArgs e)
        {
            if (FracType == 0)
            {
                TxtConstRe.IsEnabled = false;
                TxtConstIm.IsEnabled = false;
                Xmin = -2.5D;
                Xmax = 1D;
                Ymin = -1.5D;
                Ymax = 1.5D;
            }
            else if (FracType == 1)
            {
                TxtConstRe.IsEnabled = true;
                TxtConstIm.IsEnabled = true;
                Xmin = -1.5D;
                Xmax = 1.5D;
                Ymin = -1.5D;
                Ymax = 1.5D;
            }
            X1 = Xmin;
            X2 = Xmax;
            Y1 = Ymin;
            Y2 = Ymax;
            KeepRatio();
            BtnCalc_Click(this, new RoutedEventArgs());
        }

        private void MnuSize1024_Click(object sender, RoutedEventArgs e)
        {
            ResizeWindow(1024, 768);
            VeldWidth = 1024;
            VeldHeight = 768;
            KeepRatio();
            BtnCalc_Click(sender, e);
        }

        private void MnuSize1280_Click(object sender, RoutedEventArgs e)
        {
            ResizeWindow(1280, 1024);
            VeldWidth = 1280;
            VeldHeight = 1024;
            KeepRatio();
            BtnCalc_Click(sender, e);
        }

        private void MnuSize1600_Click(object sender, RoutedEventArgs e)
        {
            ResizeWindow(1600, 1200);
            VeldWidth = 1600;
            VeldHeight = 1200;
            KeepRatio();
            BtnCalc_Click(sender, e);
        }

        private void MnuSize1366_Click(object sender, RoutedEventArgs e)
        {
            ResizeWindow(1366, 768);
            VeldWidth = 1366;
            VeldHeight = 768;
            KeepRatio();
            BtnCalc_Click(sender, e);
        }

        private void MnuSize1920_Click(object sender, RoutedEventArgs e)
        {
            ResizeWindow(1920, 1080);
            VeldWidth = 1920;
            VeldHeight = 1080;
            KeepRatio();
            BtnCalc_Click(sender, e);
        }

        private void MnuZoomIn2_Click(object sender, RoutedEventArgs e)
        {
            Zoom(2);
        }

        private void MnuZoomIn3_Click(object sender, RoutedEventArgs e)
        {
            Zoom(3);
        }

        private void MnuZoomIn4_Click(object sender, RoutedEventArgs e)
        {
            Zoom(4);
        }

        private void MnuZoomIn6_Click(object sender, RoutedEventArgs e)
        {
            Zoom(6);
        }

        private void MnuZoomOut2_Click(object sender, RoutedEventArgs e)
        {
            Zoom(1.0 / 2.0);
        }

        private void MnuZoomOut3_Click(object sender, RoutedEventArgs e)
        {
            Zoom(1.0 / 3.0);
        }

        private void MnuZoomOut4_Click(object sender, RoutedEventArgs e)
        {
            Zoom(1.0 / 4.0);
        }

        private void MnuZoomOut6_Click(object sender, RoutedEventArgs e)
        {
            Zoom(1.0 / 6.0);
        }

        #endregion

        #region "Controls"

        private void BtnBailoutUP_Click(object sender, RoutedEventArgs e)
        {
            double dummy = double.Parse(TxtBailout.Text);
            dummy += 50;
            TxtBailout.Text = dummy.ToString();
        }

        private void BtnBailoutDown_Click(object sender, RoutedEventArgs e)
        {
            double dummy = double.Parse(TxtBailout.Text);
            if (dummy > 50)
            {
                dummy -= 50;
                TxtBailout.Text = dummy.ToString();
            }
        }

        private void BtnMaxIterUP_Click(object sender, RoutedEventArgs e)
        {
            double dummy = double.Parse(TxtMaxIter.Text);
            dummy += 100;
            TxtMaxIter.Text = dummy.ToString();
        }

        private void BtnMaxIterDown_Click(object sender, RoutedEventArgs e)
        {
            double dummy = double.Parse(TxtMaxIter.Text);
            if (dummy > 100)
            {
                dummy -= 100;
                TxtMaxIter.Text = dummy.ToString();
            }
        }

        private void LBFracTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FracType = LBFracTypes.SelectedIndex;
            if (FracType == 0)
            {
                TxtConstRe.IsEnabled = false;
                TxtConstIm.IsEnabled = false;
                Xmin = -2.5D;
                Xmax = 1D;
                Ymin = -1.5D;
                Ymax = 1.5D;
            }
            else if (FracType == 1)
            {
                TxtConstRe.IsEnabled = true;
                TxtConstIm.IsEnabled = true;
                Xmin = -1.5D;
                Xmax = 1.5D;
                Ymin = -1.5D;
                Ymax = 1.5D;
            }
            X1 = Xmin;
            X2 = Xmax;
            Y1 = Ymin;
            Y2 = Ymax;
        }

        private void BtnConstReUP_Click(object sender, RoutedEventArgs e)
        {
            double dummy = double.Parse(TxtConstRe.Text);
            dummy += 0.1;
            TxtConstRe.Text = dummy.ToString();
        }

        private void BtnConstReDown_Click(object sender, RoutedEventArgs e)
        {
            double dummy = double.Parse(TxtConstRe.Text);
            dummy -= 0.1;
            TxtConstRe.Text = dummy.ToString();
        }

        private void BtnConstImUp_Click(object sender, RoutedEventArgs e)
        {
            double dummy = double.Parse(TxtConstIm.Text);
            dummy += 0.1;
            TxtConstIm.Text = dummy.ToString();
        }

        private void BtnConstImDown_Click(object sender, RoutedEventArgs e)
        {
            double dummy = double.Parse(TxtConstIm.Text);
            dummy -= 0.1;
            TxtConstIm.Text = dummy.ToString();
        }

        private void SldColorMag_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SldColorMag.Value != Math.Round(SldColorMag.Value))
            {
                SldColorMag.Value = Math.Round(SldColorMag.Value);
            }
            if (SldColorMag.Value != Colormultiplier)
            {
                if (SldColorMag.Value == 0)
                {
                    Colormultiplier = 1;
                }
                else if (SldColorMag.Value < 0)
                {
                    Colormultiplier = 1 + SldColorMag.Value * 0.9 / 50;
                }
                else
                {
                    Colormultiplier = SldColorMag.Value;
                }
                LblColorMag.Content = "Color Magnifier : " + Colormultiplier.ToString();
                UpdateColors();
            }
        }

        private void SldColorScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SldColorScroll.Value != Math.Round(SldColorScroll.Value))
            {
                SldColorScroll.Value = Math.Round(SldColorScroll.Value);
            }
            if (SldColorScroll.Value != ColorStartIndex)
            {
                ColorStartIndex = SldColorScroll.Value;
                LblColorScroll.Content = "Color Scroll : " + ColorStartIndex.ToString();
                UpdateColors();
            }
        }

        private void BtnZoom2_Click(object sender, RoutedEventArgs e)
        {
            if (RBzoomIn.IsChecked.Value)
            {
                Zoom(2);
            }
            else
            {
                Zoom(1.0 / 2.0);
            }
        }

        private void BtnZoom3_Click(object sender, RoutedEventArgs e)
        {
            if (RBzoomIn.IsChecked.Value)
            {
                Zoom(3);
            }
            else
            {
                Zoom(1.0 / 3.0);
            }
        }

        private void BtnZoom4_Click(object sender, RoutedEventArgs e)
        {
            if (RBzoomIn.IsChecked.Value)
            {
                Zoom(4);
            }
            else
            {
                Zoom(1.0 / 4.0);
            }
        }

        private void BtnZoom6_Click(object sender, RoutedEventArgs e)
        {
            if (RBzoomIn.IsChecked.Value)
            {
                Zoom(6);
            }
            else
            {
                Zoom(1.0 / 6.0);
            }
        }

        #endregion

        #region "Fractal Calculation"

        private void BtnCalc_Click(object sender, RoutedEventArgs e)
        {
            //Start/stop the application
            int Y = 0;
            Started = !Started;
            if (Started)
            {
                BtnCalc.Content = "STOP";
                MnuCalculate.Header = "STOP";
                if (FracType == 1)
                {
                    double.TryParse(TxtConstRe.Text, out ConstRe);
                    double.TryParse(TxtConstIm.Text, out ConstIm);
                }
                int.TryParse(TxtBailout.Text, out Zmax);
                int.TryParse(TxtMaxIter.Text, out Nmax);
                //Set the Canvas size to the actual Fractal size
                ResizeWindow(VeldWidth, VeldHeight);
                //Reset the arrays
                ResetFractalData(VeldWidth, VeldHeight);
                CalcRatio = (double)VeldWidth / VeldHeight;
            }
            else
            {
                BtnCalc.Content = "CALCULATE";
                MnuCalculate.Header = "CALCULATE";
            }
            //Calculate the Fractal Line per Line
            Y = 0;
            while (Started)
            {
                Dispatcher.Invoke(new RenderDelegate(RenderLine), DispatcherPriority.SystemIdle, Y);
                Y += 1;
                if (Y >= VeldHeight)
                {
                    BtnCalc.Content = "CALCULATE";
                    MnuCalculate.Header = "CALCULATE";
                    Started = false;
                }
            }
            Rband.IsEnabled = true;
        }

        private void RenderLine(int J)
        {
            //Fill the buffer with the pixels from line J
            int ColIndex;
            double X0;
            double Y0 = Y1 + J * (Y2 - Y1) / VeldHeight;  //Calculate Y value of line J
            for (int I = 0; I < VeldWidth; I++)
            {
                X0 = X1 + I * (X2 - X1) / VeldWidth;      //Calculate X value of the pixel
                                                          //Use the selected fractal type
                if (FracType == 0)
                {
                    Iters[I, J] = Mandelbrot(X0, Y0);
                }
                else if (FracType == 1)
                {
                    Iters[I, J] = Julia(X0, Y0);
                }
                if (Iters[I, J] < 0)
                {
                    SetPixel(I, J, Color.FromRgb(0, 0, 0), pixelData, Stride);
                }
                else
                {
                    ColIndex = (int)(Colormultiplier * Iters[I, J] + ColorStartIndex) % MaxColIndex;
                    SetPixel(I, J, Colors[ColIndex], pixelData, Stride);
                }
            }
            //Update the progressbar
            PBStatus.Value = 100 * (J + 1) / VeldHeight;
            //Show the partially calculated fractal
            BitmapSource bmp = BitmapSource.Create(VeldWidth, VeldHeight, 96, 96, pf, null, pixelData, Stride);
            imgbrush.ImageSource = bmp;
        }

        private void SetPixel(int x, int y, Color c, byte[] buffer, int PixStride)
        {
            int xIndex = x * 3;
            int yIndex = y * PixStride;
            buffer[xIndex + yIndex] = c.R;
            buffer[xIndex + yIndex + 1] = c.G;
            buffer[xIndex + yIndex + 2] = c.B;
        }

        private double Mandelbrot(double X0, double Y0)
        {
            double X;     //X coordinate during iterations
            double Y;     //Y coordinate during iterations
            int N;
            double modul;
            double Xtemp;
            MaxColIndex = Colors.Count - 1;
            N = 0;
            X = 0;
            Y = 0;
            while (X * X + Y * Y < Zmax && N < Nmax)
            {
                Xtemp = X * X - Y * Y + X0;
                Y = 2 * X * Y + Y0;
                X = Xtemp;
                N += 1;
            }
            if (N >= Nmax)
            {
                return -1;
            }
            else
            {
                //Do 2 more iterations for the color smoothing to work OK
                Xtemp = X * X - Y * Y + X0;
                Y = 2 * X * Y + Y0;
                X = Xtemp;
                N += 1;
                Xtemp = X * X - Y * Y + X0;
                Y = 2 * X * Y + Y0;
                X = Xtemp;
                N += 1;
                //return the Color Index
                if (CBSmooth.IsChecked.Value)
                {
                    modul = Math.Sqrt(X * X + Y * Y);
                    return N - Math.Log(Math.Log(modul)) / Math.Log(2);
                }
                else
                {
                    return N;
                }
            }
        }

        private double Julia(double X0, double Y0)
        {
            double X;     //X coordinate during iterations ;
            double Y;     //Y coordinate during iterations ;
            int N;
            double modul;
            double Xtemp;
            MaxColIndex = Colors.Count - 1;
            N = 0;
            X = X0;
            Y = Y0;
            while (X * X + Y * Y < Zmax && N < Nmax)
            {
                Xtemp = X * X - Y * Y + ConstRe;
                Y = 2 * X * Y + ConstIm;
                X = Xtemp;
                N += 1;
            }
            if (N >= Nmax)
            {
                return -1;
            }
            else
            {
                //Do 2 more iterations for the color smoothing to work OK
                Xtemp = X * X - Y * Y + ConstRe;
                Y = 2 * X * Y + ConstIm;
                X = Xtemp;
                N += 1;
                Xtemp = X * X - Y * Y + ConstRe;
                Y = 2 * X * Y + ConstIm;
                X = Xtemp;
                N += 1;
                //return the Color Index
                if (CBSmooth.IsChecked.Value)
                {
                    modul = Math.Sqrt(X * X + Y * Y);
                    return N - Math.Log(Math.Log(modul)) / Math.Log(2);
                }
                else
                {
                    return N;
                }
            }
        }

        #endregion

        #region "Utilities"

        private void OpenPalette(string pal)
        {
            StreamReader myStream = null;
            string myLine;
            string[] txtparts;
            byte r;
            byte g;
            byte b;
            try
            {
                myStream = new StreamReader(pal);
                if (myStream != null)
                {
                    Colors.Clear();
                    MaxColIndex = int.Parse(myStream.ReadLine());
                    // Lees de palette kleuren
                    for (int I = 0; I < MaxColIndex; I++)
                    {
                        myLine = myStream.ReadLine();
                        txtparts = myLine.Split(';');
                        r = byte.Parse(txtparts[0]);
                        g = byte.Parse(txtparts[1]);
                        b = byte.Parse(txtparts[2]);
                        Colors.Add(Color.FromRgb(r, g, b));
                    }
                    if (ColorStartIndex <= MaxColIndex)
                    {
                        SldColorScroll.Value = ColorStartIndex;
                    }
                    else
                    {
                        SldColorScroll.Value = SldColorScroll.Minimum;
                    }
                    SldColorScroll.Maximum = MaxColIndex;
                    SldColorScroll.TickFrequency = (int)(MaxColIndex / 10);
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Cannot read file. Original error: " + Ex.Message, "Fractally error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Check this again, since we need to make sure we didn't throw an exception on open.
            if (myStream != null)
            {
                myStream.Close();
            }

        }

        private void ResizeWindow(int w, int h)
        {
            //Resize the entire window when the Fractal-Image size needs to change
            //but do not process the Window.SizeChange Event!
            CanResize = false;
            Width = w + 168;
            Height = h + 94;
            Canvas1.Width = w;
            Canvas1.Height = h;
            CanResize = true;
        }

        private void Zoom(double mag)
        {
            double X1N;
            double X2N;
            double Y1N;
            double Y2N;
            X1N = (X2 + X1) / 2 - (X2 - X1) / (2 * mag);
            X2N = (X2 + X1) / 2 + (X2 - X1) / (2 * mag);
            Y1N = (Y2 + Y1) / 2 - (Y2 - Y1) / (2 * mag);
            Y2N = (Y2 + Y1) / 2 + (Y2 - Y1) / (2 * mag);
            if (3.5 / (X2N - X1N) > 10000000000000.0)
            {
                if (MessageBox.Show("The requested zoom exceeds the calculation accuracy./nDo you wish to proceed?", "Fractally Information", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No) return;
            }
            X1 = X1N;
            X2 = X2N;
            Y1 = Y1N;
            Y2 = Y2N;
            BtnCalc_Click(this, new RoutedEventArgs());
        }

        private void KeepRatio()
        {
            double NewRatio = (double)VeldWidth / VeldHeight;
            double MidX = (X1 + X2) / 2;
            double MidY = (Y1 + Y2) / 2;
            //Scale X or Y-axis to keep aspect ratio
            if (NewRatio > CalcRatio)
            {
                //Adjust Fractal Height
                Y1 = MidY - (X2 - X1) / (2 * NewRatio);
                Y2 = MidY + (X2 - X1) / (2 * NewRatio);
            }
            else
            {
                //Adjust Fractal Width
                X1 = MidX - NewRatio * (Y2 - Y1) / 2;
                X2 = MidX + NewRatio * (Y2 - Y1) / 2;
            }
        }

        private void ResetFractalData(int w, int h)
        {
            if (w > 0 && h > 0)
            {
                Stride = (int)((w * pf.BitsPerPixel + 7) / 8.0);
                //Resize de arrays
                pixelData = new byte[Stride * h];
                Iters = new double[w, h];
            }
        }

        private void UpdateColors()
        {
            //Update the fractal colors
            MaxColIndex = Colors.Count();
            SldColorScroll.Maximum = MaxColIndex;
            SldColorScroll.TickFrequency = (int)(MaxColIndex / 10.0);
            int ColIndex;
            Cursor = Cursors.Wait;
            for (int I = 0; I < VeldWidth; I++)
            {
                for (int J = 0; J < VeldHeight; J++)
                {
                    if (Iters[I, J] < 0)
                    {
                        SetPixel(I, J, Color.FromRgb(0, 0, 0), pixelData, Stride);
                    }
                    else
                    {
                        ColIndex = (int)(Colormultiplier * Iters[I, J] + ColorStartIndex) % MaxColIndex;
                        SetPixel(I, J, Colors[ColIndex], pixelData, Stride);
                    }
                }
            }
            //Show the bitmap
            BitmapSource bmp = BitmapSource.Create(VeldWidth, VeldHeight, 96, 96, pf, null, pixelData, Stride);
            imgbrush.ImageSource = bmp;
            Cursor = Cursors.Arrow;
        }

        private void CalculateToFile(int my_W, int my_H)
        {
            int ColIndex;
            double Iter = 0.0;
            double X0;
            double Y0;
            double X1N;
            double X2N;
            double Y1N;
            double Y2N;
            int my_stride = (my_W * pf.BitsPerPixel + 7) / 8;
            byte[] pix = new byte[my_stride * my_H];
            SaveFileDialog SFD = new SaveFileDialog();
            FileStream myStream = null;
            SFD.InitialDirectory = Environment.CurrentDirectory;
            SFD.Filter = "Bitmap (*.bmp)|*.bmp|JPEG format (*.jpg)|*.jpg";
            SFD.FilterIndex = 1;
            SFD.RestoreDirectory = true;
            if (SFD.ShowDialog() == true)
            {
                try
                {
                    myStream = new FileStream(SFD.FileName, FileMode.Create);
                    if (myStream != null)
                    {
                        Cursor = Cursors.Wait;
                        MaxColIndex = Colors.Count - 1;
                        //Scale X or Y-axis to keep aspect ratio
                        if ((double)my_W / my_H > (double)VeldWidth / VeldHeight)
                        {
                            //Adjust Fractal Width
                            Y1N = Y1;
                            Y2N = Y2;
                            X1N = (X1 + X2) / 2 - ((double)my_W / my_H) / ((double)VeldWidth / VeldHeight) * (X2 - X1) / 2;
                            X2N = (X1 + X2) / 2 + ((double)my_W / my_H) / ((double)VeldWidth / VeldHeight) * (X2 - X1) / 2;
                        }
                        else
                        {
                            //Adjust Fractal Height
                            X1N = X1;
                            X2N = X2;
                            Y1N = (Y1 + Y2) / 2 - ((double)my_H / my_W) / ((double)VeldHeight / VeldWidth) * (Y2 - Y1) / 2;
                            Y2N = (Y1 + Y2) / 2 + ((double)my_H / my_W) / ((double)VeldHeight / VeldWidth) * (Y2 - Y1) / 2;
                        }
                        //Calculate number of iterations for each pixel.
                        for (int I = 0; I < my_W; I++)
                        {
                            X0 = X1N + I * (X2N - X1N) / my_W;     //Calculate X value of the pixel
                            for (int J = 0; J < my_H; J++)
                            {
                                Y0 = Y1N + J * (Y2N - Y1N) / my_H; //Calculate Y value of the pixel
                                                                   //Use the selected fractal type
                                if (FracType == 0)
                                {
                                    Iter = Mandelbrot(X0, Y0);
                                }
                                else if (FracType == 1)
                                {
                                    Iter = Julia(X0, Y0);
                                }
                                if (Iter < 0)
                                {
                                    SetPixel(I, J, Color.FromRgb(0, 0, 0), pix, my_stride);
                                }
                                else
                                {
                                    ColIndex = (int)(Colormultiplier * Iter + ColorStartIndex) % MaxColIndex;
                                    SetPixel(I, J, Colors[ColIndex], pix, my_stride);
                                }
                            }
                            Dispatcher.Invoke(new StatusDelegate(UpdateStatus), DispatcherPriority.SystemIdle, 100 * (I + 1) / my_W);
                        }
                        BitmapSource bmp = BitmapSource.Create(my_W, my_H, 96, 96, pf, null, pix, my_stride);
                        //Save the bmp to the file
                        if (SFD.FilterIndex == 1)
                        {
                            BmpBitmapEncoder enc = new BmpBitmapEncoder();
                            enc.Frames.Add(BitmapFrame.Create(bmp));
                            enc.Save(myStream);
                        }
                        else if (SFD.FilterIndex == 2)
                        {
                            JpegBitmapEncoder enc = new JpegBitmapEncoder();
                            enc.QualityLevel = 100;
                            enc.Frames.Add(BitmapFrame.Create(bmp));
                            enc.Save(myStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    PBStatus.Value = 0;
                    MessageBox.Show("Cannot save the Image data. Original error: " + ex.Message, "Fractally error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // Check this again, since we need to make sure we didn't throw an exception on open.
                if (myStream != null)
                {
                    myStream.Close();
                }
                Cursor = Cursors.Arrow;
            }
        }

        private void UpdateStatus(double v)
        {
            PBStatus.Value = v;
        }

        #endregion

    }
}
