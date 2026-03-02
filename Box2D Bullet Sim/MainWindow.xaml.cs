using Box2D.NET;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
//using static Box2D.NET.B2Joints;
//using static Box2D.NET.B2Ids;
//using static Box2D.NET.B2Geometries;
//using static Box2D.NET.B2Types;
//using static Box2D.NET.B2MathFunction;
//using static Box2D.NET.B2Bodies;
//using static Box2D.NET.B2Shapes;

namespace Box2D_Test
{
    public partial class MainWindow : Window
    {
        private List<Box> Target;
        private Vector targetCenter;
        private double targetSize = 250;   //Diameter of the target
        private double targetResolution = 80; //number of particle in the full width of the target
        private Bullet bullet;
        private Size bulletSize = new Size(40, 15);
        private bool fired = false;
        //Box2D World settings
        private B2WorldId worldId;
        private float timeStep = 1f / 120.0f;
        private int subStepCount = 4;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Target = new List<Box>();
            //Create a Box2D World
            B2WorldDef worldDef = B2Types.b2DefaultWorldDef();
            worldDef.gravity = new B2Vec2(0.0f, 0.0f);
            worldId = B2Worlds.b2CreateWorld(worldDef);
            Init();
            Title = "Bullet simulation with " + Target.Count.ToString() + " boxes.";
        }

        private void Init()
        {
            B2Worlds.b2World_SetGravity(worldId, new B2Vec2(0.0f, 0.0f));
            //Create the table top
            Vector tableLoc = new Vector(canvas1.ActualWidth / 2, canvas1.ActualHeight - 250);
            Size tableSize = new Size(400, 40);
            Box table = new Box(tableLoc, tableSize, true, worldId);
            table.Rect.Fill = Brushes.Black;
            table.Draw(canvas1);
            //Create the table leg
            Vector legLoc = new Vector(canvas1.ActualWidth / 2, canvas1.ActualHeight - 125);
            Size legSize = new Size(40.0, 250.0);
            Box leg = new Box(legLoc, legSize, true, worldId);
            leg.FillColor = Brushes.Black;
            leg.Draw(canvas1);
            //Create the target
            targetCenter = new Vector(tableLoc.X, tableLoc.Y - targetSize / 2 - tableSize.Height / 2);
            Size particleSize = new Size(targetSize / targetResolution, targetSize / targetResolution);
            Vector particleLoc;
            for (int i = 0; i < targetResolution; i++)
            {
                for (int j = 0; j < targetResolution; j++)
                {
                    particleLoc = new Vector(targetCenter.X - targetSize / 2 + i * particleSize.Width, targetCenter.Y - (targetResolution / 2 - j) * particleSize.Height);
                    if ((particleLoc - targetCenter).Length < targetSize / 2)
                    {
                        Box particle = new Box(particleLoc, particleSize, false, worldId);
                        particle.FillColor = Brushes.Red;
                        particle.LineColor = Brushes.Red;
                        particle.Draw(canvas1);
                        Target.Add(particle);
                    }
                }
            }
            //Draw a gun barrel end
            double H = targetCenter.Y;
            Polygon poly = new Polygon();
            poly.Fill = Brushes.Black;
            poly.Points.Add(new Point(0, H - 20));
            poly.Points.Add(new Point(60, H - 20));
            poly.Points.Add(new Point(70, H - 25));
            poly.Points.Add(new Point(70, H - 20));
            poly.Points.Add(new Point(75, H - 20));
            poly.Points.Add(new Point(75, H + 20));
            poly.Points.Add(new Point(0, H + 20));
            poly.Points.Add(new Point(0, H - 20));
            canvas1.Children.Add(poly);
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            //Update the Box2D world
            B2Worlds.b2World_Step(worldId, timeStep, subStepCount);
            //Update all the Boxes
            for (int i = 0; i < Target.Count; i ++)
            {
                Target[i].Update();
            }
            //Update the bullet
            if (fired)
            {
                bullet.Update();
            }
            if (Utilities.Vec2Vector(B2Bodies.b2Body_GetPosition(bullet.my_ID)).X >= targetCenter.X - targetSize / 2)
            {
                B2Worlds.b2World_SetGravity(worldId, new B2Vec2(0.0f, 5.0f));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            B2Worlds.b2DestroyWorld(worldId);
            Environment.Exit(0);
        }

        private void btnFire_Click(object sender, RoutedEventArgs e)
        {
            //Create the bullet
            Vector bulletLoc = new Vector(75.0, targetCenter.Y);
            bullet = new Bullet(bulletLoc, bulletSize, false, worldId);
            bullet.FillColor = Brushes.DarkGray;
            bullet.LineColor = Brushes.Black;
            bullet.Draw(canvas1);
            B2Bodies.b2Body_SetBullet(bullet.my_ID, true);
            B2Vec2 bulletVelocity = new B2Vec2((float)sldSpeed.Value, 0.0f);
            B2Bodies.b2Body_SetLinearVelocity(bullet.my_ID, bulletVelocity);
            fired = true;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            for (int i = 0; i < Target.Count; i++)
            {
                if (B2Worlds.b2Body_IsValid(Target[i].my_ID) == true)
                {
                    B2Bodies.b2DestroyBody(Target[i].my_ID);
                }
            }
            if (B2Worlds.b2Body_IsValid(bullet.my_ID) == true)
            {
                B2Bodies.b2DestroyBody(bullet.my_ID);
            }
            canvas1.Children.Clear();
            fired = false;
            Init();
        }
    }
}