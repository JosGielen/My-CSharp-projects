using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace PlotDigitizer
{
    public partial class DataExport : Window
    {
        private string My_Action = "";
        private string My_GraphTitle = "";
        private string My_XLabel = "";
        private string My_XOrigin = "";
        private string My_XMax = "";
        private string My_XScale = "";
        private string My_YLabel = "";
        private string My_YOrigin = "";
        private string My_YMax = "";
        private string My_YScale = "";
        private List<Point> My_Data = new List<Point>();

        public DataExport(string action)
        {
            InitializeComponent();
            My_Action = action;
            if (My_Action.Equals("copy"))
            {
                Title = "Copy Data";
                BtnInfo.Content = "Copy Data and Graph Info";
                BtnData.Content = "Copy Data Only";
            }
            else if (My_Action.Equals("save"))
            {
                Title = "Save Data";
                BtnInfo.Content = "Save Data and Graph Info";
                BtnData.Content = "Save Data Only";
            }
        }

        public string GraphTitle
        {
            get
            {
                return My_GraphTitle;
            }
            set
            {
                My_GraphTitle = value;
                TxtGraphTitle.Text = value;
            }
        }

        public string XLabel
        {
            get
            {
                return My_XLabel;
            }
            set
            {
                My_XLabel = value;
                TxtXLabel.Text = value;
            }
        }

        public string XOrigin
        {
            get
            {
                return My_XOrigin;
            }
            set
            {
                My_XOrigin = value;
                TxtXMinValue.Text = value;
            }
        }

        public string XMax
        {
            get
            {
                return My_XMax;
            }
            set
            {
                My_XMax = value;
                TxtXMaxValue.Text = value;
            }
        }

        public string XScale
        {
            get
            {
                return My_XScale;
            }
            set
            {
                My_XScale = value;
                TxtXScale.Text = value;
            }
        }

        public string YLabel
        {
            get
            {
                return My_YLabel;
            }
            set
            {
                My_YLabel = value;
                TxtYLabel.Text = value;
            }
        }

        public string YOrigin
        {
            get
            {
                return My_YOrigin;
            }
            set
            {
                My_YOrigin = value;
                TxtYMinValue.Text = value;
            }
        }

        public string YMax
        {
            get
            {
                return My_YMax;
            }
            set
            {
                My_YMax = value;
                TxtYMaxValue.Text = value;
            }
        }

        public string YScale
        {
            get
            {
                return My_YScale;
            }
            set
            {
                My_YScale = value;
                TxtYScale.Text = value;
            }
        }

        public List<Point> Data
        {
            get
            {
                return My_Data;
            }
            set
            {
                My_Data = value;
                TxtDataPointsX.Text = "";
                TxtDataPointsY.Text = "";
                for (int I = 0; I < My_Data.Count; I++)
                {
                    TxtDataPointsX.Text += My_Data[I].X.ToString("G4") + "\n";
                    TxtDataPointsY.Text += My_Data[I].Y.ToString("G4") + "\n";
                }
            }
        }

        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            My_GraphTitle = TxtGraphTitle.Text;
            My_XLabel = TxtXLabel.Text;
            My_YLabel = TxtYLabel.Text;
            if (My_Action.Equals("copy"))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Title: \t" + My_GraphTitle);
                sb.AppendLine("X-axis");
                sb.AppendLine("------");
                sb.AppendLine("- Label: \t" + My_XLabel);
                sb.AppendLine("- Range: \t" + My_XOrigin + "\tto\t" + My_XMax);
                sb.AppendLine("- Scale: \t" + My_XScale);
                sb.AppendLine("Y-axis");
                sb.AppendLine("------");
                sb.AppendLine("- Label: \t" + My_YLabel);
                sb.AppendLine("- Range: \t" + My_YOrigin + "\tto\t" + My_YMax);
                sb.AppendLine("- Scale: \t" + My_YScale);
                sb.AppendLine("Data");
                sb.AppendLine("----");
                sb.AppendLine("  X  \t" + "  Y  ");
                foreach (Point pt in Data)
                {
                    sb.AppendLine(pt.X.ToString("G4") + "\t" + pt.Y.ToString("G4"));
                }
                Clipboard.Clear();
                Clipboard.SetText(sb.ToString());
            }
            else if (My_Action.Equals("save"))
            {
                string fileName;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog()
                {
                    InitialDirectory = Environment.CurrentDirectory,
                    Filter = "Text Files (*.txt)|*.txt",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                if (saveFileDialog1.ShowDialog().Value == true)
                {
                    fileName = saveFileDialog1.FileName;
                    StreamWriter outfile = new StreamWriter(fileName);
                    outfile.WriteLine("Title: " + My_GraphTitle);
                    outfile.WriteLine();
                    outfile.WriteLine("X-axis");
                    outfile.WriteLine("------");
                    outfile.WriteLine("- Label: " + My_XLabel);
                    outfile.WriteLine("- Range: " + My_XOrigin + " to " + My_XMax);
                    outfile.WriteLine("- Scale: " + My_XScale);
                    outfile.WriteLine();
                    outfile.WriteLine("Y-axis");
                    outfile.WriteLine("------");
                    outfile.WriteLine("- Label: " + My_YLabel);
                    outfile.WriteLine("- Range: " + My_YOrigin + " to " + My_YMax);
                    outfile.WriteLine("- Scale: " + My_YScale);
                    outfile.WriteLine();
                    outfile.WriteLine("Data");
                    outfile.WriteLine("----");
                    outfile.WriteLine("  X  ,  Y  ");
                    foreach (Point pt in Data)
                    {
                        outfile.WriteLine(pt.X.ToString("G4") + " , " + pt.Y.ToString("G4"));
                    }
                    outfile.Close();
                }
            }
            Close();
        }

        private void BtnData_Click(object sender, RoutedEventArgs e)
        {
            My_GraphTitle = TxtGraphTitle.Text;
            My_XLabel = TxtXLabel.Text;
            My_YLabel = TxtYLabel.Text;
            if (My_Action.Equals("copy"))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("X \t Y");
                foreach (Point pt in Data)
                {
                    sb.AppendLine(pt.X.ToString("G4") + "\t" + pt.Y.ToString("G4"));
                }
                Clipboard.Clear();
                Clipboard.SetText(sb.ToString());
            }
            else if (My_Action.Equals("save"))
            {
                string fileName;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog()
                {
                    InitialDirectory = Environment.CurrentDirectory,
                    Filter = "Text Files (*.txt)|*.txt",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                if (saveFileDialog1.ShowDialog().Value == true)
                {
                    fileName = saveFileDialog1.FileName;
                    StreamWriter outfile = new StreamWriter(fileName);
                    outfile.WriteLine("  X  ,  Y  ");
                    foreach (Point pt in Data)
                    {
                        outfile.WriteLine(pt.X.ToString("G4") + " , " + pt.Y.ToString("G4"));
                    }
                    outfile.Close();
                }
            }
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
