using System.Windows;

namespace NeuralNet_Handwriting_Recognition
{
    //NeuralNet Handwriting Recognition Testing
    class TrainingData
    {
        private double[] my_data;
        private double[] my_targets;

        public TrainingData(int size, string data, int target)
        {
            my_data = new double[size];
            my_targets = new double[10];
            double value = 0;
            try
            {
                string[] stringdata = data.Split(',');
                if (stringdata.Length != size)
                {
                    MessageBox.Show("The data string does not have the correct size!", "TrainingData error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                for (int I = 0; I < size; I++)
                {
                    value = double.Parse(stringdata[I]);
                    my_data[I] = value / 255 * 0.99 + 0.01;
                }
                for (int I = 0; I < 10; I++)
                {
                    if (I == target)
                    {
                        my_targets[I] = 0.99;
                    }
                    else
                    {
                        my_targets[I] = 0.01;
                    }
                }
            }
            catch
            {
                MessageBox.Show("The data string does not have the correct format!", "TrainingData error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public double[] data
        {
            get { return my_data; }
        }

        public double[] targets
        {
            get { return my_targets; }
        }
    }
}
