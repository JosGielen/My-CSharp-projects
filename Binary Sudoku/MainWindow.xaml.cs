using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using System.Media;

namespace SudokuSolver
{
    public partial class MainWindow : Window
    {
        private List<TextBox> CelList;
        private Sudoku MainSudoku;
        private Sudoku OpgaveSudoku;
        private List<int> AnalyzeResult;
        private string fileName;
        private bool ProcesTextChange;

        public MainWindow()
        {
            InitializeComponent();
            CelList = new List<TextBox>();
            MainSudoku = new Sudoku();
            OpgaveSudoku = new Sudoku();
            fileName = "";
            ProcesTextChange = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox source = new TextBox();
            //Add all textboxes to CelList and add handlers to them
            for (int I = 0; I < 100; I++)
            {
                source = (TextBox)FindName("Cel" + I.ToString());
                //source.GotFocus += CelFocus;
                source.PreviewKeyDown += CelKeyPressed;
                source.TextChanged += CelTextChanged;
                source.FontSize = 28;
                source.HorizontalContentAlignment = HorizontalAlignment.Center;
                source.VerticalContentAlignment = VerticalAlignment.Top;
                CelList.Add(source);
            }
            Title = "Binary Sudoku Solver";
            CelList[0].Focus();
        }

        #region "Menu"

        private void MnuNew_Click(object sender, RoutedEventArgs e)
        {
            //clear Mainsudoku and OpgaveSudoku
            MainSudoku.Clear();
            OpgaveSudoku.Clear();
            UpdateUI(MainSudoku);
            Title = "SudokuSolver";
            CelList[0].Focus();
        }

        private void MnuOpen_Click(object sender, RoutedEventArgs e)
        {
            //Ask for a filename and read the file in MainSudoku and in OpgaveSudoku
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.InitialDirectory = Environment.CurrentDirectory;
            OFD.Filter = "Sudoku (*.sud)|*.sud";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;
            if (OFD.ShowDialog() == true)
            {
                fileName = OFD.FileName;
                MainSudoku.Load(fileName);
                OpgaveSudoku.Load(fileName);
                UpdateUI(MainSudoku);
                Title = "SudokuSolver: " + Path.GetFileName(fileName);
                CelList[0].Focus();
            }
        }

        private void MnuSave_Click(object sender, RoutedEventArgs e)
        {
            //check if MainSudoku has a filename
            if (fileName == "" | !File.Exists(fileName))
            {
                MnuSaveAs_Click(sender, e);
            }
            else
            {
                MainSudoku.Save(fileName);
            }
        }

        private void MnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.InitialDirectory = Environment.CurrentDirectory;
            SFD.Filter = "Sudoku (*.sud)|*.sud";
            SFD.FilterIndex = 1;
            SFD.RestoreDirectory = true;
            if (SFD.ShowDialog() == true)
            {
                fileName = SFD.FileName;
                MainSudoku.Save(fileName);
                Title = "SudokuSolver: " + Path.GetFileName(fileName);
            }
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MnuPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new PrintDialog();
            StackPanel PrintPanel = new StackPanel();
            double pageWidth;
            double pageHeight;
            double sudokuWidth = SudokuBorder.ActualWidth;
            double sudokuHeight = SudokuBorder.ActualHeight;
            double leftMargin;
            double topMargin;
            double rightMargin;
            double bottomMargin;
            string My_Xaml;
            StringReader stringReader;
            XmlReader MyReader;
            Border SudokuCopy;
            printDlg.PrintTicket = printDlg.PrintQueue.DefaultPrintTicket;
            if (printDlg.ShowDialog() == true)
            {
                //Get the paper size
                pageWidth = printDlg.PrintableAreaWidth;
                pageHeight = printDlg.PrintableAreaHeight;
                //Center the sudoku on the page
                leftMargin = (pageWidth - sudokuWidth) / 2 - 20;
                rightMargin = (pageWidth - sudokuWidth) / 2 - 20;
                topMargin = 80;
                bottomMargin = 50;
                PrintPanel.Margin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
                //Copy the SudokuBorder
                My_Xaml = XamlWriter.Save(SudokuBorder);
                stringReader = new StringReader(My_Xaml);
                MyReader = XmlReader.Create(stringReader);
                SudokuCopy = (Border)XamlReader.Load(MyReader);
                //Place the sudoku copy in the PrintPanel
                PrintPanel.Children.Add(SudokuCopy);
                //Set the Sudoku print size the same as the actual screen size.
                SudokuBorder.Width = sudokuWidth;
                SudokuBorder.Height = sudokuHeight;
                //Measure and arrange the PrintPanel to fit the page
                PrintPanel.Measure(new Size(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight));
                PrintPanel.Arrange(new Rect(new Point(0, 0), PrintPanel.DesiredSize));
                //Print the PrintPanel
                printDlg.PrintVisual(PrintPanel, "SudokuSolver Print");
            }
        }

