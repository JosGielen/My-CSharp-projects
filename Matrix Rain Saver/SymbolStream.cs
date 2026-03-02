
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Matrix_Rain
{
    public class SymbolStream
    {
        private Symbol[] symbols;
        private int my_length;
        private double my_speed;
        private Point my_location;
        private double my_CharHeight;
        private int UpdateChance = 3;

        public SymbolStream(int length, double speed, Point loc, int fontSize, double charHeight)
        {
            my_length = length;
            my_speed = speed;
            my_location = loc;
            my_CharHeight = charHeight;
            symbols = new Symbol[my_length];
            for (int I = 0; I < my_length; I++)
            {
                symbols[I] = new Symbol(my_location, Color.FromArgb((byte)(255 - 200 * (my_length - I) / my_length), (byte)(220 - 220 * (my_length - I) / my_length), 255, (byte)(220 - 220 * (my_length - I) / my_length)), fontSize);
                symbols[I].SetValue(Canvas.LeftProperty, my_location.X);
                symbols[I].SetValue(Canvas.TopProperty, my_location.Y + my_CharHeight * I);
            }
            symbols.Last().TxtColor = Color.FromRgb(255, 255, 255);
        }

        public double Top
        {
            get { return my_location.Y; }
            set { my_location.Y = value; }
        }

        public int length
        {
            get { return my_length; }
            set { my_length = value; }
        }

        public double Symbol1Top
        {
            get { return symbols[0].Top; }
        }

        public void Update()
        {
            my_location.Y += my_speed;
            for (int I = 0; I < my_length; I++)
            {
                symbols[I].SetValue(Canvas.TopProperty, my_location.Y + my_CharHeight * I);
                if (my_location.Y > -1 * my_length * my_CharHeight)
                {
                    symbols[I].UpdateChar(UpdateChance);
                }
            }
        }

        public void Draw(Canvas can)
        {
            for (int I = 0; I < my_length; I++)
            {
                can.Children.Add(symbols[I]);
            }
        }

    }
}
