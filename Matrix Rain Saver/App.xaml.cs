using System.Configuration;
using System.Data;
using System.Windows;

namespace Matrix_Rain
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs args)
        {
            //base.OnStartup(args);

            if (args.Args.Length == 0 || args.Args[0].ToLower().StartsWith("/c"))
            {
                MessageBox.Show("This screensaver has no options you can configure.",
                      "Screensaver", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            }
            else if (args.Args[0].ToLower().StartsWith("/s"))
            {
                //Start the screensaver in full-screen mode.
                var screensaverWindow = new MainWindow();
                screensaverWindow.Show();
            }
            else if (args.Args[0].ToLower().StartsWith("/p"))
            {
                //Display a preview of the screensaver using the specified window handle.
                IntPtr previewHwnd = new IntPtr(Convert.ToInt32(args.Args[1]));
                var previewWindow = new MainWindow(previewHwnd);
                previewWindow.Show();
            }
        }
    }
}