        private void MnuClear_Click(object sender, RoutedEventArgs e)
        {
            //clear MainSudoku but not OpgaveSudoku
            MainSudoku.Clear();
            UpdateUI(MainSudoku);
            CelList[0].Focus();
        }

        private void MnuStore_Click(object sender, RoutedEventArgs e)
        {
            //copy Mainsudoku naar OpgaveSudoku
            OpgaveSudoku = MainSudoku.Copy();
        }

        private void MnuRecall_Click(object sender, RoutedEventArgs e)
        {
            //copy OpgaveSudoku to MainSudoku
            MainSudoku = OpgaveSudoku.Copy();
            UpdateUI(MainSudoku);
            CelList[0].Focus();
        }

        private void MnuCheck_Click(object sender, RoutedEventArgs e)
        {
            //Check if the values are OK
            Sudoku backup = MainSudoku.Copy();
            int Aantal;
            do
            {
                Aantal = SingleScan(backup); //returns -1 when the Sudoku has an error 
            } while (Aantal > 0);
            if (Aantal == 0 && CheckValid(backup)) 
            {
                MessageBox.Show("All values are valid.", "SudokuSolver Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("The Sudoku is not correct!", "SudokuSolver Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void MnuAnalyze_Click(object sender, RoutedEventArgs e)
        {
            Sudoku backup = MainSudoku.Copy();
            string Result = "";
            for (int I = 0; I < 100; I++)
            {
                if (backup.GetCel(I).Number == 0 || backup.GetCel(I).Number == 1) backup.GetCel(I).Fixed = true;
            }
            //Check the sudoku with a single scan
            int Aantal = SingleScan(backup);
            if (Aantal == -1)
            {
                MessageBox.Show("The Sudoku is not correct!", "SudokuSolver Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                for (int I = 0; I < AnalyzeResult.Count; I++)
                {
                    CelList[AnalyzeResult[I]].Background = Brushes.LightGoldenrodYellow;
                }
                Result += AnalyzeResult.Count.ToString() + " Cells can have only 1 value.\n";
                Result += "The Scan result cells have been highlighted.";
                if (MessageBox.Show(Result, "Sudoku Analysis Result", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    for (int I = 0; I < AnalyzeResult.Count; I++)
                    {
                        CelList[AnalyzeResult[I]].Background = Brushes.White;
                    }
                }
            }
        }

        private void MnuSingleScan_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            for (int I = 0; I < 100; I++)
            {
                if (MainSudoku.GetCel(I).Number == 0 || MainSudoku.GetCel(I).Number == 1) MainSudoku.GetCel(I).Fixed = true;
            }
            //Check the sudoku with a single scan
            int Aantal = SingleScan(MainSudoku);
            ProcesTextChange = false;
            for (int I = 0; I < 100; I++)
            {
                if (MainSudoku.GetCel(I).Number == 0 || MainSudoku.GetCel(I).Number == 1)
                {
                    CelList[I].Text = MainSudoku.GetCel(I).Number.ToString();
                }
                else
                {
                    CelList[I].Text = "";
                }
            }
            ProcesTextChange = true;
            if (Aantal == -1)
            {
                MessageBox.Show("The Sudoku is not correct!", "SudokuSolver Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                Result += Aantal.ToString() + " Cells have been filled.\n";
                MessageBox.Show(Result, "Sudoku SingleScan Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MnuSolve_Click(object sender, RoutedEventArgs e)
        {
            for (int I = 0; I < 100; I++)
            {
                if (MainSudoku.GetCel(I).Number == 0 || MainSudoku.GetCel(I).Number == 1) MainSudoku.GetCel(I).Fixed = true;
            }
            if (Solve(MainSudoku, -1))
            {
                UpdateUI(MainSudoku);
                CelList[0].Focus();
            }
            else
            {
                MessageBox.Show("The sudoku can not be solved.", "SudokuSolver Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        #endregion

        #region "Solver"

        private int SingleScan(Sudoku s)
        {
            AnalyzeResult = new List<int>();
            //Step 1 : Check for empty cells with only 1 possible value
            s.UpdateValues();
            for (int I = 0; I < 100; I++)
            {
                if (s.GetCel(I).Number == 2)
                {
                    if (s.GetCel(I).TotalAllowed() == 0)
                    {
                        return -1;
                    }
                    else if (s.GetCel(I).TotalAllowed() == 1)
                    {
                        s.GetCel(I).Number = s.GetCel(I).GetAllowedValues()[0];
                        s.UpdateValues();
                        AnalyzeResult.Add(I);
                    }
                }
            }
            //Step 2 : Check for same value in adjacent cells of a row
            int index;
            for (int row = 0; row < 10; row++)
            {
                for (int I = 0; I < 8; I++)
                {
                    index = 10 * row + I;
                    if (s.GetCel(index).Number == 2 && s.GetCel(index + 1).Number == 0 && s.GetCel(index + 2).Number == 0)
                    {
                        s.GetCel(index).Number = 1;
                        s.UpdateValues();
                        AnalyzeResult.Add(index);
                    }
                    if (s.GetCel(index).Number == 2 && s.GetCel(index + 1).Number == 1 && s.GetCel(index + 2).Number == 1)
                    {
                        s.GetCel(index).Number = 0;
                        s.UpdateValues();
                        AnalyzeResult.Add(index);
                    }
                    if (s.GetCel(index).Number == 0 && s.GetCel(index + 1).Number == 0 && s.GetCel(index + 2).Number == 2)
                    {
                        s.GetCel(index + 2).Number = 1;
                        s.UpdateValues();
                        AnalyzeResult.Add(index + 2);
                    }
                    if (s.GetCel(index).Number == 1 && s.GetCel(index + 1).Number == 1 && s.GetCel(index + 2).Number == 2)
                    {
                        s.GetCel(index + 2).Number = 0;
                        s.UpdateValues();
                        AnalyzeResult.Add(index + 2);
                    }
                }
            }
            //Step 3 : Check for same value in adjacent cells of a column
            for (int col = 0; col < 10; col++)
            {
                for (int I = 0; I < 8; I++)
                {
                    index = 10 * I + col;
                    if (s.GetCel(index).Number == 2 && s.GetCel(index + 10).Number == 0 && s.GetCel(index + 20).Number == 0)
                    {
                        s.GetCel(index).Number = 1;
                        s.UpdateValues();
                        AnalyzeResult.Add(index);
                    }
                    if (s.GetCel(index).Number == 2 && s.GetCel(index + 10).Number == 1 && s.GetCel(index + 20).Number == 1)
                    {
                        s.GetCel(index).Number = 0;
                        s.UpdateValues();
                        AnalyzeResult.Add(index);
                    }
                    if (s.GetCel(index).Number == 0 && s.GetCel(index + 10).Number == 0 && s.GetCel(index + 20).Number == 2)
                    {
                        s.GetCel(index + 20).Number = 1;
                        s.UpdateValues();
                        AnalyzeResult.Add(index + 20);
                    }
                    if (s.GetCel(index).Number == 1 && s.GetCel(index + 10).Number == 1 && s.GetCel(index + 20).Number == 2)
                    {
                        s.GetCel(index + 20).Number = 0;
                        s.UpdateValues();
                        AnalyzeResult.Add(index + 20);
                    }
                }
            }
            //Step 4: Check for same value with 1 empty cell inbetween in a row
            for (int row = 0; row < 10; row++)
            {
                for (int I = 0; I < 8; I++)
                {
                    index = 10 * row + I;
                    if (s.GetCel(index).Number == 0 && s.GetCel(index + 1).Number == 2 && s.GetCel(index + 2).Number == 0)
                    {
                        s.GetCel(index + 1).Number = 1;
                        s.UpdateValues();
                        AnalyzeResult.Add(index + 1);
                    }
                    if (s.GetCel(index).Number == 1 && s.GetCel(index + 1).Number == 2 && s.GetCel(index + 2).Number == 1)
                    {
                        s.GetCel(index + 1).Number = 0;
                        s.UpdateValues();
                        AnalyzeResult.Add(index + 1);
                    }
                }
            }
            //Step 5: Check for same value with 1 empty cell inbetween in a column
            for (int col = 0; col < 10; col++)
            {
                for (int I = 0; I < 8; I++)
                {
                    index = 10 * I + col;
                    if (s.GetCel(index).Number == 0 && s.GetCel(index + 10).Number == 2 && s.GetCel(index + 20).Number == 0)
                    {
                        s.GetCel(index + 10).Number = 1;
                        s.UpdateValues();
                        AnalyzeResult.Add(index + 10);
                    }
                    if (s.GetCel(index).Number == 1 && s.GetCel(index + 10).Number == 2 && s.GetCel(index + 20).Number == 1)
                    {
                        s.GetCel(index + 10).Number = 0;
                        s.UpdateValues();
                        AnalyzeResult.Add(index + 10);
                    }
                }
            }
            return AnalyzeResult.Count;
        }

        private void TrySingleScanSolve(Sudoku s)
        {
            //Try solving the Sudoku with repeated single scans
            int Aantal;
            do
            {
                Aantal = SingleScan(s);
            } while (Aantal > 0);
        }

        private bool Solve(Sudoku s, int TestCel)
        {
            Sudoku backup = s.Copy();
            //Use single scans untill no more changes are possible
            TrySingleScanSolve(backup);
            if (backup.TotalFilled() == 100 && CheckValid(backup))
            {
                MainSudoku = backup.Copy();
                return true;
            }
            else
            {
                //No solution found with single scans so use Trial and Error
                while (true)
                {
                    //Find the next empty TestCel
                    TestCel += 1;
                    if (TestCel > 99) return false; //No valid Testcel found
                    if (backup.GetCel(TestCel).Number == 2)
                    {
                        //Try all available values in this empty TestCel
                        foreach (int trialValue in backup.GetCel(TestCel).GetAllowedValues())
                        {
                            backup.GetCel(TestCel).Number = trialValue;
                            if (Solve(backup, TestCel)) return true;
                        }
                        return false;  //None of the available values gives a solution
                    }
                };
            }
        }

        private bool CheckValid(Sudoku s)
        {
            int index;
            //Check for 3 consecutive values in a row
            for (int row = 0; row < 10; row++)
            {
                for (int I = 0; I < 8; I++)
                {
                    index = 10 * row + I;
                    if (s.GetCel(index).Number == 0 && s.GetCel(index + 1).Number == 0 && s.GetCel(index + 2).Number == 0)
                    {
                        return false;
                    }
                    if (s.GetCel(index).Number == 1 && s.GetCel(index + 1).Number == 1 && s.GetCel(index + 2).Number == 1)
                    {
                        return false;
                    }
                }
            }
            //Check for 3 consecutive values in a column
            for (int col = 0; col < 10; col++)
            {
                for (int I = 0; I < 8; I++)
                {
                    index = 10 * I + col;
                    if (s.GetCel(index).Number == 0 && s.GetCel(index + 10).Number == 0 && s.GetCel(index + 20).Number == 0)
                    {
                        return false;
                    }
                    if (s.GetCel(index).Number == 1 && s.GetCel(index + 10).Number == 1 && s.GetCel(index + 20).Number == 1)
                    {
                        return false;
                    }
                }
            }
            //Check for 2 identical rows or columns
            for (int I = 0; I < 10; I++)
            {
                for (int J = I + 1; J < 10; J++)
                {
                    if (Row2String(s, I).Equals(Row2String(s, J))) { return false; }
                    if (Col2String(s, I).Equals(Col2String(s, J))) { return false; }
                }
            }
            return true;
        }

        private string Row2String(Sudoku s, int row)
        {
            string result = "";
            for (int I = 0; I < 10; I++) 
            {
                result += s.GetCel(10 * row + I).Number.ToString();
            }
            return result;
        }

        private string Col2String(Sudoku s, int col)
        {
            string result = "";
            for (int I = 0; I < 10; I++)
            {
                result += s.GetCel(10 * I + col).Number.ToString();
            }
            return result;
        }

        #endregion

        #region "User Interface"

        private void CelKeyPressed(object sender, KeyEventArgs e)
        {
            //Process special keys (direction, delete, ...)
            string k = e.Key.ToString();
            int tag;
            TextBox source;
            if (e.OriginalSource.GetType() == typeof(TextBox))
            {
                source = (TextBox)e.OriginalSource;
                tag = int.Parse(source.Tag.ToString());
                switch (k)
                {
                    case "Delete":
                    {
                        if (!MainSudoku.GetCel(tag).Fixed)
                        {
                            if (source.SelectedText == source.Text)
                            {
                                MainSudoku.GetCel(tag).Number = 2;
                                source.Text = "";
                            }
                        }
                        else
                        {
                            e.Handled = true;
                            return;
                        }
                        break;
                    }
                    case "Back":
                    {
                        if (!MainSudoku.GetCel(tag).Fixed)
                        {
                            MainSudoku.GetCel(tag).Number = 2;
                            source.Text = "";
                        }
                        else
                        {
                            e.Handled = true;
                            return;
                        }
                        break;
                    }
                    case "Left":
                    {
                        if (tag % 10 == 0)
                        {
                            CelList[tag + 9].Focus();
                        }
                        else
                        {
                            CelList[tag - 1].Focus();
                        }
                        e.Handled = true;
                        break;
                    }
                    case "Right":
                    {
                        if ((tag + 1) % 10 == 0)
                        {
                            CelList[tag - 9].Focus();
                        }
                        else
                        {
                            CelList[tag + 1].Focus();
                        }
                        e.Handled = true;
                        break;
                    }
                    case "Up":
                    {
                        if (tag < 10)
                        {
                            CelList[90 + tag].Focus();
                        }
                        else
                        {
                            CelList[tag - 10].Focus();
                        }
                        e.Handled = true;
                        break;
                    }
                    case "Down":
                    {
                        if (tag > 89)
                        {
                            CelList[tag - 90].Focus();
                        }
                        else
                        {
                            CelList[tag + 10].Focus();
                        }
                        e.Handled = true;
                        break;
                    }
                }
                if (source.Text != "")
                {
                    e.Handled = true;
                }
            }
        }

        private void CelTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox source;
            int tag;
            int TestValue;
            if (MainSudoku == null) return;
            if (ProcesTextChange == false) return;
            if (e.OriginalSource.GetType() == typeof(TextBox))
            {
                source = (TextBox)e.OriginalSource;
                tag = int.Parse(source.Tag.ToString());
                if (int.TryParse(source.Text, out TestValue))
                {
                    int[] allowedCount = MainSudoku.GetCel(tag).GetAllowedValues();
                    bool hasTestValue = false;
                    //check if the int[] allowedCount contains TestValue
                    for (int I = 0; I < allowedCount.Length; I++)
                    {
                        if (allowedCount[I] == TestValue)
                        {
                            hasTestValue = true;
                            break;
                        }
                    }
                    if (hasTestValue)
                    {
                        MainSudoku.GetCel(tag).Number = TestValue;
                        if (MainSudoku.TotalFilled() == 100 && CheckValid(MainSudoku))
                        {
                            MessageBox.Show("Congratulations, You solved the Sudoku!", "SudokuSolver info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                    }
                    else
                    {
                        SystemSounds.Beep.Play();
                        MainSudoku.GetCel(tag).Number = 2;
                        source.Text = "";
                    }
                }
                else
                {
                    MainSudoku.GetCel(tag).Number = 2;
                    source.Text = "";
                }
                MainSudoku.UpdateValues();
            }
        }

        private void UpdateUI(Sudoku s)
        {
            ProcesTextChange = false;
            for (int I = 0; I < 100; I++)
            {
                if (s.GetCel(I).Number == 0 || s.GetCel(I).Number == 1)
                {
                    CelList[I].Text = s.GetCel(I).Number.ToString();
                }
                else
                {
                    CelList[I].Text = "";
                }
            }
            for (int I = 0; I < 100; I++)
            {
                if (s.GetCel(I).Given)
                {
                    CelList[I].Background = Brushes.LightBlue;
                }
                else
                {
                    CelList[I].Background = Brushes.White;
                }
            }
            ProcesTextChange = true;
        }

        #endregion
    }
}
