using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Matrix_Rain
{
    public partial class MainWindow : Window
    {
        private List<SymbolStream> Streams;
        private int MaxStreamLength = 0;
        private int my_FontSize;
        private double my_CharHeight;
        private double my_CharWidth;
        private bool isPreviewWindow = false;
        private Point lastMousePosition = default;
        private Random Rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(IntPtr previewHandle)
        {
            InitializeComponent();
            isPreviewWindow = true;
            Rect parentRect = new Rect();
            WindowState = WindowState.Normal;

            IntPtr windowHandle = new WindowInteropHelper(GetWindow(this)).EnsureHandle();

            // Set the preview window as the parent of this window
            InteropHelper.SetParent(windowHandle, previewHandle);

            // Make this window a tool window while preview.
            // A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
            // GWL_EXSTYLE = -20, WS_EX_TOOLWINDOW = 0x00000080L
            InteropHelper.SetWindowLong(windowHandle, -20, 0x00000080L);
            // Make this a child window so it will close when the parent dialog closes
            // GWL_STYLE = -16, WS_CHILD = 0x40000000
            InteropHelper.SetWindowLong(windowHandle, -16, 0x40000000L);

            // Place the window inside the parent
            InteropHelper.GetClientRect(previewHandle, ref parentRect);

            Width = parentRect.Width;
            Height = parentRect.Height;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isPreviewWindow)
            {
                WindowState = WindowState.Maximized;
                my_FontSize = 16;
            }
            Symbol sym = new Symbol(new Point(0, 0), Colors.Black, my_FontSize);
            my_CharHeight = sym.Height;
            my_CharWidth = sym.Width - 2;
            MaxStreamLength = (int)(0.8 * Canvas1.ActualHeight / my_CharHeight);
            Streams = new List<SymbolStream>();
            StartRender();
        }

        private void Window_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void StartRender()
        {
            double X = 0.0;
            SymbolStream sst;
            int length;
            double speed;
            double offset;
            Point location;
            Streams = new List<SymbolStream>();
            Canvas1.Children.Clear();
            while (X < Canvas1.ActualWidth / 2)
            {
                length = (int)(MaxStreamLength * (0.4 + 0.4 * Rnd.NextDouble()));
                speed = 5 + Rnd.NextDouble();
                offset = -1 * Rnd.Next((int)Canvas1.ActualHeight) - length * my_CharHeight;
                location = new Point(X, offset);
                sst = new SymbolStream(length, speed, location, my_FontSize, my_CharHeight);
                sst.Draw(Canvas1);
                Streams.Add(sst);
                length = (int)(MaxStreamLength * (0.4 + 0.4 * Rnd.NextDouble()));
                speed = 5 + Rnd.NextDouble();
                offset = -1 * Rnd.Next((int)Canvas1.ActualHeight) - (2 + Rnd.NextDouble()) * length * my_CharHeight;
                location = new Point(X, offset);
                sst = new SymbolStream(length, speed, location, my_FontSize, my_CharHeight);
                sst.Draw(Canvas1);
                Streams.Add(sst);
                X = X + my_CharWidth;
            }
            CompositionTarget.Rendering += Render;
        }

        public void Render(object sender, EventArgs e)
        {
            for (int I = 0; I < Streams.Count; I++)
            {
                Streams[I].Update();
                if (Streams[I].Top > Canvas1.ActualHeight)
                {
                    Streams[I].Top = -1 * Streams[I].length * my_CharHeight;
                }
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isPreviewWindow) return;
            Application.Current.Shutdown();
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isPreviewWindow) { return; }
            Point pos = e.GetPosition(this);
            if (lastMousePosition != default)
            {
                if ((lastMousePosition - pos).Length > 3)
                {
                    Application.Current.Shutdown();
                }
            }
            lastMousePosition = pos;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (isPreviewWindow) { return; }
            Application.Current.Shutdown();
        }

        internal void Dispose(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
