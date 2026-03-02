
namespace SudokuSolver
{
    class Cel
    {
        private int my_Index;
        private int my_Row;
        private int my_Column;
        private bool my_Fixed;
        private bool my_given;
        private bool[] my_AllowedValues;
        private int my_Number;

        public Cel(int row, int column)
        {
            if (row >= 0 & row <= 9)
            {
                my_Row = row;
            }
            else
            {
                throw new ArgumentException("The Row must be from 0 to 9 in new Cel");
            }
            if (column >= 0 & column <= 9)
            {
                my_Column = column;
            }
            else
            {
                throw new ArgumentException("The Column must be from 0 to 9 in new Cel");
            }
            my_Index = 10 * row + column;
            my_Fixed = false;
            my_given = false;
            //The cell can contain 0 or 1.
            my_AllowedValues = new bool[2];
            my_AllowedValues[0] = true;
            my_AllowedValues[1] = true;
            my_Number = 2; //2 indicates an empty cell!!!
        }

        public int Row
        {
            get { return my_Row; }
            set
            {
                if (value >= 0 & value <= 9)
                {
                    my_Row = value;
                }
                else
                {
                    throw new ArgumentException("The Row must be from 1 to 9 in Cel " + my_Index.ToString());
                }
            }
        }

        public int Col
        {
            get { return my_Column; }
            set
            {
                if (value >= 0 & value <= 9)
                {
                    my_Row = value;
                }
                else
                {
                    throw new ArgumentException("The Column must be from 1 to 9 in Cel " + my_Index.ToString());
                }
            }
        }

        public bool Fixed
        {
            get { return my_Fixed; }
            set { my_Fixed = value; }
        }

        public bool Given
        {
            get { return my_given; }
            set { my_given = value; }
        }

        public bool GetIsAllowed(int Number)
        {
            if (Number == 0 || Number == 1)
            {
                return my_AllowedValues[Number];
            }
            else
            {
                return false;
            }
        }

        public void SetIsAllowed(int Number, bool Value)
        {
            if (Number == 0 || Number == 1)
            {
                my_AllowedValues[Number] = Value;
            }
            else
            {
                throw new ArgumentException("The allowed number must be 0 or 1 in Cel " + my_Index.ToString());
            }
        }

        public int Number
        {
            get { return my_Number; }
            set
            {
                if (my_Fixed) throw new Exception("The number can not be changed in fixed Cel " + my_Index.ToString());
                if (value == 0)
                {
                    my_Number = 0;
                    my_AllowedValues[0] = true; 
                    my_AllowedValues[1] = false;
                }
                else if (value == 1)
                {
                    my_Number = 1;
                    my_AllowedValues[0] = false;
                    my_AllowedValues[1] = true;
                }
                else
                {
                    my_Number = 2;
                    my_AllowedValues[0] = true;
                    my_AllowedValues[1] = true;
                }
            }
        }

        public void Clear()
        {
            my_Number = 2;
            my_Fixed = false;
            my_given = false;
            my_AllowedValues[0] = true;
            my_AllowedValues[1] = true;
        }

        public int[] GetAllowedValues()
        {
            int teller = 0;
            if (my_AllowedValues[0]) teller++;
            if (my_AllowedValues[1]) teller++;
            int[] result = new int[teller];
            teller = 0;
                if (my_AllowedValues[0])
                {
                    result[teller] = 0;
                    teller += 1;
                }
            if (my_AllowedValues[1])
            {
                result[teller] = 1;
            }
            return result;
        }

        public int TotalAllowed()
        {
            int teller = 0;
            if (my_AllowedValues[0]) teller++;
            if (my_AllowedValues[1]) teller++;
            return teller;
        }
    }
}
