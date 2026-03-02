
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Track_Steering_Evo
{
    public class Wall
    {
        private Vector my_Pt1;
        private Vector my_Pt2;
        private Line my_Line;
        private Brush my_Color;

        public Wall(Vector end1, Vector end2, Brush color)
        {
            my_Pt1 = end1;
            my_Pt2 = end2;
            my_Color = color;
        }

        public Brush WallColor
        {
            get { return my_Color; }
            set { my_Color = value; }
        }

        public void Show(Canvas c)
        {
            my_Line = new Line()
            {
                Stroke = my_Color,
                StrokeThickness = 5.0,
                X1 = my_Pt1.X,
                Y1 = my_Pt1.Y,
                X2 = my_Pt2.X,
                Y2 = my_Pt2.Y
            };
            c.Children.Add(my_Line);
        }

        public Vector Intersect(Ray r)
        {
            Vector result = new Vector();
            double nom = (my_Pt1.X - my_Pt2.X) * (r.Y1 - r.Y2) - (my_Pt1.Y - my_Pt2.Y) * (r.X1 - r.X2);
            double t = ((my_Pt1.X - r.X1) * (r.Y1 - r.Y2) - (my_Pt1.Y - r.Y1) * (r.X1 - r.X2)) / nom;
            double u = -((my_Pt1.X - my_Pt2.X) * (my_Pt1.Y - r.Y1) - (my_Pt1.Y - my_Pt2.Y) * (my_Pt1.X - r.X1)) / nom;
            if (t > 0 && t < 1)
            {
                if (u > 0)
                {
                    result.X = my_Pt1.X + t * (my_Pt2.X - my_Pt1.X);
                    result.Y = my_Pt1.Y + t * (my_Pt2.Y - my_Pt1.Y);
                    return result;
                }
            }
            return new Vector(-1, -1);
        }
    }
}
