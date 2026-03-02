
namespace MultiAlarm
{
    class Alarm
    {
        private bool my_Enabled;
        private string my_Time;
        private string my_Message;

        public Alarm() 
        {
            my_Enabled = false;
            my_Time = "00:00:00";
            my_Message = "";
        }

        public bool Enabled
        { 
            get { return my_Enabled; } 
            set { my_Enabled = value; } 
        }

        public string Time
        { 
            get { return my_Time; } 
            set { my_Time = value; } 
        }

        public string Message
        { 
            get { return my_Message; } 
            set { my_Message = value; } 
        }
    }
}
