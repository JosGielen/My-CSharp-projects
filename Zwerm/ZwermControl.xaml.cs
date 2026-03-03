using System.Windows;

namespace Zwerm
{
    public partial class ZwermControl : Window
    {
        private int UnitNumber = 800;
        private double Unitspeed = 1.7;
        private int Nearestcount = 20;
        private int Prefdistance = 15;
        private int ViewDistance = 60;
        private MainWindow m_Veld;
        private bool m_started = false;

        public ZwermControl()
        {
            InitializeComponent();
        }

        public ZwermControl(MainWindow veld)
        {
            InitializeComponent();
            m_Veld = veld;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTextboxes();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            m_started = !m_started;
            if (m_started)
            {
                BtnStart.Content = "STOP";
                //Check de instel waarden en geef ze door aan het Veld
                Validate();
                m_Veld.Settings(UnitNumber, Unitspeed, Nearestcount, Prefdistance, ViewDistance);
            }
            else
            {
                BtnStart.Content = "START";
            }
            m_Veld.Start_Stop();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void UpdateTextboxes()
        {
            TxtUnitNumber.Text = UnitNumber.ToString();
            TxtUnitSpeed.Text = Unitspeed.ToString();
            TxtNearestCount.Text = Nearestcount.ToString();
            TxtPrefDistance.Text = Prefdistance.ToString();
            TxtViewDistance.Text = ViewDistance.ToString();
        }

        private void Validate()
        {
            if (!int.TryParse(TxtUnitNumber.Text, out UnitNumber))
            {
                TxtUnitNumber.Text = UnitNumber.ToString();
            }
            if (!double.TryParse(TxtUnitSpeed.Text, out Unitspeed))
            {
                TxtUnitSpeed.Text = Unitspeed.ToString();
            }
            if (!int.TryParse(TxtNearestCount.Text, out Nearestcount))
            {
                TxtNearestCount.Text = Nearestcount.ToString();
            }
            if (!int.TryParse(TxtPrefDistance.Text, out Prefdistance))
            {
                TxtPrefDistance.Text = Prefdistance.ToString();
            }
            if (!int.TryParse(TxtViewDistance.Text, out ViewDistance))
            {
                TxtViewDistance.Text = ViewDistance.ToString();
            }
            SliderUnitNumber.Value = UnitNumber;
            SliderUnitSpeed.Value = Unitspeed;
            SliderNearestCount.Value = Nearestcount;
            SliderPrefDistance.Value = Prefdistance;
            SliderViewDistance.Value = ViewDistance;
        }

        private void SliderUnitNumber_ValueChanged(object sender, RoutedEventArgs e)
         {
            TxtUnitNumber.Text = SliderUnitNumber.Value.ToString();
        }

        private void SliderUnitSpeed_ValueChanged(object sender, RoutedEventArgs e)
         {
            TxtUnitSpeed.Text = SliderUnitSpeed.Value.ToString();
        }

        private void SliderNearestCount_ValueChanged(object sender, RoutedEventArgs e)
         {
            TxtNearestCount.Text = SliderNearestCount.Value.ToString();
        }

        private void SliderPrefDistance_ValueChanged(object sender, RoutedEventArgs e)
         {
            TxtPrefDistance.Text = SliderPrefDistance.Value.ToString();
        }

        private void SliderViewDistance_ValueChanged(object sender, RoutedEventArgs e)
         {
            TxtViewDistance.Text = SliderViewDistance.Value.ToString();
        }
    }
}
