using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ImageTools
{
    public class RectangleTool : MeasurementTool
    {
        private double left;
        private double right;
        private double top;
        private double bottom;
        private double width;
        private double height;
        private Point center;
        private Rectangle my_Rectangle;
        private Label my_VLabel;
        private Label my_HLabel;

        public RectangleTool(SettingForm settings)
        {
            New(settings);
            my_Rectangle = new Rectangle();
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
            can.Children.Add(my_Rectangle);
            for (int I = 0; I < my_Handles.Count; I++)
            {
                my_Handles[I].Draw(can);
            }
            can.Children.Add(my_VLabel);
            can.Children.Add(my_HLabel);
        }

        public override void Remove(Canvas can)
        {
            if (can.Children.Contains(my_Rectangle)) { can.Children.Remove(my_Rectangle); }
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
                my_Rectangle.Stroke = Settings.ToolSelectedColor;
                my_Rectangle.StrokeThickness = Settings.ToolSelectedLineThickness;
                my_VLabel.Background = Settings.ToolSelectedColor;
                my_VLabel.Foreground = Settings.ToolSelectedTextcolor;
                my_HLabel.Background = Settings.ToolSelectedColor;
                my_HLabel.Foreground = Settings.ToolSelectedTextcolor;
            }
            else
            {
                my_Rectangle.Stroke = Settings.ToolColor;
                my_Rectangle.StrokeThickness = Settings.ToolLineThickness;
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
                right = my_Handles[1].Center.X;
            }
            else
            {
                left = my_Handles[1].Center.X;
                right = my_Handles[0].Center.X;
            }
            if (my_Handles[0].Center.Y < my_Handles[1].Center.Y)
            {
                top = my_Handles[0].Center.Y;
                bottom = my_Handles[1].Center.Y;
            }
            else
            {
                top = my_Handles[1].Center.Y;
                bottom = my_Handles[0].Center.Y;
            }
            width = Math.Abs(right - left);
            height = Math.Abs(bottom - top);
            //Calculate the Center point
            center = new Point((left + right) / 2, (top + bottom) / 2);
            if (width > 0 & height > 0)
            {
                //Set the Ellipse
                my_Rectangle.Width = width;
                my_Rectangle.Height = height;
                my_Rectangle.SetValue(Canvas.LeftProperty, left);
                my_Rectangle.SetValue(Canvas.TopProperty, top);
            }
            //Set the Vertical Radius label in middle of the Vertical Radius Line
            string labelTextFormat = "F" + Settings.ToolDecimals.ToString();
            my_VLabel.Content = height.ToString(labelTextFormat) + my_UnitName;
            my_VLabel.Measure(new Size(120, 25));
            my_VLabel.UpdateLayout();
            my_VLabel.SetValue(Canvas.LeftProperty, right - my_VLabel.ActualWidth / 2);
            my_VLabel.SetValue(Canvas.TopProperty, center.Y - my_VLabel.ActualHeight / 2);
            //Set the Horizontal Radius Label in middle of the Horizontal Radius Line
            my_HLabel.Content = width.ToString(labelTextFormat) + my_UnitName;
            my_HLabel.Measure(new Size(120, 25));
            my_HLabel.UpdateLayout();
            my_HLabel.SetValue(Canvas.LeftProperty, center.X - my_HLabel.ActualWidth / 2);
            my_HLabel.SetValue(Canvas.TopProperty, top - my_HLabel.ActualHeight / 2);
        }

        public override bool IsMouseOver(Point pt)
        {
            if (pt.X < left || pt.X > right) { return false; }
            if (pt.Y < top | pt.Y > bottom) { return false; }
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
