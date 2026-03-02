using Fourier_Complex;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Audio_Spectrum
{
    public partial class MainWindow : Window
    {
        private bool started = false;
        private MMDevice selectedDevice;
        private WasapiCapture capture;
        private BufferedWaveProvider bufferedWaveProvider;
        private int MaxBytes;
        private int bytesRead;
        private int bytesPerSamplePerChannel;
        private int sampleRate;
        private int scanRate = 4;
        private int channelCount;
        private int bytesPerSample;
        private WaveStream mainOutputStream;
        private WaveChannel32 volumeStream;
        private WaveOutEvent player;
        private string filename;
        private byte[] my_Buffer;
        private double[] Ydata;
        private double[] FreqYdata;
        private double Ymax = 4000000;
        private int Stride = 0;
        private byte[] pixelData;
        private List<Color> my_Colors;
        private WriteableBitmap bitmap;
        private int scanCount;
        private int W, H;
        private int rectW = 2;
        private int rectH = 4;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            filename = "";
            //Set the Image to fill the canvas.
            W = (int)canvas1.ActualWidth;
            H = (int)canvas1.ActualHeight;
            image1.Width = W;
            image1.Height = H;
            //Create a bitmap to show in the Image
            Stride = (int)(image1.Width * PixelFormats.Rgb24.BitsPerPixel / 8);
            pixelData = new byte[(int)(Stride * image1.Height)];
            //Get a color palette
            ColorPalette cpal = new ColorPalette(Environment.CurrentDirectory + "\\Thermal2.cpl");
            my_Colors = cpal.GetColors(256);
            BitmapPalette bpal = new BitmapPalette(my_Colors);
            bitmap = new WriteableBitmap(W, H, 96, 96, PixelFormats.Bgr24, bpal);
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
                btnPlay.IsEnabled = true;
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!started)
            {
                started = true;
                btnPlay.Content = "STOP";
                //Get the Default audio playback Device
                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                IEnumerable<MMDevice> CaptureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
                MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                selectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
                //Get the capture device and its parameters
                capture = new WasapiLoopbackCapture(selectedDevice);
                capture.WaveFormat = new WaveFormat(22050, 16, 2);
                bytesPerSamplePerChannel = capture.WaveFormat.BitsPerSample / 8;
                channelCount = capture.WaveFormat.Channels;
                bytesPerSample = bytesPerSamplePerChannel * channelCount;
                sampleRate = capture.WaveFormat.SampleRate;
                MaxBytes = sampleRate / scanRate;
                Ydata = new double[MaxBytes / bytesPerSample];
                //Start the capture of the audio 
                capture.DataAvailable += Capture_DataAvailable;
                capture.RecordingStopped += Capture_RecordingStopped; ;
                bufferedWaveProvider = new BufferedWaveProvider(capture.WaveFormat)
                {
                    DiscardOnBufferOverflow = false,
                    ReadFully = false,
                    BufferDuration = TimeSpan.FromMilliseconds(5000)
                };
                scanCount = 0;
                capture.StartRecording();
                //Play the selected wave file
                if (filename != "")
                {
                    mainOutputStream = new WaveFileReader(filename);
                    volumeStream = new WaveChannel32(mainOutputStream);
                    volumeStream.PadWithZeroes = false;
                    player = new WaveOutEvent();
                    player.Init(volumeStream);
                    player.PlaybackStopped += Player_PlaybackStopped;
                    player.Play();
                }
            }
            else
            {
                started = false;
                btnPlay.Content = "START";
                if (player != null) { player.Stop(); }
                capture.StopRecording();
            }
        }

        private void Capture_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            capture.Dispose();
            capture = null;
        }

        //when the capture device has captured audio bytes in e.Buffer, place them in a BufferedWaveProvider
        //This allows to get the audio bytes back in fixed chunks from this provider.
        //MaxBytes should be a whole fraction of the capture SampleRate.
        private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            while (bufferedWaveProvider.BufferedBytes > MaxBytes)
            {
                my_Buffer = new byte[MaxBytes];
                bytesRead = bufferedWaveProvider.Read(my_Buffer, 0, MaxBytes);
                if (channelCount == 2)
                {
                    if (bytesPerSamplePerChannel == 2 && capture.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
                    {
                        for (int i = 0; i < bytesRead; i += 4) //Skip the Right channel
                        {
                            Ydata[i / 4] = BitConverter.ToInt16(my_Buffer, i);
                        }
                    }
                    else if (bytesPerSamplePerChannel == 4 && capture.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
                    {
                        for (int i = 0; i < bytesRead; i += 8) //Skip the Right channel
                        {
                            Ydata[i / 8] = BitConverter.ToInt32(my_Buffer, i);
                        }
                    }
                    else if (bytesPerSamplePerChannel == 4 && capture.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                    {
                        for (int i = 0; i < bytesRead; i += 8) //Skip the Right channel
                        {
                            Ydata[i / 8] = 500 * BitConverter.ToSingle(my_Buffer, i);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(capture.WaveFormat.ToString());
                    }
                }
                else if (channelCount == 1)
                {
                    if (bytesPerSamplePerChannel == 2 && capture.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
                    {
                        for (int i = 0; i < bytesRead; i += 2)
                        {
                            Ydata[i / 2] = BitConverter.ToInt16(my_Buffer, i);
                        }
                    }
                    else if (bytesPerSamplePerChannel == 4 && capture.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
                    {
                        for (int i = 0; i < bytesRead; i += 4)
                        {
                            Ydata[i / 4] = BitConverter.ToInt32(my_Buffer, i);
                        }
                    }
                    else if (bytesPerSamplePerChannel == 4 && capture.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                    {
                        for (int i = 0; i < bytesRead; i += 4)
                        {
                            Ydata[i / 4] = 500 * BitConverter.ToSingle(my_Buffer, i);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(capture.WaveFormat.ToString());
                    }
                }
                else
                {
                    throw new NotSupportedException("Invalid Wave file format");
                }
                if (Ydata.Length > 0)
                {
                    //Calculate the Discreet Fourier Transform of both audio channels
                    FreqYdata = DFT.Process2double(Ydata);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //Set the Frequency amplitudes as a vertical line of color points in the image
                        Point pt;
                        int colorIndex;
                        if (rectW * (scanCount + 1) < bitmap.PixelWidth)
                        {
                            for (int I = 0; I < FreqYdata.Length; I++)
                            {
                                pt = new Point(rectW * scanCount, H - (I + 1) * rectH);
                                colorIndex = (int)(FreqYdata[I] / Ymax * 256);
                                if (colorIndex >= my_Colors.Count) { colorIndex = my_Colors.Count - 1;}
                                SetPixelArea(bitmap, pt, rectW, rectH, my_Colors[colorIndex]);
                            }
                            scanCount++;
                        }
                        else
                        {
                            //Scroll bitmap to the left by rectW pixels
                            bitmap.CopyPixels(pixelData, Stride, 0);
                            int oldIndex;
                            int newIndex;
                            for (int I = 0; I < bitmap.Height; I++)
                            {
                                for (int J = 0; J < bitmap.Width - rectW; J++)
                                {
                                    newIndex = 3 * (I * W + J);
                                    oldIndex = newIndex + 3 * rectW;
                                    pixelData[newIndex + 0] = pixelData[oldIndex + 0];
                                    pixelData[newIndex + 1] = pixelData[oldIndex + 1];
                                    pixelData[newIndex + 2] = pixelData[oldIndex + 2];
                                }
                            }
                            Int32Rect Intrect = new Int32Rect(0, 0, bitmap.PixelWidth - 1, bitmap.PixelHeight - 1);
                            bitmap.WritePixels(Intrect, pixelData, Stride, 0);
                            for (int I = 0; I < FreqYdata.Length; I++)
                            {
                                pt = new Point(rectW * scanCount, H - (I + 1) * rectH);
                                SetPixelArea(bitmap, pt, rectW, rectH, my_Colors[(int)(FreqYdata[I] * my_Colors.Count / Ymax) % 256]);
                            }
                        }
                        image1.Source = bitmap;
                    });
                }
            }
        }

        //OverWrite an area of width w by height h pixels at location Pos with Color c 
        public void SetPixelArea(WriteableBitmap WB, Point Pos, int w, int h, Color c)
        {
            //For PixelFormats.Rgb24
            byte[] PixelData = new byte[3 * w * h];
            for (int i = 0; i < PixelData.Length; i+=3)
            {
                PixelData[i + 0] = c.B;
                PixelData[i + 1] = c.G;
                PixelData[i + 2] = c.R;
            }
            if (Pos.X + w > WB.PixelWidth) { return; }
            if (Pos.X < 0) { return; }
            if (Pos.Y + h > WB.PixelHeight) { return; }
            if (Pos.Y < 0) { return; }
            WB.WritePixels(new Int32Rect((int)Pos.X, (int)Pos.Y, w, h), PixelData, 3 * w, 0);
        }

        private void Player_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (capture != null) 
            { 
                capture.StopRecording(); 
            }
            capture.Dispose();
            volumeStream.Dispose();
            mainOutputStream.Dispose();
            player.Dispose();
            Debug.Print("Player ended.");
        }
    }
}