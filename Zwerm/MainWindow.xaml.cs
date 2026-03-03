using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Zwerm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Program settings
        private int NumUnits = 400;    //Totaal aantal Units in de zwerm ;
        private double UnitSpeed = 1.5;        //Snelheid van iedere Unit ;
        private int NumNearest = 30;       //Aantal dichtbijzijnde units waarmee rekening gehouden wordt ;
        private int OptDist = 12;          //Optimale afstand tussen twee units ;
        private int maxViewdist = 60;      //Afstand dat de Units andere units kunnen zien ;
        //Program constant settings
        private double Schaalfactor = 0.001;   //Conversie van afstand naar kracht op de Unit ;
        private int MaxMouseDist = 40;         //Afstand dat een unit van de mousepointer wegloopt ;
        //Program members
        private delegate void RenderDelegate();
        private PixelFormat pf = PixelFormats.Rgb24;
        private Swarm my_Zwerm;
        private int my_width = 0;
        private int my_height = 0;
        private int rawStride = 0;
        private byte[] pixelData;
        private byte[] whiteArray;
        private bool AppRunning = false;
        private ZwermControl controlFrm;

        //DEBUG CODE
        private DateTime startTijd = DateTime.Now;
        private int Framecount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            controlFrm = new ZwermControl(this);
            controlFrm.Left = Left + ActualWidth;
            controlFrm.Top = Top;
            controlFrm.Show();
        }

        public void Settings(int Num, double speed, int near, int opt, int view)
        {
            if (NumUnits != Num | UnitSpeed != speed | NumNearest != near | OptDist != opt | maxViewdist != view)
            {
                AppRunning = false;
                NumUnits = Num;
                UnitSpeed = speed;
                NumNearest = near;
                OptDist = opt;
                maxViewdist = view;
                Init();
            }
        }

        private void Init()
        {
            my_width = (int)canvas1.ActualWidth;
            my_height = (int)canvas1.ActualHeight;
            Image1.Width = my_width;
            Image1.Height = my_height;
            rawStride = (int)((my_width * pf.BitsPerPixel + 7) / 8);
            //Resize de arrays
            pixelData = new byte[rawStride * my_height];
            whiteArray = new byte[rawStride * my_height];
            //Vul de white Array met Witte pixels
            for (int x = 0; x < my_width; x++)
            {
                for (int y = 0; y < my_height; y++)
                {
                    SetPixel(x, y, Color.FromRgb(255, 255, 255), whiteArray, rawStride);
                }
            }
            //Maak een Zwerm
            my_Zwerm = new Swarm(NumUnits);
            //Correct de schaalfactor voor het aantal nearest units
            my_Zwerm.Scale = Schaalfactor / NumNearest;
            my_Zwerm.Optdistance = OptDist;
            my_Zwerm.MaxMouseDistance = MaxMouseDist;
            my_Zwerm.NumNear = NumNearest;
            my_Zwerm.MaxViewDistance = maxViewdist;
            //Initialize de Zwerm
            my_Zwerm.InitUnits(my_height, my_width, UnitSpeed);
        }

        private void SetPixel(int x, int y, Color c, byte[] buffer, int rawStride)
        {
            int xIndex = x * 3;
            int yIndex = y * rawStride;
            buffer[xIndex + yIndex] = c.R;
            buffer[xIndex + yIndex + 1] = c.G;
            buffer[xIndex + yIndex + 2] = c.B;
        }

        private void Render()
        {
            int x = 0;
            int y = 0;
            //Fill the pixelData array with white pixels
            Array.Copy(whiteArray, pixelData, pixelData.Length);
            //Fill the buffer with the pixels that need drawing in black
            for (int I = 0; I < NumUnits; I++)
            {
                x = (int)(my_Zwerm.getUnitX(I));
                y = (int)(my_Zwerm.getUnitY(I));
                SetPixel(x, y, Color.FromRgb(0, 0, 0), pixelData, rawStride);
                SetPixel(x + 1, y, Color.FromRgb(0, 0, 0), pixelData, rawStride);
                SetPixel(x - 1, y, Color.FromRgb(0, 0, 0), pixelData, rawStride);
                SetPixel(x, y + 1, Color.FromRgb(0, 0, 0), pixelData, rawStride);
                SetPixel(x, y - 1, Color.FromRgb(0, 0, 0), pixelData, rawStride);
                //SetPixel(x + 1, y + 1, Color.FromRgb(0, 0, 0), pixelData, rawStride);
            }
            BitmapSource bitmap = BitmapSource.Create(my_width, my_height, 96, 96, pf, null, pixelData, rawStride);
            Image1.Source = bitmap;
            //DEBUG CODE
            Framecount += 1;
            if (Framecount == 10)
            {
                Framecount = 0;
                Title = "FPS = " + ((int)(10000 / (DateTime.Now - startTijd).TotalMilliseconds)).ToString();
                startTijd = DateTime.Now;
            }
        }


        public void Start_Stop()
        {
            //Start/stop the application
            AppRunning = !AppRunning;
            //Render when the application is idle
            while (AppRunning)
            {
                my_Zwerm.UpdateUnits();
                Dispatcher.Invoke(DispatcherPriority.SystemIdle, new RenderDelegate(Render));
            }
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!AppRunning) { return; }
            Point pt = e.GetPosition(canvas1);
            my_Zwerm.SetMouse(pt.X, pt.Y);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!AppRunning) { return; }
            Image1.Width = ActualWidth - 8;
            Image1.Height = ActualHeight - 30;
            controlFrm.Left = Left + ActualWidth;
            controlFrm.Top = Top;
            Init();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}