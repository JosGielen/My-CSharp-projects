using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Differential_Line_Growth
{
    class Agent
    {
        private Vector my_Location;
        private Vector my_Velocity;
        private Vector my_Acceleration;
        private double my_MaxSpeed;
        private double my_MaxForce;
        private Shape my_Shape;
        private Size my_Size;
        private Brush my_FillColor;
        private Brush my_LineColor;
        private double my_LineThickness;

        public Agent(Vector location, Shape shape)
        {
            my_Location = location;
            my_Velocity = new Vector();
            my_Acceleration = new Vector();
            my_MaxSpeed = 1.0;
            my_MaxForce = 1.0;
            my_Size = new Size(1.0, 1.0);
            my_FillColor = Brushes.White;
            my_LineColor = Brushes.Black;
            my_LineThickness = 1.0;
            my_Shape = shape;
            my_Shape.SetValue(Canvas.LeftProperty, my_Location.X - my_Size.Width / 2);
            my_Shape.SetValue(Canvas.TopProperty, my_Location.Y - my_Size.Height / 2);
        }

        #region "Properties"

        public Vector Location
        {
            get { return my_Location; }
            set { my_Location = value; }
        }

        public Vector Velocity
        {
            get { return my_Velocity; }
            set { my_Velocity = value; }
        }

        public Size Size
        {
            get { return my_Size; }
            set
            {
                my_Size = value;
                my_Shape.Width = my_Size.Width;
                my_Shape.Height = my_Size.Height;
            }
        }

        public Shape Shape
        {
            get { return my_Shape; }
            set { my_Shape = value; }
        }

        public Brush Fill
        {
            get { return my_FillColor; }
            set
            {
                my_FillColor = value;
                my_Shape.Fill = my_FillColor;
            }
        }

        public Brush Stroke
        {
            get { return my_LineColor; }
            set
            {
                my_LineColor = value;
                my_Shape.Stroke = my_LineColor;
            }
        }

        public double StrokeThickness
        {
            get { return my_LineThickness; }
            set
            {
                my_LineThickness = value;
                my_Shape.StrokeThickness = my_LineThickness;
            }
        }

        public double MaxSpeed
        {
            get { return my_MaxSpeed; }
            set { my_MaxSpeed = value; }
        }

        public double MaxForce
        {
            get { return my_MaxForce; }
            set { my_MaxForce = value; }
        }

        #endregion 

        public void Draw(Canvas c)
        {
            c.Children.Add(my_Shape);
        }

        public void Update(List<Agent> nodes, List<Agent> neighbors)
        {
            Vector separation = Separation(neighbors);
            Vector cohesion = Cohesion(nodes);
            my_Acceleration += separation;
            my_Acceleration += cohesion;
            my_Velocity += my_Acceleration;
            if (my_Velocity.Length > my_MaxSpeed)
            {
                my_Velocity.Normalize();
                my_Velocity *= my_MaxSpeed;
            }
            my_Location += my_Velocity;
            my_Acceleration *= 0;
            my_Shape.SetValue(Canvas.LeftProperty, my_Location.X - my_Shape.Width);
            my_Shape.SetValue(Canvas.TopProperty, my_Location.Y - my_Shape.Width);
        }

        public void checkBorders(Canvas c)
        {
            double maxWidth = c.ActualWidth;
            double maxHeight = c.ActualHeight;
            if (my_Location.X < my_Shape.Width)
            {
                my_Location.X = my_Shape.Width + 2;
                my_Velocity.X = -1 * my_Velocity.X;
            }
            if (my_Location.X > maxWidth - my_Shape.Width)
            {
                my_Location.X = maxWidth - my_Shape.Width - 2;
                my_Velocity.X = -1 * my_Velocity.X;
            }
            if (my_Location.Y < my_Shape.Height)
            {
                my_Location.Y = my_Shape.Height + 2;
                my_Velocity.Y = -1 * my_Velocity.Y;
            }
            if (my_Location.Y > maxHeight - my_Shape.Height)
            {
                my_Location.Y = maxHeight - my_Shape.Height - 2;
                my_Velocity.Y = -1 * my_Velocity.Y;
            }
            my_Shape.SetValue(Canvas.LeftProperty, my_Location.X - my_Shape.Width);
            my_Shape.SetValue(Canvas.TopProperty, my_Location.Y - my_Shape.Width);
        }

        private Vector Separation(List<Agent> neighbors)
        {
            //Get the net Separation force due to the distance to other nodes
            Vector Steering = new Vector();
            Vector diff;
            double dist;
            int total = 0;
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (this != neighbors[i])
                {
                    diff = my_Location - neighbors[i].Location;
                    dist = diff.Length;
                    diff /= dist * dist;
                    Steering += diff;
                    total++;
                }
            }
            if (total > 0)
            {
                Steering /= total;
                Steering.Normalize();
                Steering *= my_MaxSpeed;
                Steering -= my_Velocity;
                if (Steering.Length > my_MaxForce)
                {
                    Steering.Normalize();
                    Steering *= my_MaxForce;
                }
            }
            return Steering;
        }

        private Vector Cohesion(List<Agent> nodes)
        {
            //Get the net Cohesion force between adjacent nodes
            Vector Steering = new Vector();
            int total = 0;
            int thisIndex = nodes.IndexOf(this);
            int nextIndex = (thisIndex + 1) % nodes.Count;
            int prevIndex = (thisIndex - 1 + nodes.Count) % nodes.Count;
            Steering += nodes[nextIndex].Location;
            Steering += nodes[prevIndex].Location;
            total += 2;
            if (total > 0)
            {
                Steering /= total;
                Steering -= my_Location;
                Steering.Normalize();
                Steering *= my_MaxSpeed;
                Steering -= my_Velocity;
                if (Steering.Length > my_MaxForce)
                {
                    Steering.Normalize();
                    Steering *= my_MaxForce;
                }
            }
            return Steering;
        }
    }
}

