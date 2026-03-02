using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageTools
{
    public class AngleTool : MeasurementTool
    {
        private double my_Angle;
        private Line my_Line1;
        private Line my_Line2;
        private Label my_Label;
        private Path my_Path;
        private PathGeometry my_PG;
        private PathFigure my_figure;

        public AngleTool(SettingForm settings)
        {
            New(settings);
            my_Line1 = new Line();
            my_Line2 = new Line();
            my_HandleCount = 3; //0 = Corner , 1 = End of Line1 , 2 = End of Line2;
            for (int I = 0; I < my_HandleCount; I++)
            {
                my_Handles.Add(new Handle(new Point(), this));
            }
            my_Path = new Path();
            my_PG = new PathGeometry();
            my_figure = new PathFigure();
            my_PG.Figures.Add(my_figure);
            my_Path.Data = my_PG;
            my_Label = new Label();
            my_DrawingState = 0;
        }

        public override void Draw(Canvas can)
        {
            can.Children.Add(my_Line1);
            can.Children.Add(my_Line2);
            can.Children.Add(my_Path);
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
            if (can.Children.Contains(my_Path)) { can.Children.Remove(my_Path); }
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
                my_Path.Stroke = Settings.ToolSelectedColor;
                my_Path.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_Label.Background = Settings.ToolSelectedColor;
                my_Label.Foreground = Settings.ToolSelectedTextcolor;
            }
            else
            {
                my_Line1.Stroke = Settings.ToolColor;
                my_Line1.StrokeThickness = Settings.ToolLineThickness;
                my_Line2.Stroke = Settings.ToolColor;
                my_Line2.StrokeThickness = Settings.ToolLineThickness;
                my_Path.Stroke = Settings.ToolColor;
                my_Path.StrokeThickness = Settings.ToolLineThickness;
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
            my_Line2.X1 = my_Handles[0].Center.X;
            my_Line2.Y1 = my_Handles[0].Center.Y;
            my_Line2.X2 = my_Handles[2].Center.X;
            my_Line2.Y2 = my_Handles[2].Center.Y;
            //Calculate the Angle
            Vector V1 = new Vector(my_Handles[1].Center.X - my_Handles[0].Center.X, my_Handles[1].Center.Y - my_Handles[0].Center.Y);
            Vector V2 = new Vector(my_Handles[2].Center.X - my_Handles[0].Center.X, my_Handles[2].Center.Y - my_Handles[0].Center.Y);
            my_Angle = Math.Abs(Vector.AngleBetween(V1, V2));
            //Set the arc segment
            //Step1: Determine the startpoint, endpoint and SweepDirection of the arc
            double dist1 = Math.Sqrt(V1.X * V1.X + V1.Y * V1.Y);
            double dist2 = Math.Sqrt(V2.X * V2.X + V2.Y * V2.Y);
            double ArcRadius = 0.0;
            if (dist1 > dist2)
            {
                ArcRadius = dist2 / 2;
            }
            else
            {
                ArcRadius = dist1 / 2;
            }
            if (ArcRadius > 150.0) ArcRadius = 150.0;
            V1.Normalize();
            V1 = ArcRadius * V1;
            V2.Normalize();
            V2 = ArcRadius * V2;
            SweepDirection sweepDir;
            if (Vector.AngleBetween(V1, V2) > 0)
            {
                sweepDir = SweepDirection.Clockwise;
            }
            else
            {
                sweepDir = SweepDirection.Counterclockwise;
            }
            Point arcpt1 = new Point(my_Handles[0].Center.X + V1.X, my_Handles[0].Center.Y + V1.Y);
            Point arcpt2 = new Point(my_Handles[0].Center.X + V2.X, my_Handles[0].Center.Y + V2.Y);
            //Step2: Make an ArcSegment and set it in my_figure
            my_figure.Segments.Clear();
            my_figure.StartPoint = arcpt1;
            my_figure.Segments.Add(new ArcSegment(arcpt2, new Size(ArcRadius, ArcRadius), my_Angle, false, sweepDir, true));
            if (my_DrawingState != 1)
            {
                //Set the arc label in middle of the arc
                string labelTextFormat = "F" + Settings.ToolDecimals.ToString();
                my_Label.Content = my_Angle.ToString(labelTextFormat) + "°";
                my_Label.Measure(new Size(120, 25));
                my_Label.UpdateLayout();
                Vector VC = new Vector((arcpt1.X + arcpt2.X) / 2 - my_Handles[0].Center.X, (arcpt1.Y + arcpt2.Y) / 2 - my_Handles[0].Center.Y);
                VC.Normalize();
                VC = VC * ArcRadius;
                my_Label.SetValue(Canvas.LeftProperty, my_Handles[0].Center.X + VC.X - my_Label.ActualWidth / 2);
                my_Label.SetValue(Canvas.TopProperty, my_Handles[0].Center.Y + VC.Y - my_Label.ActualHeight / 2);
            }

        }

        public override bool IsMouseOver(Point pt)
        {
            //Check if Pt lies inside the triangle of Handles(0 to 2)
            Vector V0 = new Vector(my_Handles[0].Center.X, my_Handles[0].Center.Y);
            Vector V1 = new Vector(my_Handles[1].Center.X, my_Handles[1].Center.Y);
            Vector V2 = new Vector(my_Handles[2].Center.X, my_Handles[2].Center.Y);
            Vector P = new Vector(pt.X, pt.Y);
            Vector VP0 = V0 - P;
            Vector VP1 = V1 - P;
            Vector VP2 = V2 - P;
            double Angle = Math.Abs(Vector.AngleBetween(VP0, VP1));
            Angle += Math.Abs(Vector.AngleBetween(VP1, VP2));
            Angle += Math.Abs(Vector.AngleBetween(VP2, VP0));
            return !(Angle <= 355);
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
                my_Handles[2].SetCenter(pt);
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
