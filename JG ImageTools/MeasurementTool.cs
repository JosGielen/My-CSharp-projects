using System.Windows;
using System.Windows.Controls;

namespace ImageTools
{
    public abstract class MeasurementTool
    {
        protected int my_DrawingState;
        protected SettingForm my_Settings;
        protected List<Handle> my_Handles;
        protected int my_HandleCount;
        protected bool my_Highlighted;
        protected string my_UnitName;
        private bool my_Finished;

        public void New(SettingForm settings)
        {
            my_Settings = settings;
            my_Handles = new List<Handle>();
            my_HandleCount = 0;
            my_Highlighted = false;
            my_UnitName = " pix";
            my_DrawingState = 0;
            my_Finished = false;
        }

        public SettingForm Settings
        {
            get { return my_Settings; }
            set
            {
                my_Settings = value;
                Update();
            }
        }

        public List<Handle> ToolHandles
        {
            get
            {
                return my_Handles;
            }
        }

        public bool Highlighted
        {
            get { return my_Highlighted; }
            set
            {
                my_Highlighted = value;
                Update();
            }
        }

        public int DrawingState
        {
            get { return my_DrawingState; }
            set { my_DrawingState = value; }
        }

        public bool Finished
        {
            get { return my_Finished; }
            set { my_Finished = value; }
        }

        public void HideHandles()
        {
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Hide();
                my_Handles[I].Selected = false;
            }
        }

        public void ShowHandles()
        {
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Highlighted = false;
                my_Handles[I].Update();
                my_Handles[I].Show();
            }
        }

        public void Move(Vector dist)
        {
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].SetCenter(my_Handles[I].Center + dist);
            }
            Update();
        }

        public abstract void Draw(Canvas can);

        public abstract void Remove(Canvas can);

        public abstract void Update();

        public abstract bool IsMouseOver(Point pt);

        public abstract void MouseUp(Point pt);

        public abstract void MouseMove(Point pt);
    }
}
