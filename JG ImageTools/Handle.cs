using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ImageTools
{
    public class Handle
    {
        private MeasurementTool my_Parent;
        private double my_Left;
        private double my_Top;
        private double my_Right;
        private double my_Bottom;
        private Point my_Center;
        private bool my_Highlighted;
        private bool my_Selected;
        private Rectangle my_rect;

        public Handle(Point center, MeasurementTool parent)
        {
            my_Parent = parent;
            my_Center = center;
            my_Highlighted = false;
            my_Selected = false;
            my_rect = new Rectangle();
        }

        public Point Center
        {
            get { return my_Center; }
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

        public bool Selected
        {
            get { return my_Selected; }
            set
            {
                my_Selected = value;
                Update();
            }
        }

        public void SetCenter(Point Pt)
        {
            my_Center = Pt;
            Update();
        }

        public void Draw(Canvas can)
        {
            can.Children.Add(my_rect);
        }

        public void Remove(Canvas can)
        {
            if (can.Children.Contains(my_rect)) { can.Children.Remove(my_rect); }
        }

        public void Update()
        {
            double my_Size;
            int my_FillAlpha;
            if (my_Parent.Highlighted)
            {
                my_rect.Stroke = my_Parent.Settings.ToolSelectedColor;
            }
            else
            {
                my_rect.Stroke = my_Parent.Settings.HandleColor;
            }
            if (my_Selected)
            {
                my_Size = my_Parent.Settings.HandleSelectedSize;
                my_rect.Stroke = my_Parent.Settings.HandleSelectedColor;
                my_FillAlpha = my_Parent.Settings.HandleSelectedAlpha;
            }
            else
            {
                my_rect.Stroke = my_Parent.Settings.HandleColor;
                my_FillAlpha = my_Parent.Settings.HandleAlpha;
                my_Size = my_Parent.Settings.HandleSize;
            }
            if (my_Highlighted) { my_Size = my_Parent.Settings.HandleSelectedSize; }
            my_rect.Fill = my_rect.Stroke;
            if (!my_rect.Fill.IsFrozen) { my_rect.Fill.Opacity = my_FillAlpha; }
            my_Left = my_Center.X - my_Size / 2;
            my_Right = my_Center.X + my_Size / 2;
            my_Top = my_Center.Y - my_Size / 2;
            my_Bottom = my_Center.Y + my_Size / 2;
            my_rect.Width = my_Size;
            my_rect.Height = my_Size;
            my_rect.SetValue(Canvas.LeftProperty, my_Left);
            my_rect.SetValue(Canvas.TopProperty, my_Top);
        }

        public void Hide()
        {
            my_rect.Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            my_rect.Visibility = Visibility.Visible;
        }

        public bool IsMouseOver(Point Pt)
        {
            if (Pt.X < my_Left || Pt.X > my_Right) { return false; }
            if (Pt.Y < my_Top | Pt.Y > my_Bottom) { return false; }
            return true;
        }
    }
}
