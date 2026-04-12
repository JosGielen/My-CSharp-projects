using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JG_Graphs;

namespace NeuralNet_Handwriting_Recognition
{
    /// <summary>
    /// Interaction logic for NeuralNet_TrainingGraph.xaml
    /// </summary>
    public partial class NeuralNet_TrainingGraph : Window
    {
        private ScatterSeries ss;
        private MainWindow my_Parent;

        public NeuralNet_TrainingGraph(MainWindow parent)
        {
            InitializeComponent();
            my_Parent = parent;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ss = new ScatterSeries(1);
            ss.SeriesName = "Guess Failure Percentage";
            ss.MarkerType = MarkerType.None;
            ss.AddDataPoint(new Point(0, 100));
            TrainGraph.DataSeries.Add(ss);
            TrainGraph.YAxis.FixedMaximum = false;
            TrainGraph.YAxis.Maximum = 100.0;
            TrainGraph.YAxis.FixedMinimum = true;
            TrainGraph.YAxis.Minimum = 0.0;
            TrainGraph.XAxis.AxisLabel = "Training Count";
            TrainGraph.LegendPosition = LegendPosition.Top;
        }

        public void AddDataPoint(Point dataPt)
        {
            ss.AddDataPoint(dataPt);
            TrainGraph.Draw();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            my_Parent.StopTraining();
        }
    }
}
