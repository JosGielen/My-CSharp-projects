using System;
using System.IO;
using System.Windows;
using JG_Math;

namespace JG_NeuralNet
{
    public class NeuralNet
    {
        private int my_InputNr;
        private int my_HiddenNr;
        private int my_OutputNr;
        private double my_LearningRate;
        private bool my_RandomizeNormal;
        private Matrix WeightsIH;
        private Matrix WeightsHO;
        private Matrix BiasH;
        private Matrix BiasO;
        static readonly Random Rnd = new Random();

        public NeuralNet(int inputNodes, int hiddenNodes, int outputNodes, double learningRate, bool NormalWeights)
        {
            my_InputNr = inputNodes;
            my_HiddenNr = hiddenNodes;
            my_OutputNr = outputNodes;
            my_LearningRate = learningRate;
            my_RandomizeNormal = NormalWeights;
            WeightsIH = new Matrix(my_HiddenNr, my_InputNr);
            WeightsHO = new Matrix(my_OutputNr, my_HiddenNr);
            if (NormalWeights)
            {
                WeightsIH.RandomizeNormal(0.0, Math.Pow(my_InputNr, -0.5));
                WeightsHO.RandomizeNormal(0.0, Math.Pow(my_HiddenNr, -0.5));
            }
            else
            {
                WeightsIH.Randomize();
                WeightsHO.Randomize();
            }
            BiasH = new Matrix(my_HiddenNr, 1);
            BiasO = new Matrix(my_OutputNr, 1);
            BiasH.Randomize();
            BiasO.Randomize();
        }

        public int InputNodes
        {
            get { return my_InputNr; }
        }

        public int HiddenNodes
        {
            get { return my_HiddenNr; }
        }

        public int OutputNodes
        {
            get { return my_OutputNr; }
        }

        public double LearningRate
        {
            get { return my_LearningRate; }
        }

        public bool NormalizeWeights
        {
            get { return my_RandomizeNormal; }
        }

        /// <summary>
        /// Train the NeuralNet with known data
        /// </summary>
        /// <param name="inputs">An array of values between 0 and 1 with the same size as the number of InputNodes</param>
        /// <param name="targets">An array of values between 0 and 1 with the same size as the number of OutputNodes</param>
        /// <returns>True if the predicted value was correct, else return false</returns>
        /// <exception cref="Exception">if the parameters do not match the NeuralNet configuration</exception>
        public bool Train(double[] inputs, double[] targets)
        {
            if (inputs.Length != my_InputNr)
            {
                throw new Exception("The number of inputs does not match this Neural Net configuration.");
            }
            if (targets.Length != my_OutputNr)
            {
                throw new Exception("The number of targets does not match this Neural Net configuration.");
            }
            //Convert parameters to Matrices
            Matrix my_Inputs = Matrix.FromArray(inputs);
            Matrix my_targets = Matrix.FromArray(targets);
            //Create the signal matrices
            Matrix Hidden_In = new Matrix(my_HiddenNr, 1); //Hidden node values without activation ;
            Matrix Hidden_Out = new Matrix(my_HiddenNr, 1); //Hidden node values after activation ;
            Matrix Output_In = new Matrix(my_OutputNr, 1); //Output node values without activation ;
            Matrix Output_Out = new Matrix(my_OutputNr, 1); //Output node values after activation ;

            //STEP1: Calculate the guess with the FeedForward mechanism
            //  Generating the Hidden output
            Hidden_In = WeightsIH * my_Inputs;
            Hidden_In.AddMatrix(BiasH);
            Hidden_Out = Hidden_In.MapTo(Activation);
            //  Generating the final output
            Output_In = WeightsHO * Hidden_Out;
            Output_In.AddMatrix(BiasO);
            Output_Out = Output_In.MapTo(Activation);

            //STEP2: Check if the guess was correct to allow monitoring of the training efficiency
            double maxResult = 0;
            int maxResultIndex = 0; //The index of the result array with the predicted answer
            double maxTarget = 0;
            int maxTargetIndex = 0; //The index of the targets array with the correct answer
            double[] result = Output_Out.ColToArray(0);
            for (int I = 0; I < targets.Length; I++)
            {
                if (result[I] > maxResult)
                {
                    maxResult = result[I];
                    maxResultIndex = I;
                }
                if (targets[I] > maxTarget)
                {
                    maxTarget = targets[I];
                    maxTargetIndex = I;
                }
            }

            //STEP3: Use Backpropagation to optimize the weights
            //  Calculate the output errors
            Matrix Output_Errors = my_targets - Output_Out;
            //  Calculate the hidden errors
            Matrix Hidden_Errors = Matrix.Transpose(WeightsHO) * Output_Errors;
            //  Change the Weights using simplyfied Gradient Descent mechanism
            //  ** Change the Weights of Hidden to Output
            Matrix Output_Gradients = Output_In.MapTo(Gradient);
            Output_Gradients.MultiplyHadamard(Output_Errors);
            Output_Gradients.MultiplyScalar(my_LearningRate);
            Matrix WeightsHO_Delta = Output_Gradients * Matrix.Transpose(Hidden_Out);
            WeightsHO.AddMatrix(WeightsHO_Delta);
            //  ** Change the Weights of Input to Hidden
            Matrix Hidden_Gradients = Hidden_In.MapTo(Gradient);
            Hidden_Gradients.MultiplyHadamard(Hidden_Errors);
            Hidden_Gradients.MultiplyScalar(my_LearningRate);
            Matrix WeightsIH_Delta = Hidden_Gradients * Matrix.Transpose(my_Inputs);
            WeightsIH.AddMatrix(WeightsIH_Delta);
            //  ** Change the Biases
            BiasO.AddMatrix(Output_Gradients);
            BiasH.AddMatrix(Hidden_Gradients);
            //Return true if the predicted value was correct, else return false
            return maxResultIndex == maxTargetIndex; 
        }

