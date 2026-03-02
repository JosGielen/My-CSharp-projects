using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StringArt;

public partial class MainWindow : Window
{
    private int ImgWidth;
    private int ImgHeight;
    private string fileName;
    private BitmapImage bitmap;
    private WriteableBitmap Wbitmap;
    private int Stride;
    private byte[] PixelData;
    private List<Vector> Nails;
    private int StartIndex;
    private int EndIndex;
    private List<int> Indices;
    private Random Rnd = new Random();
    //Parameters
    private readonly int NailCount = 200;
    private readonly double NailSize = 6.0;
    private readonly int MaxLines = 4000;
    private byte LineAlpha = 50;
    private double LineThickness = 0.4;
    private byte BrightStep = 20;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void mnuOpen_Click(object sender, RoutedEventArgs e)
    {
        StreamReader myStream = null;
        OpenFileDialog openFileDialog1 = new OpenFileDialog();
        openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
        openFileDialog1.Multiselect = false;
        openFileDialog1.DefaultExt = ".jpg";
        openFileDialog1.Filter = "Windows Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|TIFF (*.tiff)|*.tiff|PNG (*.png)|*.png";
        openFileDialog1.FilterIndex = 2;
        openFileDialog1.RestoreDirectory = true;
        if (openFileDialog1.ShowDialog().Value)
        {
            fileName = openFileDialog1.FileName;
            bitmap = new BitmapImage(new Uri(fileName));
            ImgWidth = bitmap.PixelWidth;
            ImgHeight = bitmap.PixelHeight;
            //Convert the image to grayscale (1 byte per pixel)
            FormatConvertedBitmap convertBitmap;
            if (bitmap.Format.BitsPerPixel != 8)
            {
                convertBitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Gray8, null, 0);
                Wbitmap = new WriteableBitmap(convertBitmap);
            }
            else
            {
                Wbitmap = new WriteableBitmap(bitmap);
            }
            Stride = Wbitmap.PixelWidth * Wbitmap.Format.BitsPerPixel / 8;
            //Adjust the Canvas to the image size
            Canvas1.Width = ImgWidth;
            Canvas1.Height = ImgHeight;
            Canvas1.UpdateLayout();
            PixelData = new byte[Stride * Wbitmap.PixelHeight];
            Wbitmap.CopyPixels(PixelData, Stride, 0);
            //Create the nails
            Nails = new List<Vector>();
            double angle;
            double R = Canvas1.Width / 2 - 20;
            Vector v;
            Ellipse El;
            for (int i = 0; i < NailCount; i++)
            {
                angle = 2 * Math.PI * i / NailCount;
                v = new Vector(Canvas1.Width / 2 + R * Math.Cos(angle), Canvas1.Height / 2 - R * Math.Sin(angle));
                Nails.Add(v);
                El = new Ellipse()
                {
                    Width = NailSize,
                    Height = NailSize,
                    Fill = Brushes.Black,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.0
                };
                El.SetValue(Canvas.LeftProperty, v.X - NailSize / 2);
                El.SetValue(Canvas.TopProperty, v.Y - NailSize / 2);
                Canvas1.Children.Add(El);
            }
            StartIndex = 0;
            Indices = new List<int>();
            Indices.Add(StartIndex);
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }
    }


    private void CompositionTarget_Rendering(object? sender, EventArgs e)
    {
        for (int I = 0; I < 10; I++)
        {
            if (Indices.Count > MaxLines)
            {
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                Title = "Finished";
                return;
            }
            StartIndex = Rnd.Next(NailCount);
            EndIndex = GetEndIndex(StartIndex);
            if (EndIndex < 0)
            {
                StartIndex = Rnd.Next(NailCount);
                return;
            }
            Indices.Add(EndIndex);
            Line L = new Line()
            {
                X1 = Nails[StartIndex].X,
                Y1 = Nails[StartIndex].Y,
                X2 = Nails[EndIndex].X,
                Y2 = Nails[EndIndex].Y,
                Stroke = new SolidColorBrush(Color.FromArgb(LineAlpha, 0, 0, 0)),
                StrokeThickness = LineThickness
            };
            Canvas1.Children.Add(L);
            UpdatePixelData();
        }
    }

    private int GetEndIndex(int start)
    {
        int End = -1;
        double TotalContrast;
        double MaxContrast = -1;
        int length;   //Number of pixels between Nails[startIndex] and Nails[I] 
        Vector pixel; //Pixel between the 2 nails
        int index = 0; 
        for (int I = 0; I < NailCount; I++)
        {
            if (I != start)
            {
                TotalContrast = 0;
                length = (int)(Nails[I] - Nails[start]).Length;
                for (int J = 0; J < length; J++)
                {
                    pixel = VLerp(Nails[start], Nails[I], J / (double)length);
                    index = (int)(pixel.Y * Stride + pixel.X);
                    TotalContrast += (255 - PixelData[index]);
                }
                TotalContrast /= length;
                if (TotalContrast > MaxContrast) 
                {
                    MaxContrast = TotalContrast;
                    End = I;
                }
            }
        }
        return End;
    }

    private void UpdatePixelData()
    {
        Vector pixel;
        int index = 0;
        int length = (int)(Nails[EndIndex] - Nails[StartIndex]).Length;
        for (int J = 0; J < length; J++)
        {
            pixel = VLerp(Nails[StartIndex], Nails[EndIndex], J / (double)length);
            index = (int)(pixel.Y * Stride + pixel.X);  //Only 1 byte per pixel in 256 gray images
            if (PixelData[index] < 255 - BrightStep) { PixelData[index] += BrightStep; }
        }

    }

    private Vector VLerp(Vector v1, Vector v2, double fraction)
    {
        Vector result = new Vector();
        result.X = (int)(v1.X + fraction * (v2.X - v1.X));
        result.Y = (int)(v1.Y + fraction * (v2.Y - v1.Y));
        return result;
    }

    private void mnuExit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Environment.Exit(0);
    }

}