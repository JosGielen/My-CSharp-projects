using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Threading;

namespace MultiAlarm;

public partial class MainWindow : Window
{
    private delegate void WaitDelegate();
    private bool shouldClose;       //Prevents closing the application with the Window Close button
    private bool Alarm_On = false;  //Start the alarm timer after the application is fully initialised
    private System.Windows.Forms.Timer Timer1 = new System.Windows.Forms.Timer();        //Alarm timer
    private int BeepCount = 0;     //Actual number of times the alarm sound was played
    private int MaxBeepCount = 60; //Maximum number of times the alarm sound may be played
    private ObservableCollection<Alarm> AlarmList = new ObservableCollection<Alarm>(); //Contains the alarm data
    private AlarmNotification WndAlarm;   //Alarm notification window
    private NotifyIcon My_notifyIcon; //NotificationArea Icon

    public MainWindow()
    {
        InitializeComponent();
        WndAlarm = new AlarmNotification(this);
        //Create the context menu for the NotificationArea Icon
        ContextMenuStrip Cmenu = new ContextMenuStrip();
        Cmenu.Items.Add(new ToolStripMenuItem("Edit Alarms", null, MenuItemEdit_Click));
        Cmenu.Items.Add(new ToolStripMenuItem("Reset Alarm", null, MenuItemReset_Click));
        Cmenu.Items.Add(new ToolStripSeparator());
        Cmenu.Items.Add(new ToolStripMenuItem("Exit", null, MenuItemExit_Click));
        Cmenu.Items[0].Font = new Font(Cmenu.Items[1].Font, System.Drawing.FontStyle.Bold);
        //Create the NotificationArea Icon and add the context menu to it.
        My_notifyIcon = new NotifyIcon();
        My_notifyIcon.Icon = new System.Drawing.Icon(Environment.CurrentDirectory + "\\Icon1.ico");
        My_notifyIcon.ContextMenuStrip = Cmenu;
        My_notifyIcon.DoubleClick += MenuItemEdit_Click;
        My_notifyIcon.Visible = true;
        shouldClose = false;
        //Start the alarm timer and hide the window
        Start();
        Hide();
    }

    private void Window1_Closing(object? sender, CancelEventArgs e)
    {
        //Hides the window when the Window Close button is clicked
        //Exit the application only through the Exit menu of the NotificationArea Icon
        if ((!shouldClose))
        {
            e.Cancel = true;
            Hide();
        }
    }

    #region "NotificationAreaIcon Mouse Events"

    private void MenuItemEdit_Click(object? sender, EventArgs e)
    {
        Show();
        ShowAlarms();
    }

    private void MenuItemReset_Click(object? sender, EventArgs e)
    {
        ResetAlarm();
    }

    private void MenuItemExit_Click(object? sender, EventArgs e)
    {
        WriteAlarmsToFile();
        //Exit the application
        shouldClose = true;
        My_notifyIcon.Dispose();
        Close();
        Environment.Exit(0);
    }

    #endregion

    #region "AlarmEdit events"

    private void BtnRemove_Click(object? sender, RoutedEventArgs e)
    {
        for (int i = AlarmDataGrid.Items.Count - 1; i >=0; i--)
        {
            if (((Alarm)AlarmDataGrid.Items[i]).Enabled == false)
            {
                AlarmList.RemoveAt(i);
            }
        }
        ShowAlarms();
    }

