using Fourier_Complex;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Audio_Vieuwer
{
    public partial class MainWindow : Window
    {
        private bool isRecording = false;
        private bool isPlaying = false;
        private MMDevice selectedDevice;
        private WasapiCapture capture;
        private BufferedWaveProvider bufferedWaveProvider;
        private WaveStream OutputStream;
        private WaveFileWriter writer;
        private WaveOutEvent player;
        private int MaxBytes;  //should be a whole fraction of the capture SampleRate.
        private int bytesRead;
        private int bytesPerSamplePerChannel;
        private int sampleRate;
        private int scanRate = 8; //number of times the sampleRate is divided.
        private int channelCount;
        private int bytesPerSample;
        private string filename;
        private byte[] my_Buffer;
        private double[] Xdata;
        private double[] Ydata;
        private double[] FreqXdata;
        private double[] FreqYdata;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            filename = "";
        }

        private void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Wave Files (*.wav) | *.wav",
                InitialDirectory = Environment.CurrentDirectory,
            };
            if (ofd.ShowDialog() == true)
            {
                filename = ofd.FileName;
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                isRecording = true;
                btnRecord.Content = "STOP";
                if (cbSpeakers.IsChecked == true)
                {
                    //Get the Default audio playback Device
                    MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                    IEnumerable<MMDevice> CaptureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
                    MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    selectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
                    capture = new WasapiLoopbackCapture(selectedDevice);
                    capture.WaveFormat = new WaveFormat(22050, 16, 2);
                }
                else if(cbMicrophone.IsChecked == true)
                {
                    MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                    IEnumerable<MMDevice> CaptureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
                    MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                    selectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
                    capture = new WasapiCapture(selectedDevice);
                    capture.WaveFormat = new WaveFormat(22050, 16, 2);
                }
                //Get the capture device parameters
                bytesPerSamplePerChannel = capture.WaveFormat.BitsPerSample / 8;
                channelCount = capture.WaveFormat.Channels;
                bytesPerSample = bytesPerSamplePerChannel * channelCount;
                sampleRate = capture.WaveFormat.SampleRate;
                MaxBytes = sampleRate / scanRate;
                //Set the Amplitude graph Axis parameters
                graphAmplitude.ShowAxes = false;
                graphAmplitude.Xmin = 0;
                graphAmplitude.Xmax = MaxBytes / bytesPerSample;
                graphAmplitude.Xstep = MaxBytes / (10 * bytesPerSample);
                graphAmplitude.Ymin = Int16.MinValue;
                graphAmplitude.Ymax = Int16.MaxValue;
                graphAmplitude.Ystep = Int16.MaxValue / 5;
                //Set the Frequency graph Axis parameters
                graphFrequency.ShowAxes = true;
                graphFrequency.Xmin = 0;
                graphFrequency.Xmax = sampleRate / bytesPerSample; ;
                graphFrequency.Xstep = 500;
                graphFrequency.Ymin = 0;
                graphFrequency.Ymax = 5000000;
                graphFrequency.Ystep = 500000;
                //Set the Xdata (remains constant)
                Xdata = new double[MaxBytes / bytesPerSample];
                Ydata = new double[MaxBytes / bytesPerSample];
                FreqXdata = new double[MaxBytes / bytesPerSample];
                for (int i = 0; i < Xdata.Length; i++)
                {
                    Xdata[i] = i;
                    FreqXdata[i] = Xdata[i] * sampleRate / (MaxBytes / bytesPerSample);
                }
                //Start the capture of the audio 
                capture.DataAvailable += Capture_DataAvailable;
                capture.RecordingStopped += Capture_RecordingStopped;
                if (txtFileName.Text == "")
                {
                    filename = "Default.wav";
                }
                else
                {
                    filename = txtFileName.Text + ".wav";
                }
                writer = new WaveFileWriter(filename, capture.WaveFormat);
                bufferedWaveProvider = new BufferedWaveProvider(capture.WaveFormat)
                {
                    DiscardOnBufferOverflow = false,
                    ReadFully = false,
                    BufferDuration = TimeSpan.FromMilliseconds(5000)
                };
                capture.StartRecording();
            }
            else
            {
                isRecording = false;
                btnRecord.Content = "START RECORDING";
                capture.StopRecording();
            }
        }

        private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            //save the recorded bytes to the wave file
            writer.Write(e.Buffer, 0, e.BytesRecorded);
            //place the recorded bytes in a BufferedWaveProvider.
            //This allows to get the audio bytes back in fixed chunks from this provider to show in the graphs.
            bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            while (bufferedWaveProvider.BufferedBytes > MaxBytes)
            {
                my_Buffer = new byte[MaxBytes];
                bytesRead = bufferedWaveProvider.Read(my_Buffer, 0, MaxBytes);
                UpdateGraphs(my_Buffer, bytesRead, capture.WaveFormat);
            }
        }

        private void Capture_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            writer.Close();
            writer.Dispose();
            writer = null;
            capture.Dispose();
            capture = null;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (capture != null) { capture.StopRecording(); }
            if (!isPlaying)
            {
                if (filename == "" || !File.Exists(filename)) 
                { 
                    return; 
                }
                btnPlay.Content = "STOP";
                mnuPlay.Header = "STOP";
                isPlaying = true;
                //Play the recorded wave file
                OutputStream = new WaveFileReader(filename);
                player = new WaveOutEvent();
                player.Init(OutputStream);
                player.PlaybackStopped += Player_PlaybackStopped;
                player.Play();
            }
            else
            {
                if (player.PlaybackState == PlaybackState.Playing)
                {
                    player.Stop();
                }
                btnPlay.Content = "PLAY";
                mnuPlay.Header = "PLAY";
                isPlaying = false;
            }
        }

        private void Player_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            OutputStream.Dispose();
            player.Dispose();
            btnPlay.Content = "PLAY";
            mnuPlay.Header = "PLAY";
            isPlaying = false;
        }

        private void UpdateGraphs(byte[] bytes, int byteCount, WaveFormat format)
        {
            //Convert the raw bytes into 16 or 32 bit samples
            if (channelCount == 2)
            {
                if (bytesPerSamplePerChannel == 2 && format.Encoding == WaveFormatEncoding.Pcm)
                {
                    for (int i = 0; i < byteCount; i += 4) //Skip the Right channel
                    {
                        Ydata[i / 4] = BitConverter.ToInt16(bytes, i);
                    }
                }
                else if (bytesPerSamplePerChannel == 4 && format.Encoding == WaveFormatEncoding.Pcm)
                {
                    for (int i = 0; i < byteCount; i += 8) //Skip the Right channel
                    {
                        Ydata[i / 8] = BitConverter.ToInt32(bytes, i);
                    }
                }
                else if (bytesPerSamplePerChannel == 4 && format.Encoding == WaveFormatEncoding.IeeeFloat)
                {
                    for (int i = 0; i < byteCount; i += 8) //Skip the Right channel
                    {
                        Ydata[i / 8] = 500 * BitConverter.ToSingle(bytes, i);
                    }
                }
                else
                {
                    throw new NotSupportedException(format.ToString());
                }
            }
            else if (channelCount == 1)
            {
                if (bytesPerSamplePerChannel == 2 && format.Encoding == WaveFormatEncoding.Pcm)
                {
                    for (int i = 0; i < byteCount; i += 2)
                    {
                        Ydata[i / 2] = BitConverter.ToInt16(bytes, i);
                    }
                }
                else if (bytesPerSamplePerChannel == 4 && format.Encoding == WaveFormatEncoding.Pcm)
                {
                    for (int i = 0; i < byteCount; i += 4)
                    {
                        Ydata[i / 4] = BitConverter.ToInt32(bytes, i);
                    }
                }
                else if (bytesPerSamplePerChannel == 4 && format.Encoding == WaveFormatEncoding.IeeeFloat)
                {
                    for (int i = 0; i < byteCount; i += 4)
                    {
                        Ydata[i / 4] = 500 * BitConverter.ToSingle(bytes, i);
                    }
                }
                else
                {
                    throw new NotSupportedException(format.ToString());
                }
            }
            else
            {
                throw new NotSupportedException("Invalid Wave file format");
            }
            if (Xdata.Length > 0)
            {
                //Calculate the Discreet Fourier Transform of both audio channels
                FreqYdata = DFT.Process2double(Ydata);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    graphAmplitude.SetData(Xdata, Ydata);
                    graphFrequency.SetData(FreqXdata, FreqYdata);
                    graphAmplitude.Draw();
                    graphFrequency.Draw();
                });
            }
        }

        private void cbSpeakers_Click(object sender, RoutedEventArgs e)
        {
            cbMicrophone.IsChecked = false;
        }

        private void cbMicrophone_Click(object sender, RoutedEventArgs e)
        {
            cbSpeakers.IsChecked = false;
        }
    }
}