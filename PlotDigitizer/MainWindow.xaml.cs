using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlotDigitizer
{
    public partial class MainWindow : Window
    {
        private string filepath = "";
        private string fileName = "";
        private BitmapImage bitmap = new BitmapImage();
        private double XPos = 0.0;  //Mouse X position as a fraction of Image1.ActualWidth (0 ... 1)
        private double YPos = 0.0;  //Mouse Y position as a fraction of Image1.ActualHeight (0 ... 1)
        double ImgXOffset;  //Left position of Image1 in Canvas1
        double ImgYOffset;  //Top position of Image1 in Canvas1
        int ImageWidth;
        int ImageHeight;
        private PlotAction My_Action = PlotAction.None;
        private bool Im_Loaded = false;
        private bool UseImagePix = false;
        //All axes and datapoint data are stored as a fraction of Image1.Size
        private Point XOrigin = new Point(0, 0);
        private Point XMax = new Point(0, 0);
        private Point YOrigin = new Point(0, 0);
        private Point YMax = new Point(0, 0);
        private List<Point> Data = new List<Point>();
        private List<Point> DataPos = new List<Point>();
        private bool XOriginSet = false;
        private bool XMaxSet = false;
        private bool YOriginSet = false;
        private bool YMaxSet = false;
        private Line Xaxis = new Line();
        private Line Yaxis = new Line();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region "Window Events"

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = "PlotDigitizer";
            Image1.Width = 0;
            Image1.Height = 0;
            ImageWidth = 0;
            ImageHeight = 0;
            Init();
            Im_Loaded = true;
            UseImagePix = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (XPos < 0 || YPos < 0) return;
            if (!Im_Loaded) return;
            double X = 0.0;
            double Y = 0.0;
            double XValue = (int)(XPos * ImageWidth);
            double YValue = (int)(YPos * ImageHeight); 
            if (e.ChangedButton == MouseButton.Left)
            {
                switch (My_Action)
                {
                    case PlotAction.XOrigin:
                    {
                        XOrigin = new Point(XPos, YPos);
                        TxtXOriginX.Text = XValue.ToString();
                        TxtXOriginY.Text = YValue.ToString();
                        XOriginSet = true;
                        DrawXAxis();
                        Image1.Cursor = Cursors.Arrow;
                        My_Action = PlotAction.None;
                        break;
                    }
                    case PlotAction.XMax:
                    {
                        XMax = new Point(XPos, YPos);
                        TxtXMaxX.Text = XValue.ToString();
                        TxtXMaxY.Text = YValue.ToString();
                        XMaxSet = true;
                        DrawXAxis();
                        Image1.Cursor = Cursors.Arrow;
                        My_Action = PlotAction.None;
                        break;
                    }
                    case PlotAction.YOrigin:
                    {
                        YOrigin = new Point(XPos, YPos);
                        TxtYOriginX.Text = XValue.ToString();
                        TxtYOriginY.Text = YValue.ToString();
                        YOriginSet = true;
                        DrawYAxis();
                        Image1.Cursor = Cursors.Arrow;
                        My_Action = PlotAction.None;
                        break;
                    }
                    case PlotAction.YMax:
                    {
                        YMax = new Point(XPos, YPos);
                        TxtYMaxX.Text = XValue.ToString();
                        TxtYMaxY.Text = YValue.ToString();
                        YMaxSet = true;
                        DrawYAxis();
                        Image1.Cursor = Cursors.Arrow;
                        My_Action = PlotAction.None;
                        break;
                    }
                    case PlotAction.DataPoint:
                    {
                        double A1, B1, A2, B2;
                        double XTemp, YTemp;
                        double XOriginValue = double.Parse(TxtXOriginValue.Text);
                        double XMaxValue = double.Parse(TxtXMaxValue.Text);
                        double YOriginValue = double.Parse(TxtYOriginValue.Text);
                        double YMaxValue = double.Parse(TxtYMaxValue.Text);
                        //Calculate X and Y coordinates allowing for rotated or skewed graphs
                        if (XOrigin.Y == XMax.Y && YOrigin.X == YMax.X) //Both axis are OK
                        {                                               //Use orthogonal projection for X and Y
                            XTemp = XPos;
                            YTemp = YPos;
                        }
                        else if (XOrigin.Y != XMax.Y && YOrigin.X == YMax.X) //X-axis is rotated
                        {                                                    //Use orthogonal projection for X
                            XTemp = XPos;
                            //Y = intersection of:
                            //  a line Parallel To X axis through point (XPos, YPos)
                            //  and the Y-axis (X = YOrigin.X)
                            A2 = (XMax.Y - XOrigin.Y) / (XMax.X - XOrigin.X);
                            YTemp = YPos + (YOrigin.X - XPos) * A2;
                        }
                        else if (XOrigin.Y == XMax.Y && YOrigin.X != YMax.X) //Y-axis is rotated
                        {                                                    //Use orthogonal projection for Y
                            YTemp = YPos;
                            //X = intersection of:
                            //  a line Parallel To Y-axis through point (XPos, YPos)
                            // -and the X-axis (Y = XOrigin.Y)
                            A1 = (YMax.Y - YOrigin.Y) / (YMax.X - YOrigin.X);
                            XTemp = XPos + (XOrigin.Y - YPos) / A1;
                        }
                        else //Both axes are rotated
                        {
                            A1 = (YMax.Y - YOrigin.Y) / (YMax.X - YOrigin.X);
                            B1 = YPos - (YMax.Y - YOrigin.Y) / (YMax.X - YOrigin.X) * XPos;
                            A2 = (XMax.Y - XOrigin.Y) / (XMax.X - XOrigin.X);
                            B2 = XOrigin.Y - (XMax.Y - XOrigin.Y) / (XMax.X - XOrigin.X) * XOrigin.X;
                            XTemp = (B2 - B1) / (A1 - A2);
                            A1 = (XMax.Y - XOrigin.Y) / (XMax.X - XOrigin.X);
                            B1 = YPos - (XMax.Y - XOrigin.Y) / (XMax.X - XOrigin.X) * XPos;
                            A2 = (YMax.Y - YOrigin.Y) / (YMax.X - YOrigin.X);
                            B2 = YOrigin.Y - (YMax.Y - YOrigin.Y) / (YMax.X - YOrigin.X) * YOrigin.X;
                            YTemp = (B2 - B1) / (A1 - A2) * A1 + B1;
                        }
                        //Calculate X and Y values
                        if (RbXLinear.IsChecked.Value)
                        {
                            X = XOriginValue + (XMaxValue - XOriginValue) * (XTemp - XOrigin.X) / (XMax.X - XOrigin.X);
                        }
                        else if (RbXLog.IsChecked.Value)
                        {
                            X = Math.Pow(10.0, Math.Log10(XOriginValue) + (Math.Log10(XMaxValue) - Math.Log10(XOriginValue)) * (XTemp - XOrigin.X) / (XMax.X - XOrigin.X));
                        }
                        if (RbYLinear.IsChecked.Value)
                        {
                            Y = YOriginValue + (YMaxValue - YOriginValue) * (YTemp - YOrigin.Y) / (YMax.Y - YOrigin.Y);
                        }
                        else if (RbYLog.IsChecked.Value)
                        {
                            Y = Math.Pow(10.0, Math.Log10(YOriginValue) + (Math.Log10(YMaxValue) - Math.Log10(YOriginValue)) * (YTemp - YOrigin.Y) / (YMax.Y - YOrigin.Y));
                        }
                        if (double.IsNaN(X) || double.IsNaN(Y))
                        {
                            MessageBox.Show("Division by zero error! Check axis values and orientation.", "PlotDigitizer Error.", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        DataPos.Add(new Point(XPos, YPos));
                        Data.Add(new Point(X, Y));
                        TxtDataCount.Text = Data.Count.ToString();
                        TxtDataPointsX.Text += X.ToString("G4") + "\n";
                        TxtDataPointsY.Text += Y.ToString("G4") + "\n";
                        DrawDataPoint(new Point(XPos, YPos));
                        BtnClearData.IsEnabled = true;
                        break;
                    }
                }
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (My_Action != PlotAction.None)
            {
                XPos = e.GetPosition(Image1).X / ImageWidth ;
                YPos = e.GetPosition(Image1).Y / ImageHeight ;
                if (XPos >= 0 && XPos <= 1 && YPos >= 0 && YPos <= 1)
                {
                    Title = "Location: (" + ((int)(XPos*ImageWidth)).ToString() + " - " + ((int)(YPos * ImageHeight)).ToString() + ")";
                    return;
                }
            }
            XPos = -1;
            YPos = -1;
            Title = "PlotDigitizer";
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Im_Loaded)
            {
                Canvas1.Children.Clear();
                //Resize the image
                Canvas1.Children.Add(Image1);
                Image1.Width = Canvas1.ActualWidth;
                Image1.Height = Canvas1.ActualHeight;
                Image1.Source = bitmap;
                Image1.Stretch = Stretch.Uniform;
                Canvas1.UpdateLayout();
                ImageWidth = (int)Image1.ActualWidth;
                ImageHeight = (int)Image1.ActualHeight;
                ImgXOffset = (Canvas1.ActualWidth - Image1.ActualWidth) / 2.0;
                ImgYOffset = (Canvas1.ActualHeight - Image1.ActualHeight) / 2.0;
                //Redraw the Axes
                DrawXAxis();
                DrawYAxis();
                Canvas1.Children.Add(Xaxis);
                Canvas1.Children.Add(Yaxis);
                //Redraw the Datapoints
                for (int I = 0; I < DataPos.Count; I++)
                {
                    DrawDataPoint(DataPos[I]);
                }
                //Update the Axes position Textboxes
                TxtXOriginX.Text = ((int)(XOrigin.X * ImageWidth)).ToString();
                TxtXOriginY.Text = ((int)(XOrigin.Y * ImageHeight)).ToString();
                TxtXMaxX.Text = ((int)(XMax.X * ImageWidth)).ToString();
                TxtXMaxY.Text = ((int)(XMax.Y * ImageHeight)).ToString();
                TxtYOriginX.Text = ((int)(YOrigin.X * ImageWidth)).ToString();
                TxtYOriginY.Text = ((int)(YOrigin.Y * ImageHeight)).ToString();
                TxtYMaxX.Text = ((int)(YMax.X * ImageWidth)).ToString();
                TxtYMaxY.Text = ((int)(YMax.Y * ImageHeight)).ToString();
                if (UseImagePix == true )
                {
                    //Update the Axes Values Textboxes
                    TxtXOriginValue.Text = "0";
                    TxtXMaxValue.Text = ImageWidth.ToString();
                    TxtYOriginValue.Text = "0";
                    TxtYMaxValue.Text = ImageHeight.ToString();
                    //Update the Datapoints
                    Data.Clear();
                    TxtDataPointsX.Text = "";
                    TxtDataPointsY.Text = "";
                    for (int I=0; I < DataPos.Count; I++)
                    {
                        Data.Add(new Point(DataPos[I].X * ImageWidth , DataPos[I].Y * ImageHeight));
                        TxtDataPointsX.Text += (DataPos[I].X * ImageWidth).ToString("G4") + "\n";
                        TxtDataPointsY.Text += (DataPos[I].Y * ImageHeight).ToString("G4") + "\n";
                    }
                }
            }
        }

        #endregion

        #region "Menu Events"

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (filepath == "")
            {
                openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            }
            else
            {
                openFileDialog1.InitialDirectory = filepath;
            }
            openFileDialog1.Filter = "Windows Bitmap (*.bmp,*.dib)|*.bmp;*.dib|JPEG (*.jpg,*.jpeg,*.jfif,*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe|TIFF (*.tif,tiff)|*.tif;*.tiff|PNG (*.png)|*.png|GIF (*.gif)| *.gif|All Image files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog().Value == true)
            {
                try
                {
                    Init();
                    bitmap = new BitmapImage(new System.Uri(openFileDialog1.FileName));
                    Image1.Source = bitmap;
                    filepath = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
                    fileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                    Canvas1.UpdateLayout();
                    ImageWidth = (int)Image1.ActualWidth;
                    ImageHeight = (int)Image1.ActualHeight;
                    ImgXOffset = (Canvas1.ActualWidth - Image1.ActualWidth) / 2.0;
                    ImgYOffset = (Canvas1.ActualHeight - Image1.ActualHeight) / 2.0;
                    BtnImgPix.IsEnabled = true;
                    BtnAddData.IsEnabled = true;
                    BtnClearData.IsEnabled = false;
                    UseImagePix = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot open the image file. Original error: " + ex.Message);
                };
            }
        }

        private void MenuSaveData_Click(object sender, RoutedEventArgs e)
        {
            DataExport dex = new DataExport("save");
            dex.GraphTitle = fileName;
            dex.XLabel = "";
            dex.XOrigin = TxtXOriginValue.Text;
            dex.XMax = TxtXMaxValue.Text;
            if (RbXLinear.IsChecked.Value)
            {
                dex.XScale = "Linear";
            }

            else if (RbXLog.IsChecked.Value)
            {
                dex.XScale = "Logarithmic";
            }
            dex.YLabel = "";
            dex.YOrigin = TxtYOriginValue.Text;
            dex.YMax = TxtYMaxValue.Text;
            if (RbYLinear.IsChecked.Value)
            {
                dex.YScale = "Linear";
            }
            else if (RbYLog.IsChecked.Value)
            {
                dex.YScale = "Logarithmic";
            }

            dex.Data = Data;
            dex.ShowDialog();
        }

        private void MenuCopyData_Click(object sender, RoutedEventArgs e)
        {
            DataExport dex = new DataExport("copy");
            dex.GraphTitle = fileName;
            dex.XLabel = "";
            dex.XOrigin = TxtXOriginValue.Text;
            dex.XMax = TxtXMaxValue.Text;
            if (RbXLinear.IsChecked.Value)
            {
                dex.XScale = "Linear";
            }
            else if (RbXLog.IsChecked.Value)
            {
                dex.XScale = "Logarithmic";
            }
            dex.YLabel = "";
            dex.YOrigin = TxtYOriginValue.Text;
            dex.YMax = TxtYMaxValue.Text;
            if (RbYLinear.IsChecked.Value)
            {
                dex.YScale = "Linear";
            }
            else if (RbYLog.IsChecked.Value)
            {
                dex.YScale = "Logarithmic";
            }

            dex.Data = Data;
            dex.ShowDialog();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuReset_Click(object sender, RoutedEventArgs e)
        {
            Init();
        }

        #endregion

        #region "Button Events"

        private void BtnXOrigin_Click(object sender, RoutedEventArgs e)
        {
            My_Action = PlotAction.XOrigin;
            Image1.Cursor = Cursors.Cross;
        }

        private void BtnXMax_Click(object sender, RoutedEventArgs e)
        {
            My_Action = PlotAction.XMax;
            Image1.Cursor = Cursors.Cross;
        }

        private void BtnYOrigin_Click(object sender, RoutedEventArgs e)
        {
            My_Action = PlotAction.YOrigin;
            Image1.Cursor = Cursors.Cross;
        }

        private void BtnYMax_Click(object sender, RoutedEventArgs e)
        {
            My_Action = PlotAction.YMax;
            Image1.Cursor = Cursors.Cross;
        }

        private void BtnAddData_Click(object sender, RoutedEventArgs e)
        {
            bool status = true;
            double dummy;
            Brush col = TxtDataCount.Background;
            if (!XOriginSet)
            {
                status = false;
                TxtXOriginX.Background = Brushes.Red;
                TxtXOriginY.Background = Brushes.Red;
            }
            if (!XMaxSet)
            {
                status = false;
                TxtXMaxX.Background = Brushes.Red;
                TxtXMaxY.Background = Brushes.Red;
            }
            if (!YOriginSet)
            {
                status = false;
                TxtYOriginX.Background = Brushes.Red;
                TxtYOriginY.Background = Brushes.Red;
            }
            if (!YMaxSet)
            {
                status = false;
                TxtYMaxX.Background = Brushes.Red;
                TxtYMaxY.Background = Brushes.Red;
            }
            if (TxtXOriginValue.Text == "" || double.TryParse(TxtXOriginValue.Text, out dummy) == false)
            {
                status = false;
                TxtXOriginValue.Background = Brushes.Red;
            }
            if (TxtXMaxValue.Text == "" || double.TryParse(TxtXMaxValue.Text, out dummy) == false)
            {
                status = false;
                TxtXMaxValue.Background = Brushes.Red;
            }
            if (TxtYOriginValue.Text == "" || double.TryParse(TxtYOriginValue.Text, out dummy) == false)
            {
                status = false;
                TxtYOriginValue.Background = Brushes.Red;
            }
            if (TxtYMaxValue.Text == "" || double.TryParse(TxtYMaxValue.Text, out dummy) == false)
            {
                status = false;
                TxtYMaxValue.Background = Brushes.Red;
            }
            if (!status)
            {
                MessageBox.Show("Provide all missing X and Y axis coördinates and values.", "PlotDigitizer Error.", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtXOriginX.Background = col;
                TxtXOriginY.Background = col;
                TxtXMaxX.Background = col;
                TxtXMaxY.Background = col;
                TxtYOriginX.Background = col;
                TxtYOriginY.Background = col;
                TxtYMaxX.Background = col;
                TxtYMaxY.Background = col;
                TxtXOriginValue.Background = col;
                TxtXMaxValue.Background = col;
                TxtYOriginValue.Background = col;
                TxtYMaxValue.Background = col;
            }
            else
            {
                //Check the X and Y-axis orientation
                if (Math.Abs(XMax.X - XOrigin.X) < 0.05 * Canvas1.ActualWidth && Math.Abs(XMax.Y - XOrigin.Y) > 0.5 * Canvas1.ActualHeight)
                {
                    if (Math.Abs(YMax.Y - YOrigin.Y) < 0.05 * Canvas1.ActualHeight && Math.Abs(YMax.X - YOrigin.X) > 0.5 * Canvas1.ActualWidth)
                    {
                        //X and Y-axis are swapped
                        if (MessageBox.Show("The X-axis and Y-axis appear to be switched.\n" + "The X-axis must be horizontal.\n" + "The Y-axis must be vertical.\n\n" + "Do you want to switch the selected Axes?", "PlotDigitizer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            Point Ptdummy;
                            Ptdummy = XOrigin;
                            XOrigin = YOrigin;
                            YOrigin = Ptdummy;
                            Ptdummy = XMax;
                            XMax = YMax;
                            YMax = Ptdummy;
                            TxtXOriginX.Text = XOrigin.X.ToString();
                            TxtXOriginY.Text = XOrigin.Y.ToString();
                            TxtXMaxX.Text = XMax.X.ToString();
                            TxtXMaxY.Text = XMax.Y.ToString();
                            TxtYOriginX.Text = YOrigin.X.ToString();
                            TxtYOriginY.Text = YOrigin.Y.ToString();
                            TxtYMaxX.Text = YMax.X.ToString();
                            TxtYMaxY.Text = YMax.Y.ToString();
                            if (MessageBox.Show("Do you also want to switch the axis values?", "PlotDigitizer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                string txtdummy;
                                txtdummy = TxtXOriginValue.Text;
                                TxtXOriginValue.Text = TxtYOriginValue.Text;
                                TxtYOriginValue.Text = txtdummy;
                                txtdummy = TxtXMaxValue.Text;
                                TxtXMaxValue.Text = TxtYMaxValue.Text;
                                TxtYMaxValue.Text = txtdummy;
                            }
                        }
                    }
                }
                My_Action = PlotAction.DataPoint;
                Image1.Cursor = Cursors.Cross;
            }
        }

        private void BtnClearData_Click(object sender, RoutedEventArgs e)
        {
            Data.Clear();
            DataPos.Clear();
            for (int I = 0; I < Canvas1.Children.Count; I++)
            {
                if (Canvas1.Children[I].GetType() == typeof(Ellipse))
                {
                    Canvas1.Children.RemoveAt(I);
                    I--;
                }
                if (I >= Canvas1.Children.Count - 1) break;
            }
            TxtDataCount.Text = Data.Count().ToString();
            TxtDataPointsX.Text = "";
            TxtDataPointsY.Text = "";
            My_Action = PlotAction.None;
            Image1.Cursor = Cursors.Arrow;
        }

        private void BtnImgPix_Click(object sender, RoutedEventArgs e)
        {
            XOrigin = new Point(0, 0);
            XMax = new Point(1, 0);
            YOrigin = new Point(0, 0);
            YMax = new Point(0, 1);
            TxtXOriginX.Text = "0";
            TxtXOriginY.Text = "0";
            TxtXMaxX.Text = ImageWidth.ToString();
            TxtXMaxY.Text = "0";
            TxtYOriginX.Text = "0";
            TxtYOriginY.Text = "0";
            TxtYMaxX.Text = "0";
            TxtYMaxY.Text = ImageHeight.ToString();
            TxtXOriginValue.Text = "0";
            TxtXMaxValue.Text = ImageWidth.ToString();
            TxtYOriginValue.Text = "0";
            TxtYMaxValue.Text = ImageHeight.ToString();
            XOriginSet = true;
            XMaxSet = true;
            YOriginSet = true;
            YMaxSet = true;
            Xaxis.X1 = ImgXOffset;
            Xaxis.Y1 = ImgYOffset;
            Xaxis.X2 = ImageWidth + ImgXOffset;
            Xaxis.Y2 = ImgYOffset;
            Yaxis.X1 = ImgXOffset;
            Yaxis.Y1 = ImgYOffset;
            Yaxis.X2 = ImgXOffset;
            Yaxis.Y2 = ImageHeight + ImgYOffset;
            UseImagePix = true;
        }

        #endregion

        #region "Drawing"

        private void Init()
        {
            Canvas1.Children.Clear();
            //Add Image1 and the Axes to the Canvas
            Image1.Width = Canvas1.ActualWidth;
            Image1.Height = Canvas1.ActualHeight;
            Image1.Stretch = Stretch.Uniform;
            Canvas1.Children.Add(Image1);
            Xaxis = new Line()
            {
                X1 = 0.0,
                Y1 = 0.0,
                X2 = 0.0,
                Y2 = 0.0,
                Stroke = Brushes.Red,
                StrokeThickness = 3
            };
            Yaxis = new Line()
            {
                X1 = 0.0,
                Y1 = 0.0,
                X2 = 0.0,
                Y2 = 0.0,
                Stroke = Brushes.Red,
                StrokeThickness = 3
            };
            Canvas1.Children.Add(Xaxis);
            Canvas1.Children.Add(Yaxis);
            //Reset the Axes
            XOrigin = new Point(0, 0);
            XOriginSet = false;
            TxtXOriginX.Text = "";
            TxtXOriginY.Text = "";
            TxtXOriginValue.Text = "";
            XMax = new Point(0, 0);
            XMaxSet = false;
            TxtXMaxX.Text = "";
            TxtXMaxY.Text = "";
            TxtXMaxValue.Text = "";
            YOrigin = new Point(0, 0);
            YOriginSet = false;
            TxtYOriginX.Text = "";
            TxtYOriginY.Text = "";
            TxtYOriginValue.Text = "";
            YMax = new Point(0, 0);
            YMaxSet = false;
            TxtYMaxX.Text = "";
            TxtYMaxY.Text = "";
            TxtYMaxValue.Text = "";
            //Reset the Data points
            Data.Clear();
            DataPos.Clear();
            TxtDataCount.Text = "0";
            TxtDataPointsX.Text = "";
            TxtDataPointsY.Text = "";
            My_Action = PlotAction.None;
            Image1.Cursor = Cursors.Arrow;
        }

        private void DrawXAxis()
        {
            if (XOriginSet && XMaxSet)
            {
                Xaxis.X1 = (XOrigin.X * ImageWidth) + ImgXOffset;
                Xaxis.Y1 = (XOrigin.Y * ImageHeight) + ImgYOffset;
                Xaxis.X2 = (XMax.X * ImageWidth) + ImgXOffset;
                Xaxis.Y2 = (XMax.Y * ImageHeight) + ImgYOffset;
            }
        }

        private void DrawYAxis()
        {
            if (YOriginSet && YMaxSet)
            {
                Yaxis.X1 = (YOrigin.X * ImageWidth) + ImgXOffset;
                Yaxis.Y1 = (YOrigin.Y * ImageHeight) + ImgYOffset;
                Yaxis.X2 = (YMax.X * ImageWidth) + ImgXOffset;
                Yaxis.Y2 = (YMax.Y * ImageHeight) + ImgYOffset;
            }
        }

        private void DrawDataPoint(Point pt)
        {
            Ellipse El = new Ellipse()
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.LightGreen
            };
            El.SetValue(Canvas.LeftProperty, (pt.X * ImageWidth) + ImgXOffset - 4);
            El.SetValue(Canvas.TopProperty, (pt.Y * ImageHeight) + ImgYOffset - 4);
            Canvas1.Children.Add(El);
        }

        #endregion 

    }

    public enum PlotAction
    {
        None = 0,
        XOrigin = 1,
        XMax = 2,
        YOrigin = 3,
        YMax = 4,
        DataPoint = 5
    }
}
