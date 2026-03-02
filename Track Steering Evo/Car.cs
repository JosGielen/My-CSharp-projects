using JG_NeuralNet;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Track_Steering_Evo
{
    public class Car
    {
        private Vector my_Pos;
        private double my_Angle;
        private Vector my_Dir;
        private double my_Speed;
        private double my_TurnSpeed;
        private double my_MaxTurn;
        private int my_FOV;
        private double my_Size;
        private Rectangle my_Dot;
        private NeuralNet my_Brain;
        private bool my_Alive;
        private double my_Fitness;
        private int my_Fuel;
        private int my_CurrentCheckpoint;
        private int my_NextCheckPoint;
        private bool my_Finished;
        private RotateTransform rotT;
        private List<Ray> my_Rays;
        private bool my_ShowRays;
        private Ray[] my_Sensors = new Ray[5];
        private double my_SteeringAngle;
        private DateTime my_StartTime;
        private double my_FinishTime;
        private static Random Rnd = new Random();

        public Car(Vector pos, double angle, int fov, int raycount)
        {
            my_Pos = pos;
            my_Angle = angle;
            my_Size = 10.0;
            rotT = new RotateTransform(angle);
            rotT.CenterX = my_Size;
            rotT.CenterY = my_Size / 2;
            my_Dir = new Vector(Math.Cos(angle * Math.PI / 180), Math.Sin(angle * Math.PI / 180));
            my_FOV = fov;
            my_Alive = true;
            my_Speed = 0.0;
            my_TurnSpeed = 0.0;
            my_MaxTurn = 0.0;
            my_Fitness = 0.0;
            my_Fuel = 50;
            my_CurrentCheckpoint = 0;
            my_NextCheckPoint = 1;
            my_Finished = false;
            my_ShowRays = false;
            my_StartTime = DateTime.Now;
            my_FinishTime = double.MaxValue;
        //Create the FOV Rays
        my_Rays = new List<Ray>();
            Ray r;
            double rayAngle = my_Angle - my_FOV / 2.0;
            for (int I = 0; I < raycount; I++)
            {
                r = new Ray(my_Pos + my_Size * my_Dir, rayAngle);
                my_Rays.Add(r);
                rayAngle += my_FOV / (raycount - 1.0);
            }
            //Create the sensor Rays
            for (int I = 0; I < 5; I++)
            {
                r = new Ray(my_Pos + my_Size * my_Dir, my_Angle + (I - 2) * my_FOV / 2.0);
                r.RayColor = Brushes.Red;
                r.RayThickness = 2.0;
                my_Sensors[I] = r;
            }
        }

        public Vector Pos
        {
            get { return my_Pos; }
            set { my_Pos = value; }
        }

        public Vector Dir
        {
            get { return my_Dir; }
        }

        public int FOV
        {
            get { return my_FOV; }
            set { my_FOV = value; }
        }

        public double Size
        {
            get { return my_Size; }
            set { my_Size = value; }
        }

        public NeuralNet Brain
        {
            get { return my_Brain; }
        }

        public DateTime StartTime
        {
            get { return my_StartTime; } 
            set { my_StartTime = value; }
        }

        public double FinishTime
        {
            get { return my_FinishTime; }
            set { my_FinishTime = value; }
        }

        public double Fitness
        {
            get { return my_Fitness; }
            set { my_Fitness = value; }
        }

        public bool Alive
        {
            get { return my_Alive; }
            set { my_Alive = value; }
        }

        public double Angle
        {
            get { return my_Angle; }
            set { my_Angle = value; }
        }

        public List<Ray> Rays
        {
            get { return my_Rays; }
        }

        public Ray[] Sensors
        {
            get { return my_Sensors; }
        }

        public double Speed
        {
            get { return my_Speed; }
            set { my_Speed = value; }
        }

        public double TurnSpeed
        {
            get { return my_TurnSpeed; }
            set { my_TurnSpeed = value; }
        }

        public int Fuel
        {
            get { return my_Fuel; }
            set { my_Fuel = value; }
        }

        public double MaxTurn
        {
            get { return my_MaxTurn; }
            set { my_MaxTurn = value; }
        }

        public double SteeringAngle
        {
            get { return my_SteeringAngle; }
        }

        public bool Finished
        {
            get { return my_Finished; }
        }

        public int CurrentCheckpoint
        {
            get { return my_CurrentCheckpoint; }
        }

        public int NextCheckPoint
        {
            get { return my_NextCheckPoint; }
        }

        public void ShowRays(Canvas c)
        {
            my_ShowRays = true;
            for (int I = 0; I < my_Rays.Count; I++)
            {
                my_Rays[I].Show(c);
            }
            for (int I = 0; I < my_Sensors.Length; I++)
            {
                my_Sensors[I].Show(c);
            }
        }

        //TODO : Show small car sprite
        public void Show(Canvas c)
        {
            my_Dot = new Rectangle()
            {
                Width = 2 * my_Size,
                Height = my_Size,
                Stroke = Brushes.Black,
                StrokeThickness = 1.0,
                Fill = Brushes.Red
            };
            my_Dot.SetValue(Canvas.LeftProperty, my_Pos.X - my_Size);
            my_Dot.SetValue(Canvas.TopProperty, my_Pos.Y - my_Size / 2);
            my_Dot.RenderTransform = rotT;
            c.Children.Add(my_Dot);
        }

        public void Remove(Canvas c)
        {
            c.Children.Remove(my_Dot);
            for (int I = 0; I < my_Rays.Count; I++)
            {
                my_Rays[I].Remove(c);
            }
            for (int I = 0; I < 5; I++)
            {
                my_Sensors[I].Remove(c);
            }
        }

        public void SetBrain(NeuralNet NN)
        {
            my_Brain = NN.Copy();
        }

        public void Mutate(double mutateRate, double mutateFactor)
        {
            if (Rnd.NextDouble() < mutateRate / 100)
            {
                if (Rnd.NextDouble() > 0.5)
                {
                    my_Speed *= (1 + mutateFactor / 100);
                }
                else
                {
                    my_Speed *= (1 - mutateFactor / 100);
                }
            }
            if (Rnd.NextDouble() < mutateRate / 100)
            {
                if (Rnd.NextDouble() > 0.5)
                {
                    my_TurnSpeed *= (1 + mutateFactor / 100);
                    if (my_TurnSpeed > my_MaxTurn) { my_TurnSpeed = my_MaxTurn; }
                }
                else
                {
                    my_TurnSpeed *= (1 - mutateFactor / 100);
                    if (my_TurnSpeed < 0.1) { my_TurnSpeed = 0.1; }

                }
            }
            my_Brain.MutateIncrement(mutateRate, mutateFactor, true);
        }

        public void Scan(List<Wall> walls, double trackWidth)
        {
            //Get the Distance of each Sensor Ray to the Track Wall
            double dist;
            double mindist;
            Vector intPt;
            Vector closestPt;
            for (int I = 0; I < my_Sensors.Length; I++)
            {
                mindist = double.MaxValue;
                closestPt = new Vector(-1, -1);
                //Find the closest wall intersect
                for (int J = 0; J < walls.Count; J++)
                {
                    intPt = walls[J].Intersect(my_Sensors[I]);
                    if (intPt.X >= 0 && intPt.Y >= 0)
                    {
                        dist = (my_Sensors[I].Pos - intPt).Length;
                        if (dist < mindist)
                        {
                            mindist = dist;
                            closestPt = intPt;
                        }
                    }
                }
                //End the ray at the closest intersect
                if (my_ShowRays)
                {
                    my_Sensors[I].X2 = closestPt.X;
                    my_Sensors[I].Y2 = closestPt.Y;
                }
                my_Sensors[I].Distance = trackWidth / mindist;
                if (mindist < my_Speed + my_Size / 2)
                {
                    Alive = false;
                }
            }
        }

        public void Think()
        {
            //Use the sensor distances as inputs to determine steering left or right.
            double[] inputs = new double[5];
            double[] output;
            double SumDistance = 0.0;
            for (int I = 0; I < 5; I++)
            {
                SumDistance += my_Sensors[I].Distance;
            }
            for (int I = 0; I < 5; I++)
            {
                inputs[I] = my_Sensors[I].Distance / SumDistance;
            }
            output = my_Brain.Query(inputs);
            if (my_Brain.OutputNodes == 1)
            {
                //Single output NN;
                my_SteeringAngle = my_TurnSpeed * (output[0] - 0.5);
            }
            else if (my_Brain.OutputNodes == 2)
            {
                //Double output NN;
                my_SteeringAngle = my_TurnSpeed * (output[0] - output[1]);
            }
            else if (my_Brain.OutputNodes == 3)
            {
                //Triple output NN;
                my_Speed = output[2] * 3.0;
                my_SteeringAngle = my_TurnSpeed * (output[0] - output[1]);
            }
            else
            {
                throw new Exception("Invalide Neural Net Format");
            }
            my_Angle += my_SteeringAngle;
            my_Dir = new Vector(Math.Cos(my_Angle * Math.PI / 180), Math.Sin(my_Angle * Math.PI / 180));
            rotT.Angle = my_Angle;
        }

        public void Update(List<Vector> checkpoints, double trackWidth, int Laps)
        {
            Pos = Pos + my_Speed * Dir;
            my_Fuel -= 1;
            if ((my_Pos - checkpoints[my_NextCheckPoint]).Length < trackWidth / 2)
            {
                my_Fitness += 1;
                my_Fuel += 50;
                my_CurrentCheckpoint = (my_CurrentCheckpoint + 1) % checkpoints.Count;
                my_NextCheckPoint = (my_NextCheckPoint + 1) % checkpoints.Count;
                if (my_Fitness == Laps * checkpoints.Count + 1)
                {
                    my_Finished = true;
                    my_FinishTime = (DateTime.Now - my_StartTime).TotalSeconds;
                    my_Alive = false;
                }
            }
            my_Dot.SetValue(Canvas.LeftProperty, my_Pos.X - my_Size);
            my_Dot.SetValue(Canvas.TopProperty, my_Pos.Y - my_Size / 2);
            double rayAngle = my_Angle - my_FOV / 2.0;
            for (int I = 0; I < my_Rays.Count; I++)
            {
                my_Rays[I].Update(my_Pos + my_Size * my_Dir, rayAngle);
                rayAngle += my_FOV / (my_Rays.Count - 1.0);
            }
            for (int I = 0; I < 5; I++)
            {
                my_Sensors[I].Update(my_Pos + my_Size * my_Dir, my_Angle + (I - 2) * my_FOV / 2.0);
            }
        }

        public Car Copy()
        {
            Car result = new Car(my_Pos, my_Angle, my_FOV, my_Rays.Count);
            result.SetBrain(my_Brain.Copy());
            result.Speed = my_Speed;
            result.TurnSpeed = my_TurnSpeed;
            result.MaxTurn = my_MaxTurn;
            return result;
        }

        public void SaveCar(string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            try
            {
                if (sw != null)
                {
                    //Write the Car configuration to the file
                    sw.WriteLine(my_Speed.ToString());
                    sw.WriteLine(my_TurnSpeed.ToString());
                    sw.Close();
                    my_Brain.SaveToFile(filename, true); //Appends the NeuralNet partameters to this file
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Cannot save the Car data. Original error: " + Ex.Message, "Track Steering Evo error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }
        }

        public void LoadCar(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            try
            {
                //Read the Car configuration from the file
                my_Speed = double.Parse(sr.ReadLine());
                my_TurnSpeed = double.Parse(sr.ReadLine());
                my_Brain = new NeuralNet(1, 1, 1,0.1, false);
                my_Brain.GetDataFromStream(sr); //Reads the NeuralNet partameters from the open StreamReader
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Cannot Load the Car data. Original error: " + Ex.Message, "Track Steering Evo error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }
            }
        }

        public void SaveNN(string filename)
        {
            my_Brain.SaveToFile(filename);
        }

        public void LoadNN(string file)
        {
            my_Brain = NeuralNet.LoadFromFile(file);
        }
    }
}
