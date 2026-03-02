using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ImageTools
{
    public class CaliperTool : MeasurementTool
    {
        private double my_Dist;
        private Line my_Line1;
        private Line my_Line2;
        private Line my_Line3;
        private Label my_Label;

        public CaliperTool(SettingForm settings)
        {
            New(settings);
            my_Line1 = new Line();
            my_Line2 = new Line();
            my_Line3 = new Line();
            my_HandleCount = 3; //0 = start Line1 , 1 = End of Line1 , 2 = Distance from Line1;
            for (int I = 0; I < my_HandleCount; I++)
            {
                my_Handles.Add(new Handle(new Point(), this));
            }
            my_Label = new Label();
            my_DrawingState = 0;
        }

        public override void Draw(Canvas can)
        {
            can.Children.Add(my_Line1);
            can.Children.Add(my_Line2);
            can.Children.Add(my_Line3);
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
            if (can.Children.Contains(my_Line3)) { can.Children.Remove(my_Line3); }
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
                my_Line3.Stroke = Settings.ToolSelectedColor;
                my_Line3.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_Label.Background = Settings.ToolSelectedColor;
                my_Label.Foreground = Settings.ToolSelectedTextcolor;
            }
            else
            {
                my_Line1.Stroke = Settings.ToolColor;
                my_Line1.StrokeThickness = Settings.ToolLineThickness;
                my_Line2.Stroke = Settings.ToolColor;
                my_Line2.StrokeThickness = Settings.ToolLineThickness;
                my_Line3.Stroke = Settings.ToolColor;
                my_Line3.StrokeThickness = Settings.ToolLineThickness;
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
            Vector V0 = new Vector(my_Handles[0].Center.X, my_Handles[0].Center.Y);
            Vector V1 = new Vector(my_Handles[1].Center.X, my_Handles[1].Center.Y);
            Vector V2 = new Vector(my_Handles[2].Center.X, my_Handles[2].Center.Y);

            if (V0 != V1)
            {
                //Calculate the perpendicular distance from Handle2 to the line between handles 0 and 1.
                Vector N = V1 - V0;
                Vector PA = V0 - V2;
                Vector C = N * ((PA * N) / (N * N));
                Vector D = PA - C;
                my_Dist = Math.Sqrt(D * D);
                //Calculate the start and end points of the distance line
                Vector VL1 = V1 - V0; //Vector along line1
                double L1Length = Math.Sqrt(VL1 * VL1); //Length of line1
                Vector VL2Start;
                Vector VL2End;
                VL2Start = V1 - VL1 / 2;  //Distance Line starts in the middle of Line1
                Vector VL2 = new Vector(VL1.Y, -VL1.X); //Vector perpendicular to line 1
                VL2.Normalize();
                VL2End = VL2Start + my_Dist * VL2;
                double RC1 = (V1.Y - V0.Y) / (V1.X - V0.X);
                double RC2 = (VL2End.Y - V2.Y) / (VL2End.X - V2.X);
                if (Math.Abs(RC1 - RC2) > 0.01)
                {
                    VL2End = VL2Start - my_Dist * VL2;
                }
                my_Line2.X1 = VL2Start.X;
                my_Line2.Y1 = VL2Start.Y;
                my_Line2.X2 = VL2End.X;
                my_Line2.Y2 = VL2End.Y;
                if (V2 != VL2End)
                {
                    //Determine the length and end points of line 3.
                    Vector VL3 = VL2End - V2; //Vector along Line3 
                    double L3Length = Math.Sqrt(VL3 * VL3);
                    VL3.Normalize();
                    Vector VL3Start;
                    Vector VL3End;
                    if (L3Length < L1Length / 2)
                    { //Line 3 as long as line1 and centered around distanceLine
                        VL3Start = VL2End - L1Length / 2 * VL3;
                        VL3End = VL2End + L1Length / 2 * VL3;
                    }
                    else if (L3Length < 0.8 * L1Length)
                    { //Line 3 as long as line1 but start at Handle 2
                        VL3Start = V2;
                        VL3End = V2 + L1Length * VL3;
                    }
                    else
                    { //Line 3 start at Handle 2 and goes 10% past the distanceLine
                        VL3Start = V2;
                        VL3End = VL2End + 0.2 * L1Length * VL3;
                    }
                    my_Line3.X1 = VL3Start.X;
                    my_Line3.Y1 = VL3Start.Y;
                    my_Line3.X2 = VL3End.X;
                    my_Line3.Y2 = VL3End.Y;
                }
            }
            if (my_DrawingState != 1)
            {
                //Set the label in middle of the distance line
                string labelTextFormat = "F" + Settings.ToolDecimals.ToString();
                my_Label.Content = my_Dist.ToString(labelTextFormat) + my_UnitName;
                my_Label.Measure(new Size(120, 25));
                my_Label.UpdateLayout();
                my_Label.SetValue(Canvas.LeftProperty, (my_Line2.X1 + my_Line2.X2) / 2 - my_Label.ActualWidth / 2);
                my_Label.SetValue(Canvas.TopProperty, (my_Line2.Y1 + my_Line2.Y2) / 2 - my_Label.ActualHeight / 2);
            }
        }

        public override bool IsMouseOver(Point pt)
        {
            //Check if Pt lies inside the concave polygon formed by the endpoints of both lines
            //For symplicity 3 possible orientations are checked. One of them must always be concave.
            Vector V0 = new Vector(my_Handles[0].Center.X, my_Handles[0].Center.Y);
            Vector V1 = new Vector(my_Handles[1].Center.X, my_Handles[1].Center.Y);
            Vector V2 = new Vector(my_Line3.X1, my_Line3.Y1);
            Vector V3 = new Vector(my_Line3.X2, my_Line3.Y2);
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
            if (my_DrawingState == 2)
            {
                my_Handles[2].SetCenter(pt);
                Update();
            }
        }
    }
}