        /// <summary>
        /// Let the NeuralNet predict the result for the given input array.
        /// </summary>
        /// <param name="inputs">An array of values between 0 and 1 with the same size as the number of InputNodes</param>
        /// <returns>An array of values between 0 and 1 with the same size as the number of OutputNodes</returns>
        /// <exception cref="Exception">If the input parameter does not match the NeuralNet configuration</exception>
        public double[] Query(double[] inputs)
        {
            if (inputs.Length != my_InputNr)
            {
                throw new Exception("The number of inputs does not match this Neural Net configuration.");
            }
            Matrix my_Inputs = Matrix.FromArray(inputs);
            Matrix Hidden = new Matrix(my_HiddenNr, 1);
            Matrix Outputs = new Matrix(my_OutputNr, 1);
            //Generating the Hidden output
            Hidden = WeightsIH * my_Inputs;
            Hidden.AddMatrix(BiasH);
            Hidden.Map(Activation);
            //Generating the final output
            Outputs = WeightsHO * Hidden;
            Outputs.AddMatrix(BiasO);
            Outputs.Map(Activation);
            return Outputs.ColToArray(0);
        }

        /// <summary>
        /// Create a deep copy of the NeuralNet
        /// </summary>
        /// <returns>A new NeuralNet with the same configuration and weights</returns>
        public NeuralNet Copy()
        {
            NeuralNet result = new NeuralNet(my_InputNr, my_HiddenNr, my_OutputNr, my_LearningRate, my_RandomizeNormal);
            result.WeightsHO = WeightsHO.Copy();
            result.WeightsIH = WeightsIH.Copy();
            result.BiasH = BiasH.Copy();
            result.BiasO = BiasO.Copy();
            return result;
        }

        /// <summary>
        /// Modify some of the weights of the NeuralNet with a random increase or decrease by a given factor.
        /// </summary>
        /// <param name="rate">The percentage chance to modify each weight seperately.</param>
        /// <param name="factor">The percentage change of a weight.</param>
        /// <param name="withBias">Mutate the biases?</param>
        public void MutateIncrement(double rate, double factor, bool withBias)
        {
            for (int I = 0; I < WeightsHO.Rows; I++)
            {
                for (int J = 0; J < WeightsHO.Columns; J++)
                {
                    if (100 * Rnd.NextDouble() < rate)
                    {
                        if (Rnd.NextDouble() < 0.5)
                        {
                            WeightsHO.SetValue(I, J, WeightsHO.GetValue(I, J) * (100 - factor) / 100);
                        }
                        else
                        {
                            WeightsHO.SetValue(I, J, WeightsHO.GetValue(I, J) * (100 + factor) / 100);
                        }
                    }
                }
            }
            for (int I = 0; I < WeightsIH.Rows; I++)
            {
                for (int J = 0; J < WeightsIH.Columns; J++)
                {
                    if (100 * Rnd.NextDouble() < rate)
                    {
                        if (Rnd.NextDouble() < 0.5)
                        {
                            WeightsIH.SetValue(I, J, WeightsIH.GetValue(I, J) * (100 - factor) / 100);
                        }
                        else
                        {
                            WeightsIH.SetValue(I, J, WeightsIH.GetValue(I, J) * (100 + factor) / 100);
                        }
                    }
                }
            }

            //Mutation of the Bias values
            if (withBias)
            { 
                for (int I = 0; I < BiasH.Rows; I++)
                {
                    for (int J = 0; J < BiasH.Columns; J++)
                    {
                        if (100 * Rnd.NextDouble() < rate)
                        {
                            if (Rnd.NextDouble() < 0.5)
                            {
                                BiasH.SetValue(I, J, BiasH.GetValue(I, J) * (100 - factor) / 100);
                            }
                            else
                            {
                                BiasH.SetValue(I, J, BiasH.GetValue(I, J) * (100 + factor) / 100);
                            }
                        }
                    }
                }
            for (int I = 0; I < BiasO.Rows; I++)
            {
                for (int J = 0; J < BiasO.Columns; J++)
                {
                    if (100 * Rnd.NextDouble() < rate)
                    {
                        if (Rnd.NextDouble() < 0.5)
                        {
                            BiasO.SetValue(I, J, BiasO.GetValue(I, J) * (100 - factor) / 100);
                        }
                        else
                        {
                            BiasO.SetValue(I, J, BiasO.GetValue(I, J) * (100 + factor) / 100);
                        }
                    }
                }
            }
        }
        }

