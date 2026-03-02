using System;
using System.IO;
using System.Windows;

namespace SudokuSolver
{
    class Sudoku
    {
        private Cel[] cells = new Cel[100];

        public Sudoku()
        {
            for (int I = 0; I < 100; I++)
            {
                cells[I] = new Cel(I / 10, I % 10);
            }
        }

        public Sudoku Copy()
        {
            Sudoku s = new Sudoku();
            for (int I = 0; I < 100; I++)
            {
                s.SetCel(I, new Cel(cells[I].Row, cells[I].Col));
                s.GetCel(I).Number = cells[I].Number;
                s.GetCel(I).SetIsAllowed(0, cells[I].GetIsAllowed(0));
                s.GetCel(I).SetIsAllowed(1, cells[I].GetIsAllowed(1));
                s.GetCel(I).Fixed = cells[I].Fixed;
                s.GetCel(I).Given = cells[I].Given;
            }
            return s;
        }

        public Cel GetCel(int index)
        {
            return cells[index];
        }

        public void SetCel(int index, Cel value)
        {
            cells[index] = value;
        }

        public void SetFixedCel(int index, int value)
        {
            cells[index].Number = value;
            cells[index].Fixed = true;
        }

        public void ClearFixedCel(int index)
        {
            cells[index].Fixed = false;
            cells[index].Number = 0;
        }

        public void SetGivenCel(int index, int value)
        {
            cells[index].Number = value;
            cells[index].Given = true;
        }

        public void ClearGivenCel(int index)
        {
            cells[index].Given = false;
            cells[index].Number = 0;
        }
        public void Clear()
        {
            for (int I = 0; I < 100; I++)
            {
                cells[I] = new Cel(I / 10, I % 10);
            }
        }

        public void ClearCell(int index)
        {
            cells[index].Clear();
        }

        public int TotalFilled()
        {
            int teller = 0;
            for (int I = 0; I < 100; I++)
            {
                if (cells[I].Number == 0 || cells[I].Number == 1)
                {
                    teller += 1;
                }
            }
            return teller;
        }

        public void UpdateValues()
        {
            //Check the allowed values for each cel
            int r;
            int c;
            int b;
            for (int I = 0; I < 100; I++)
            {
                if (cells[I].Number == 2) //Cell is empty
                {
                    r = cells[I].Row;
                    c = cells[I].Col;
                    //Step1: allow both values 0 and 1
                    cells[I].SetIsAllowed(0, true);
                    cells[I].SetIsAllowed(1, true);
                    //Step 2: Check all cells in the same row and column
                    //        if the row or column already has 5 0's then no 0 allowed
                    int rCount = 0;
                    int cCount = 0;
                    for (int J = 0; J < 100; J++)
                    {
                        if (cells[J].Row == r && cells[J].Number == 0) { rCount++; }
                        if (cells[J].Col == c && cells[J].Number == 0) { cCount++; }
                    }
                    if ( rCount == 5 || cCount == 5)
                    {
                        cells[I].SetIsAllowed(0, false);
                    }
                    //        if the row or column already has 5 1's then no 1 allowed
                    rCount = 0;
                    cCount = 0;
                    for (int J = 0; J < 100; J++)
                    {
                        if (cells[J].Row == r && cells[J].Number == 1) { rCount++; }
                        if (cells[J].Col == c && cells[J].Number == 1) { cCount++; }
                    }
                    if (rCount == 5 || cCount == 5)
                    {
                        cells[I].SetIsAllowed(1, false);
                    }
                }
            }
        }

        public void Load(string filename)
        {
            Clear();
            StreamReader myStream = null;
            string S = "";
            try
            {
                myStream = new StreamReader(filename);
                if (myStream != null)
                {
                    //Read the Sudoku data from the file
                    S = myStream.ReadLine();
                    //fill the numbers in the sudoku cells
                    for (int I = 0; I < 100; I++)
                    {
                        cells[I].Number = int.Parse(S.Substring(I, 1));
                        if (cells[I].Number == 0 || cells[I].Number == 1)
                        {
                            cells[I].Fixed = true;
                            cells[I].Given = true;
                        }
                    }
                    UpdateValues();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Cannot read the Sudoku data. Original error: " + Ex.Message, "SudokuSolver error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (myStream != null) { myStream.Close(); }
            }
        }

        public void Save(string filename)
        {
            StreamWriter myStream = null;
            try
            {
                myStream = new StreamWriter(filename);
                if (myStream != null)
                {
                    //Write the Sudoku data to the file
                    for (int I = 0; I < 100; I++)
                    {
                        myStream.Write(cells[I].Number);
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Cannot save the Sudoku data. Original error: " + Ex.Message, "SudokuSolver error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (myStream != null) { myStream.Close(); }
            }
        }
    }
}
