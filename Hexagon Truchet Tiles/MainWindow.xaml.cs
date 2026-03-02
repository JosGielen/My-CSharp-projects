using System.Windows;
using System.Windows.Media;

namespace Hexagon_Truchet_Tiles
{
    public partial class MainWindow : Window
    {
        private Hexagon[,] Hexagons;
        private Brush RotateColor = Brushes.Red;
        private Brush arcColor = Brushes.Black;
        private double size = 40;
        private int cols;
        private int rows;
        private int waitSteps;
        private int maxWaitSteps = 100;
        private readonly Random Rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double xStep = 1.5 * size;
            double yStep = Math.Sqrt(3) * size;
            cols = (int)Math.Floor(canvas1.ActualWidth / xStep) + 1;
            rows = (int)Math.Floor(canvas1.ActualHeight / yStep + 1);
            double x;
            double y;
            //Create a Hexagon tile pattern
            Hexagons = new Hexagon[cols, rows];
            for (int i = 0; i < cols; i++)
            {
                x = i * xStep;
                for (int j = 0; j < rows; j++)
                {
                    if (i % 2 == 0)
                    {
                        y = (j+0.5) * yStep;
                    }
                    else
                    {
                        y = j * yStep;
                    }
                    Hexagons[i, j] = new Hexagon(x, y, size, Rnd.Next(4))
                    {
                        arcColor = arcColor,
                        rotateColor = RotateColor
                    };
                    Hexagons[i, j].Draw(canvas1);
                }
            }
            waitSteps = 1;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(Object? sender, EventArgs e)
        {
            //Skip frames to reduce the amount of rotating Hexagons
            waitSteps--;
            if (waitSteps == 0)
            {
                for (int j = 0; j < rows; j++)
                {
                    for (int i = 0; i < cols; i++)
                    {
                        if (Rnd.NextDouble() < 0.05)
                        {
                            if (!Hexagons[i, j].Rotating)
                            {
                                Hexagons[i, j].StartRotate();
                            }
                        }   
                    }
                }
                waitSteps = maxWaitSteps;
            }
            //Update the rotating Hexagons
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (Hexagons[i, j].Rotating)
                    {
                        Hexagons[i, j].Update();
                    }
                }
            }
        }
    }
}