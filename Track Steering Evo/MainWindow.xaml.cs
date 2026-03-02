using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JG_NeuralNet;
using System.Diagnostics;

namespace Track_Steering_Evo
{
    public partial class MainWindow : Window
    {
        private bool Rendering = false;
        private bool GotTrack = false;
        private bool GotCars = false;
        private Random Rnd = new Random();
        //Car parameters
        private List<Car> my_Cars;
        private int PopulationSize = 100;
        private double CarStartSpeed = 1.5;
        private double CarStartTurnSpeed = 15;
        private double CarMaxTurn = 30.0;
        private int FOV = 70;
        private int RayCount;
        private Car BestCar;
        //Track parameters
        private double TrackAngleStep = 6.0;
        private double MaxTrackRadius = 0.0;
        private double TrackTwistingFactor = 0.3; //Determines how fast the Track Diameter changes
        private double MaxTwistingFactor = 1.3;
        private double TrackWidth = 80.0;
        private Vector StartPoint;
        private double StartAngle;
        private List<Vector> Checkpoints;
        private int LapCount = 1;
        private List<Wall> my_Walls;
        private Polygon InnerPolygon;
        private Polygon OuterPolygon;
        //Neural Net Parameters
        private int HiddenLayerNodes = 10;
        private int OutputNodes = 3;
        private double MutationRate = 5; //Percent chance that a Car property or weight in the NeuralNet changes
        private double MutationFactor = 20; //Percentage that a Car property or weight in the NeuralNet can change (+ or -)
        //First Person View Parameters
        private List<Line> WallLines;
        private double FirstPersonViewScale = 45;
        private Image Speedo;
        private Label lblSpeed;
        private Line SpeedLine;
        private Line WheelSpoke1;
        private Line WheelSpoke2;
        private Line WheelSpoke3;
        private Line WheelSpoke4;
        private Ellipse WheelKnob;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(Object sender, RoutedEventArgs e)
        {
            RayCount = (int)(canvas2.ActualWidth);
            TrackAngleStep = TrackAngleStep * Math.PI / 180;
            MaxTrackRadius = canvas1.ActualHeight / 2.1;
            //Set the ground and sky in the First Person View
            Rectangle ground = new Rectangle()
            {
                Width = canvas2.ActualWidth,
                Height = canvas2.ActualHeight / 2,
                Fill = new SolidColorBrush(Color.FromRgb(50, 50, 50))
            };
            ground.SetValue(Canvas.LeftProperty, 0.0);
            ground.SetValue(Canvas.TopProperty, canvas2.ActualHeight / 2);
            canvas2.Children.Add(ground);
            Rectangle sky = new Rectangle()
            {
                Width = canvas2.ActualWidth,
                Height = canvas2.ActualHeight / 2,
                Fill = Brushes.LightBlue
            };
            sky.SetValue(Canvas.LeftProperty, 0.0);
            sky.SetValue(Canvas.TopProperty, 0.0);
            canvas2.Children.Add(sky);
            //Set the color of the Area inside the Track
            InnerPolygon = new Polygon()
            {
                Fill = Brushes.Green
            };
            //Set the color of the Track
            OuterPolygon = new Polygon()
            {
                Fill = new SolidColorBrush(Color.FromRgb(50, 50, 50))
            };
            //Make the Vertical Lines for the First Person View
            WallLines = new List<Line>();
            Line wl;
            for (int I = 0; I <= RayCount; I++)
            {
                wl = new Line()
                {
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = 2.0,
                    X1 = I,
                    Y1 = 0.0,
                    X2 = I,
                    Y2 = canvas2.ActualHeight
                };
                WallLines.Add(wl);
                canvas2.Children.Add(wl);
            }
            //Show a Speedometer
            Speedo = new Image()
            {
                Width = canvas2.ActualHeight / 3,
                Height = canvas2.ActualHeight / 3,
            };
            Speedo.SetValue(Canvas.LeftProperty, (canvas2.ActualWidth - Speedo.Width) / 2);
            Speedo.SetValue(Canvas.TopProperty, canvas2.ActualHeight - Speedo.Height + 10);
            Speedo.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "//Speedo.jpg"));
            lblSpeed = new Label()
            {
                Foreground = Brushes.White,
                Background = Brushes.Black,
                Width = Speedo.Width,
                Height = 25,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalContentAlignment= HorizontalAlignment.Center,
                Content = "0.00"
            };
            lblSpeed.SetValue(Canvas.LeftProperty, (canvas2.ActualWidth - lblSpeed.Width) / 2);
            lblSpeed.SetValue(Canvas.TopProperty, canvas2.ActualHeight - lblSpeed.Height);
            SpeedLine = new Line()
            {
                Stroke = Brushes.Red,
                StrokeThickness = 3.0,
                X1 = canvas2.ActualWidth / 2,
                Y1 = canvas2.ActualHeight - Speedo.Height / 3,
                X2 = canvas2.ActualWidth / 2 + (Speedo.Width / 2.5) * Math.Cos(160 * Math.PI / 180),
                Y2 = canvas2.ActualHeight - Speedo.Height / 3 + (Speedo.Width / 2.5) * Math.Sin(160 * Math.PI / 180)
            };
            //Show a SteeringWheel
            WheelSpoke1 = new Line()
            {
                X1 = canvas2.ActualWidth / 2,
                Y1 = canvas2.ActualHeight,
                X2 = canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos(160 * Math.PI / 180),
                Y2 = canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin(160 * Math.PI / 180),
                Stroke = Brushes.Gray,
                StrokeThickness = 15.0
            };
            WheelSpoke2 = new Line()
            {
                X1 = canvas2.ActualWidth / 2,
                Y1 = canvas2.ActualHeight,
                X2 = canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos(120 * Math.PI / 180),
                Y2 = canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin(120 * Math.PI / 180),
                Stroke = Brushes.Gray,
                StrokeThickness = 15.0
            };
            WheelSpoke3 = new Line()
            {
                X1 = canvas2.ActualWidth / 2,
                Y1 = canvas2.ActualHeight,
                X2 = canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos(60 * Math.PI / 180),
                Y2 = canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin(60 * Math.PI / 180),
                Stroke = Brushes.Gray,
                StrokeThickness = 15.0
            };
            WheelSpoke4 = new Line()
            {
                X1 = canvas2.ActualWidth / 2,
                Y1 = canvas2.ActualHeight,
                X2 = canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos(20 * Math.PI / 180),
                Y2 = canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin(20 * Math.PI / 180),
                Stroke = Brushes.Gray,
                StrokeThickness = 15.0
            };
            Ellipse wheel = new Ellipse()
            {
                Width = canvas2.ActualWidth / 2.5,
                Height = canvas2.ActualWidth / 2.5,
                Stroke = Brushes.Gray,
                StrokeThickness = 10.0,
                Fill = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)),  
            };
            wheel.SetValue(Canvas.LeftProperty, canvas2.ActualWidth / 2 - canvas2.ActualWidth / 5.0);
            wheel.SetValue(Canvas.TopProperty, canvas2.ActualHeight - canvas2.ActualWidth / 5.0);
            WheelKnob = new Ellipse()
            {
                Width = 15,
                Height = 15,
                Stroke = Brushes.Red,
                StrokeThickness = 1.0,
                Fill = Brushes.Red,
            };
            WheelKnob.SetValue(Canvas.LeftProperty, canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos(90 * Math.PI / 180));
            WheelKnob.SetValue(Canvas.TopProperty, canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin(90 * Math.PI / 180));

            canvas2.Children.Add(wheel);
            canvas2.Children.Add(WheelSpoke1);
            canvas2.Children.Add(WheelSpoke2);
            canvas2.Children.Add(WheelSpoke3);
            canvas2.Children.Add(WheelSpoke4);
            canvas2.Children.Add(WheelKnob);
            canvas2.Children.Add(Speedo);
            canvas2.Children.Add(SpeedLine);
            canvas2.Children.Add(lblSpeed);
            //Set the default parameters
            TxtPopSize.Text = PopulationSize.ToString();
            TxtVehicleSpeed.Text = CarStartSpeed.ToString();
            TxtVehicleTurn.Text = CarMaxTurn.ToString();
            TxtVehicleFOV.Text = FOV.ToString();
            TxtTrackTwist.Text = TrackTwistingFactor.ToString();
            TxtTrackWidth.Text = TrackWidth.ToString();
            TxtTrackLaps.Text = LapCount.ToString();
            TxtNNHidden.Text = HiddenLayerNodes.ToString();
            TxtNNOutput.Text = OutputNodes.ToString();
            TxtNNMutationRate.Text = MutationRate.ToString();
            TxtNNMutationChange.Text = MutationFactor.ToString();
            MakeTrack();
            GotCars = false;
        }

        public void MakeCars()
        {
            //Make the first Generation of Vehicles
            my_Cars = new List<Car>();
            Car veh;
            for (int N = 0; N < PopulationSize; N++)
            {
                veh = new Car(StartPoint, StartAngle, FOV, RayCount);
                veh.Speed = CarStartSpeed * (0.5 + 0.5*Rnd.NextDouble());
                veh.TurnSpeed = CarStartTurnSpeed;
                veh.MaxTurn = CarMaxTurn;
                veh.SetBrain(new NeuralNet(5, HiddenLayerNodes, OutputNodes, 0.3, false));
                veh.Show(canvas1);
                my_Cars.Add(veh);
            }
            my_Cars[0].ShowRays(canvas1);
            TxtCarSpeed.Text = my_Cars[0].Speed.ToString("F2");
            TxtCarTurn.Text = my_Cars[0].TurnSpeed.ToString("F2");
            GotCars = true;
        }

        private void MakeTrack()
        {
            List<Vector> InnerPoints = new List<Vector>();
            List<Vector> OuterPoints = new List<Vector>();
            Checkpoints = new List<Vector>();
            Brush WallColor = Brushes.DarkKhaki;
            Wall w;
            double XC = 0.0;
            double YC = 0.0;
            double XOff = Rnd.NextDouble();
            double YOff = Rnd.NextDouble();
            double R = 0.0;
            double X = 0.0;
            double Y = 0.0;
            //Generate the track CheckPoints and Walls corner points
            for (double Angle = 0; Angle <= 2 * Math.PI; Angle += TrackAngleStep)
            {
                XC = TrackTwistingFactor * Math.Cos(Angle);
                YC = TrackTwistingFactor * Math.Sin(Angle);
                R = MaxTrackRadius * (0.4 + 0.5 * FastSimplexNoise.Noise2D(XC + XOff, YC + YOff)) + 80;
                X = R * Math.Cos(Angle) + canvas1.ActualWidth / 2;
                Y = R * Math.Sin(Angle) + canvas1.ActualHeight / 2;
                OuterPoints.Add(new Vector(X, Y));
                R -= TrackWidth / 2;
                X = R * Math.Cos(Angle) + canvas1.ActualWidth / 2;
                Y = R * Math.Sin(Angle) + canvas1.ActualHeight / 2;
                Checkpoints.Add(new Vector(X, Y));
                R -= TrackWidth / 2;
                X = R * Math.Cos(Angle) + canvas1.ActualWidth / 2;
                Y = R * Math.Sin(Angle) + canvas1.ActualHeight / 2;
                InnerPoints.Add(new Vector(X, Y));
            }
            StartPoint = Checkpoints[0];
            StartAngle = -1 * Vector.AngleBetween(Checkpoints[1] - Checkpoints[0], new Vector(1, 0));
            canvas1.Children.Clear();
            //Make the Track walls between the Walls corner points
            my_Walls = new List<Wall>();
            for (int I = 0; I < InnerPoints.Count - 1; I++)
            {
                if (I % 2 == 0)
                {
                    w = new Wall(InnerPoints[I], InnerPoints[I + 1], Brushes.LightGray);
                }
                else
                {
                    w = new Wall(InnerPoints[I], InnerPoints[I + 1], Brushes.DarkGray);
                }
                my_Walls.Add(w);
            }
            my_Walls.Last().WallColor = Brushes.Yellow;
            for (int I = 0; I < OuterPoints.Count - 1; I++)
            {
                if (I % 2 == 0)
                {
                    w = new Wall(OuterPoints[I], OuterPoints[I + 1], Brushes.Brown);
                }
                else
                {
                    w = new Wall(OuterPoints[I], OuterPoints[I + 1], Brushes.DarkRed);
                }
                my_Walls.Add(w);
            }
            my_Walls.Last().WallColor = Brushes.Yellow;
            //Show the track area
            OuterPolygon.Points.Clear();
            for (int I = 0; I < OuterPoints.Count; I++)
            {
                OuterPolygon.Points.Add(new Point(OuterPoints[I].X, OuterPoints[I].Y));
            }
            canvas1.Children.Add(OuterPolygon);
            //Show the area inside the track
            InnerPolygon.Points.Clear();
            for (int I = 0; I < InnerPoints.Count; I++)
            {
                InnerPolygon.Points.Add(new Point(InnerPoints[I].X, InnerPoints[I].Y));
            }
            canvas1.Children.Add(InnerPolygon);
            //Show the walls
            for (int I = 0; I < my_Walls.Count; I++)
            {
                my_Walls[I].Show(canvas1);
            }
            //Show the FinishLine
            w = new Wall(InnerPoints.Last(), OuterPoints.Last(), Brushes.Yellow);
            w.Show(canvas1);
            GotTrack = true;
        }

        private void CompositionTarget_Rendering(Object sender, EventArgs e)
        {
            int LiveCounter = 0;
            int FinishCounter = 0;
            int Index = 0;
            Brush WallColor = Brushes.White;
            double dist;
            double mindist;
            Vector intPt;
            Vector closestPt;
            double WallLineHeight;
            LiveCounter = 0;
            for (int N = 0; N < my_Cars.Count; N++)
            {
                if (my_Cars[N].Finished) { FinishCounter += 1; }
                if (my_Cars[N].Alive)
                {
                    LiveCounter += 1;
                    my_Cars[N].Update(Checkpoints, TrackWidth, LapCount);
                    if (my_Cars[N].Fuel <= 0) { my_Cars[N].Alive = false; }
                    if (N == 0)
                    {
                        //Show the First Person Vieuw of Car[0] (= last best Car);
                        for (int I = 0; I < my_Cars[N].Rays.Count; I++)
                        {
                            //Get the Wall intersects of all the Rays
                            mindist = double.MaxValue;
                            closestPt = new Vector(-1, -1);
                            for (int J = 0; J < my_Walls.Count; J++)
                            {
                                intPt = my_Walls[J].Intersect(my_Cars[N].Rays[I]);
                                if (intPt.X >= 0 && intPt.Y >= 0)
                                {
                                    dist = (my_Cars[N].Rays[I].Pos - intPt).Length;
                                    if (dist < mindist)
                                    {
                                        mindist = dist;
                                        closestPt = intPt;
                                        WallColor = my_Walls[J].WallColor;
                                    }
                                }
                            }
                            //End the ray at the closest intersect Vector
                            my_Cars[N].Rays[I].X2 = closestPt.X;
                            my_Cars[N].Rays[I].Y2 = closestPt.Y;
                            //Set the wall height in the First Person View of the best Car
                            double rayAngleOffset = Vector.AngleBetween(my_Cars[N].Rays[I].Dir, my_Cars[N].Dir) * Math.PI / 180;
                            WallLineHeight = FirstPersonViewScale * canvas2.ActualHeight / (mindist * Math.Abs(Math.Cos(rayAngleOffset)));
                            if (WallLineHeight > canvas2.ActualHeight) { WallLineHeight = canvas2.ActualHeight; }
                            WallLines[I].Y1 = (canvas2.ActualHeight - WallLineHeight) / 2;
                            WallLines[I].Y2 = (canvas2.ActualHeight + WallLineHeight) / 2;
                            WallLines[I].Stroke = WallColor;
                        }
                        //Show the Best Car Parameters
                        TxtCarTurn.Text = my_Cars[N].TurnSpeed.ToString("F2");
                        TxtCarSpeed.Text = my_Cars[N].Speed.ToString("F2");
                        double SpeedAngle;
                        if (my_Cars[N].Alive)
                        {
                            SpeedAngle = 160 + 220 * my_Cars[N].Speed / 3.0;
                        }
                        else
                        {
                            SpeedAngle = 160;
                        }
                        SpeedLine.X2 = canvas2.ActualWidth / 2 + (Speedo.Width / 2.5) * Math.Cos(SpeedAngle * Math.PI / 180);
                        SpeedLine.Y2 = canvas2.ActualHeight - Speedo.Height / 3 + (Speedo.Width / 2.5) * Math.Sin(SpeedAngle * Math.PI / 180);
                        lblSpeed.Content = my_Cars[N].Speed.ToString("F2");
                        double SteerAngle = 50 * my_Cars[N].SteeringAngle;
                        WheelSpoke1.X2 = canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos((160 - SteerAngle) * Math.PI / 180);
                        WheelSpoke1.Y2 = canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin((160 - SteerAngle) * Math.PI / 180);
                        WheelSpoke2.X2 = canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos((120 - SteerAngle) * Math.PI / 180);
                        WheelSpoke2.Y2 = canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin((120 - SteerAngle) * Math.PI / 180);
                        WheelSpoke3.X2 = canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos((60 - SteerAngle) * Math.PI / 180);
                        WheelSpoke3.Y2 = canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin((60 - SteerAngle) * Math.PI / 180);
                        WheelSpoke4.X2 = canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos((20 - SteerAngle) * Math.PI / 180);
                        WheelSpoke4.Y2 = canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin((20 - SteerAngle) * Math.PI / 180);
                        WheelKnob.SetValue(Canvas.LeftProperty, canvas2.ActualWidth / 2 + (canvas2.ActualWidth / 5.0) * Math.Cos((90 - SteerAngle) * Math.PI / 180));
                        WheelKnob.SetValue(Canvas.TopProperty, canvas2.ActualHeight - (canvas2.ActualWidth / 5.0) * Math.Sin((90 - SteerAngle) * Math.PI / 180));
                    }
                    //Let the Car move autonomous
                    my_Cars[N].Scan(my_Walls, TrackWidth);
                    my_Cars[N].Think();
                }
            }
            if (LiveCounter == 0)
            {
                if (FinishCounter > 10)
                {
                    //Increase the Track difficulty
                    TrackTwistingFactor += 0.1;
                    if (TrackTwistingFactor > MaxTwistingFactor) { TrackTwistingFactor = MaxTwistingFactor; }
                    TxtTrackTwist.Text = TrackTwistingFactor.ToString();
                    MakeTrack();
                }
                //Make the next population of Cars
                List<Car> OldCars = new List<Car>();
                //  Get the normalized fitness of each Car
                double sumFitness = 0.0;
                double TrackLength = 2 * Math.PI * MaxTrackRadius;
                for (int I = 0; I < my_Cars.Count; I++)
                {
                    my_Cars[I].Fitness += TrackWidth / (my_Cars[I].Pos - Checkpoints[my_Cars[I].NextCheckPoint]).Length;
                    if (my_Cars[I].Finished)
                    {
                        my_Cars[I].Fitness += TrackLength / my_Cars[I].FinishTime;
                    }
                    sumFitness += my_Cars[I].Fitness;
                }
                for (int I = 0; I < my_Cars.Count; I++)
                {
                    my_Cars[I].Fitness = my_Cars[I].Fitness / sumFitness;
                }
                //  Get the best Car
                double maxFitness = -10.0;
                int BestIndex = 0;
                for (int I = 0; I < my_Cars.Count; I++)
                {
                    if (my_Cars[I].Fitness > maxFitness)
                    {
                        maxFitness = my_Cars[I].Fitness;
                        BestIndex = I;
                    }
                }
                BestCar = my_Cars[BestIndex].Copy();
                //  Copy all the Cars into a selection pool (= OldCars)
                for (int I = 0; I < my_Cars.Count; I++)
                {
                    OldCars.Add(my_Cars[I].Copy());
                    OldCars[I].Fitness = my_Cars[I].Fitness;
                }
                //  Remove all Cars from the field
                for (int I = 0; I < my_Cars.Count; I++)
                {
                    my_Cars[I].Remove(canvas1);
                }
                //  Pick random OldCars to form the next Generation
                //  (Generic Algorithm where the pickchance = fitness);
                double r = 0.0;
                if (BestCar != null) //Make shure to Keep the Best Car
                {
                    my_Cars[0] = BestCar.Copy();
                }
                else
                {
                    my_Cars[0] = OldCars[0].Copy();
                }
                my_Cars[0].Pos = StartPoint;
                my_Cars[0].Angle = StartAngle;
                my_Cars[0].StartTime = DateTime.Now;
                my_Cars[0].Show(canvas1);
                my_Cars[0].ShowRays(canvas1);
                for (int I = 1; I < OldCars.Count; I++)
                {
                    r = Rnd.NextDouble();
                    Index = 0;
                    while (r > 0)
                    {
                        r = r - OldCars[Index].Fitness;
                        Index += 1;
                    }
                    Index -= 1;
                    my_Cars[I] = OldCars[Index].Copy();
                    //Mutate some of the new Cars
                    if (Rnd.Next(100) > MutationRate) { my_Cars[I].Mutate(MutationRate, MutationFactor); }
                    my_Cars[I].Pos = StartPoint;
                    my_Cars[I].Angle = StartAngle;
                    my_Cars[I].StartTime = DateTime.Now;
                    my_Cars[I].Show(canvas1);
                }
            }
            else
            {
                Title = LiveCounter.ToString() + " Raytracing RaceCars left in the Race.";
            }
        }


        private void SaveImage(string filename)
        {
            BitmapEncoder MyEncoder = new JpegBitmapEncoder();
            RenderTargetBitmap renderbmp = new RenderTargetBitmap((int)(ActualWidth), (int)(ActualHeight), 96.0, 96.0, PixelFormats.Default);
            renderbmp.Render(this);
            try
            {
                MyEncoder.Frames.Add(BitmapFrame.Create(renderbmp));
                // Create a FileStream to write the image to the file.
                using FileStream sw = new FileStream(filename, FileMode.Create);
                {
                    MyEncoder.Save(sw);
                }
            }
            catch
            {
                Exception ex;
                MessageBox.Show("The Image could not be saved.", "TrackSteeringEvo error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!Rendering)
            {
                ReadSettings();
                if (!GotTrack) { MakeTrack(); }
                if (!GotCars) { MakeCars(); }
                //Set the StartTime for each Car
                for (int I = 0; I < my_Cars.Count; I++)
                {
                    my_Cars[I].StartTime = DateTime.Now;
                }
                CompositionTarget.Rendering += CompositionTarget_Rendering;
                BtnStart.Content = "PAUSE";
                Rendering = true;
            }
            else
            {
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                BtnStart.Content = "START";
                Rendering = false;
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            BtnStart.Content = "START";
            Rendering = false;
            if (my_Cars != null)
            {
                for (int I = 0; I < my_Cars.Count; I++)
                {
                    my_Cars[I].Remove(canvas1);
                }
            }
            GotTrack = false;
            GotCars = false;
        }

        private void ReadSettings()
        {
            //Read the settings
            try
            {
                PopulationSize = int.Parse(TxtPopSize.Text);
                CarStartSpeed = double.Parse(TxtVehicleSpeed.Text);
                CarMaxTurn = double.Parse(TxtVehicleTurn.Text);
                FOV = int.Parse(TxtVehicleFOV.Text);
                TrackTwistingFactor = double.Parse(TxtTrackTwist.Text);
                if (TrackTwistingFactor < 0) { TrackTwistingFactor = 0; }
                if (TrackTwistingFactor > MaxTwistingFactor) { TrackTwistingFactor = MaxTwistingFactor; }
                TrackWidth = double.Parse(TxtTrackWidth.Text);
                LapCount = int.Parse(TxtTrackLaps.Text);
                HiddenLayerNodes = int.Parse(TxtNNHidden.Text);
                OutputNodes = int.Parse(TxtNNOutput.Text);
                MutationRate = double.Parse(TxtNNMutationRate.Text);
                MutationFactor = double.Parse(TxtNNMutationChange.Text);
            }
            catch
            {
                return;
            }
        }

        private void MnuLoad_Click(Object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            //Show an OpenFile dialog
            OFD.InitialDirectory = Environment.CurrentDirectory;
            OFD.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;
            if (OFD.ShowDialog() == true)
            {
                if (my_Cars == null)
                {
                    my_Cars = new List<Car>();
                    for (int I = 0; I < PopulationSize; I++)
                    {
                        my_Cars.Add(new Car(StartPoint, 0.0, FOV, RayCount));
                    }
                }
                else
                {
                    //Remove all vehicles from the field
                    for (int I = 0; I < my_Cars.Count; I++)
                    {
                        my_Cars[I].Remove(canvas1);
                    }
                }
                ReadSettings();
                //Load the Car 
                my_Cars[0] = new Car(StartPoint, StartAngle, FOV, RayCount);
                my_Cars[0].LoadCar(OFD.FileName);
                my_Cars[0].Pos = StartPoint;
                my_Cars[0].Angle = StartAngle;
                my_Cars[0].MaxTurn = CarMaxTurn;
                my_Cars[0].Show(canvas1);
                for (int I = 1; I < my_Cars.Count; I++)
                {
                    my_Cars[I] = my_Cars[0].Copy();
                    if (Rnd.Next(100) > MutationRate) { my_Cars[I].Mutate(MutationRate, MutationFactor); }
                    my_Cars[I].Pos = StartPoint;
                    my_Cars[I].Angle = StartAngle;
                    my_Cars[I].MaxTurn = CarMaxTurn;
                    my_Cars[I].Show(canvas1);
                }
                GotCars = true;
            }
        }

        private void MnuSaveBest_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.InitialDirectory = Environment.CurrentDirectory;
            SFD.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            SFD.FilterIndex = 1;
            SFD.RestoreDirectory = true;
            if (SFD.ShowDialog() == true)
            {
                if (BestCar != null) { BestCar.SaveCar(SFD.FileName); }
            }
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}