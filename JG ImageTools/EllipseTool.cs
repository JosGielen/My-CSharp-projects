using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ImageTools
{
    public class EllipseTool : MeasurementTool
    {
        private double left;
        private double top;
        private double width;
        private double height;
        private Point center;
        private Ellipse my_Ellipse;
        private Line my_HRadius;
        private Line my_VRadius;
        private Label my_VLabel;
        private Label my_HLabel;

        public EllipseTool(SettingForm settings)
        {
            New(settings);
            my_Ellipse = new Ellipse();
            my_VRadius = new Line();
            my_HRadius = new Line();
            my_HandleCount = 2; //0 = TopLeft , 1 = BottomRight
            for (int I = 0; I < my_HandleCount; I++)
            {
                my_Handles.Add(new Handle(new Point(), this));
            }
            my_VLabel = new Label();
            my_HLabel = new Label();
        }

        public override void Draw(Canvas can)
        {
            can.Children.Add(my_Ellipse);
            can.Children.Add(my_VRadius);
            can.Children.Add(my_HRadius);
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Draw(can);
            }
            can.Children.Add(my_VLabel);
            can.Children.Add(my_HLabel);
        }

        public override void Remove(Canvas can)
        {
            if (can.Children.Contains(my_Ellipse)) { can.Children.Remove(my_Ellipse); }
            if (can.Children.Contains(my_VRadius)) { can.Children.Remove(my_VRadius); }
            if (can.Children.Contains(my_HRadius)) { can.Children.Remove(my_HRadius); }
            if (can.Children.Contains(my_VLabel)) { can.Children.Remove(my_VLabel); }
            if (can.Children.Contains(my_HLabel)) { can.Children.Remove(my_HLabel); }
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
                my_VRadius.Stroke = Settings.ToolSelectedColor;
                my_VRadius.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_HRadius.Stroke = Settings.ToolSelectedColor;
                my_HRadius.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_VLabel.Background = Settings.ToolSelectedColor;
                my_VLabel.Foreground = Settings.ToolSelectedTextcolor;
                my_HLabel.Background = Settings.ToolSelectedColor;
                my_HLabel.Foreground = Settings.ToolSelectedTextcolor;
            }
            else
            {
                my_Ellipse.Stroke = Settings.ToolColor;
                my_Ellipse.StrokeThickness = Settings.ToolLineThickness;
                my_VRadius.Stroke = Settings.ToolColor;
                my_VRadius.StrokeThickness = Settings.ToolLineThickness;
                my_HRadius.Stroke = Settings.ToolColor;
                my_HRadius.StrokeThickness = Settings.ToolLineThickness;
                my_VLabel.Background = Settings.ToolColor;
                my_VLabel.Foreground = Settings.ToolTextColor;
                my_HLabel.Background = Settings.ToolColor;
                my_HLabel.Foreground = Settings.ToolTextColor;
            }
            my_VLabel.FontSize = Settings.ToolFontSize;
            my_HLabel.FontSize = Settings.ToolFontSize;
            my_VLabel.Padding = new Thickness(2.0);
            my_HLabel.Padding = new Thickness(2.0);
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Update();
            }
            //Calculate the position and size
            if (my_Handles[0].Center.X < my_Handles[1].Center.X)
            {
                left = my_Handles[0].Center.X;
            }
            else
            {
                left = my_Handles[1].Center.X;
            }
            if (my_Handles[0].Center.Y < my_Handles[1].Center.Y)
            {
                top = my_Handles[0].Center.Y;
            }
            else
            {
                top = my_Handles[1].Center.Y;
            }
            width = Math.Abs(my_Handles[0].Center.X - my_Handles[1].Center.X);
            height = Math.Abs(my_Handles[0].Center.Y - my_Handles[1].Center.Y);
            //Calculate the Center point
            center = new Point((my_Handles[0].Center.X + my_Handles[1].Center.X) / 2, (my_Handles[0].Center.Y + my_Handles[1].Center.Y) / 2);
            if (width > 0 & height > 0)
            {
                //Set the Ellipse
                my_Ellipse.Width = width;
                my_Ellipse.Height = height;
                my_Ellipse.SetValue(Canvas.LeftProperty, left);
                my_Ellipse.SetValue(Canvas.TopProperty, top);
                //Set the Vertical Radius line
                my_VRadius.X1 = center.X;
                my_VRadius.Y1 = center.Y;
                my_VRadius.X2 = center.X;
                my_VRadius.Y2 = top;
                //Set the Horizontal Radius line
                my_HRadius.X1 = center.X;
                my_HRadius.Y1 = center.Y;
                my_HRadius.X2 = left + width;
                my_HRadius.Y2 = center.Y;
            }
            //Set the Vertical Radius label in middle of the Vertical Radius Line
            string labelTextFormat = "F" + Settings.ToolDecimals.ToString();
            my_VLabel.Content = (height / 2).ToString(labelTextFormat) + my_UnitName;
            my_VLabel.Measure(new Size(120, 25));
            my_VLabel.UpdateLayout();
            my_VLabel.SetValue(Canvas.LeftProperty, center.X - my_VLabel.ActualWidth / 2);
            my_VLabel.SetValue(Canvas.TopProperty, (center.Y + top) / 2 - my_VLabel.ActualHeight / 2);
            //Set the Horizontal Radius Label in middle of the Horizontal Radius Line
            my_HLabel.Content = (width / 2).ToString(labelTextFormat) + my_UnitName;
            my_HLabel.Measure(new Size(120, 25));
            my_HLabel.UpdateLayout();
            my_HLabel.SetValue(Canvas.LeftProperty, (center.X + left + width) / 2 - my_HLabel.ActualWidth / 2);
            my_HLabel.SetValue(Canvas.TopProperty, center.Y - my_HLabel.ActualHeight / 2);
        }

        public override bool IsMouseOver(Point pt)
        {
            if (pt.X < left || pt.X > left + width) { return false; }
            if (pt.Y < top | pt.Y > top + height) { return false; }
            return true;
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
