using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Hexagon_Truchet_Tiles
{
    class Hexagon
    {
        private double x;
        private double y;
        private double size;
        private double angle;
        private double endAngle;
        private bool isRotating;
        private int type;
        private Brush RotateColor;
        private Brush ArcColor;
        private Point[] corners = new Point[6];
        private Point[] midPoints = new Point[6];
        private Path my_Path;
        private PathGeometry PGeo;
        private PathFigure[] Pfigs = new PathFigure[3];
        private LineSegment line;
        private ArcSegment arc;
        private RotateTransform RT = new RotateTransform();

        public Hexagon(double x, double y, double size, int type)
        {
            this.x = x;
            this.y = y;
            this.size = size;
            this.angle = 0.0;
            this.endAngle = 0.0;
            this.isRotating = false;
            this.type = type;
            this.RotateColor = Brushes.Black;
            this.ArcColor = Brushes.Black;
            Init();
        }

        public Brush rotateColor
        {
            get { return RotateColor; }
            set
            {
                RotateColor = value;
            }
        }

        public Brush arcColor
        {
            get { return ArcColor; }
            set 
            { 
                ArcColor = value; 
                my_Path.Stroke = ArcColor;
            }
        }

        public bool Rotating
        {
            get { return isRotating; }
            set { isRotating = value; }
        }

        private void Init()
        {
            RT.CenterX = this.x;
            RT.CenterY = this.y;
            //Create the Hexagon points
            for (int i = 0; i < 6; i++)
            {
                corners[i] = new Point(x + size * Math.Cos(i * Math.PI / 3), y + size * Math.Sin(i * Math.PI / 3));
            }
            for (int i = 0; i < 6; i++)
            {
                midPoints[i] = PointLerp(corners[i], corners[(i + 1) % 6], 0.5);
            }
            //Create the arcs
            switch (type)
            {
                case 0:
                    //Vertical line in the middle
                    Pfigs[0] = new PathFigure()
                    {
                        StartPoint = midPoints[1]
                    };
                    line = new LineSegment();
                    line.Point = midPoints[4];
                    Pfigs[0].Segments.Add(line);
                    //Small arc at corner 0
                    Pfigs[1] = new PathFigure()
                    {
                        StartPoint = midPoints[0]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Clockwise,
                        Point = midPoints[5],
                        Size = new Size(0.5 * size, 0.5 * size),
                    };
                    Pfigs[1].Segments.Add(arc);
                    //Small arc at corner 3
                    Pfigs[2] = new PathFigure()
                    {
                        StartPoint = midPoints[2]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Counterclockwise,
                        Point = midPoints[3],
                        Size = new Size(0.5 * size, 0.5 * size),
                    };
                    Pfigs[2].Segments.Add(arc);
                    break;
                case 1:
                    //Small arc at corner 0
                    Pfigs[0] = new PathFigure()
                    {
                        StartPoint = midPoints[0]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Clockwise,
                        Point = midPoints[5],
                        Size = new Size(0.5 * size, 0.5 * size),
                    };
                    Pfigs[0].Segments.Add(arc);
                    //Small arc at corner 2
                    Pfigs[1] = new PathFigure()
                    {
                        StartPoint = midPoints[1]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Counterclockwise,
                        Point = midPoints[2],
                        Size = new Size(0.5 * size, 0.5 * size),
                    };
                    Pfigs[1].Segments.Add(arc);
                    //Small arc at corner 4
                    Pfigs[2] = new PathFigure()
                    {
                        StartPoint = midPoints[3]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Counterclockwise,
                        Point = midPoints[4],
                        Size = new Size(0.5 * size, 0.5 * size),
                    };
                    Pfigs[2].Segments.Add(arc);
                    break;
                case 2:
                    //Vertical line in the middle
                    Pfigs[0] = new PathFigure()
                    {
                        StartPoint = midPoints[1]
                    };
                    line = new LineSegment();
                    line.Point = midPoints[4];
                    Pfigs[0].Segments.Add(line);
                    //Large arc at midpoint 1
                    Pfigs[1] = new PathFigure()
                    {
                        StartPoint = midPoints[0]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Counterclockwise,
                        Point = midPoints[2],
                        Size = new Size(1.5 * size, 1.5 * size),
                    };
                    Pfigs[1].Segments.Add(arc);
                    //Large arc at midpoint 4
                    Pfigs[2] = new PathFigure()
                    {
                        StartPoint = midPoints[3]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Counterclockwise,
                        Point = midPoints[5],
                        Size = new Size(1.5 * size, 1.5 * size),
                    };
                    Pfigs[2].Segments.Add(arc);
                    break;
                case 3:
                    //Small arc at corner 0
                    Pfigs[0] = new PathFigure()
                    {
                        StartPoint = midPoints[0]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Clockwise,
                        Point = midPoints[5],
                        Size = new Size(0.5 * size, 0.5 * size),
                    };
                    Pfigs[0].Segments.Add(arc);
                    //Large arc at midpoint 2
                    Pfigs[1] = new PathFigure()
                    {
                        StartPoint = midPoints[1]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Counterclockwise,
                        Point = midPoints[3],
                        Size = new Size(1.5 * size, 1.5 * size),
                    };
                    Pfigs[1].Segments.Add(arc);
                    //Large arc at midpoint 3
                    Pfigs[2] = new PathFigure()
                    {
                        StartPoint = midPoints[2]
                    };
                    arc = new ArcSegment()
                    {
                        SweepDirection = SweepDirection.Counterclockwise,
                        Point = midPoints[4],
                        Size = new Size(1.5 * size, 1.5 * size),
                    };
                    Pfigs[2].Segments.Add(arc);
                    break;
                default:
                    Pfigs[0] = new PathFigure();
                    Pfigs[1] = new PathFigure();
                    Pfigs[2] = new PathFigure();
                    break;
            }
            //Add the PathFigures to the PathGeometry
            PGeo = new PathGeometry();
            for (int i = 0; i < 3; i++)
            {
                PGeo.Figures.Add(Pfigs[i]);
            }
            //Add the PathGeometry to the Path
            my_Path = new Path
            {
                Data = PGeo,
                Stroke = ArcColor,
                StrokeThickness = 3,
                RenderTransform = RT
            };
        }

        private Point PointLerp(Point p1, Point p2, double factor)
        {
            Point result = new Point();
            result.X = p1.X + (p2.X - p1.X) * factor;
            result.Y = p1.Y + (p2.Y - p1.Y) * factor;
            return result;
        }

        public void Draw(Canvas canv)
        {
            canv.Children.Add(my_Path);
        }

        public void StartRotate()
        {
            endAngle = angle + 60;
            isRotating = true;
            my_Path.Stroke = RotateColor;
        }

        public void Update()
        {
            angle += 0.5;
            RT.Angle = angle;
            if (angle >= endAngle)
            {
                angle = endAngle;
                isRotating= false;
                my_Path.Stroke = ArcColor;
            }
        }
    }
}
