using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ImageTools
{
    public class LineTool : MeasurementTool
    {
        private Line my_Line;
        private Line my_EndCap1;
        private Line my_EndCap2;
        private double my_EndCapLength;
        private Label my_Label;

        public LineTool(SettingForm settings)
        {
            New(settings);
            my_Line = new Line();
            my_EndCap1 = new Line();
            my_EndCap2 = new Line();
            my_HandleCount = 2; //0 = StartPoint , 1 = EndPoint
            for (int I = 0; I < my_HandleCount; I++)
            {
                my_Handles.Add(new Handle(new Point(), this));
            }
            my_EndCapLength = 20;
            my_Label = new Label();
        }

        public double EndCapLength
        {
            get { return my_EndCapLength; }
            set { my_EndCapLength = value; }
        }

        public override void Draw(Canvas can)
        {
            can.Children.Add(my_Line);
            can.Children.Add(my_EndCap1);
            can.Children.Add(my_EndCap2);
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Draw(can);
            }
            can.Children.Add(my_Label);
        }

        public override void Remove(Canvas can)
        {
            if (can.Children.Contains(my_Line)) { can.Children.Remove(my_Line); }
            if (can.Children.Contains(my_EndCap1)) { can.Children.Remove(my_EndCap1); }
            if (can.Children.Contains(my_EndCap2)) { can.Children.Remove(my_EndCap2); }
            if (can.Children.Contains(my_Label)) { can.Children.Remove(my_Label); }
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Remove(can);
            }
        }

        public override void Update()
        {
            //Set the colors
            if (Highlighted)
            {
                my_Line.Stroke = Settings.ToolSelectedColor;
                my_Line.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_EndCap1.Stroke = Settings.ToolSelectedColor;
                my_EndCap1.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_EndCap2.Stroke = Settings.ToolSelectedColor;
                my_EndCap2.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_Label.Background = Settings.ToolSelectedColor;
                my_Label.Foreground = Settings.ToolSelectedTextcolor;
            }
            else
            {
                my_Line.Stroke = Settings.ToolColor;
                my_Line.StrokeThickness = Settings.ToolLineThickness;
                my_EndCap1.Stroke = Settings.ToolColor;
                my_EndCap1.StrokeThickness = Settings.ToolLineThickness;
                my_EndCap2.Stroke = Settings.ToolColor;
                my_EndCap2.StrokeThickness = Settings.ToolLineThickness;
                my_Label.Background = Settings.ToolColor;
                my_Label.Foreground = Settings.ToolTextColor;
            }
            my_Label.FontSize = Settings.ToolFontSize;
            my_Label.Padding = new Thickness(2.0);
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Update();
            }
            Point center;
            //Calculate the Center point
            center = new Point((my_Handles[0].Center.X + my_Handles[1].Center.X) / 2, (my_Handles[0].Center.Y + my_Handles[1].Center.Y) / 2);
            //Set the Line
            my_Line.X1 = my_Handles[0].Center.X;
            my_Line.Y1 = my_Handles[0].Center.Y;
            my_Line.X2 = my_Handles[1].Center.X;
            my_Line.Y2 = my_Handles[1].Center.Y;
            //Set the EndCap1 line
            Vector V1 = new Vector(my_Handles[0].Center.X, my_Handles[0].Center.Y);
            Vector V2 = new Vector(my_Handles[1].Center.X, my_Handles[1].Center.Y);
            if (V1 != V2)
            {
                Vector V = V2 - V1;
                Vector VE1 = new Vector(V.Y, -V.X);
                VE1.Normalize();
                VE1 = my_EndCapLength * VE1;
                my_EndCap1.X1 = my_Line.X1 - VE1.X;
                my_EndCap1.Y1 = my_Line.Y1 - VE1.Y;
                my_EndCap1.X2 = my_Line.X1 + VE1.X;
                my_EndCap1.Y2 = my_Line.Y1 + VE1.Y;
                //Set the EndCap2 line
                V = V1 - V2;
                Vector VE2 = new Vector(V.Y, -V.X);
                VE2.Normalize();
                VE2 = my_EndCapLength * VE2;
                my_EndCap2.X1 = my_Line.X2 - VE2.X;
                my_EndCap2.Y1 = my_Line.Y2 - VE2.Y;
                my_EndCap2.X2 = my_Line.X2 + VE2.X;
                my_EndCap2.Y2 = my_Line.Y2 + VE2.Y;
                //Set the label in middle of the Line
                double length = Math.Sqrt((my_Handles[0].Center.X - my_Handles[1].Center.X) * (my_Handles[0].Center.X - my_Handles[1].Center.X) + (my_Handles[0].Center.Y - my_Handles[1].Center.Y) * (my_Handles[0].Center.Y - my_Handles[1].Center.Y));
                string labelTextFormat = "F" + Settings.ToolDecimals.ToString();
                my_Label.Content = length.ToString(labelTextFormat) + my_UnitName;
                my_Label.Measure(new Size(120, 25));
                my_Label.UpdateLayout();
                my_Label.SetValue(Canvas.LeftProperty, center.X - my_Label.ActualWidth / 2);
                my_Label.SetValue(Canvas.TopProperty, center.Y - my_Label.ActualHeight / 2);
            }
        }

        public override bool IsMouseOver(Point pt)
        {
            //Check if Pt lies inside the rectangle formed by the endpoints of both Endcap lines
            Vector V0 = new Vector(my_EndCap1.X1, my_EndCap1.Y1);
            Vector V1 = new Vector(my_EndCap1.X2, my_EndCap1.Y2);
            Vector V2 = new Vector(my_EndCap2.X1, my_EndCap2.Y1);
            Vector V3 = new Vector(my_EndCap2.X2, my_EndCap2.Y2);
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
            return false;
        }

        public override void MouseUp(Point pt)
        {
            if (my_DrawingState == 0)
            { //Start with all Handles at the same location;
                for (int I = 0; I < my_Handles.Count; I++)
                {
                    my_Handles[I].SetCenter(pt);
                    my_Handles[I].Hide(); //Allows more precise positioning of the mousepointer
                }
                my_DrawingState = 1;
                return;
            }
            if (my_DrawingState == 1)
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
        }
    }
}
