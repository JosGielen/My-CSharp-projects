using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Steering_Behaviors
{
    class Agent
    {
        private Vector my_Location;
        private Vector my_Velocity;
        private Vector my_Acceleration;
        private double my_Mass;
        private double my_MaxSpeed;
        private double my_MaxForce;
        private Vector my_Target;
        private double my_Size;
        private Brush my_Color;
        private double my_Breakingdistance;
        private Vector my_Force;
        private Ellipse my_Shape;
        private double maxWidth;
        private double maxHeight;

        public Agent(Point location, double mass, double maxSpeed, double maxForce, Brush color)
        {
            my_Location = new Vector(location.X, location.Y);
            my_Velocity = new Vector();
            my_Acceleration = new Vector();
            my_Mass = mass;
            my_MaxSpeed = maxSpeed;
            my_MaxForce = maxForce;
            my_Color = color;
            my_Size = 2.0;
            my_Shape = new Ellipse()
            {
                Stroke = color,
                StrokeThickness = 1.0,
                Fill = color,
                Width = my_Size,
                Height = my_Size
            };
            my_Shape.SetValue(Canvas.LeftProperty, Location.X - my_Size / 2);
            my_Shape.SetValue(Canvas.TopProperty, Location.Y - my_Size / 2);
        }

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

        public double Breakingdistance
        {
            get { return my_Breakingdistance; }
            set { my_Breakingdistance = value; }
        }

        public double Size
        {
            get { return my_Size; }
            set
            {
                my_Size = value;
                my_Shape.Width = my_Size;
                my_Shape.Height = my_Size;
            }
        }

        public Brush Color
        {
            get { return my_Color; }
            set
            {
                my_Color = value;
                my_Shape.Stroke = my_Color;
                my_Shape.Fill = my_Color;
            }
        }

        public void SetTarget(Vector target)
        {
            my_Target = new Vector(target.X, target.Y);
        }

        public void Draw(Canvas c)
        {
            c.Children.Add(my_Shape);
            maxWidth = c.ActualWidth;
            maxHeight = c.ActualHeight;
        }

        public void ApplyForce(Vector force)
        {
            my_Force = my_Force + force;
            if (my_Force.Length > my_MaxForce)
            {
                my_Force.Normalize();
                my_Force = my_MaxForce * my_Force;
            }
        }

        private Vector GetSteeringForce(Vector target)
        {
            Vector DesiredVelocity;
            Vector Steering;
            double maxspeed;
            double dist;
            my_Target = new Vector(target.X, target.Y);
            DesiredVelocity = my_Target - my_Location;
            dist = DesiredVelocity.Length;
            if (dist > my_Breakingdistance)
            {
                maxspeed = my_MaxSpeed;
            }
            else if (dist < 0.5)
            {
                maxspeed = 0;
            }
            else
            {
                maxspeed = (my_MaxSpeed * dist / my_Breakingdistance);
            }
            if (dist > 0) { DesiredVelocity.Normalize(); }
            DesiredVelocity = maxspeed * DesiredVelocity;
            Steering = DesiredVelocity - my_Velocity;
            return Steering;
        }

        public void Update()
        {
            ApplyForce(GetSteeringForce(my_Target));
            my_Acceleration = my_Force / my_Mass;
            my_Velocity += my_Acceleration;
            my_Location += my_Velocity;
            my_Force = 0 * my_Force;
            my_Shape.SetValue(Canvas.LeftProperty, Location.X - my_Size / 2);
            my_Shape.SetValue(Canvas.TopProperty, Location.Y - my_Size / 2);
        }

        public void Edges (bool bounce)
        {
            if (bounce)
            {
                if (my_Location.X < 0)
                {
                    my_Location.X = 0;
                    my_Velocity.X *= -1;
                }
                if (my_Location.X > maxWidth )
                {
                    my_Location.X = maxWidth;
                    my_Velocity.X *= -1;
                }
                if (my_Location.Y < 0)
                {
                    my_Location.Y = 0;
                    my_Velocity.Y *= -1;
                }
                if (my_Location.Y > maxHeight )
                {
                    my_Location.Y = maxHeight;
                    my_Velocity.Y *= -1;
                }
            }
            else
            {
                if (my_Location.X < 0)
                {
                    my_Location.X = maxWidth;
                }
                if (my_Location.X > maxWidth)
                {
                    my_Location.X = 0;
                }
                if (my_Location.Y < 0)
                {
                    my_Location.Y = maxHeight;
                }
                if (my_Location.Y > maxHeight)
                {
                    my_Location.Y = 0;
                }
            }
        }
    }
}