        /// <summary>
        /// Replace some of the weights of the NeuralNet with a random value.
        /// </summary>
        /// <param name="rate">The percentage chance to modify each weight seperately.</param>
        /// <param name="withBias">Mutate the biases?</param>
        public void MutateRandom(double rate, bool withBias)
        {
            for (int I = 0; I < WeightsHO.Rows; I++)
            {
                for (int J = 0; J < WeightsHO.Columns; J++)
                {
                    if (100 * Rnd.NextDouble() < rate)
                    {
                        WeightsHO.SetValue(I, J, 2.0 * Rnd.NextDouble() - 1.0);
                    }
                }
            }
            for (int I = 0; I < WeightsIH.Rows; I++)
            {
                for (int J = 0; J < WeightsIH.Columns; J++)
                {
                    if (100 * Rnd.NextDouble() < rate)
                    {
                        WeightsIH.SetValue(I, J, 2.0 * Rnd.NextDouble() - 1.0);
                    }
                }
            }

            //Mutation of the Bias values
            if (withBias)
            {
                for (int I = 0; I < BiasH.Rows; I++)
                {
                    for (int J = 0; J < BiasH.Columns; J++)
                    {
                        if (100 * Rnd.NextDouble() < rate)
                        {
                            BiasH.SetValue(I, J, 2.0 * Rnd.NextDouble() - 1.0);
                        }
                    }
                }
                for (int I = 0; I < BiasO.Rows; I++)
                {
                    for (int J = 0; J < BiasO.Columns; J++)
                    {
                        if (100 * Rnd.NextDouble() < rate)
                        {
                            BiasO.SetValue(I, J, 2.0 * Rnd.NextDouble() - 1.0);
                        }
                    }
                }
            }
        }

        private double Activation(double value)
        {
            return SigmoidActivation(value); //Can be modified to use different functions
        }

        private double Gradient(double value)
        {
            //This is the derivitive of the Activation
            //Gradient(X) = d(Activation(X))/dX;
            return SigmoidGradient(value); //Can be modified to use different functions
        }

        /// <summary>
        /// Save the NeuralNet parameters to a new file.
        /// </summary>
        /// <param name="filename"></param>
        public void SaveToFile(string filename)
        {
            SaveToFile(filename, false);
        }

