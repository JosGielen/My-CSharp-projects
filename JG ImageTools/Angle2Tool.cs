using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ImageTools
{
    public class Angle2Tool : MeasurementTool
    {
        private double my_Angle;
        private Line my_Line1;
        private Line my_Line2;
        private Line my_ArcLine;
        private Label my_Label;

        public Angle2Tool(SettingForm settings)
        {
            New(settings);
            my_Line1 = new Line();
            my_Line2 = new Line();
            my_HandleCount = 4;   //0 = start Line1 , 1 = End of Line1 , 2 = Start of Line2 , 3 = End of Line2;
            for (int I = 0; I < my_HandleCount; I++)
            {
                my_Handles.Add(new Handle(new Point(), this));
            }
            my_ArcLine = new Line();
            my_Label = new Label();
            my_DrawingState = 0;
        }

        public override void Draw(Canvas can)
        {
            can.Children.Add(my_Line1);
            can.Children.Add(my_Line2);
            can.Children.Add(my_ArcLine);
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Draw(can);
            }
            can.Children.Add(my_Label);
        }

        public override void Remove(Canvas can)
        {
            if (can.Children.Contains(my_Line1)) { can.Children.Remove(my_Line1); }
            if (can.Children.Contains(my_Line2)) { can.Children.Remove(my_Line2); }
            if (can.Children.Contains(my_ArcLine)) { can.Children.Remove(my_ArcLine); }
            if (can.Children.Contains(my_Label)) { can.Children.Remove(my_Label); }
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Remove(can);
            }
        }

        public override void Update()
        {
            //Set the colors
            if (Highlighted == true)
            {
                my_Line1.Stroke = Settings.ToolSelectedColor;
                my_Line1.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_Line2.Stroke = Settings.ToolSelectedColor;
                my_Line2.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_ArcLine.Stroke = Settings.ToolSelectedColor;
                my_ArcLine.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_Label.Background = Settings.ToolSelectedColor;
                my_Label.Foreground = Settings.ToolSelectedTextcolor;
            }
            else
            {
                my_Line1.Stroke = Settings.ToolColor;
                my_Line1.StrokeThickness = Settings.ToolLineThickness;
                my_Line2.Stroke = Settings.ToolColor;
                my_Line2.StrokeThickness = Settings.ToolLineThickness;
                my_ArcLine.Stroke = Settings.ToolColor;
                my_ArcLine.StrokeThickness = Settings.ToolLineThickness;
                my_Label.Background = Settings.ToolColor;
                my_Label.Foreground = Settings.ToolTextColor;
            }
            my_Label.FontSize = Settings.ToolFontSize;
            my_Label.Padding = new Thickness(2.0);
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Update();
            }
            my_Line1.X1 = my_Handles[0].Center.X;
            my_Line1.Y1 = my_Handles[0].Center.Y;
            my_Line1.X2 = my_Handles[1].Center.X;
            my_Line1.Y2 = my_Handles[1].Center.Y;
            my_Line2.X1 = my_Handles[2].Center.X;
            my_Line2.Y1 = my_Handles[2].Center.Y;
            my_Line2.X2 = my_Handles[3].Center.X;
            my_Line2.Y2 = my_Handles[3].Center.Y;
            //Calculate the Angle
            Vector V1 = new Vector(my_Handles[1].Center.X - my_Handles[0].Center.X, my_Handles[1].Center.Y - my_Handles[0].Center.Y);
            Vector V2 = new Vector(my_Handles[3].Center.X - my_Handles[2].Center.X, my_Handles[3].Center.Y - my_Handles[2].Center.Y);
            double Angle1 = Math.Abs(Vector.AngleBetween(V1, V2));
            double Angle2 = Math.Abs(Vector.AngleBetween(V1, -V2));
            if (Angle1 < Angle2)
            {
                my_Angle = Angle1;
            }
            else
            {
                my_Angle = Angle2;
            }
            //Step1: Determine the startpoint and endpoint of the arcLine
            my_ArcLine.X1 = (my_Handles[1].Center.X + my_Handles[0].Center.X) / 2;
            my_ArcLine.Y1 = (my_Handles[1].Center.Y + my_Handles[0].Center.Y) / 2;
            my_ArcLine.X2 = (my_Handles[3].Center.X + my_Handles[2].Center.X) / 2;
            my_ArcLine.Y2 = (my_Handles[3].Center.Y + my_Handles[2].Center.Y) / 2;
            if (my_DrawingState != 1)
            {
                //Set the arc label in middle of the arc
                string labelTextFormat = "F" + Settings.ToolDecimals.ToString();
                my_Label.Content = my_Angle.ToString(labelTextFormat) + "°";
                my_Label.Measure(new Size(120, 25));
                my_Label.UpdateLayout();
                my_Label.SetValue(Canvas.LeftProperty, (my_ArcLine.X1 + my_ArcLine.X2) / 2 - my_Label.ActualWidth / 2);
                my_Label.SetValue(Canvas.TopProperty, (my_ArcLine.Y1 + my_ArcLine.Y2) / 2 - my_Label.ActualHeight / 2);
            }
        }

        public override bool IsMouseOver(Point pt)
        {
            //Check if Pt lies inside the concave Polygon formed by Handles(0 to 3)
            //For symplicity 3 possible orientations are checked. One of them must always be concave.
            Vector V0 = new Vector(my_Handles[0].Center.X, my_Handles[0].Center.Y);
            Vector V1 = new Vector(my_Handles[1].Center.X, my_Handles[1].Center.Y);
            Vector V2 = new Vector(my_Handles[2].Center.X, my_Handles[2].Center.Y);
            Vector V3 = new Vector(my_Handles[3].Center.X, my_Handles[3].Center.Y);
            Vector P = new Vector(pt.X, pt.Y);
            Vector VP0 = V0 - P;
            Vector VP1 = V1 - P;
            Vector VP2 = V2 - P;
            Vector VP3 = V3 - P;
            double Angle = Math.Abs(Vector.AngleBetween(VP0, VP1));
            Angle += Math.Abs(Vector.AngleBetween(VP1, VP2));
            Angle += Math.Abs(Vector.AngleBetween(VP2, VP3));
            Angle += Math.Abs(Vector.AngleBetween(VP3, VP0));
            if (Math.Abs(360 - Angle) < 1) { return true; }
            Angle = Math.Abs(Vector.AngleBetween(VP0, VP1));
            Angle += Math.Abs(Vector.AngleBetween(VP1, VP3));
            Angle += Math.Abs(Vector.AngleBetween(VP3, VP2));
            Angle += Math.Abs(Vector.AngleBetween(VP2, VP0));
            if (Math.Abs(360 - Angle) < 1) { return true; }
            Angle = Math.Abs(Vector.AngleBetween(VP0, VP2));
            Angle += Math.Abs(Vector.AngleBetween(VP2, VP1));
            Angle += Math.Abs(Vector.AngleBetween(VP1, VP3));
            Angle += Math.Abs(Vector.AngleBetween(VP3, VP0));
            if (Math.Abs(360 - Angle) < 1) { return true; }
            return false;
        }

        public override void MouseUp(Point pt)
        {
            if (my_DrawingState == 0)
            {   //Start with all Handles at the same location;
                for (int I = 0; I < my_Handles.Count; I++)
                {
                    my_Handles[I].SetCenter(pt);
                    my_Handles[I].Hide(); //Allows more precise positioning of the mousepointer
                }
                my_DrawingState = 1;
                my_Handles[0].Show();
                return;
            }
            if (my_DrawingState == 1)
            {
                my_Handles[1].SetCenter(pt);
                my_Handles[1].Show();
                Update();
                my_DrawingState = 2;
                return;
            }
            if (my_DrawingState == 2)
            {
                my_Handles[2].SetCenter(pt);
                my_Handles[2].Show();
                my_DrawingState = 3;
                return;
            }
            if (my_DrawingState == 3)
            {
                Finished = true;
            }
        }

        public override void MouseMove(Point pt)
        {
            if (my_DrawingState == 1)
            {
                my_Handles[1].SetCenter(pt);
                Update();
            }
            if (my_DrawingState == 3)
            {
                my_Handles[3].SetCenter(pt);
                Update();
            }
        }
    }
}
