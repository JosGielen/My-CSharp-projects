using JG_Graphs;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace ImageTools
{
    public partial class PictureControl
    {
        private MainWindow MyParent;
        private bool MyLoaded = false;
        private BitmapImage myImage;
        //private int myMag;
        private WriteableBitmap CurrentBitmap;
        private int bytesPerPixel = 0;
        private Size MaxCurrentSize;
        private byte[] CurrentPixelData;  //Back-up of the current image pixels, set at start of ImageControl do not change during ImageControl
        private byte[] TempPixelData;     //Used as temporary bytes during calculations
        private byte[] CalcPixelData;     //Used to show the image. Calculated from CurrentPixelData
        private int CurrentStride = 0;
        private Int32Rect CurrentIntrect;
        private WriteableBitmap PreviewBitmap;
        private Size MaxPreviewSize;
        private byte[] PreviewPixelData;
        private byte[] PreviewTempPixelData;
        private int PreviewStride = 0;
        private Int32Rect PreviewIntrect;
        private string FileName = "" ;
        private string FileSavePath = "" ;
        private int FileSaveIndex = 1; //TODO: Use a setting to determine the format ;
        private Line LineContrast = new Line();
        private Line LineBrightness = new Line();
        private bool MyMouseDown = false;
        private ControlDrawMode MyDrawMode = ControlDrawMode.None;
        private bool IsDrawing = false;
        private bool DrawingHorizontalLine = false;
        private bool DrawingVerticalLine = false;
        private double myAngle = 0.0;
        private int Brightness = 0;
        private int Contrast = 0;
        private double Gamma = 1.0;
        private double Saturation = 1.0;
        private bool Inverse = false;
        private readonly int[] Iorig = new int[256];
        private byte[] Icorr = new byte[256];

        public PictureControl(MainWindow Parent)
        {
            InitializeComponent();
            MyParent = Parent;
            for (int I = 0; I <= 255; I++)
            {
                Iorig[I] = I;
                Icorr[I] = (byte)I;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScatterSeries ser = new ScatterSeries(0);
            //Set the control lines
            LineContrast.X1 = ControlCanvas.ActualWidth / 2;
            LineContrast.X2 = ControlCanvas.ActualWidth / 2;
            LineContrast.Y1 = 0;
            LineContrast.Y2 = ControlCanvas.ActualHeight;
            LineContrast.Stroke = Brushes.Black;
            LineContrast.StrokeThickness = 2;
            LineBrightness.X1 = 0;
            LineBrightness.X2 = ControlCanvas.ActualWidth;
            LineBrightness.Y1 = ControlCanvas.ActualHeight / 2;
            LineBrightness.Y2 = ControlCanvas.ActualHeight / 2;
            LineBrightness.Stroke = Brushes.Black;
            LineBrightness.StrokeThickness = 2;
            ControlCanvas.Children.Add(LineContrast);
            ControlCanvas.Children.Add(LineBrightness);
            //Initialize the Intensity Graph
            for (int I = 0; I <= 255; I++)
            {
                ser.AddDataPoint(new Point(Iorig[I], Icorr[I]));
            }
            ser.ShowLine = true;
            ser.ShowMarker = false;
            ser.LineColor = Brushes.Black;
            ResultGraph.DataSeries.Add(ser);
            ResultGraph.XAxis.Minimum = 0;
            ResultGraph.XAxis.Maximum = 255;
            ResultGraph.XAxis.TickInterval = 50;
            ResultGraph.XAxis.AxisLabel = "Original";
            ResultGraph.YAxis.Minimum = 0;
            ResultGraph.YAxis.Maximum = 255;
            ResultGraph.YAxis.TickInterval = 50;
            ResultGraph.YAxis.AxisLabel = "Modified";
            ResultGraph.LegendPosition = LegendPosition.None;
            ResultGraph.VerticalAlignment = VerticalAlignment.Stretch;
            ResultGraph.Draw();
            WindowState = WindowState.Maximized;
            //Calculate the maximum size of the current image and preview image
            Measure(new Size(SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height));
            UpdateLayout();
            MaxCurrentSize = new Size(CurrentCanvas.ActualWidth, CurrentCanvas.ActualHeight);
            MaxPreviewSize = new Size(PreviewCanvas.ActualWidth, PreviewCanvas.ActualHeight);
            MyLoaded = true;
        }

        public void SetImage(ImageBorder img)
        {
            SetImage(img.Image, img.Filename);
        }

        public void SetImage(BitmapImage image, string file)
        {
            if (image != null)
            {
                myImage = image;
                FileName = file;
                CurrentBitmap = new WriteableBitmap(myImage);
                bytesPerPixel = CurrentBitmap.Format.BitsPerPixel / 8;
                //Show the image
                CurrentCanvas.Width = CurrentBorder.ActualWidth;
                CurrentCanvas.Height = CurrentBorder.ActualHeight;
                MaxCurrentSize = new Size(CurrentCanvas.Width, CurrentCanvas.Height);
                SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
                CurrentIntrect = new Int32Rect(0, 0, CurrentBitmap.PixelWidth, CurrentBitmap.PixelHeight);
                CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
                //Get the pixel data from the image
                CurrentStride = (int)(CurrentBitmap.PixelWidth * bytesPerPixel);
                CurrentPixelData = new byte[CurrentStride * CurrentBitmap.PixelHeight];
                CalcPixelData = new byte[CurrentStride * CurrentBitmap.PixelHeight];
                TempPixelData = new byte[CurrentStride * CurrentBitmap.PixelHeight];
                CurrentBitmap.CopyPixels(CurrentPixelData, CurrentStride, 0);
                CurrentPixelData.CopyTo(CalcPixelData, 0);
                CurrentPixelData.CopyTo(TempPixelData, 0);
                SetCanvasSize(MaxPreviewSize, PreviewCanvas, CurrentBitmap);
                TControl1.SelectedIndex = 0;
                ResetDrawMode();
                SetPreviewImage(CurrentBitmap);
            }
            //myMag = 1;
            Title = "Image Editor: " + file;
        }

        private void SetCanvasSize(Size MaxSize, Canvas canvas, WriteableBitmap bitmap)
        {
            double imgScale = 0.0;
            if (bitmap.PixelWidth > bitmap.PixelHeight)
            {
                imgScale = MaxSize.Width / bitmap.PixelWidth;
                if (imgScale * bitmap.PixelHeight > MaxSize.Height)
                {
                    imgScale = MaxSize.Height / bitmap.PixelHeight;
                }
                canvas.Width = imgScale * bitmap.PixelWidth;
                canvas.Height = imgScale * bitmap.PixelHeight;
            }
            else
            {
                imgScale = MaxSize.Height / bitmap.PixelHeight;
                if (imgScale * bitmap.PixelWidth > MaxSize.Width)
                {
                    imgScale = MaxSize.Width / bitmap.PixelWidth;
                }
                canvas.Height = imgScale * bitmap.PixelHeight;
                canvas.Width = imgScale * bitmap.PixelWidth;
            }
        }

        private void SetPreviewImage(WriteableBitmap bmp)
        {
            int oldW = 0;
            int oldH = 0;
            int oldIndex = 0;
            int newW = 0;
            int newH = 0;
            int index = 0;
            oldW = bmp.PixelWidth;
            oldH = bmp.PixelHeight;
            //Set the size of the preview Image
            newW = (int)(PreviewCanvas.Width);
            newH = (int)(PreviewCanvas.Height);
            PreviewIntrect = new Int32Rect(0, 0, newW, newH);
            //Get the pixel data for the preview Image
            PreviewStride = (int)(newW * bytesPerPixel);
            PreviewPixelData = new byte[PreviewStride * newH];
            PreviewTempPixelData = new byte[PreviewStride * newH];
            //Copy the bitmap pixels to a reduced image
            double StepX = oldW / newW;
            double StepY = oldH / newH;
            try
            {
                for (int J = 0; J <= newH; J++)
                {
                    for (int I = 0; I <= newW; I++)
                    {
                        if (bytesPerPixel == 1)
                        {
                            index = J * newW + I;
                            oldIndex = (int)(StepY * J) * oldW + (int)(StepX * I);
                            if (index < PreviewPixelData.Count() && oldIndex < CurrentPixelData.Count())
                            {
                                PreviewPixelData[index] = CurrentPixelData[oldIndex];
                            }
                        }
                        else if (bytesPerPixel == 2)
                        {
                            index = 2 * (J * newW + I);
                            oldIndex = 3 * ((int)(StepY * J) * oldW + (int)(StepX * I));
                            if (index + 2 < PreviewPixelData.Count() && oldIndex + 2 < CurrentPixelData.Count())
                            {
                                PreviewPixelData[index] = CurrentPixelData[oldIndex];
                                PreviewPixelData[index + 1] = CurrentPixelData[oldIndex + 1];
                            }
                        }
                        else if (bytesPerPixel == 3)
                        {
                            index = 3 * (J * newW + I);
                            oldIndex = 3 * ((int)(StepY * J) * oldW + (int)(StepX * I));
                            if (index + 2 < PreviewPixelData.Count() && oldIndex + 2 < CurrentPixelData.Count())
                            {
                                PreviewPixelData[index] = CurrentPixelData[oldIndex];
                                PreviewPixelData[index + 1] = CurrentPixelData[oldIndex + 1];
                                PreviewPixelData[index + 2] = CurrentPixelData[oldIndex + 2];
                            }
                        }
                        else if (bytesPerPixel == 4)
                        {
                            index = 4 * (J * newW + I);
                            oldIndex = 4 * ((int)(StepY * J) * oldW + (int)(StepX * I));
                            if (index + 3 < PreviewPixelData.Count() && oldIndex + 3 < CurrentPixelData.Count())
                            {
                                PreviewPixelData[index] = CurrentPixelData[oldIndex];
                                PreviewPixelData[index + 1] = CurrentPixelData[oldIndex + 1];
                                PreviewPixelData[index + 2] = CurrentPixelData[oldIndex + 2];
                                PreviewPixelData[index + 3] = CurrentPixelData[oldIndex + 3];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Error : " + ex.ToString());
            }
            PreviewBitmap = new WriteableBitmap(BitmapSource.Create(newW, newH, 96, 96, CurrentBitmap.Format, CurrentBitmap.Palette, PreviewPixelData, PreviewStride));
            PreviewCanvas.Background = new ImageBrush(PreviewBitmap);
            PreviewPixelData.CopyTo(PreviewTempPixelData, 0);
        }

        private byte[] IntensityCorrection()
        {
            byte[] result = new byte[256];
            double[] Y = new double[256];
            double A = 0.0;
            double B = 0.0;
            double Min = double.MaxValue;
            double Max = double.MinValue;
            if (Brightness < -255) { Brightness = -255; }
            if (Brightness > 255) { Brightness = 255; }
            if (Contrast < -127) { Contrast = -127; }
            if (Contrast > 127) { Contrast = 127; }
            if (Gamma < 0.1) { Gamma = 0.1; }
            if (Gamma > 10) { Gamma = 10; }
            if (Contrast <= 0)
            {
                A = 1 + Contrast / 128.0;
                B = -1 * Contrast;
            }
            else
            {
                A = 128.0 / (128 - Contrast);
                B = 127.0 * Contrast / (Contrast - 128);
            }
            //Step1: Correct for Contrast and brightness
            for (int I = 0; I <= 255; I++)
            {
                Y[I] = A * Iorig[I] + B;
                if (Y[I] < 0) { Y[I] = 0; }
                if (Y[I] > 255) { Y[I] = 255; }
                Y[I] += Brightness;
                if (Y[I] < 0) { Y[I] = 0; }
                if (Y[I] > 255) { Y[I] = 255; }
                if (Y[I] > Max) { Max = Y[I]; }
                if (Y[I] < Min)
                {
                    Min = Y[I];
                }
            }
            if (Max == Min)
            {
                if (Min > -255)
                {
                    Min -= 1;
                }
                else
                {
                    Max += 1;
                }
            }
            //Step2: Correct for Gamma
            for (int I = 0; I <= 255; I++)
            {
                Y[I] = (Max - Min) * Math.Pow((Y[I] - Min) / (Max - Min), Gamma) + Min;
                if (Y[I] < 0) { Y[I] = 0; }
                if (Y[I] > 255) { Y[I] = 255; }
            }
            for (int I = 0; I <= 255; I++)
            {
                if (Inverse)
                {
                    result[I] = (byte)(255 - (int)Y[I]);
                }
                else
                {
                    result[I] = (byte)Y[I];
                }
            }
            return result;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Allow showing this window again
            Hide();
            e.Cancel = true;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyLoaded)
            {
                if ((string)((TabItem)TControl1.SelectedItem).Header == "Image control")
                {
                    ResetSettings();
                    CalcPixelData.CopyTo(CurrentPixelData, 0);
                    SetCanvasSize(MaxPreviewSize, PreviewCanvas, CurrentBitmap);
                    SetPreviewImage(CurrentBitmap);
                }
            }
        }

        #region "Menu"

        private void MnuOpen_Click(object sender, RoutedEventArgs e)
        {
            ImageBorder bord;
            OpenFileDialog OFD = new OpenFileDialog();
            //Show an OpenFile dialog
            OFD.InitialDirectory = MyParent.Settings.StartFolder;
            OFD.Filter = "Windows Bitmap (*.bmp,*.dib)|*.bmp;*.dib|JPEG (*.jpg,*.jpeg,*.jfif,*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe|TIFF (*.tif,tiff)|*.tif;*.tiff|PNG (*.png)|*.png|GIF (*.gif)| *.gif|All Image files (*.*)|*.*";
            OFD.FilterIndex = 6;
            OFD.RestoreDirectory = true;
            if (OFD.ShowDialog() == true)
            {
                MyParent.Settings.ImageFileName = OFD.FileName;
                MyParent.Settings.StartFolder = Path.GetDirectoryName(OFD.FileName);
                bord = new ImageBorder(OFD.FileName, MyParent);
                SetImage(bord);
                ResetSettings();
            }
        }

        private void MnuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(MyParent.Settings.ImageFileName);
        }

        private void MnuSaveAs_Click(Object sender, RoutedEventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.InitialDirectory = MyParent.Settings.StartFolder;
            SFD.FileName = Path.GetFileNameWithoutExtension(MyParent.Settings.ImageFileName);
            SFD.Filter = "Windows Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|TIFF (*.tiff)|*.tiff|PNG (*.png)|*.png";
            SFD.FilterIndex = MyParent.Settings.ImageFormatIndex;
            SFD.RestoreDirectory = true;
            if (SFD.ShowDialog() == true)
            {
                MyParent.Settings.ImageFormatIndex = SFD.FilterIndex;
                SaveImage(SFD.FileName);
                MyParent.Settings.ImageFileName = SFD.FileName;
                MyParent.Settings.StartFolder = Path.GetDirectoryName(SFD.FileName);
            }
        }

        private void MnuPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new PrintDialog();
            StackPanel PrintPanel = new StackPanel();
            if (printDlg.ShowDialog() == true)
            {
                Canvas printCanvas = new Canvas();
                double scale = 0.0;
                double WidthScale = (printDlg.PrintableAreaWidth - 50) / CurrentCanvas.ActualWidth;
                double HeightScale = (printDlg.PrintableAreaHeight - 50) / CurrentCanvas.ActualHeight;
                if (WidthScale < HeightScale)
                {
                    scale = WidthScale;
                }
                else
                {
                    scale = HeightScale;
                }
                Size pageSize = new Size(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight);
                printCanvas.Width = scale * CurrentCanvas.ActualWidth;
                printCanvas.Height = scale * CurrentCanvas.ActualHeight;
                printCanvas.Background = new VisualBrush(CurrentCanvas);
                //Place the item in the PrintPanel
                PrintPanel.Children.Add(printCanvas);
                //Measure and arrange the PrintPanel to fit the page
                PrintPanel.Margin = new Thickness(0);
                PrintPanel.Measure(pageSize);
                PrintPanel.Arrange(new Rect(15, 25, pageSize.Width - 15, pageSize.Height - 25));
                //Print the PrintPanel
                printDlg.PrintVisual(PrintPanel, "ImgViewer Print Image");
            }
        }

        private void MnuClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void MnuCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(CurrentBitmap);
        }

        private void MnuPaste_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource img = Clipboard.GetImage();
            if (img != null)
            {
                bytesPerPixel = img.Format.BitsPerPixel / 8;
                CurrentBitmap = new WriteableBitmap(img.PixelWidth, img.PixelHeight, img.DpiX, img.DpiY, img.Format, img.Palette);
                CurrentStride = (int)(CurrentBitmap.PixelWidth * bytesPerPixel);
                CurrentIntrect = new Int32Rect(0, 0, CurrentBitmap.PixelWidth - 1, CurrentBitmap.PixelHeight - 1);
                CurrentPixelData = new byte[CurrentStride * CurrentBitmap.PixelHeight];
                CalcPixelData = new byte[CurrentStride * CurrentBitmap.PixelHeight];
                TempPixelData = new byte[CurrentStride * CurrentBitmap.PixelHeight];
                img.CopyPixels(CurrentPixelData, CurrentStride, 0);
                //Set the opacity to 255
                if (bytesPerPixel == 4)
                {
                    for (int I = 3; I < CurrentPixelData.Count(); I += 4)
                    {
                        CurrentPixelData[I] = 255;
                    }
                }
                CurrentPixelData.CopyTo(CalcPixelData, 0);
                CurrentPixelData.CopyTo(TempPixelData, 0);
                CurrentBitmap.WritePixels(CurrentIntrect, CurrentPixelData, CurrentStride, 0);
                SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
                CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
                SetCanvasSize(MaxPreviewSize, PreviewCanvas, CurrentBitmap);
                SetPreviewImage(CurrentBitmap);
                TControl1.SelectedIndex = 0;
                MyParent.Settings.ImageFileName = "Untitled";
                //myMag = 1;
                myImage = ToBitmapImage(CurrentBitmap);
                ResetSettings();
                Title = "Image Editor: Untitled";
            }
        }

        private void MnuRestore_Click(object sender, RoutedEventArgs e)
        {
            //Set default values
            Brightness = 0;
            Contrast = 0;
            Gamma = 1;
            Saturation = 1;
            SldGamma.Value = 0;
            SldSaturation.Value = 0;
            Inverse = false;
            //Set the new settings
            SetSettings();
            SetImage(myImage, MyParent.Settings.ImageFileName);
        }

        private void MnuMainWindow_Click(object sender, RoutedEventArgs e)
        {
            MyParent.WindowState = WindowState.Normal;
            MyParent.Show();
            MyParent.Focus();
        }

        private void MnuMeasure_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage bmpimage = new BitmapImage();
            MyParent.ShowViewer(ToBitmapImage(CurrentBitmap), MyParent.Settings.ImageFileName);
        }

        #endregion

        #region "Image Control"

        public void CorrectPreviewImage()
        {
            double Pr = 0.299;
            double Pg = 0.587;
            double Pb = 0.114;
            double R = 0;
            double G = 0;
            double B = 0;
            double P = 0.0;
            double Rcorr = 0.0;
            double Gcorr = 0.0;
            double Bcorr = 0.0;
            int teller = 0;
            do
            {
                B = Icorr[PreviewPixelData[teller]];
                G = Icorr[PreviewPixelData[teller + 1]];
                R = Icorr[PreviewPixelData[teller + 2]];
                P = Math.Sqrt(R * R * Pr + G * G * Pg + B * B * Pb);
                Rcorr = P + (R - P) * Saturation;
                Gcorr = P + (G - P) * Saturation;
                Bcorr = P + (B - P) * Saturation;
                if (Rcorr < 0) { Rcorr = 0; }
                if (Rcorr > 255) { Rcorr = 255; }
                if (Gcorr < 0) { Gcorr = 0; }
                if (Gcorr > 255) { Gcorr = 255; }
                if (Bcorr < 0) { Bcorr = 0; }
                if (Bcorr > 255) { Bcorr = 255; }
                PreviewTempPixelData[teller] = (byte)Bcorr;
                PreviewTempPixelData[teller + 1] = (byte)Gcorr;
                PreviewTempPixelData[teller + 2] = (byte)Rcorr;
                if (PreviewBitmap.Format.BitsPerPixel <= 24)
                {
                    teller += 3;
                }
                else if (PreviewBitmap.Format.BitsPerPixel == 32)
                {
                    teller += 4;
                }
            } while (teller < PreviewPixelData.Count() - 3);
            PreviewBitmap.WritePixels(PreviewIntrect, PreviewTempPixelData, PreviewStride, 0);
            PreviewCanvas.Background = new ImageBrush(PreviewBitmap);
        }

        public void CorrectOriginalImage()
        {
            double Pr = 0.299;
            double Pg = 0.587;
            double Pb = 0.114;
            double R = 0;
            double G = 0;
            double B = 0;
            double P = 0.0;
            double Rcorr = 0.0;
            double Gcorr = 0.0;
            double Bcorr = 0.0;
            int teller = 0;
            do
            {
                B = Icorr[CurrentPixelData[teller]];
                G = Icorr[CurrentPixelData[teller + 1]];
                R = Icorr[CurrentPixelData[teller + 2]];
                P = Math.Sqrt(R * R * Pr + G * G * Pg + B * B * Pb);
                Rcorr = P + (R - P) * Saturation;
                Gcorr = P + (G - P) * Saturation;
                Bcorr = P + (B - P) * Saturation;
                if (Rcorr < 0) { Rcorr = 0; }
                if (Rcorr > 255) { Rcorr = 255; }
                if (Gcorr < 0) { Gcorr = 0; }
                if (Gcorr > 255) { Gcorr = 255; }
                if (Bcorr < 0) { Bcorr = 0; }
                if (Bcorr > 255) { Bcorr = 255; }
                TempPixelData[teller] = (byte)Bcorr;
                TempPixelData[teller + 1] = (byte)Gcorr;
                TempPixelData[teller + 2] = (byte)Rcorr;
                if (PreviewBitmap.Format.BitsPerPixel <= 24)
                {
                    teller += 3;
                }
                else if (PreviewBitmap.Format.BitsPerPixel == 32)
                {
                    teller += 4;
                }
            } while (teller < CurrentPixelData.Count() - 3);
            TempPixelData.CopyTo(CalcPixelData, 0);
            CurrentBitmap.WritePixels(CurrentIntrect, CalcPixelData, CurrentStride, 0);
            CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
        }

        private void SldContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyLoaded)
            {
                Contrast = (int)SldContrast.Value;
                SetSettings();
                CorrectPreviewImage();
            }
        }

        private void SldBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyLoaded)
            {
                Brightness = (int)SldBrightness.Value;
                SetSettings();
                CorrectPreviewImage();
            }
        }

        private void SldGamma_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyLoaded)
            {
                if (SldGamma.Value >= 0)
                {
                    Gamma = Math.Round(0.9 * SldGamma.Value + 1, 2);
                }
                else
                {
                    Gamma = Math.Round(0.09 * (SldGamma.Value + 10) + 0.1, 2);
                }
                TxtGamma.Text = Gamma.ToString();
                SetSettings();
                CorrectPreviewImage();
            }
        }

        private void SldSaturation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyLoaded)
            {
                if (SldSaturation.Value >= 0)
                {
                    Saturation = 3 * SldSaturation.Value / 255 + 1;
                }
                else
                {
                    Saturation = (SldSaturation.Value + 255) / 255;
                }
                SetSettings();
                CorrectPreviewImage();
            }
        }

        private void ControlCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double X = e.GetPosition(ControlCanvas).X;
            double Y = e.GetPosition(ControlCanvas).Y;
            Brightness = (int)(510 * (1 - Y / ControlCanvas.ActualHeight)) - 255;
            Contrast = (int)(255 * X / ControlCanvas.ActualWidth) - 127;
            SetSettings();
            CorrectPreviewImage();
            MyMouseDown = true;
        }

        private void ControlCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MyMouseDown)
            {
                double X = e.GetPosition(ControlCanvas).X;
                double Y = e.GetPosition(ControlCanvas).Y;
                Brightness = (int)(510 * (1 - Y / ControlCanvas.ActualHeight)) - 255;
                Contrast = (int)(255 * X / ControlCanvas.ActualWidth) - 127;
                SetSettings();
                CorrectPreviewImage();
            }
        }

        private void ControlCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MyMouseDown = false;
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            //Set default values
            Brightness = 0;
            Contrast = 0;
            Gamma = 1;
            Saturation = 1;
            SldGamma.Value = 0;
            SldSaturation.Value = 0;
            //Set the new settings
            SetSettings();
            CorrectPreviewImage();
        }

        private void BtnInverse_Click(object sender, RoutedEventArgs e)
        {
            Inverse = !Inverse;
            SetSettings();
            CorrectPreviewImage();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            CorrectOriginalImage();
        }

        private void SetSettings()
        {
            double X = (Contrast + 127) / 255.0 * ControlCanvas.ActualWidth;
            double Y = (255 - Brightness) / 510.0 * ControlCanvas.ActualHeight;
            //Set the control lines
            LineContrast.X1 = X;
            LineContrast.X2 = X;
            LineBrightness.Y1 = Y;
            LineBrightness.Y2 = Y;
            //Set the slider positions
            SldBrightness.Value = Brightness;
            SldContrast.Value = Contrast;
            //Update the intensity Graph
            Icorr = IntensityCorrection();
            ResultGraph.DataSeries[0].DataList.Clear();
            for (int I = 0; I <= 255; I++)
            {
                ResultGraph.DataSeries[0].AddDataPoint(new Point(Iorig[I], Icorr[I]));
            }
            ResultGraph.Draw();
        }

        private void ResetSettings()
        {
            //Set default values
            Brightness = 0;
            Contrast = 0;
            Gamma = 1;
            Saturation = 1;
            SldGamma.Value = 0;
            SldSaturation.Value = 0;
            //Set the new settings
            Inverse = false;
            SetSettings();
            ResetDrawMode();
        }

        #endregion

        #region "Image Manipulation"

        private void BtnRotLeft_Click(object sender, RoutedEventArgs e)
        {
            //Rotate the image 90 degrees counter clockwise
            int oldH = CurrentBitmap.PixelHeight;
            int oldW = CurrentBitmap.PixelWidth;
            int oldIndex = 0;
            int newH = 0;
            int newW = 0;
            int newIndex = 0;
            //Switch Width and Height of the bitmap
            newW = oldH;
            newH = oldW;
            //Correct the stride, rectangle and writebitmap for the new size
            CurrentStride = (int)(newW * CurrentBitmap.Format.BitsPerPixel / 8);
            CurrentIntrect = new Int32Rect(0, 0, newW - 1, newH - 1);
            CurrentBitmap = new WriteableBitmap(newW, newH, CurrentBitmap.DpiY, CurrentBitmap.DpiX, CurrentBitmap.Format, CurrentBitmap.Palette);
            if (bytesPerPixel == 1)
            {
                for (int J = 0; J < oldH; J++)
                {
                    for (int I = 0; I < oldW; I++)
                    {
                        oldIndex = J * oldW + I;
                        newIndex = (newH - I - 1) * newW + J;
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                    }
                }
            }
            else if (bytesPerPixel == 2)
            {
                for (int J = 0; J < oldH; J++)
                {
                    for (int I = 0; I < oldW; I++)
                    {
                        oldIndex = 2 * (J * oldW + I);
                        newIndex = 2 * ((newH - I - 1) * newW + J);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                    }
                }
            }
            else if (bytesPerPixel == 3)
            {
                for (int J = 0; J < oldH; J++)
                {
                    for (int I = 0; I < oldW; I++)
                    {
                        oldIndex = 3 * (J * oldW + I);
                        newIndex = 3 * ((newH - I - 1) * newW + J);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                    }
                }
            }
            else if (bytesPerPixel == 4)
            {
                for (int J = 0; J < oldH; J++)
                {
                    for (int I = 0; I < oldW; I++)
                    {
                        oldIndex = 4 * (J * oldW + I);
                        newIndex = 4 * ((newH - I - 1) * newW + J);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                        TempPixelData[newIndex + 3] = CalcPixelData[oldIndex + 3];
                    }
                }
            }
            TempPixelData.CopyTo(CalcPixelData, 0);
            CurrentBitmap.WritePixels(CurrentIntrect, CalcPixelData, CurrentStride, 0);
            SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
            CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
        }

        private void BtnRotRight_Click(object sender, RoutedEventArgs e)
        {
            //Rotate the image 90 degrees clockwise
            int oldH = CurrentBitmap.PixelHeight;
            int oldW = CurrentBitmap.PixelWidth;
            int oldIndex = 0;
            int newH = 0;
            int newW = 0;
            int newIndex = 0;
            //Switch Width and Height of the bitmap
            newW = oldH;
            newH = oldW;
            //Correct the stride, rectangle and writebitmap for the new size
            CurrentStride = (int)(newW * CurrentBitmap.Format.BitsPerPixel / 8);
            CurrentIntrect = new Int32Rect(0, 0, newW - 1, newH - 1);
            CurrentBitmap = new WriteableBitmap(newW, newH, CurrentBitmap.DpiY, CurrentBitmap.DpiX, CurrentBitmap.Format, CurrentBitmap.Palette);
            if (bytesPerPixel == 1)
            {
                for (int J = 0; J < newH; J++)
                {
                    for (int I = 0; I < newW; I++)
                    {
                        newIndex = J * newW + I;
                        oldIndex = (newW - 1 - I) * newH + J;
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                    }
                }
            }
            else if (bytesPerPixel == 2)
            {
                for (int J = 0; J < newH; J++)
                {
                    for (int I = 0; I < newW; I++)
                    {
                        newIndex = 2 * (J * newW + I);
                        oldIndex = 2 * ((newW - 1 - I) * newH + J);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                    }
                }
            }
            else if (bytesPerPixel == 3)
            {
                for (int J = 0; J < newH; J++)
                {
                    for (int I = 0; I < newW; I++)
                    {
                        newIndex = 3 * (J * newW + I);
                        oldIndex = 3 * ((newW - 1 - I) * newH + J);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                    }
                }
            }
            else if (bytesPerPixel == 4)
            {
                for (int J = 0; J < newH; J++)
                {
                    for (int I = 0; I < newW; I++)
                    {
                        newIndex = 4 * (J * newW + I);
                        oldIndex = 4 * ((newW - 1 - I) * newH + J);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                        TempPixelData[newIndex + 3] = CalcPixelData[oldIndex + 3];
                    }
                }
            }
            TempPixelData.CopyTo(CalcPixelData, 0);
            CurrentBitmap.WritePixels(CurrentIntrect, CalcPixelData, CurrentStride, 0);
            SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
            CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
        }

        private void BtnFlipHorizontal_Click(object sender, RoutedEventArgs e)
        {
            //Flip the image along a vertical center axis (switch left-right)
            int H = CurrentBitmap.PixelHeight;
            int W = CurrentBitmap.PixelWidth;
            int oldIndex = 0;
            int newIndex = 0;
            if (bytesPerPixel == 1)
            {
                for (int J = 0; J < H; J++)
                {
                    for (int I = 0; I < W; I++)
                    {

                        newIndex = J * W + I;
                        oldIndex = J * W + (W - 1 - I);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                    }
                }
            }
            else if (bytesPerPixel == 2)
            {
                for (int J = 0; J < H; J++)
                {
                    for (int I = 0; I < W; I++)
                    {
                        newIndex = 2 * (J * W + I);
                        oldIndex = 2 * (J * W + (W - 1 - I));
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                    }
                }
            }
            else if (bytesPerPixel == 3)
            {
                for (int J = 0; J < H; J++)
                {
                    for (int I = 0; I < W; I++)
                    {
                        newIndex = 3 * (J * W + I);
                        oldIndex = 3 * (J * W + (W - 1 - I));
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                    }
                }
            }
            else if (bytesPerPixel == 4)
            {
                for (int J = 0; J < H; J++)
                {
                    for (int I = 0; I < W; I++)
                    {
                        newIndex = 4 * (J * W + I);
                        oldIndex = 4 * (J * W + (W - 1 - I));
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                        TempPixelData[newIndex + 3] = CalcPixelData[oldIndex + 3];
                    }
                }
            }
            TempPixelData.CopyTo(CalcPixelData, 0);
            CurrentBitmap.WritePixels(CurrentIntrect, CalcPixelData, CurrentStride, 0);
            SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
            CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
        }

        private void BtnFlipVertical_Click(object sender, RoutedEventArgs e)
        {
            //    'Flip the image along a horizontal center axis (switch top-bottom)
            int H = CurrentBitmap.PixelHeight;
            int W = CurrentBitmap.PixelWidth;
            int oldIndex = 0;
            int newIndex = 0;
            if (bytesPerPixel == 1)
            {
                for (int J = 0; J < H; J++)
                {
                    for (int I = 0; I < W; I++)
                    {
                        newIndex = J * W + I;
                        oldIndex = (H - 1 - J) * W + I;
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                    }
                }
            }
            else if (bytesPerPixel == 2)
            {
                for (int J = 0; J < H; J++)
                {
                    for (int I = 0; I < W; I++)
                    {
                        newIndex = 2 * (J * W + I);
                        oldIndex = 2 * ((H - 1 - J) * W + I);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                    }
                }
            }
            else if (bytesPerPixel == 3)
            {
                for (int J = 0; J < H; J++)
                {
                    for (int I = 0; I < W; I++)
                    {
                        newIndex = 3 * (J * W + I);
                        oldIndex = 3 * ((H - 1 - J) * W + I);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                    }
                }
            }
            else if (bytesPerPixel == 4)
            {
                for (int J = 0; J < H; J++)
                {
                    for (int I = 0; I < W; I++)
                    {
                        newIndex = 4 * (J * W + I);
                        oldIndex = 4 * ((H - 1 - J) * W + I);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                        TempPixelData[newIndex + 3] = CalcPixelData[oldIndex + 3];
                    }
                }
            }
            TempPixelData.CopyTo(CalcPixelData, 0);
            CurrentBitmap.WritePixels(CurrentIntrect, CalcPixelData, CurrentStride, 0);
            SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
            CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
        }

        private void BtnCustomRotate_Click(object sender, RoutedEventArgs e)
        {
            //Rotate the image over a custom number of degrees (positive = Clockwise);
            try
            {
                double angle = double.Parse(TxtDegrees.Text);
                Rotate(-1 * Math.PI * angle / 180);
            }
            catch
            {
                //Do nothing
            }
        }

        private void BtnSelectHorizontal_Click(object sender, RoutedEventArgs e)
        {
            //Let the user draw a line along a horizontal edge
            ResetDrawMode();
            DrawingHorizontalLine = true;
            CurrentCanvas.Cursor = Cursors.Cross;
            MyDrawMode = ControlDrawMode.Line;
        }

        private void BtnSelectVertical_Click(object sender, RoutedEventArgs e)
        {
            //Let the user draw a line along a horizontal edge
            ResetDrawMode();
            DrawingVerticalLine = true;
            CurrentCanvas.Cursor = Cursors.Cross;
            MyDrawMode = ControlDrawMode.Line;
        }

        private void BtnRotateToLine_Click(object sender, RoutedEventArgs e)
        {
            if (DrawingHorizontalLine)
            {
                DrawingHorizontalLine = false;
                Rotate(myAngle);
            }
            else if (DrawingVerticalLine)
            {
                DrawingVerticalLine = false;
                if (myAngle > 0)
                {
                    Rotate(myAngle - Math.PI / 2);
                }
                else
                {
                    Rotate(Math.PI / 2 + myAngle);
                }
            }
            ResetDrawMode();
        }

        private void BtnCancelRotate_Click(object sender, RoutedEventArgs e)
        {
            ResetDrawMode();
        }

        private void CurrentCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!MyLoaded) { return; }
            if (MyDrawMode == ControlDrawMode.Line)
            {
                //Start Rotation correction Line;
                MyLine.X1 = e.GetPosition(CurrentCanvas).X;
                MyLine.Y1 = e.GetPosition(CurrentCanvas).Y;
                IsDrawing = true;
            }
            else if (MyDrawMode == ControlDrawMode.CropArea)
            {
                //Start defining a Crop area;
                CropBand.Visibility = Visibility.Visible;
                CropBand.Mouse_Down(e.GetPosition(CurrentCanvas));
            }
        }

        private void CurrentCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!MyLoaded) { return; }
            if (MyDrawMode == ControlDrawMode.Line && IsDrawing)
            {
                //Draw Rotation correction Line;
                MyLine.X2 = e.GetPosition(CurrentCanvas).X;
                MyLine.Y2 = e.GetPosition(CurrentCanvas).Y;
                MyLine.StrokeThickness = 1;
            }
            else if (MyDrawMode == ControlDrawMode.CropArea)
            {
                //Draw Crop rectangle;
                CropBand.Mouse_Move(e.GetPosition(CurrentCanvas));
            }
        }

        private void CurrentCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!MyLoaded) { return; }
            if (MyDrawMode == ControlDrawMode.Line)
            {
                //End of Rotation correction Line
                //Calculate the angle of the line
                MyDrawMode = ControlDrawMode.None;
                IsDrawing = false;
                myAngle = Math.Atan((MyLine.Y2 - MyLine.Y1) / (MyLine.X2 - MyLine.X1));
                BtnRotateToLine.IsEnabled = true;
            }
            else if (MyDrawMode == ControlDrawMode.CropArea)
            {
                //End of Crop Rectangle;
                CropBand.Mouse_Up(e.GetPosition(CurrentCanvas));
            }
        }

        private void Rotate(double RotateAngle)
        {
            int teller = 0;
            double X = 0;
            double Y = 0;
            double XN = 0;
            double YN = 0;
            int index = 0;
            double Angle = 0.0;
            double Dist = 0.0;
            double Xc = Math.Round(CurrentBitmap.PixelWidth / 2.0, 0);
            double Yc = Math.Round(CurrentBitmap.PixelHeight / 2.0, 0);
            //Reset the Pixeldata
            for (int I = 0; I < TempPixelData.Count(); I++)
            {
                TempPixelData[I] = 0;
            }
            //Rotate the WriteableBitmap
            do
            {
                X = (int)(teller / bytesPerPixel) % CurrentBitmap.PixelWidth - Xc;
                Y = Yc - (int)(teller / (bytesPerPixel * CurrentBitmap.PixelWidth));

                Dist = Math.Sqrt(X * X + Y * Y);
                if (X > 0)
                {
                    Angle = Math.Atan(Y / X);
                }
                else if (X == 0)
                {
                    if (Y > 0)
                    {
                        Angle = Math.PI / 2;
                    }
                    else
                    {
                        Angle = 3 * Math.PI / 2;
                    }
                }
                else
                {
                    Angle = Math.Atan(Y / X) + Math.PI;
                }
                XN = (int)(Dist * Math.Cos(Angle - RotateAngle));
                YN = (int)(Dist * Math.Sin(Angle - RotateAngle));
                if (bytesPerPixel == 1)
                {
                    if (Math.Abs(XN) > CurrentBitmap.PixelWidth / 2 || Math.Abs(YN) > CurrentBitmap.PixelHeight / 2)
                    {
                        teller += 1;
                        continue;
                    }
                    index = (int)((Yc - YN) * CurrentBitmap.PixelWidth + (XN + Xc));
                    if (index >= 0 && index < CalcPixelData.Count() - 1)
                    {
                        TempPixelData[teller] = CalcPixelData[index];
                    }
                    teller += 1;
                }
                else if (bytesPerPixel == 2)
                {
                    if (Math.Abs(XN) > CurrentBitmap.PixelWidth / 2 || Math.Abs(YN) > CurrentBitmap.PixelHeight / 2)
                    {
                        teller += 2;
                        continue;
                    }
                    index = 2 * (int)((Yc - YN) * CurrentBitmap.PixelWidth + (XN + Xc));
                    if (index >= 0 && index < CalcPixelData.Count() - 3)
                    {
                        TempPixelData[teller] = CalcPixelData[index];
                        TempPixelData[teller + 1] = CalcPixelData[index + 1];
                    }
                    teller += 2;
                }
                else if (bytesPerPixel == 3)
                {
                    if (Math.Abs(XN) > CurrentBitmap.PixelWidth / 2 || Math.Abs(YN) > CurrentBitmap.PixelHeight / 2)
                    {
                        teller += 3;
                        continue;
                    }
                    index = 3 * (int)((Yc - YN) * CurrentBitmap.PixelWidth + (XN + Xc));
                    if (index >= 0 && index < CalcPixelData.Count() - 3)
                    {
                        TempPixelData[teller] = CalcPixelData[index];
                        TempPixelData[teller + 1] = CalcPixelData[index + 1];
                        TempPixelData[teller + 2] = CalcPixelData[index + 2];
                    }
                    teller += 3;
                }
                else if (bytesPerPixel == 4)
                {
                    if (Math.Abs(XN) > CurrentBitmap.PixelWidth / 2 || Math.Abs(YN) > CurrentBitmap.PixelHeight / 2)
                    {
                        teller += 4;
                        continue;
                    }
                    index = 4 * (int)((Yc - YN) * CurrentBitmap.PixelWidth + (XN + Xc));
                    if (index >= 0 && index < CalcPixelData.Count() - 3)
                    {
                        TempPixelData[teller] = CalcPixelData[index];
                        TempPixelData[teller + 1] = CalcPixelData[index + 1];
                        TempPixelData[teller + 2] = CalcPixelData[index + 2];
                        TempPixelData[teller + 3] = CalcPixelData[index + 3];
                    }
                    teller += 4;
                }
            } while (teller < CalcPixelData.Count() - 3);
            TempPixelData.CopyTo(CalcPixelData, 0);
            CurrentBitmap.WritePixels(CurrentIntrect, CalcPixelData, CurrentStride, 0);
            SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
            CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
        }

        private void BtnSmooth_Click(object sender, RoutedEventArgs e)
        {
            //Smooth the image (running pixel + 8 surrounding pixels average method)
            int W = CurrentBitmap.PixelWidth;
            int H = CurrentBitmap.PixelHeight;
            int[] int1 = new int[10];
            int[] int2 = new int[10];
            int[] int3 = new int[10]; ;
            int[] int4 = new int[10];
            int count = 0;
            int index = 0;
            Cursor = Cursors.Wait;
            if (bytesPerPixel == 1)
            {
                for (int Iter = 1; Iter <= (int)SldSmoothValue.Value; Iter++)
                {
                    for (int X = 0; X < W; X++)
                    {
                        for (int Y = 0; Y < H; Y++)
                        {
                            index = Y * W + X;
                            count = 0;
                            for (int I = 0; I < 10; I++)
                            {
                                int1[I] = 0;
                            }
                            if (X >= 1)
                            {
                                if (Y >= 1)
                                {
                                    int1[1] = CalcPixelData[index - W - 1];
                                    count += 1;
                                }
                                if (Y < H - 1)
                                {
                                    int1[2] = CalcPixelData[index + W - 1];
                                    count += 1;
                                }
                                int1[3] = CalcPixelData[index - 1];
                                count += 1;
                            }
                            if (X < W - 1)
                            {
                                if (Y >= 1)
                                {
                                    int1[4] = CalcPixelData[index - W + 1];
                                    count += 1;
                                }
                                if (Y < H - 1)
                                {
                                    int1[5] = CalcPixelData[index + W + 1];
                                    count += 1;
                                }
                                int1[6] = CalcPixelData[index + 1];
                                count += 1;
                            }
                            if (Y >= 1)
                            {
                                int1[7] = CalcPixelData[index - W];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[8] = CalcPixelData[index + W];
                                count += 1;
                            }
                            int1[9] = CalcPixelData[index];
                            count += 1;
                            TempPixelData[index] = (byte)((int1[1] + int1[2] + int1[3] + int1[4] + int1[5] + int1[6] + int1[7] + int1[8] + int1[9]) / count);
                        }
                    }
                }
            }
            else if (bytesPerPixel == 2)
            {
                for (int Iter = 1; Iter <= (int)(SldSmoothValue.Value); Iter++)
                {
                    for (int X = 0; X < W; X++)
                    {
                        for (int Y = 0; Y < H; Y++)
                        {
                            index = 2 * (Y * W + X);
                            count = 0;
                            for (int I = 0; I < 10; I++)
                            {
                                int1[I] = 0;
                                int2[I] = 0;
                            }
                            if (X >= 1)
                            {
                                if (Y >= 1)
                                {
                                    int1[1] = CalcPixelData[index - 2 * W - 2];
                                    int2[1] = CalcPixelData[index - 2 * W - 2 + 1];
                                    count += 1;
                                }
                                if (Y < H - 1)
                                {
                                    int1[2] = CalcPixelData[index + 2 * W - 2];
                                    int2[2] = CalcPixelData[index + 2 * W - 2 + 1];
                                    count += 1;
                                }
                                int1[3] = CalcPixelData[index - 2];
                                int2[3] = CalcPixelData[index - 2 + 1];
                                count += 1;
                            }
                            if (X < W - 1)
                            {
                                if (Y >= 1)
                                {
                                    int1[4] = CalcPixelData[index - 2 * W + 2];
                                    int2[4] = CalcPixelData[index - 2 * W + 2 + 1];
                                    count += 1;
                                }
                                if (Y < H - 1)
                                {
                                    int1[5] = CalcPixelData[index + 2 * W + 2];
                                    int2[5] = CalcPixelData[index + 2 * W + 2 + 1];
                                    count += 1;
                                }
                                int1[6] = CalcPixelData[index + 2];
                                int2[6] = CalcPixelData[index + 2 + 1];
                                count += 1;
                            }
                            if (Y >= 1)
                            {
                                int1[7] = CalcPixelData[index - 2 * W];
                                int2[7] = CalcPixelData[index - 2 * W + 1];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[8] = CalcPixelData[index + 2 * W];
                                int2[8] = CalcPixelData[index + 2 * W + 1];
                                count += 1;
                            }
                            int1[9] = CalcPixelData[index];
                            int2[9] = CalcPixelData[index + 1];
                            count += 1;
                            TempPixelData[index] = (byte)((int1[1] + int1[2] + int1[3] + int1[4] + int1[5] + int1[6] + int1[7] + int1[8] + int1[9]) / count);
                            TempPixelData[index + 1] = (byte)((int2[1] + int2[2] + int2[3] + int2[4] + int2[5] + int2[6] + int2[7] + int2[8] + int2[9]) / count);
                        }
                    }
                }
            }
            else if (bytesPerPixel == 3)
            {
                for (int Iter = 1; Iter <= (int)SldSmoothValue.Value; Iter++)
                {
                    for (int X = 0; X < W; X++)
                    {
                        for (int Y = 0; Y < H; Y++)
                        {
                            index = 3 * (Y * W + X);
                            count = 0;
                            for (int I = 0; I < 10; I++)
                            {
                                int1[I] = 0;
                                int2[I] = 0;
                                int3[I] = 0;
                            }
                            if (X >= 1)
                            {
                                if (Y >= 1)
                                {
                                    int1[1] = CalcPixelData[index - 3 * W - 3];
                                    int2[1] = CalcPixelData[index - 3 * W - 3 + 1];
                                    int3[1] = CalcPixelData[index - 3 * W - 3 + 2];
                                    count += 1;
                                }
                                if (Y < H - 1)
                                {
                                    int1[2] = CalcPixelData[index + 3 * W - 3];
                                    int2[2] = CalcPixelData[index + 3 * W - 3 + 1];
                                    int3[2] = CalcPixelData[index + 3 * W - 3 + 2];
                                    count += 1;
                                }
                                int1[3] = CalcPixelData[index - 3];
                                int2[3] = CalcPixelData[index - 3 + 1];
                                int3[3] = CalcPixelData[index - 3 + 2];
                                count += 1;
                            }
                            if (X < W - 1)
                            {
                                if (Y >= 1)
                                {
                                    int1[4] = CalcPixelData[index - 3 * W + 3];
                                    int2[4] = CalcPixelData[index - 3 * W + 3 + 1];
                                    int3[4] = CalcPixelData[index - 3 * W + 3 + 2];
                                    count += 1;
                                }
                                if (Y < H - 1)
                                {
                                    int1[5] = CalcPixelData[index + 3 * W + 3];
                                    int2[5] = CalcPixelData[index + 3 * W + 3 + 1];
                                    int3[5] = CalcPixelData[index + 3 * W + 3 + 2];
                                    count += 1;
                                }
                                int1[6] = CalcPixelData[index + 3];
                                int2[6] = CalcPixelData[index + 3 + 1];
                                int3[6] = CalcPixelData[index + 3 + 2];
                                count += 1;
                            }
                            if (Y >= 1)
                            {
                                int1[7] = CalcPixelData[index - 3 * W];
                                int2[7] = CalcPixelData[index - 3 * W + 1];
                                int3[7] = CalcPixelData[index - 3 * W + 2];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[8] = CalcPixelData[index + 3 * W];
                                int2[8] = CalcPixelData[index + 3 * W + 1];
                                int3[8] = CalcPixelData[index + 3 * W + 2];
                                count += 1;
                            }
                            int1[9] = CalcPixelData[index];
                            int2[9] = CalcPixelData[index + 1];
                            int3[9] = CalcPixelData[index + 2];
                            count += 1;
                            TempPixelData[index] = (byte)((int1[1] + int1[2] + int1[3] + int1[4] + int1[5] + int1[6] + int1[7] + int1[8] + int1[9]) / count);
                            TempPixelData[index + 1] = (byte)((int2[1] + int2[2] + int2[3] + int2[4] + int2[5] + int2[6] + int2[7] + int2[8] + int2[9]) / count);
                            TempPixelData[index + 2] = (byte)((int3[1] + int3[2] + int3[3] + int3[4] + int3[5] + int3[6] + int3[7] + int3[8] + int3[9]) / count);
                        }
                    }
                }
            }
            else if (bytesPerPixel == 4)
            {
                for (int Iter = 1; Iter <= (int)SldSmoothValue.Value; Iter++)
                {
                    for (int X = 0; X < W; X++)
                    {
                        for (int Y = 0; Y < H; Y++)
                        {
                            index = 4 * (Y * W + X);
                            count = 0;
                            for (int I = 0; I < 10; I++)
                            {
                                int1[I] = 0;
                                int2[I] = 0;
                                int3[I] = 0;
                                int4[I] = 0;
                            }
                            if (X >= 1)
                            {
                                if (Y >= 1)
                                {
                                    int1[1] = CalcPixelData[index - 4 * W - 4];
                                    int2[1] = CalcPixelData[index - 4 * W - 4 + 1];
                                    int3[1] = CalcPixelData[index - 4 * W - 4 + 2];
                                    int4[1] = CalcPixelData[index - 4 * W - 4 + 3];
                                    count += 1;
                                }
                                if (Y < H - 1)
                                {
                                    int1[2] = CalcPixelData[index + 4 * W - 4];
                                    int2[2] = CalcPixelData[index + 4 * W - 4 + 1];
                                    int3[2] = CalcPixelData[index + 4 * W - 4 + 2];
                                    int4[2] = CalcPixelData[index + 4 * W - 4 + 3];
                                    count += 1;
                                }
                                int1[3] = CalcPixelData[index - 4];
                                int2[3] = CalcPixelData[index - 4 + 1];
                                int3[3] = CalcPixelData[index - 4 + 2];
                                int4[3] = CalcPixelData[index - 4 + 3];
                                count += 1;
                            }
                            if (X < W - 1)
                            {
                                if (Y >= 1)
                                {
                                    int1[4] = CalcPixelData[index - 4 * W + 4];
                                    int2[4] = CalcPixelData[index - 4 * W + 4 + 1];
                                    int3[4] = CalcPixelData[index - 4 * W + 4 + 2];
                                    int4[4] = CalcPixelData[index - 4 * W + 4 + 3];
                                    count += 1;
                                }
                                if (Y < H - 1)
                                {
                                    int1[5] = CalcPixelData[index + 4 * W + 4];
                                    int2[5] = CalcPixelData[index + 4 * W + 4 + 1];
                                    int3[5] = CalcPixelData[index + 4 * W + 4 + 2];
                                    int4[5] = CalcPixelData[index + 4 * W + 4 + 3];
                                    count += 1;
                                }
                                int1[6] = CalcPixelData[index + 4];
                                int2[6] = CalcPixelData[index + 4 + 1];
                                int3[6] = CalcPixelData[index + 4 + 2];
                                int4[6] = CalcPixelData[index + 4 + 3];
                                count += 1;
                            }
                            if (Y >= 1)
                            {
                                int1[7] = CalcPixelData[index - 4 * W];
                                int2[7] = CalcPixelData[index - 4 * W + 1];
                                int3[7] = CalcPixelData[index - 4 * W + 2];
                                int4[7] = CalcPixelData[index - 4 * W + 3];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[8] = CalcPixelData[index + 4 * W];
                                int2[8] = CalcPixelData[index + 4 * W + 1];
                                int3[8] = CalcPixelData[index + 4 * W + 2];
                                int4[8] = CalcPixelData[index + 4 * W + 3];
                                count += 1;
                            }
                            int1[9] = CalcPixelData[index];
                            int2[9] = CalcPixelData[index + 1];
                            int3[9] = CalcPixelData[index + 2];
                            int4[9] = CalcPixelData[index + 3];
                            count += 1;
                            TempPixelData[index] = (byte)((int1[1] + int1[2] + int1[3] + int1[4] + int1[5] + int1[6] + int1[7] + int1[8] + int1[9]) / count);
                            TempPixelData[index + 1] = (byte)((int2[1] + int2[2] + int2[3] + int2[4] + int2[5] + int2[6] + int2[7] + int2[8] + int2[9]) / count);
                            TempPixelData[index + 2] = (byte)((int3[1] + int3[2] + int3[3] + int3[4] + int3[5] + int3[6] + int3[7] + int3[8] + int3[9]) / count);
                            TempPixelData[index + 3] = (byte)((int4[1] + int4[2] + int4[3] + int4[4] + int4[5] + int4[6] + int4[7] + int4[8] + int4[9]) / count);
                        }
                    }
                }
            }
            TempPixelData.CopyTo(CalcPixelData, 0);
            CurrentBitmap.WritePixels(CurrentIntrect, CalcPixelData, CurrentStride, 0);
            SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
            CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
            Cursor = Cursors.Arrow;
        }

        private void BtnSharpen_Click(Object sender, RoutedEventArgs e)
        {
            //Sharpen the image (method = (1+factor)*running pixel - factor * average of surrounding pixels);
            int W = CurrentBitmap.PixelWidth;
            int H = CurrentBitmap.PixelHeight;
            double factor = SldSharpenValue.Value;
            int[] int1 = new int[10];
            int[] int2 = new int[10];
            int[] int3 = new int[10];
            int[] int4 = new int[10];
            int count = 0;
            int index = 0;
            double newB = 0.0;
            double newG = 0.0;
            double newR = 0.0;
            double newA = 0.0;
            Cursor = Cursors.Wait;
            if (bytesPerPixel == 1)
            {
                for (int X = 0; X < W; X++)
                {
                    for (int Y = 0; Y < H; Y++)
                    {
                        index = Y * W + X;
                        count = 0;
                        for (int I = 0; I < 10; I++)
                        {
                            int1[I] = 0;
                        }
                        if (X >= 1)
                        {
                            if (Y >= 1)
                            {
                                int1[1] = CalcPixelData[index - W - 1];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[2] = CalcPixelData[index + W - 1];
                                count += 1;
                            }
                            int1[3] = CalcPixelData[index - 1];
                            count += 1;
                        }
                        if (X < W - 1)
                        {
                            if (Y >= 1)
                            {
                                int1[4] = CalcPixelData[index - W + 1];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[5] = CalcPixelData[index + W + 1];
                                count += 1;
                            }
                            int1[6] = CalcPixelData[index + 1];
                            count += 1;
                        }
                        if (Y >= 1)
                        {
                            int1[7] = CalcPixelData[index - W];
                            count += 1;
                        }
                        if (Y < H - 1)
                        {
                            int1[8] = CalcPixelData[index + W];
                            count += 1;
                        }
                        int1[9] = CalcPixelData[index];
                        count += 1;
                        newB = (1 + factor) * CalcPixelData[index] - factor * (int1[1] + int1[2] + int1[3] + int1[4] + int1[5] + int1[6] + int1[7] + int1[8] + int1[9]) / count;
                        if (newB < 0) { newB = 0; }
                        if (newB > 255) { newB = 255; }
                        TempPixelData[index] = (byte)newB;
                    }
                }
            }
            else if (bytesPerPixel == 2)
            {
                for (int X = 0; X < W; X++)
                {
                    for (int Y = 0; Y < H; Y++)
                    {
                        index = 2 * (Y * W + X);
                        count = 0;
                        for (int I = 0; I < 10; I++)
                        {
                            int1[I] = 0;
                            int2[I] = 0;
                        }
                        if (X >= 1)
                        {
                            if (Y >= 1)
                            {
                                int1[1] = CalcPixelData[index - 2 * W - 2];
                                int2[1] = CalcPixelData[index - 2 * W - 2 + 1];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[2] = CalcPixelData[index + 2 * W - 2];
                                int2[2] = CalcPixelData[index + 2 * W - 2 + 1];
                                count += 1;
                            }
                            int1[3] = CalcPixelData[index - 2];
                            int2[3] = CalcPixelData[index - 2 + 1];
                            count += 1;
                        }
                        if (X < W - 1)
                        {
                            if (Y >= 1)
                            {
                                int1[4] = CalcPixelData[index - 2 * W + 2];
                                int2[4] = CalcPixelData[index - 2 * W + 2 + 1];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[5] = CalcPixelData[index + 2 * W + 2];
                                int2[5] = CalcPixelData[index + 2 * W + 2 + 1];
                                count += 1;
                            }
                            int1[6] = CalcPixelData[index + 2];
                            int2[6] = CalcPixelData[index + 2 + 1];
                            count += 1;
                        }
                        if (Y >= 1)
                        {
                            int1[7] = CalcPixelData[index - 2 * W];
                            int2[7] = CalcPixelData[index - 2 * W + 1];
                            count += 1;
                        }
                        if (Y < H - 1)
                        {
                            int1[8] = CalcPixelData[index + 2 * W];
                            int2[8] = CalcPixelData[index + 2 * W + 1];
                            count += 1;
                        }
                        int1[9] = CalcPixelData[index];
                        int2[9] = CalcPixelData[index + 1];
                        count += 1;
                        newB = (1 + factor) * CalcPixelData[index] - factor * (int1[1] + int1[2] + int1[3] + int1[4] + int1[5] + int1[6] + int1[7] + int1[8] + int1[9]) / count;
                        newG = (1 + factor) * CalcPixelData[index + 1] - factor * (int2[1] + int2[2] + int2[3] + int2[4] + int2[5] + int2[6] + int2[7] + int2[8] + int2[9]) / count;
                        if (newB < 0)
                        {
                            newB = 0;
                        }
                        if (newB > 255)
                        {
                            newB = 255;
                        }
                        if (newG < 0)
                        {
                            newG = 0;
                        }
                        if (newG > 255)
                        {
                            newG = 255;
                        }
                        TempPixelData[index] = (byte)newB;
                        TempPixelData[index + 1] = (byte)newG;
                    }
                }
            }
            else if (bytesPerPixel == 3)
            {
                for (int X = 0; X < W; X++)
                {
                    for (int Y = 0; Y < H; Y++)
                    {
                        index = 3 * (Y * W + X);
                        count = 0;
                        for (int I = 0; I < 10; I++)
                        {
                            int1[I] = 0;
                            int2[I] = 0;
                            int3[I] = 0;
                        }
                        if (X >= 1)
                        {
                            if (Y >= 1)
                            {
                                int1[1] = CalcPixelData[index - 3 * W - 3];
                                int2[1] = CalcPixelData[index - 3 * W - 3 + 1];
                                int3[1] = CalcPixelData[index - 3 * W - 3 + 2];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[2] = CalcPixelData[index + 3 * W - 3];
                                int2[2] = CalcPixelData[index + 3 * W - 3 + 1];
                                int3[2] = CalcPixelData[index + 3 * W - 3 + 2];
                                count += 1;
                            }
                            int1[3] = CalcPixelData[index - 3];
                            int2[3] = CalcPixelData[index - 3 + 1];
                            int3[3] = CalcPixelData[index - 3 + 2];
                            count += 1;
                        }
                        if (X < W - 1)
                        {
                            if (Y >= 1)
                            {
                                int1[4] = CalcPixelData[index - 3 * W + 3];
                                int2[4] = CalcPixelData[index - 3 * W + 3 + 1];
                                int3[4] = CalcPixelData[index - 3 * W + 3 + 2];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[5] = CalcPixelData[index + 3 * W + 3];
                                int2[5] = CalcPixelData[index + 3 * W + 3 + 1];
                                int3[5] = CalcPixelData[index + 3 * W + 3 + 2];
                                count += 1;
                            }
                            int1[6] = CalcPixelData[index + 3];
                            int2[6] = CalcPixelData[index + 3 + 1];
                            int3[6] = CalcPixelData[index + 3 + 2];
                            count += 1;
                        }
                        if (Y >= 1)
                        {
                            int1[7] = CalcPixelData[index - 3 * W];
                            int2[7] = CalcPixelData[index - 3 * W + 1];
                            int3[7] = CalcPixelData[index - 3 * W + 2];
                            count += 1;
                        }
                        if (Y < H - 1)
                        {
                            int1[8] = CalcPixelData[index + 3 * W];
                            int2[8] = CalcPixelData[index + 3 * W + 1];
                            int3[8] = CalcPixelData[index + 3 * W + 2];
                            count += 1;
                        }
                        int1[9] = CalcPixelData[index];
                        int2[9] = CalcPixelData[index + 1];
                        int3[9] = CalcPixelData[index + 2];
                        count += 1;
                        newB = (1 + factor) * CalcPixelData[index] - factor * (int1[1] + int1[2] + int1[3] + int1[4] + int1[5] + int1[6] + int1[7] + int1[8] + int1[9]) / count;
                        newG = (1 + factor) * CalcPixelData[index + 1] - factor * (int2[1] + int2[2] + int2[3] + int2[4] + int2[5] + int2[6] + int2[7] + int2[8] + int2[9]) / count;
                        newR = (1 + factor) * CalcPixelData[index + 2] - factor * (int3[1] + int3[2] + int3[3] + int3[4] + int3[5] + int3[6] + int3[7] + int3[8] + int3[9]) / count;
                        if (newB < 0)
                        {
                            newB = 0;
                        }
                        if (newB > 255)
                        {
                            newB = 255;
                        }
                        if (newG < 0)
                        {
                            newG = 0;
                        }
                        if (newG > 255)
                        {
                            newG = 255;
                        }
                        if (newR < 0)
                        {
                            newR = 0;
                        }
                        if (newR > 255)
                        {
                            newR = 255;
                        }
                        TempPixelData[index] = (byte)newB;
                        TempPixelData[index + 1] = (byte)newG;
                        TempPixelData[index + 2] = (byte)newR;
                    }
                }
            }
            else if (bytesPerPixel == 4)
            {
                for (int X = 0; X < W; X++)
                {
                    for (int Y = 0; Y < H; Y++)
                    {
                        index = 4 * (Y * W + X);
                        count = 0;
                        for (int I = 0; I < 10; I++)
                        {
                            int1[I] = 0;
                            int2[I] = 0;
                            int3[I] = 0;
                            int4[I] = 0;
                        }
                        if (X >= 1)
                        {
                            if (Y >= 1)
                            {
                                int1[1] = CalcPixelData[index - 4 * W - 4];
                                int2[1] = CalcPixelData[index - 4 * W - 4 + 1];
                                int3[1] = CalcPixelData[index - 4 * W - 4 + 2];
                                int4[1] = CalcPixelData[index - 4 * W - 4 + 3];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[2] = CalcPixelData[index + 4 * W - 4];
                                int2[2] = CalcPixelData[index + 4 * W - 4 + 1];
                                int3[2] = CalcPixelData[index + 4 * W - 4 + 2];
                                int4[2] = CalcPixelData[index + 4 * W - 4 + 3];
                                count += 1;
                            }
                            int1[3] = CalcPixelData[index - 4];
                            int2[3] = CalcPixelData[index - 4 + 1];
                            int3[3] = CalcPixelData[index - 4 + 2];
                            int4[3] = CalcPixelData[index - 4 + 3];
                            count += 1;
                        }
                        if (X < W - 1)
                        {
                            if (Y >= 1)
                            {
                                int1[4] = CalcPixelData[index - 4 * W + 4];
                                int2[4] = CalcPixelData[index - 4 * W + 4 + 1];
                                int3[4] = CalcPixelData[index - 4 * W + 4 + 2];
                                int4[4] = CalcPixelData[index - 4 * W + 4 + 3];
                                count += 1;
                            }
                            if (Y < H - 1)
                            {
                                int1[5] = CalcPixelData[index + 4 * W + 4];
                                int2[5] = CalcPixelData[index + 4 * W + 4 + 1];
                                int3[5] = CalcPixelData[index + 4 * W + 4 + 2];
                                int4[5] = CalcPixelData[index + 4 * W + 4 + 3];
                                count += 1;
                            }
                            int1[6] = CalcPixelData[index + 4];
                            int2[6] = CalcPixelData[index + 4 + 1];
                            int3[6] = CalcPixelData[index + 4 + 2];
                            int4[6] = CalcPixelData[index + 4 + 3];
                            count += 1;
                        }
                        if (Y >= 1)
                        {
                            int1[7] = CalcPixelData[index - 4 * W];
                            int2[7] = CalcPixelData[index - 4 * W + 1];
                            int3[7] = CalcPixelData[index - 4 * W + 2];
                            int4[7] = CalcPixelData[index - 4 * W + 3];
                            count += 1;
                        }
                        if (Y < H - 1)
                        {
                            int1[8] = CalcPixelData[index + 4 * W];
                            int2[8] = CalcPixelData[index + 4 * W + 1];
                            int3[8] = CalcPixelData[index + 4 * W + 2];
                            int4[8] = CalcPixelData[index + 4 * W + 3];
                            count += 1;
                        }
                        int1[9] = CalcPixelData[index];
                        int2[9] = CalcPixelData[index + 1];
                        int3[9] = CalcPixelData[index + 2];
                        int4[9] = CalcPixelData[index + 3];
                        count += 1;
                        newB = (1 + factor) * CalcPixelData[index] - factor * (int1[1] + int1[2] + int1[3] + int1[4] + int1[5] + int1[6] + int1[7] + int1[8] + int1[9]) / count;
                        newG = (1 + factor) * CalcPixelData[index + 1] - factor * (int2[1] + int2[2] + int2[3] + int2[4] + int2[5] + int2[6] + int2[7] + int2[8] + int2[9]) / count;
                        newR = (1 + factor) * CalcPixelData[index + 2] - factor * (int3[1] + int3[2] + int3[3] + int3[4] + int3[5] + int3[6] + int3[7] + int3[8] + int3[9]) / count;
                        newA = (1 + factor) * CalcPixelData[index + 3] - factor * (int4[1] + int4[2] + int4[3] + int4[4] + int4[5] + int4[6] + int4[7] + int4[8] + int4[9]) / count;
                        if (newB < 0)
                        {
                            newB = 0;
                        }
                        if (newB > 255)
                        {
                            newB = 255;
                        }
                        if (newG < 0)
                        {
                            newG = 0;
                        }
                        if (newG > 255)
                        {
                            newG = 255;
                        }
                        if (newR < 0)
                        {
                            newR = 0;
                        }
                        if (newR > 255)
                        {
                            newR = 255;
                        }
                        if (newA < 0)
                        {
                            newA = 0;
                        }
                        if (newA > 255)
                        {
                            newA = 255;
                        }
                        TempPixelData[index] = (byte)newB;
                        TempPixelData[index + 1] = (byte)newG;
                        TempPixelData[index + 2] = (byte)newR;
                        TempPixelData[index + 3] = (byte)newA;
                    }
                }
            }
            TempPixelData.CopyTo(CalcPixelData, 0);
            CurrentBitmap.WritePixels(CurrentIntrect, CalcPixelData, CurrentStride, 0);
            SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
            CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
            Cursor = Cursors.Arrow;
        }

        private void BtnSelectCrop_Click(object sender, RoutedEventArgs e)
        {
            ResetDrawMode();
            MyDrawMode = ControlDrawMode.CropArea;
            BtnCropImage.IsEnabled = true;
            CurrentCanvas.Cursor = Cursors.Cross;
        }

        private void BtnCropImage_Click(object sender, RoutedEventArgs e)
        {
            int oldIndex;
            int newIndex;
            int Xmin = (int)(CropBand.TopLeftCorner.X / CurrentCanvas.ActualWidth * CurrentBitmap.PixelWidth);
            int Xmax = (int)(CropBand.BottomRightCorner.X / CurrentCanvas.ActualWidth * CurrentBitmap.PixelWidth);
            int Ymin = (int)(CropBand.TopLeftCorner.Y / CurrentCanvas.ActualHeight * CurrentBitmap.PixelHeight);
            int Ymax = (int)(CropBand.BottomRightCorner.Y / CurrentCanvas.ActualHeight * CurrentBitmap.PixelHeight);
            if (Xmin < 0) { Xmin = 0; }
            if (Ymin < 0) { Ymin = 0; }
            if (Xmax >= CurrentBitmap.PixelWidth) { Xmax = CurrentBitmap.PixelWidth - 1; }
            if (Ymax >= CurrentBitmap.PixelHeight) { Ymax = CurrentBitmap.PixelHeight - 1; }
            CurrentIntrect = new Int32Rect(0, 0, Xmax - Xmin + 1, Ymax - Ymin + 1);
            if (bytesPerPixel == 1)
            {
                CurrentStride = Xmax - Xmin + 1;
                TempPixelData = new byte[(Ymax - Ymin + 1) * (Xmax - Xmin + 1)];
                newIndex = 0;
                for (int Y = Ymin; Y <= Ymax; Y++)
                {
                    for (int X = Xmin; X <= Xmax; X++)
                    {
                        oldIndex = Y * CurrentBitmap.PixelWidth + X;
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        newIndex += 1;
                    }
                }
            }
            else if (bytesPerPixel == 2)
            {
                CurrentStride = 2 * (Xmax - Xmin + 1);
                TempPixelData = new byte[2 * (Ymax - Ymin + 1) * (Xmax - Xmin + 1)];
                newIndex = 0;
                for (int Y = Ymin; Y <= Ymax; Y++)
                {
                    for (int X = Xmin; X <= Xmax; X++)
                    {
                        oldIndex = 2 * (Y * CurrentBitmap.PixelWidth + X);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        newIndex += 2;
                    }
                }
            }
            else if (bytesPerPixel == 3)
            {
                CurrentStride = 3 * (Xmax - Xmin + 1);
                TempPixelData = new byte[3 * (Ymax - Ymin + 1) * (Xmax - Xmin + 1)];
                newIndex = 0;
                for (int Y = Ymin; Y <= Ymax; Y++)
                {
                    for (int X = Xmin; X <= Xmax; X++)
                    {
                        oldIndex = 3 * (Y * CurrentBitmap.PixelWidth + X);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                        newIndex += 3;
                    }
                }
            }
            else if (bytesPerPixel == 4)
            {
                CurrentStride = 4 * (Xmax - Xmin + 1);
                TempPixelData = new byte[4 * (Ymax - Ymin + 1) * (Xmax - Xmin + 1)];
                newIndex = 0;
                for (int Y = Ymin; Y <= Ymax; Y++)
                {
                    for (int X = Xmin; X <= Xmax; X++)
                    {

                        oldIndex = 4 * (Y * CurrentBitmap.PixelWidth + X);
                        TempPixelData[newIndex] = CalcPixelData[oldIndex];
                        TempPixelData[newIndex + 1] = CalcPixelData[oldIndex + 1];
                        TempPixelData[newIndex + 2] = CalcPixelData[oldIndex + 2];
                        TempPixelData[newIndex + 3] = CalcPixelData[oldIndex + 3];
                        newIndex += 4;
                    }
                }
            }
            CurrentBitmap = new WriteableBitmap((Xmax - Xmin + 1), (Ymax - Ymin + 1), CurrentBitmap.DpiX, CurrentBitmap.DpiY, CurrentBitmap.Format, CurrentBitmap.Palette);
            CalcPixelData = new byte[TempPixelData.Length];
            CurrentPixelData = new byte[TempPixelData.Length];
            TempPixelData.CopyTo(CalcPixelData, 0);
            TempPixelData.CopyTo(CurrentPixelData, 0);
            CurrentBitmap.WritePixels(CurrentIntrect, CalcPixelData, CurrentStride, 0);
            MaxCurrentSize = new Size(CurrentCanvas.ActualWidth, CurrentCanvas.ActualHeight);
            SetCanvasSize(MaxCurrentSize, CurrentCanvas, CurrentBitmap);
            CurrentCanvas.Background = new ImageBrush(CurrentBitmap);
            ResetDrawMode();
            SetPreviewImage(CurrentBitmap);
            CurrentCanvas.Cursor = Cursors.Arrow;
        }

        private void CbKeepAspect_Click(object sender, RoutedEventArgs e)
        {
            if (CbKeepAspect.IsChecked == true)
            {
                CropBand.AspectRatio = CurrentBitmap.PixelWidth / CurrentBitmap.PixelHeight;
                CropBand.FixedAspectRatio = true;
            }
            else
            {
                CropBand.FixedAspectRatio = false;
            }
        }

        private void BtnCancelCrop_Click(object sender, RoutedEventArgs e)
        {
            ResetDrawMode();
        }

        private void ResetDrawMode()
        {
            //Reset the CropBand
            CropBand.Draw(false);
            CropBand.Visibility = Visibility.Hidden;
            BtnCropImage.IsEnabled = false;
            //Reset the RotateToLine
            DrawingHorizontalLine = false;
            DrawingVerticalLine = false;
            MyLine.StrokeThickness = 0;
            BtnRotateToLine.IsEnabled = false;
            //Reset the DrawMode
            MyDrawMode = ControlDrawMode.None;
            CurrentCanvas.Cursor = Cursors.Arrow;
        }

        #endregion

        private void SaveImage(string filename)
        {
            BitmapEncoder MyEncoder;
            BitmapSource bmp = BitmapSource.Create(CurrentBitmap.PixelWidth, CurrentBitmap.PixelHeight, CurrentBitmap.DpiX, CurrentBitmap.DpiY, CurrentBitmap.Format, CurrentBitmap.Palette, TempPixelData, CurrentStride);
            try
            {
                switch (MyParent.Settings.ImageFormatIndex)
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
                    default:
                        //Should not occur
                        return;
                }
                MyEncoder.Frames.Add(BitmapFrame.Create(bmp));
                // Create a FileStream to write the image to the file.
                using (FileStream sw = new FileStream(filename, FileMode.Create))
                {
                    MyEncoder.Save(sw);
                }
            }
            catch
            {
                MessageBox.Show("The Image could not be saved.", "ImageTools error", MessageBoxButton.OK, MessageBoxImage.Error);
           }
        }

        public BitmapImage ToBitmapImage(WriteableBitmap wbm)
        {
            if (wbm == null) { return null; }
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream mystream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(mystream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = mystream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }

    }

    public enum ControlDrawMode
    {
        None,
        Line,
        CropArea,
    }
}