        /// <summary>
        /// Save the NeuralNet parameters to a new file or append to an existing file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="append">Allows appending to an existing file</param>
        public void SaveToFile(string filename, bool append)
        {
            try
            {
                StreamWriter myStream = new StreamWriter(filename, append);
                if (myStream != null)
                {
                    //Write the NeuralNet configuration to the file
                    myStream.WriteLine(my_InputNr);
                    myStream.WriteLine(my_HiddenNr);
                    myStream.WriteLine(my_OutputNr);
                    myStream.WriteLine(my_LearningRate.ToString());
                    myStream.WriteLine(my_RandomizeNormal.ToString());
                    //Write the Matrices in multiline form to the file
                    string[] data;
                    data = WeightsIH.ToRowStrings("");
                    for (int I = 0; I < data.Length; I++)
                    {
                        myStream.WriteLine(data[I]);
                    }
                    data = WeightsHO.ToRowStrings("");
                    for (int I = 0; I < data.Length; I++)
                    {
                        myStream.WriteLine(data[I]);
                    }
                    data = BiasH.ToRowStrings("");
                    for (int I = 0; I < data.Length; I++)
                    {
                        myStream.WriteLine(data[I]);
                    }
                    data = BiasO.ToRowStrings("");
                    for (int I = 0; I < data.Length; I++)
                    {
                        myStream.WriteLine(data[I]);
                    }
                    myStream.Close();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Cannot save the NeuralNet data. Original error: " + Ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load the parameters for this NeuralNet from a File.
        /// </summary>
        /// <param name="filename"></param>
        public void GetDataFromFile(string filename)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(filename);
                my_InputNr = int.Parse(sr.ReadLine());
                my_HiddenNr = int.Parse(sr.ReadLine());
                my_OutputNr = int.Parse(sr.ReadLine());
                my_LearningRate = double.Parse(sr.ReadLine());
                my_RandomizeNormal = bool.Parse(sr.ReadLine());
                //Construct the Matrices from the multiline format.
                string sizestring;
                string[] sizedata;
                string[] valuedata;
                int rows;
                //WeightsIH
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newWeightsIH = Matrix.FromRowStrings(valuedata);
                //WeightsHO
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newWeightsHO = Matrix.FromRowStrings(valuedata);
                //BiasH
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newBiasH = Matrix.FromRowStrings(valuedata);
                //BiasO
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newBiasO = Matrix.FromRowStrings(valuedata);
                WeightsIH = newWeightsIH;
                WeightsHO = newWeightsHO;
                BiasH = newBiasH;
                BiasO = newBiasO;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load the NeuralNet data. Original error: " + ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return;
        }

        /// <summary>
        /// Load the parameters for this NeuralNet from an existing StreamReader
        /// </summary>
        /// <param name="sr"></param>
        public void GetDataFromStream(StreamReader sr)
        {
            try
            {
                my_InputNr = int.Parse(sr.ReadLine());
                my_HiddenNr = int.Parse(sr.ReadLine());
                my_OutputNr = int.Parse(sr.ReadLine());
                my_LearningRate = double.Parse(sr.ReadLine());
                my_RandomizeNormal = bool.Parse(sr.ReadLine());
                //Construct the Matrices from the multiline format.
                string sizestring;
                string[] sizedata;
                string[] valuedata;
                int rows;
                //WeightsIH
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newWeightsIH = Matrix.FromRowStrings(valuedata);
                //WeightsHO
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newWeightsHO = Matrix.FromRowStrings(valuedata);
                //BiasH
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newBiasH = Matrix.FromRowStrings(valuedata);
                //BiasO
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newBiasO = Matrix.FromRowStrings(valuedata);
                WeightsIH = newWeightsIH;
                WeightsHO = newWeightsHO;
                BiasH = newBiasH;
                BiasO = newBiasO;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load the NeuralNet data. Original error: " + ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return;
        }

        /// <summary>
        /// Create a new NeuralNet with parameters from the File
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static NeuralNet LoadFromFile(string filename)
        {
            NeuralNet result;
            StreamReader sr;
            int InputNr;
            int HiddenNr;
            int OutputNr;
            double lr;
            bool normalRand;
            try
            {
                sr = new StreamReader(filename);
                InputNr = int.Parse(sr.ReadLine());
                HiddenNr = int.Parse(sr.ReadLine());
                OutputNr = int.Parse(sr.ReadLine());
                lr = double.Parse(sr.ReadLine());
                normalRand = bool.Parse(sr.ReadLine());
                //Construct the Matrices from the multiline format.
                string sizestring;
                string[] sizedata;
                string[] valuedata;
                int rows;
                //WeightsIH
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newWeightsIH = Matrix.FromRowStrings(valuedata);
                //WeightsHO
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newWeightsHO = Matrix.FromRowStrings(valuedata);
                //BiasH
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newBiasH = Matrix.FromRowStrings(valuedata);
                //BiasO
                sizestring = sr.ReadLine();
                sizedata = sizestring.Split(';');
                rows = int.Parse(sizedata[0]);
                valuedata = new string[rows + 1];
                valuedata[0] = sizestring;
                for (int I = 0; I < rows; I++)
                {
                    valuedata[I + 1] = sr.ReadLine();
                }
                Matrix newBiasO = Matrix.FromRowStrings(valuedata);
                //Make the new NeuralNet
                result = new NeuralNet(InputNr, HiddenNr, OutputNr, lr, normalRand)
                {
                    WeightsIH = newWeightsIH,
                    WeightsHO = newWeightsHO,
                    BiasH = newBiasH,
                    BiasO = newBiasO
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load the NeuralNet data. Original error: " + ex.Message, "NeuralNet error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new NeuralNet(0, 0, 0, 0.0, false);
            }
            return result;
        }

        private static double SigmoidActivation(double value)
        {
            //Example of an Activation function
            //S(x) = 1/(1+e^(-x));
            return 1 / (1 + Math.Exp(-1 * value));
        }

        private static double SigmoidGradient(double value)
        {
            //Example of an Gradient function
            //dS(x)/dx = s(x)*(1-s(x));
            return 1 / (1 + Math.Exp(-1 * value)) * (1 - 1 / (1 + Math.Exp(-1 * value)));
        }
    }
}
