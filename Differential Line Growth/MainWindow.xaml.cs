using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Differential_Line_Growth
{
    public partial class MainWindow : Window
    {
        private List<Agent> nodes = new List<Agent>();
        private double insertDist = 5;
        private double SeparationDist;
        private double agentSize = 6;
        private QTree qt;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SeparationDist = 5 * insertDist;
            //Create 10 starting nodes in a circle
            Agent node;
            double angle;
            double radius = 150;
            Ellipse agentShape;
            for (int i = 0; i < 10; i++)
            {
                angle = Math.PI / 5 * i;
                agentShape = new Ellipse()
                {
                    Width = agentSize,
                    Height = agentSize,
                };
                node = new Agent(new Vector(canvas1.ActualWidth / 2 + radius * Math.Cos(angle), canvas1.ActualHeight / 2 + radius * Math.Sin(angle)), agentShape)
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2.0,
                    Fill = Brushes.Black,
                };
                nodes.Add(node);
            }
            //Create a QuadTree
            qt = new QTree(new Rect(new Point(0, 0), new Point(canvas1.ActualWidth, canvas1.ActualHeight)), 10);
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            List<Agent> neighbors = new List<Agent>();
            //Draw a line between adjacent nodes
            Line l;
            canvas1.Children.Clear();
            for (int i = 0; i < nodes.Count; i++)
            {
                Vector N1 = nodes[i].Location;
                Vector N2 = nodes[(i + 1)%nodes.Count].Location;
                l = new Line()
                {
                    X1 = N1.X,
                    Y1 = N1.Y,
                    X2 = N2.X,
                    Y2 = N2.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2.0
                };
                canvas1.Children.Add(l);
            }
            //Insert new nodes when there is enough space between 2 adjacent nodes
            Insert();
            //Fill the QuadTree
            qt.Clear();
            for (int i = 0; i < nodes.Count; i++)
            {
                qt.Insert(nodes[i].Location, nodes[i]);
            }
            //Calculate the steering forces on each node
            for (int i = 0; i < nodes.Count; i++)
            {
                //Use the QuadTree to get neighboring nodes.
                neighbors.Clear();
                neighbors = qt.QueryObjects<Agent>(nodes[i].Location, SeparationDist);
                nodes[i].Update(nodes, neighbors);
                nodes[i].checkBorders(canvas1);
            }
        }

        private int Insert()
        {
            int insetCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                Vector v = (nodes[(i + 1) % nodes.Count].Location - nodes[i].Location);
                if (v.Length > insertDist)
                {
                    Agent node;
                    Ellipse agentShape;
                    agentShape = new Ellipse()
                    {
                        Width = agentSize,
                        Height = agentSize,
                    };
                    node = new Agent(nodes[i].Location + 0.5 * v, agentShape)
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2.0,
                        Fill = Brushes.Black
                    };
                    nodes.Insert(i + 1, node);
                    insetCount++;
                }
            }
            return insetCount;
        }
    }
}