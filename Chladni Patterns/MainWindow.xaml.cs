using Steering_Behaviors;
using System.Windows;
using System.Windows.Media;

namespace Chladni_Patterns;

public partial class MainWindow : Window
{
    private int N;
    private int M;
    private readonly int count = 6000;
    private List<Agent> Grains;
    private double MaxSpeed = 5.0;
    private double MaxForce = 2.0;
    private readonly Random Rnd = new Random();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        N = 1;
        M = 5;
        Grains = new List<Agent>();
        //Create the grains
        Agent g;
        for (int I = 0; I < count; I++)
        {
            g = new Agent(new Point(Canvas1.ActualWidth * Rnd.NextDouble(), Canvas1.ActualHeight * Rnd.NextDouble()), 1.0, MaxSpeed, MaxForce, Brushes.White)
            {
                Velocity = new Vector( MaxSpeed * (2 * Rnd.NextDouble() - 1), MaxSpeed * (2 * Rnd.NextDouble() - 1)),
                Size = 3.0,
                Breakingdistance = -10.0
            };
            Grains.Add(g);
        }
        //Show the grains
        for (int I = 0; I < count; I++)
        {
            Grains[I].Draw(Canvas1);
        }
        CompositionTarget.Rendering += CompositionTarget_Rendering;
    }

    private void CompositionTarget_Rendering(object? sender, EventArgs e)
    {
        for (int I = 0; I < count; I++)
        {
            double val = 2 * Chladni(Grains[I].Location);
            Vector target = Grains[I].Location;
            if (Math.Abs(val) > 0.05)
            {
                target = Grains[I].Location + val * new Vector(2 * Rnd.NextDouble() - 1, 2 * Rnd.NextDouble() - 1);
            }
            Grains[I].SetTarget(target);
            Grains[I].Update();
            Grains[I].Edges( false);
        }
    }

    private void BtnNUP_Click(object sender, RoutedEventArgs e)
    {
        N = int.Parse(TxtNValue.Text);
        N++;
        if (N == M)
        {
            M++;
            TxtMValue.Text = M.ToString();
        }
        TxtNValue.Text = N.ToString();
        Reset();
    }

    private void BtnNDown_Click(object sender, RoutedEventArgs e)
    {
        N = int.Parse(TxtNValue.Text);
        if (N > 1) { N--; }
        if (N == M) 
        {
            M++;
            TxtMValue.Text = M.ToString();
        }
        TxtNValue.Text = N.ToString();
        Reset();
    }

    private void BtnMUP_Click(object sender, RoutedEventArgs e)
    {
        M = int.Parse(TxtMValue.Text);
        M++;
        if (N == M)
        {
            N++;
            TxtNValue.Text = N.ToString();
        }
        TxtMValue.Text = M.ToString();
        Reset();
    }

    private void BtnMDown_Click(object sender, RoutedEventArgs e)
    {
        M = int.Parse(TxtMValue.Text);
        if (M > 1) { M--; }
        if (N == M)
        {
            N++;
            TxtNValue.Text = N.ToString();
        }
        TxtMValue.Text = M.ToString();
        Reset();
    }

    private void BtnRandom_Click(object sender, RoutedEventArgs e)
    {
        N = Rnd.Next(1, 15);
        TxtNValue.Text = N.ToString();
        do
        {
            M = Rnd.Next(1, 15);
        } while (N == M);
        TxtMValue.Text = M.ToString();
        Reset();
    }

    /// <summary>
    /// Used to dislodge particles from the X = Y diagonal line that occurs in every pattern
    /// </summary>
    private void Reset()
    {
        for (int I = 0; I < count; I++)
        {
            Grains[I].Velocity = new Vector(2 * MaxSpeed * (2 * Rnd.NextDouble() - 1), 2 * MaxSpeed * (2 * Rnd.NextDouble() - 1));
            Grains[I].Update();
        }
    }

    private double Chladni(Vector Loc)
    {
        double NX = N * Math.PI * Loc.X / Canvas1.ActualWidth;
        double MX = M * Math.PI * Loc.X / Canvas1.ActualWidth;
        double NY = N * Math.PI * Loc.Y / Canvas1.ActualHeight;
        double MY = M * Math.PI * Loc.Y / Canvas1.ActualHeight;
        return Math.Cos(NX) * Math.Cos(MY) - Math.Cos(MX) * Math.Cos(NY);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Environment.Exit(0);
    }
}