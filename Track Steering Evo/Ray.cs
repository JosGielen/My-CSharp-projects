using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Track_Steering_Evo
{

    public class Ray
    {
        private Vector my_Pos;
        private Vector my_Dir;
        private Line my_Line;
        private Brush my_Color;
        //private bool ShowRay;
        private double my_Distance;

        public Ray(Vector pos, double angle)
        {
            my_Pos = pos;
            my_Dir = new Vector(Math.Cos(angle * Math.PI / 180), Math.Sin(angle * Math.PI / 180));
            my_Color = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
            my_Line = new Line()
            {
                Stroke = my_Color,
                StrokeThickness = 1.0,
                X1 = my_Pos.X,
                Y1 = my_Pos.Y,
                X2 = (my_Pos + my_Dir).X,
                Y2 = (my_Pos + my_Dir).Y
            };
        }

        public Vector Pos
        {
            get { return my_Pos; }
            set { my_Pos = value; }
        }

        public Vector Dir
        {
            get { return my_Dir; }
            set { my_Dir = value; }
        }

        public double X1
        {
            get { return my_Pos.X; }
        }

        public double Y1
        {
            get { return my_Pos.Y; }
        }

        public double X2
        {
            get { return (my_Pos + my_Dir).X; }
            set { my_Line.X2 = value; }
        }

        public double Y2
        {
            get { return (my_Pos + my_Dir).Y; }
            set { my_Line.Y2 = value; }
        }

        public double Distance
        {
            get { return my_Distance; }
            set { my_Distance = value; }
        }

        public Brush RayColor
        {
            get { return my_Color; }
            set 
            { 
                my_Color = value;
                my_Line.Stroke = my_Color;
            }
        }

        public double RayThickness
        {
            get { return my_Line.StrokeThickness; }
            set { my_Line.StrokeThickness = value; }
        }

        public void Show(Canvas c)
        {
            c.Children.Add(my_Line);
        }

        public void Remove(Canvas c)
        {
            c.Children.Remove(my_Line);
        }

        public void Update(Vector Pos, double angle)
        {
            my_Pos = Pos;
            my_Dir = new Vector(Math.Cos(angle * Math.PI / 180), Math.Sin(angle * Math.PI / 180));
            my_Line.X1 = my_Pos.X;
            my_Line.Y1 = my_Pos.Y;
            my_Line.X2 = (my_Pos + my_Dir).X;
            my_Line.Y2 = (my_Pos + my_Dir).Y;
        }

    }
}