    private void BtnAdd_Click(object? sender, RoutedEventArgs e)
    {
        //Check the format of the new alarm text.
        string TimeString;
        Alarm dummy;
        DateTime d;
        try
        {
            int Hour = int.Parse(TxtHours.Text);
            int Min = int.Parse(TxtMinutes.Text);
            int Sec = int.Parse(TxtSeconds.Text);
            TimeString = String.Format("{0:00}:{1:00}:{2:00}", Hour, Min, Sec);
            d = DateTime.Parse(TimeString);
        }
        catch
        {
            FormatException ex;
            System.Windows.MessageBox.Show("Invalid Time string", "MultiAlarm Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        //Add the new alarm to the AlarmList
        Alarm alarm = new Alarm();
        alarm.Enabled = true;
        alarm.Time = TimeString;
        alarm.Message = TxtMessage.Text;
        if (!AlarmList.Contains(alarm))
        {
            AlarmList.Add(alarm);
        }
        //Sort the alarms in increasing time order
        for (int I = 0; I < AlarmList.Count; I++)
        {
            for (int J = I + 1; J < AlarmList.Count; J++)
            {
                if (string.Compare(AlarmList[I].Time, AlarmList[J].Time) > 0)
                {
                    dummy = AlarmList[I];
                    AlarmList[I] = AlarmList[J];
                    AlarmList[J] = dummy;
                }
            }
        }
        ShowAlarms();
        TxtHours.Text = "00";
        TxtMinutes.Text = "00";
        TxtSeconds.Text = "00";
        TxtMessage.Text = "";
    }

    private void ShowAlarms()
    {
        AlarmDataGrid.Items.Clear();
        for (int i = 0; i < AlarmList.Count; i++)
        {
            AlarmDataGrid.Items.Add(AlarmList[i]);
        }
    }

    private void BtnOK_Click(object? sender, RoutedEventArgs e)
    {
        WriteAlarmsToFile();
        updateNotification();
        Hide();
        if (AlarmList.Count > 0)
        {
            Timer1.Interval = 500;
            Timer1.Tick += Timer1_Tick;
            Timer1.Start();
        }
        else
        {
            Timer1.Stop();
        }
    }

    private void WriteAlarmsToFile()
    {
        //Write the alarm times to the Alarms.ini file
        string Inifile = Environment.CurrentDirectory + "\\Alarms.ini";
        if (File.Exists(Inifile)) { File.Delete(Inifile); }
        if (AlarmList.Count > 0)
        {
            using (StreamWriter outfile = new StreamWriter(Inifile))
            {
                for (int I = 0; I < AlarmList.Count; I++)
                {
                    outfile.WriteLine(AlarmList[I].Time);
                    outfile.WriteLine(AlarmList[I].Message);
                    outfile.WriteLine(AlarmList[I].Enabled.ToString());
                }
            }
        }
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Hide();
    }

    #endregion

    private void Start()
    {
        //Get the alarm time from the Alarm.ini file
        string Inifile = Environment.CurrentDirectory + "\\Alarms.ini";
        AlarmList = new ObservableCollection<Alarm>();
        Alarm alarm;
        try
        {
            // Create an instance of StreamReader to read from the file.
            using (StreamReader sr = new StreamReader(Inifile))
            {
                //Read the alarm times from the file.
                while (!sr.EndOfStream)
                {
                    alarm = new Alarm();
                    alarm.Time = sr.ReadLine();
                    alarm.Message = sr.ReadLine();
                    alarm.Enabled = bool.Parse(sr.ReadLine());
                    AlarmList.Add(alarm);
                }
            }
        }
        catch
        {
            Exception Ex;
            //No ini file or empty file.
        }
        ShowAlarms();
        updateNotification();
        //Start the timer
        if (AlarmList.Count > 0)
        {
            Timer1.Interval = 500;
            Timer1.Tick += Timer1_Tick;
            Timer1.Start();
        }
    }

    private void Timer1_Tick(object? sender, System.EventArgs e)
    {
        string TimeStr = DateTime.Now.ToString("HH:mm:ss");
        if (!Alarm_On)
        {
            //Check if the alarm list contains the current time and that alarm is enabled
            for (int i = 0; i < AlarmList.Count; i++)
            {
                if (AlarmList[i].Time.Equals(TimeStr) && AlarmList[i].Enabled == true)
                {
                    //Start the alarm and show the alarm window
                    Alarm_On = true;
                    BeepCount = 0;
                    WndAlarm.TxtAlarmNotify.Text = "ALARM TIME = " + AlarmList[i].Time;
                    WndAlarm.TxtAlarmMessage.Text = AlarmList[i].Message;
                    WndAlarm.Show();
                }
            }
        }
        else
        {
            //if (alarm is on: play sound a fixed number of times and then turn alarm off
            SystemSounds.Exclamation.Play();
            Dispatcher.Invoke(new WaitDelegate(Wait), DispatcherPriority.ApplicationIdle);
            BeepCount += 1;
            if (BeepCount == MaxBeepCount)
            {
                Alarm_On = false;
                updateNotification();
            }
        }
    }

    private void updateNotification()
    {
        //Display the next alarm time when mouse over the notificationarea icon
        if (AlarmList.Count > 0)
        {
            for (int I = 0; I < AlarmList.Count; I++)
            {
                if (DateTime.Parse(AlarmList[I].Time ) > DateTime.Now && AlarmList[I].Enabled == true)
                {
                    My_notifyIcon.Text = AlarmList[I].Time + " : " + AlarmList[I].Message;
                    return;
                }
            }
            for (int I = 0; I < AlarmList.Count; I++)
            {
                if (AlarmList[I].Enabled == true)
                {
                    My_notifyIcon.Text = AlarmList[I].Time + " : " + AlarmList[I].Message;
                    return;
                }
            }
        }
        My_notifyIcon.Text = "No Alarm";
    }

    public void ResetAlarm()
    {
        Alarm_On = false;
        updateNotification();
        if (WndAlarm != null)
        {
            WndAlarm.Hide();
        }
    }

    private void CBEnabled_Click(object sender, RoutedEventArgs e)
    {
        int index = AlarmDataGrid.SelectedIndex;
        AlarmList[index].Enabled = !AlarmList[index].Enabled;
    }

    private void Wait()
    {
        Thread.Sleep(200);
    }
}
