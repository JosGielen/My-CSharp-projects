using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.VisualBasic;

namespace NeuralNet_Handwriting_Recognition
{
    public partial class MainWindow : Window
    {
        private int WaitTime = 10;
        private NeuralNet NN;
        private string FilePath = "";
        private string TrainingDataFile = "";
        private string TestDataFile = "";
        private List<TrainingData> TrainingDatas;
        private List<TrainingData> TestDatas;
        private List<int> TestTargets;
        private int TestNr = 0;
        private bool IsNNCreated = false;
        private int correctNr = 0;
        private bool IsTraining = false;
        private double progress = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MnuLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (FilePath == "")
            {
                openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            }
            else
            {
                openFileDialog1.InitialDirectory = FilePath;
            }
            openFileDialog1.Filter = "All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == true)
            {
                FilePath = Path.GetDirectoryName(openFileDialog1.FileName);
                try
                {
                    Cursor = Cursors.Wait;
                    NN = NeuralNet.LoadFromFile(openFileDialog1.FileName);
                    TxtInputNodes.Text = NN.InputNodes.ToString();
                    TxtHiddenNodes.Text = NN.HiddenNodes.ToString();
                    TxtOutputNodes.Text = NN.OutputNodes.ToString();
                    TxtLearningRate.Text = NN.LearningRate.ToString();
                    CBNormalize.IsChecked = NN.NormalizeWeights;
                    Cursor = Cursors.Arrow;
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Cannot load the NeuralNet data. Original error: " + Ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            IsNNCreated = true;
            MnuData.IsEnabled = true;
            correctNr = 0;
        }

        private void MnuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            if (FilePath == "")
            {
                saveFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            }
            else
            {
                saveFileDialog1.InitialDirectory = FilePath;
            }
            saveFileDialog1.Filter = "All Files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == true)
            {
                FilePath = Path.GetDirectoryName(saveFileDialog1.FileName);
                try
                {
                    Cursor = Cursors.Wait;
                    NN.SaveToFile(saveFileDialog1.FileName);
                    Cursor = Cursors.Arrow;
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Cannot save the NeuralNet data. Original error: " + Ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MnuSelectTrainData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            StreamReader sr;
            string line;
            int target;
            string data;
            TrainingData td;
            if (TrainingDataFile == "")
            {
                openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            }
            else
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(TrainingDataFile);
            }
            openFileDialog1.Filter = "All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == true)
            {
                TrainingDataFile = openFileDialog1.FileName;
                TrainingDatas = new List<TrainingData>();
                try
                {
                    sr = new StreamReader(openFileDialog1.FileName);
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        target = int.Parse(line[0].ToString());
                        data = Strings.Right(line, line.Length - 2);
                        td = new TrainingData(784, data, target);
                        TrainingDatas.Add(td);
                    }
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Cannot load the Trainingdata. Original error: " + Ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            TxtTrainingData.Text = Path.GetFileName(TrainingDataFile);
            if (IsNNCreated)
            {
                BtnTrain.IsEnabled = true;
            }
        }

        private void MnuSelectTestData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            StreamReader sr;
            string line;
            int target;
            string data;
            TrainingData td;

            if (TestDataFile == "")
            {
                openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            }
            else
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(TestDataFile);
            }
            openFileDialog1.Filter = "All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == true)
            {
                TestDataFile = openFileDialog1.FileName;
                TestDatas = new List<TrainingData>();
                TestTargets = new List<int>();
                try
                {
                    sr = new StreamReader(openFileDialog1.FileName);
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        target = int.Parse(line[0].ToString());
                        data = Strings.Right(line, line.Length - 2);
                        td = new TrainingData(784, data, target);
                        TestDatas.Add(td);
                        TestTargets.Add(target);
                    }
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Cannot load the testdata. Original error: " + Ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                TxtTestData.Text = Path.GetFileName(TestDataFile);
                TestNr = 0;
                TxtSampleNr.Text = (TestNr + 1).ToString();
                ShowTest(TestNr);
                if (IsNNCreated)
                {
                    BtnTest.IsEnabled = true;
                    BtnNext.IsEnabled = true;
                    BtnAll.IsEnabled = true;
                }
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            int inputNr;
            int hiddenNr;
            int outputNr;
            double lr;
            bool normaldist;
            try
            {
                inputNr = int.Parse(TxtInputNodes.Text);
                hiddenNr = int.Parse(TxtHiddenNodes.Text);
                outputNr = int.Parse(TxtOutputNodes.Text);
                lr = double.Parse(TxtLearningRate.Text);
                normaldist = (bool)CBNormalize.IsChecked;
                NN = new NeuralNet(inputNr, hiddenNr, outputNr, lr, normaldist);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create the NeuralNet with the given parameters. Original error: " + ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IsNNCreated = true;
            correctNr = 0;
            MnuData.IsEnabled = true;
        }

        private void BtnTrain_Click(object sender, RoutedEventArgs e)
        {
            int trainSize;
            int index = 0;
            bool trainResult;
            int wrongGuessCount = 0;
            Random rnd = new Random();
            //Show a Training Graph
            NeuralNet_TrainingGraph TG = new NeuralNet_TrainingGraph(this);
            TG.Show();
            try
            {
                trainSize = int.Parse(TxtTrainingSize.Text);
                TxtMaxTrainingSize.Text = trainSize.ToString();
                IsTraining = true;
                for (int I = 0; I < trainSize; I++)
                {
                    if (!IsTraining) break;
                    index = rnd.Next(TrainingDatas.Count);
                    trainResult = NN.Train(TrainingDatas[index].data, TrainingDatas[index].targets);
                    if (!trainResult) wrongGuessCount += 1;
                    //Show the training progress
                    progress = I / (double)trainSize;
                    TxtTrainNr.Text = (I + 1).ToString();
                    //Update the Training Graph
                    if (I > 0 && I % 100 == 0)
                    {
                        TG.AddDataPoint(new Point(I, wrongGuessCount));
                        wrongGuessCount = 0;
                    }
                    Dispatcher.Invoke(UpdateStatus, DispatcherPriority.SystemIdle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot train the NeuralNet with the given parameters. Original error: " + ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
                TG.Close();
            }
            correctNr = 0;
        }

        public void StopTraining()
        {
            IsTraining = false;
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            double[] outp;
            double max;
            int result = 0;
            outp = NN.Query(TestDatas[TestNr].data);
            TxtOutput0.Text = outp[0].ToString();
            TxtOutput1.Text = outp[1].ToString();
            TxtOutput2.Text = outp[2].ToString();
            TxtOutput3.Text = outp[3].ToString();
            TxtOutput4.Text = outp[4].ToString();
            TxtOutput5.Text = outp[5].ToString();
            TxtOutput6.Text = outp[6].ToString();
            TxtOutput7.Text = outp[7].ToString();
            TxtOutput8.Text = outp[8].ToString();
            TxtOutput9.Text = outp[9].ToString();
            max = 0;
            for (int I = 0; I < 10; I++)
            {
                if (outp[I] > max)
                {
                    max = outp[I];
                    result = I;
                }
            }
            txtResult.Text = result.ToString();
            if (result == TestTargets[TestNr]) correctNr += 1;
            txtPercentOK.Text = Math.Round(100.0 * correctNr / (TestNr + 1), 2).ToString();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            TestNr += 1;
            TxtSampleNr.Text = (TestNr + 1).ToString();
            ShowTest(TestNr);
        }

        private void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            double[] outp = new double[10];
            double max = 0;
            int result = 0;
            correctNr = 0;
            txtPercentOK.Text = "0";
            for (int I = 0; I < TestDatas.Count; I++)
            {
                outp = NN.Query(TestDatas[I].data);
                TxtOutput0.Text = outp[0].ToString();
                TxtOutput1.Text = outp[1].ToString();
                TxtOutput2.Text = outp[2].ToString();
                TxtOutput3.Text = outp[3].ToString();
                TxtOutput4.Text = outp[4].ToString();
                TxtOutput5.Text = outp[5].ToString();
                TxtOutput6.Text = outp[6].ToString();
                TxtOutput7.Text = outp[7].ToString();
                TxtOutput8.Text = outp[8].ToString();
                TxtOutput9.Text = outp[9].ToString();
                max = 0;
                for (int J = 0; J < 10; J++)
                {
                    if (outp[J] > max)
                    {
                        max = outp[J];
                        result = J;
                    }
                }
                if (result == TestTargets[I]) correctNr += 1;
                TxtSampleNr.Text = I.ToString();
                txtPercentOK.Text = Math.Round(100.0 * correctNr / (I + 1), 2).ToString();
                Dispatcher.Invoke(Wait, DispatcherPriority.ApplicationIdle);
            }
        }

        private void ShowTest(int Nr)
        {
            WriteableBitmap Writebitmap = new WriteableBitmap(28, 28, 96, 96, PixelFormats.Gray8, BitmapPalettes.Gray256);
            int Stride = (Writebitmap.PixelWidth * Writebitmap.Format.BitsPerPixel / 8);
            Int32Rect Intrect = new Int32Rect(0, 0, Writebitmap.PixelWidth, Writebitmap.PixelHeight);
            ImageBrush brush = new ImageBrush();
            byte[] invertedData = new byte[TestDatas[Nr].data.Length];


            double max = 0.0;
            double min = double.MaxValue;
            double value;

            for (int I = 0; I < TestDatas[Nr].data.Length; I++)
            {
                invertedData[I] = (byte)(255 - (255 * TestDatas[Nr].data[I]));

                value = TestDatas[Nr].data[I];
                if (value > max) max = value;
                if (value < min) min = value;
            }
            Writebitmap.WritePixels(Intrect, invertedData, Stride, 0);
            brush.ImageSource = Writebitmap;
            CnvSample.Background = brush;
            txtTarget.Text = TestTargets[Nr].ToString();
        }

        private void UpdateStatus()
        {
            PBTraining.Value = 100 * progress;
        }

        private void Wait()
        {
            Thread.Sleep(WaitTime);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
