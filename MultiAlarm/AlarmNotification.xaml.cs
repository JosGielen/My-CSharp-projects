using System.Windows;

namespace MultiAlarm
{
    /// <summary>
    /// Interaction logic for AlarmNotification.xaml
    /// </summary>
    public partial class AlarmNotification : Window
    {
        private MultiAlarm.MainWindow main;
        public AlarmNotification(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
        }
        private void BtnOKAlarm_Click(Object sender, RoutedEventArgs e)
        {
            main.ResetAlarm();
        }

        private void AlarmNotification_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            main.ResetAlarm();
        }
    }
}
