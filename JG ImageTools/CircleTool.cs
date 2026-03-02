using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ImageTools
{
    public class CircleTool : MeasurementTool
    {
        private Point center;
        private double radius;
        private Ellipse my_Ellipse;
        private Line my_Radius;
        private Label my_Label;

        public CircleTool(SettingForm settings)
        {
            New(settings);
            my_Ellipse = new Ellipse();
            my_Radius = new Line();
            my_HandleCount = 4; //Circle is defined through 3 points and 1 handle to set the radius line direction;
            for (int I = 0; I < my_HandleCount; I++)
            {
                my_Handles.Add(new Handle(new Point(), this));
            }
            my_Label = new Label();
        }

        public override void Draw(Canvas can)
        {
            can.Children.Add(my_Ellipse);
            can.Children.Add(my_Radius);
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Draw(can);
            }
            can.Children.Add(my_Label);
        }

        public override void Remove(Canvas can)
        {
            if (can.Children.Contains(my_Ellipse)) { can.Children.Remove(my_Ellipse); }
            if (can.Children.Contains(my_Radius)) { can.Children.Remove(my_Radius); }
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
                my_Ellipse.Stroke = Settings.ToolSelectedColor;
                my_Ellipse.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_Radius.Stroke = Settings.ToolSelectedColor;
                my_Radius.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_Label.Background = Settings.ToolSelectedColor;
                my_Label.Foreground = Settings.ToolSelectedTextcolor;
            }
            else
            {
                my_Ellipse.Stroke = Settings.ToolColor;
                my_Ellipse.StrokeThickness = Settings.ToolLineThickness;
                my_Radius.Stroke = Settings.ToolColor;
                my_Radius.StrokeThickness = Settings.ToolLineThickness;
                my_Label.Background = Settings.ToolColor;
                my_Label.Foreground = Settings.ToolTextColor;
            }
            my_Label.FontSize = Settings.ToolFontSize;
            my_Label.Padding = new Thickness(2.0);
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Update();
            }
            //Calculate the radius and center
            if (!(my_Handles[0].Center.X == my_Handles[1].Center.X ||
            my_Handles[0].Center.X == my_Handles[2].Center.X ||
            my_Handles[0].Center.Y == my_Handles[1].Center.Y ||
            my_Handles[0].Center.Y == my_Handles[2].Center.Y))
            {
                double A1 = my_Handles[1].Center.X * my_Handles[1].Center.X - my_Handles[0].Center.X * my_Handles[0].Center.X;
                double B1 = my_Handles[1].Center.X - my_Handles[0].Center.X;
                double C1 = my_Handles[1].Center.Y * my_Handles[1].Center.Y - my_Handles[0].Center.Y * my_Handles[0].Center.Y;
                double D1 = my_Handles[1].Center.Y - my_Handles[0].Center.Y;
                double A2 = my_Handles[2].Center.X * my_Handles[2].Center.X - my_Handles[0].Center.X * my_Handles[0].Center.X;
                double B2 = my_Handles[2].Center.X - my_Handles[0].Center.X;
                double C2 = my_Handles[2].Center.Y * my_Handles[2].Center.Y - my_Handles[0].Center.Y * my_Handles[0].Center.Y;
                double D2 = my_Handles[2].Center.Y - my_Handles[0].Center.Y;
                double A = 1 - (D2 * B1) / (B2 * D1);
                double B = (A2 + C2) / (2 * B2);
                double C = D2 * (A1 + C1) / (2 * B2 * D1);
                center.X = (B - C) / A;
                center.Y = (A1 + C1) / (2 * D1) - B1 * center.X / D1;
            }
            radius = Math.Sqrt((center.X - my_Handles[0].Center.X) * (center.X - my_Handles[0].Center.X) + (center.Y - my_Handles[0].Center.Y) * (center.Y - my_Handles[0].Center.Y));
            if (radius > 10000) { radius = 10000; } //Prevent 3 points on 1 line cause infinite radius
                                                    //Set the Ellipse
            my_Ellipse.Width = 2 * radius;
            my_Ellipse.Height = 2 * radius;
            my_Ellipse.SetValue(Canvas.LeftProperty, center.X - radius);
            my_Ellipse.SetValue(Canvas.TopProperty, center.Y - radius);

            //Set Handle(3) on the circle
            Vector VR = my_Handles[3].Center - center;
            VR.Normalize();
            VR = radius * VR;
            my_Handles[3].SetCenter(center + VR);
            //Set the Radius line
            my_Radius.X1 = center.X;
            my_Radius.Y1 = center.Y;
            my_Radius.X2 = my_Handles[3].Center.X;
            my_Radius.Y2 = my_Handles[3].Center.Y;
            //Set the Radius label in middle of the Radius Line
            string labelTextFormat = "F" + Settings.ToolDecimals.ToString();
            my_Label.Content = radius.ToString(labelTextFormat) + my_UnitName;
            my_Label.Measure(new Size(120, 25));
            my_Label.UpdateLayout();
            my_Label.SetValue(Canvas.LeftProperty, (center.X + my_Radius.X2) / 2 - my_Label.ActualWidth / 2);
            my_Label.SetValue(Canvas.TopProperty, (center.Y + my_Radius.Y2) / 2 - my_Label.ActualHeight / 2);
        }

        public override bool IsMouseOver(Point pt)
        {
            return Math.Sqrt((pt.X - center.X) * (pt.X - center.X) + (pt.Y - center.Y) * (pt.Y - center.Y)) < radius;
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
                my_Handles[0].Show();
                return;
            }
            if (my_DrawingState == 1)
            {
                my_Handles[1].SetCenter(pt);
                my_DrawingState = 2;
                my_Handles[1].Show();
                return;
            }
            if (my_DrawingState == 2)
            {
                my_Handles[2].SetCenter(pt);
                my_Handles[2].Show();
                Update();
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
            if (my_DrawingState == 3)
            {
                my_Handles[3].SetCenter(pt);
                Update();
            }
        }
    }
}
