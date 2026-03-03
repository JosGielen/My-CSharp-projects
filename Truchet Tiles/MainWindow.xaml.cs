using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Truchet_Tiles;

public partial class MainWindow : Window
{
    private Size TileSize;
    private int Cols = 20;
    private int Rows = 20;
    private Random Rnd = new Random();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        TileSize = new Size(canvas1.ActualWidth / Cols, canvas1.ActualHeight / Rows);
        for (int I = 0; I <= Cols; I++)
        {
            for (int J = 0; J <= Rows; J++)
            {
                DrawTriangle(TileSize.Width * I, TileSize.Height * J, Rnd.Next(4), false);
            }
        }
    }

    private void RbType_Click(object sender, RoutedEventArgs e)
    {
        if (RbTriangles.IsChecked == true )
        {
            canvas1.Children.Clear();
            for (int I = 0; I <= Cols; I++)
            {
                for (int J = 0; J <= Rows; J++)
                {
                    DrawTriangle(TileSize.Width * I, TileSize.Height * J, Rnd.Next(4), false);
                }
            }
        }
        else if (RbArcs.IsChecked == true)
        {
            canvas1.Children.Clear();
            for (int I = 0; I <= Cols; I++)
            {
                for (int J = 0; J <= Rows; J++)
                {
                    DrawArc(TileSize.Width * I, TileSize.Height * J, Rnd.Next(2), false);
                }
            }
        }
        else if (RbLines.IsChecked == true)
        {
            canvas1.Children.Clear();
            for (int I = 0; I <= Cols; I++)
            {
                for (int J = 0; J <= Rows; J++)
                {
                    DrawDiagonal(TileSize.Width * I, TileSize.Height * J, Rnd.Next(2), false);
                }
            }
        }
    }

    private void DrawTriangle(double x, double y, int Orientation, bool UseColor)
    {
        Polygon poly = new Polygon();
        Brush br = Brushes.Black;
        switch (Orientation)
        {
            case 0:
                poly.Points.Add(new Point(TileSize.Width, 0));
                poly.Points.Add(new Point(TileSize.Width, TileSize.Height));
                poly.Points.Add(new Point(0, TileSize.Height));
                if (UseColor)
                {
                    br = Brushes.Red;
                }
                break;
            case 1:
                poly.Points.Add(new Point(TileSize.Width, TileSize.Height));
                poly.Points.Add(new Point(0, TileSize.Height));
                poly.Points.Add(new Point(0, 0));
                if (UseColor)
                {
                    br = Brushes.Blue;
                }
                break;
            case 2:
                poly.Points.Add(new Point(0, TileSize.Height));
                poly.Points.Add(new Point(0, 0));
                poly.Points.Add(new Point(TileSize.Width, 0));
                if (UseColor)
                {
                    br = Brushes.Yellow;
                }
                break;
            case 3:
                poly.Points.Add(new Point(0, 0));
                poly.Points.Add(new Point(TileSize.Width, 0));
                poly.Points.Add(new Point(TileSize.Width, TileSize.Height));
                if (UseColor)
                {
                    br = Brushes.Lime;
                }
                break;
            default:
                return;
        }
        poly.Stroke = br;
        poly.Fill = br;
        poly.SetValue(Canvas.LeftProperty, x);
        poly.SetValue(Canvas.TopProperty, y);
        canvas1.Children.Add(poly);
    }

    private void DrawArc(double x, double y, int Orientation, bool UseColor)
    {
        Path my_Path = new Path();
        PathGeometry my_PG = new PathGeometry();
        PathFigure my_figure = new PathFigure();
        if (Orientation == 0)
        {
            my_figure.StartPoint = new Point(0.0, TileSize.Height / 2);
            my_figure.Segments.Add(new ArcSegment(new Point(TileSize.Width / 2, 0.0), new Size(TileSize.Width / 2, TileSize.Height / 2), 0.0, false, SweepDirection.Counterclockwise, true));
            my_PG.Figures.Add(my_figure);
            my_figure = new PathFigure();
            my_figure.StartPoint = new Point(TileSize.Width / 2, TileSize.Height);
            my_figure.Segments.Add(new ArcSegment(new Point(TileSize.Width, TileSize.Height / 2), new Size(TileSize.Width / 2, TileSize.Height / 2), 0.0, false, SweepDirection.Clockwise, true));
            my_PG.Figures.Add(my_figure);
        }
        else
        {
            my_figure.StartPoint = new Point(0.0, TileSize.Height / 2);
            my_figure.Segments.Add(new ArcSegment(new Point(TileSize.Width / 2, TileSize.Height), new Size(TileSize.Width / 2, TileSize.Height / 2), 0.0, false, SweepDirection.Clockwise, true));
            my_PG.Figures.Add(my_figure);
            my_figure = new PathFigure();
            my_figure.StartPoint = new Point(TileSize.Width / 2, 0.0);
            my_figure.Segments.Add(new ArcSegment(new Point(TileSize.Width, TileSize.Height / 2), new Size(TileSize.Width / 2, TileSize.Height / 2), 0.0, false, SweepDirection.Counterclockwise, true));
            my_PG.Figures.Add(my_figure);
        }
        my_Path.Data = my_PG;
        my_Path.Stroke = Brushes.Black;
        my_Path.StrokeThickness = TileSize.Width / 10;
        my_Path.SetValue(Canvas.LeftProperty, x);
        my_Path.SetValue(Canvas.TopProperty, y);
        canvas1.Children.Add(my_Path);
    }

    private void DrawDiagonal(double x, double y, int Orientation, bool UseColor)
    {
        Line l = new Line();
        if (Orientation == 0)
        {
            l = new Line()
            {
                X1 = 0.0,
                Y1 = TileSize.Height,
                X2 = TileSize.Width,
                Y2 = 0.0
            };
        }
        else
        {
            l = new Line()
            {
                X1 = 0.0,
                Y1 = 0.0,
                X2 = TileSize.Width,
                Y2 = TileSize.Height
            };
        }
        l.Stroke = Brushes.Black;
        l.StrokeThickness = TileSize.Width / 10;
        l.SetValue(Canvas.LeftProperty, x);
        l.SetValue(Canvas.TopProperty, y);
        canvas1.Children.Add(l);
    }


    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Environment.Exit(0);
    }
}