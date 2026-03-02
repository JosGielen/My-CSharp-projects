using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Belousov_Zhabotinsky
{
    public partial class MainWindow : Window
    {
        private WriteableBitmap writeableBmp;
        private readonly int size = 2;
        private int cols;
        private int rows;
        private double[,,] A;
        private double[,,] B;
        private double[,,] C;
        private double Alpha = 1.15;
        private double Beta = 1.0;
        private double Gamma = 1.0;
        private int p = 0;
        private int q = 1;
        private Random Rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            writeableBmp = BitmapFactory.New((int)UserArea.ActualWidth, (int)UserArea.ActualHeight);
            Image1.Source = writeableBmp;
            cols = writeableBmp.PixelWidth / size + 1;
            rows = writeableBmp.PixelWidth / size + 1;
            A = new double[cols, rows, 2];
            B = new double[cols, rows, 2];
            C = new double[cols, rows, 2];

            for (int I = 0; I < cols; I++)
            {
                for (int J = 0; J < rows; J++)
                {
                    A[I, J, p] = Rnd.NextDouble();
                    B[I, J, p] = Rnd.NextDouble();
                    C[I, J, p] = Rnd.NextDouble();
                }
            }
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            double c_a, anew;
            double c_b, bnew;
            double c_c, cnew;
            int xIndex, yIndex;
            for (int X = 0; X < cols; X++)
            {
                for (int Y = 0; Y < rows; Y++)
                {
                    c_a = 0.0;
                    c_b = 0.0;
                    c_c = 0.0;
                    //Calculate the average concentration in the surrounding cells.
                    for (int i = X - 1; i <= X + 1; i++)
                    {
                        for (int j = Y - 1; j <= Y + 1; j++)
                        {
                            xIndex = (i + cols) % cols;
                            yIndex = (j + rows) % rows;

                            c_a += A[xIndex, yIndex, p];
                            c_b += B[xIndex, yIndex, p];
                            c_c += C[xIndex, yIndex, p];
                        }
                    }
                    c_a /= 9.0;
                    c_b /= 9.0;
                    c_c /= 9.0;
                    //Calculate the new concentrations
                    anew = c_a + c_a * (Alpha * c_b - Gamma * c_c);
                    bnew = c_b + c_b * (Beta * c_c - Alpha * c_a);
                    cnew = c_c + c_c * (Gamma * c_a - Beta * c_b);
                    //Constrain the concentrations between 0 and 1.
                    if (anew < 0.0) { anew = 0.0; }
                    if (anew > 1.0) { anew = 1.0; }
                    if (bnew < 0.0) { bnew = 0.0; }
                    if (bnew > 1.0) { bnew = 1.0; }
                    if (cnew < 0.0) { cnew = 0.0; }
                    if (cnew > 1.0) { cnew = 1.0; }
                    A[X, Y, q] = anew;
                    B[X, Y, q] = bnew;
                    C[X, Y, q] = cnew;
                }
            }
            //Update the output fields
            if (p == 0)
            {
                p = 1; q = 0;
            }
            else
            {
                p = 0; q = 1;
            }
            //Draw the Afield1
            using (writeableBmp.GetBitmapContext())
            {
                byte r, g, b;
                writeableBmp.Clear();
                for (int I = 0; I < cols; I++)
                {
                    for (int J = 0; J < rows; J++)
                    {
                        r = Lerp(220, 150, A[I, J, p]);
                        g = Lerp(90, 210, A[I, J, p]);
                        b = Lerp(0, 255, A[I, J, p]);

                        writeableBmp.FillRectangle(I * size, J * size, (I + 1) * size, (J + 1) * size, Color.FromRgb(r,g,b));
                    }
                }
            }
            Thread.Sleep(100);
        }

        private byte Lerp(byte a, byte b, double factor)
        {
            return (byte)(a + (b - a) * factor);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
