using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Matrix_Rain
{
    public class Symbol : Shape
    {
        private Point my_Origin;
        private string my_Text;
        private Typeface face;
        private FormattedText ftext;
        private Color my_Color;
        private FontFamily my_FontFamily;
        private int my_FontSize;
        private FontStyle my_FontStyle;
        private FontWeight My_FontWeight;
        private static Random Rnd = new Random();

        public Symbol(Point origin, Color color, int fontsize)
        {
            my_Origin = origin;
            my_Text = ((char)(12448 + Rnd.Next(96))).ToString();
            my_Color = color;
            my_FontFamily = new FontFamily("Arial");
            my_FontSize = fontsize;
            my_FontStyle = FontStyles.Normal;
            My_FontWeight = FontWeights.Bold;
            face = new Typeface(my_FontFamily, my_FontStyle, My_FontWeight, FontStretches.Normal);
            ftext = new FormattedText(my_Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, face, my_FontSize * 96.0 / 72.0, new SolidColorBrush(my_Color), VisualTreeHelper.GetDpi(this).PixelsPerDip);
            Height = ftext.Height;
            Width = ftext.Width;
        }

        public double Top
        {
            get { return (double)this.GetValue(Canvas.TopProperty); }
        }

        public Color TxtColor
        {
            get { return my_Color; }
            set { my_Color = value; }
        }

        public void UpdateChar(int chance)
        {
            if (Rnd.Next(100) < chance)
            {
                my_Text = ((char)(12448 + Rnd.Next(96))).ToString();
                ftext = new FormattedText(my_Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, face, my_FontSize * 96.0 / 72.0, new SolidColorBrush(my_Color), VisualTreeHelper.GetDpi(this).PixelsPerDip);
                InvalidateVisual();
            }
        }

        protected Geometry Property
        {
            get { return Geometry.Empty; }
        }

        protected override Geometry DefiningGeometry 
        {
            get { return Geometry.Empty; }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(ftext, new Point(my_Origin.X, 0));
        }
    }
}
