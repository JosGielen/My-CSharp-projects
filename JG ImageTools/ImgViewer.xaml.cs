using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageTools
{
    public partial class ImgViewer
    {
        private MainWindow MyParent;
        private BitmapImage myImage;
        private double InitialScale;
        private double Zoom;
        private bool myLoaded = false;
        private DrawMode MyDrawMode = DrawMode.None;
        private EditState MyEditState = EditState.None;
        private LineTool MyLine;
        private RectangleTool myRect;
        private EllipseTool myEllipse;
        private CircleTool myCircle;
        private Circle2Tool myCircle2;
        private AngleTool myAngle;
        private Angle2Tool myAngle2;
        private CaliperTool myCaliper;
        private List<MeasurementTool> Tools;
        private MeasurementTool My_SelectedTool;
        private List<Handle> my_Handles;
        private Handle my_SelectedHandle;
        private bool myMouseDown;
        private Point MouseStart;


        public ImgViewer(MainWindow Parent)
        {
            InitializeComponent();
            MyParent = Parent;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imgZoomOut.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\ZoomOut.jpg"));
            imgZoomIn.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\ZoomIn.jpg"));
            imgEditTool.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\EditTool.jpg"));
            imgLineTool.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\LineTool.jpg"));
            imgRectangleTool.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\RectangleTool.jpg"));
            imgEllipseTool.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\EllipseTool.jpg"));
            imgCircleTool.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\CircleTool.jpg"));
            imgCircle2Tool.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\Circle2Tool.jpg"));
            imgAngleTool.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\AngleTool.jpg"));
            imgAngle2Tool.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\Angle2Tool.jpg"));
            imgCaliperTool.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Images\\CaliperTool.jpg"));
            WindowState = WindowState.Maximized;
            Tools = new List<MeasurementTool>();
            my_Handles = new List<Handle>();
            Zoom = 100;
            txtCurrentZoom.Text = Zoom.ToString("F1") + "%";
            myLoaded = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            int index = 0;
            if (MyEditState == EditState.EditStateSelectHandle)
            {
                if (My_SelectedTool != null)
                {
                    index = Tools.IndexOf(My_SelectedTool);
                    switch (e.Key)
                    {
                        case Key.Up:
                            if (index > 0)
                            {
                                index -= 1;
                            }
                            else
                            {
                                index = Tools.Count - 1;
                            }
                            break;
                        case Key.Down:
                            if (index < Tools.Count - 1)
                            {
                                index += 1;
                            }
                            else
                            {
                                index = 0;
                            }
                            break;
                        case Key.Delete:
                            if (Tools.Contains(My_SelectedTool)) { Tools.Remove(My_SelectedTool); }
                            My_SelectedTool.Remove(canvas1);
                            My_SelectedTool = null;
                            MyEditState = EditState.EditStateSelectTool;
                            break;
                    }
                }
                if (My_SelectedTool != null)
                {
                    //Reset previous selected Tool
                    My_SelectedTool.HideHandles();
                    My_SelectedTool.Highlighted = false;
                    //Set new selected Tool
                    My_SelectedTool = Tools[index];
                    My_SelectedTool.ShowHandles();
                    My_SelectedTool.Highlighted = true;
                    my_Handles = My_SelectedTool.ToolHandles;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Allow showing this window again
            Hide();
            e.Cancel = true;
        }

        #region "Menu"

        private void MnuOpen_Click(object sender, RoutedEventArgs e)
        {
            ImageBorder bord;
            int mag = -1;
            string Ext = "";
            OpenFileDialog OFD = new OpenFileDialog();
            //Show an OpenFile dialog
            OFD.InitialDirectory = MyParent.Settings.StartFolder;
            OFD.Filter = "Windows Bitmap (*.bmp,*.dib)|*.bmp;*.dib|JPEG (*.jpg,*.jpeg,*.jfif,*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe|TIFF (*.tif,tiff)|*.tif;*.tiff|PNG (*.png)|*.png|GIF (*.gif)| *.gif|All Image files (*.*)|*.*";
            OFD.FilterIndex = 6;
            OFD.RestoreDirectory = true;
            if (OFD.ShowDialog() == true)
            {
                MyParent.Settings.ImageFileName = OFD.FileName;
                MyParent.Settings.StartFolder = Path.GetDirectoryName(OFD.FileName);
                bord = new ImageBorder(OFD.FileName, MyParent);
                SetImage(bord);
            }
        }

        private void MnuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(MyParent.Settings.ImageFileName);
        }

        private void MnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.InitialDirectory = MyParent.Settings.StartFolder;
            SFD.FileName = Path.GetFileNameWithoutExtension(MyParent.Settings.ImageFileName);
            SFD.Filter = "Windows Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|TIFF (*.tiff)|*.tiff|PNG (*.png)|*.png";
            SFD.FilterIndex = MyParent.Settings.ImageFormatIndex;
            SFD.RestoreDirectory = true;
            if (SFD.ShowDialog() == true)
            {
                MyParent.Settings.ImageFormatIndex = SFD.FilterIndex;
                SaveImage(SFD.FileName);
                MyParent.Settings.ImageFileName = SFD.FileName;
                MyParent.Settings.StartFolder = Path.GetDirectoryName(SFD.FileName);
            }
        }

        private void MnuPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new PrintDialog();
            StackPanel PrintPanel = new StackPanel();
            if (printDlg.ShowDialog() == true)
            {
                Canvas printCanvas = new Canvas();
                double scale = 0.0;
                double WidthScale = (printDlg.PrintableAreaWidth - 50) / canvas1.ActualWidth;
                double HeightScale = (printDlg.PrintableAreaHeight - 50) / canvas1.ActualHeight;
                if (WidthScale < HeightScale)
                {
                    scale = WidthScale;
                }
                else
                {
                    scale = HeightScale;
                }
                Size pageSize = new Size(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight);
                printCanvas.Width = scale * canvas1.ActualWidth;
                printCanvas.Height = scale * canvas1.ActualHeight;
                printCanvas.Background = new VisualBrush(canvas1);
                //Place the item in the PrintPanel
                PrintPanel.Children.Add(printCanvas);
                //Measure and arrange the PrintPanel to fit the page
                PrintPanel.Margin = new Thickness(0);
                PrintPanel.Measure(pageSize);
                PrintPanel.Arrange(new Rect(15, 25, pageSize.Width - 15, pageSize.Height - 25));
                //Print the PrintPanel
                printDlg.PrintVisual(PrintPanel, "ImgViewer Print Image");
            }
        }

        private void MnuClose_Click(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MnuCopy_Click(Object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderbmp = new RenderTargetBitmap((int)canvas1.ActualWidth, (int)canvas1.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderbmp.Render(canvas1);
            Clipboard.SetImage(renderbmp);
        }

        private void MnuPaste_Click(object sender, RoutedEventArgs e)
        {
            byte[] PixelData;
            BitmapSource img = Clipboard.GetImage();
            if (img != null)
            {
                WriteableBitmap Writebitmap = new WriteableBitmap(img.PixelWidth, img.PixelHeight, img.DpiX, img.DpiY, img.Format, img.Palette);
                int Stride = Writebitmap.PixelWidth * Writebitmap.Format.BitsPerPixel / 8;
                Int32Rect Intrect = new Int32Rect(0, 0, Writebitmap.PixelWidth - 1, Writebitmap.PixelHeight - 1);
                PixelData = new byte[Stride * Writebitmap.PixelHeight];
                img.CopyPixels(PixelData, Stride, 0);
                //Set the opacity to 255
                for (int I = 3; I < PixelData.Count(); I += 4)
                {
                    PixelData[I] = 255;
                }
                Writebitmap.WritePixels(Intrect, PixelData, Stride, 0);
                SetImage(ToBitmapImage(Writebitmap), "Untitled");
            }
        }

        private void MnuRestore_Click(object sender, RoutedEventArgs e)
        {
            canvas1.Children.Clear();
            Tools.Clear();
            My_SelectedTool = null;
            my_SelectedHandle = null;
            foreach (object mi in MnuMeasure.Items)
            {
                if (mi.GetType() == typeof(MenuItem))
                {
                    ((MenuItem)mi).IsChecked = false;
                }
            }
            Cursor = Cursors.Arrow;
            MyDrawMode = DrawMode.None;
        }

        private void MnuEditImage_Click(object sender, RoutedEventArgs e)
        {
            MyParent.ShowEditor(myImage, MyParent.Settings.ImageFileName);
        }

        private void MnuOptions_Click(object sender, RoutedEventArgs e)
        {
            MyParent.ShowSettings();
        }

        private void MnuMainWindow_Click(object sender, RoutedEventArgs e)
        {
            MyParent.WindowState = WindowState.Normal;
            MyParent.Show();
            MyParent.Focus();
        }

        private void MnuEditorWindow_Click(object sender, RoutedEventArgs e)
        {
            MyParent.ShowEditor(myImage, MyParent.Settings.ImageFileName);
        }

        #endregion

        #region "Measurement Tools Selection"

        #region "Measure Menu"

        private void MnuEdit_Click(object sender, RoutedEventArgs e)
        {
            UseEditTool();
        }

        private void MnuLine_Click(object sender, RoutedEventArgs e)
        {
            UseLineTool();
        }

        private void MnuRectangle_Click(object sender, RoutedEventArgs e)
        {
            UseRectangleTool();
        }

        private void MnuEllipse_Click(object sender, RoutedEventArgs e)
        {
            UseEllipseTool();
        }

        private void MnuCircle_Click(object sender, RoutedEventArgs e)
        {
            UseCircleTool();
        }

        private void MnuCircle2_Click(object sender, RoutedEventArgs e)
        {
            UseCircle2Tool();
        }

        private void MnuAngle_Click(object sender, RoutedEventArgs e)
        {
            UseAngleTool();
        }

        private void MnuAngle2_Click(object sender, RoutedEventArgs e)
        {
            UseAngle2Tool();
        }

        private void MnuCaliper_Click(object sender, RoutedEventArgs e)
        {
            UseCaliperTool();
        }

        #endregion

        #region "ToolBar"

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom > 100)
            {
                Zoom -= 25;
                ScaleTransform ST = new ScaleTransform();
                ST.CenterX = ScrlViewer.ActualWidth / 2;
                ST.CenterY = ScrlViewer.ActualHeight / 2;
                ST.ScaleX = Zoom / 100;
                ST.ScaleY = Zoom / 100;
                canvas1.LayoutTransform = ST;
            }
            txtCurrentZoom.Text = Zoom.ToString("F1") + "%";
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom < 300)
            {
                Zoom += 25;
                ScaleTransform ST = new ScaleTransform();
                ST.CenterX = ScrlViewer.ActualWidth / 2;
                ST.CenterY = ScrlViewer.ActualHeight / 2;
                ST.ScaleX = Zoom / 100;
                ST.ScaleY = Zoom / 100;
                canvas1.LayoutTransform = ST;
            }
            txtCurrentZoom.Text = Zoom.ToString("F1") + "%";
        }

        private void BtnEditTool_Click(object sender, RoutedEventArgs e)
        {
            UseEditTool();
        }

        private void BtnLineTool_Click(object sender, RoutedEventArgs e)
        {
            UseLineTool();
        }

        private void BtnRectangleTool_Click(object sender, RoutedEventArgs e)
        {
            UseRectangleTool();
        }

        private void BtnEllipseTool_Click(object sender, RoutedEventArgs e)
        {
            UseEllipseTool();
        }

        private void BtnCirleTool_Click(object sender, RoutedEventArgs e)
        {
            UseCircleTool();
        }

        private void BtnCircle2Tool_Click(object sender, RoutedEventArgs e)
        {
            UseCircle2Tool();
        }

        private void BtnAngleTool_Click(object sender, RoutedEventArgs e)
        {
            UseAngleTool();
        }

        private void BtnAngle2Tool_Click(object sender, RoutedEventArgs e)
        {
            UseAngle2Tool();
        }

        private void BtnCaliperTool_Click(object sender, RoutedEventArgs e)
        {
            UseCaliperTool();
        }

        private void UseEditTool()
        {
            MyDrawMode = DrawMode.Edit;
            MyEditState = EditState.EditStateSelectTool;
            SetMnuMeasure(MnuEdit);
            Cursor = Cursors.Arrow;
        }

        private void UseLineTool()
        {
            MyDrawMode = DrawMode.Line;
            SetMnuMeasure(MnuLine);
            Cursor = Cursors.Pen;
        }

        private void UseRectangleTool()
        {
            MyDrawMode = DrawMode.Rectangle;
            SetMnuMeasure(MnuRectangle);
            Cursor = Cursors.Pen;
        }

        private void UseEllipseTool()
        {
            MyDrawMode = DrawMode.Ellipse;
            SetMnuMeasure(MnuEllipse);
            Cursor = Cursors.Pen;
        }

        private void UseCircleTool()
        {
            MyDrawMode = DrawMode.Circle;
            SetMnuMeasure(MnuCircle);
            Cursor = Cursors.Pen;
        }

        private void UseCircle2Tool()
        {
            MyDrawMode = DrawMode.Circle2;
            SetMnuMeasure(MnuCircle2);
            Cursor = Cursors.Pen;
        }

        private void UseAngleTool()
        {
            MyDrawMode = DrawMode.Angle;
            SetMnuMeasure(MnuAngle);
            Cursor = Cursors.Pen;
        }

        private void UseAngle2Tool()
        {
            MyDrawMode = DrawMode.Angle2;
            SetMnuMeasure(MnuAngle2);
            Cursor = Cursors.Pen;
        }

        private void UseCaliperTool()
        {
            MyDrawMode = DrawMode.Caliper;
            SetMnuMeasure(MnuCaliper);
            Cursor = Cursors.Pen;
        }

        private void SetMnuMeasure(MenuItem itm)
        {
            foreach (object mi in MnuMeasure.Items)
            {
                if (mi.GetType() == typeof(MenuItem))
                {
                    ((MenuItem)mi).IsChecked = false;
                }
            }
            itm.IsChecked = true;
        }

        #endregion

        #endregion

        #region "Mouse events to draw the Measurement Tools"

        private void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            myMouseDown = true;
            Point pt = e.GetPosition(canvas1);
            if (MyDrawMode == DrawMode.Edit)
            {
                if (MyEditState == EditState.EditStateSelectHandle) //A Tool was selected in MouseUp event.
                {
                    if (My_SelectedTool != null)
                    {
                        //Check if a Handle is selected
                        my_SelectedHandle = null;
                        foreach (Handle h in my_Handles)
                        {
                            if (h.IsMouseOver(pt))
                            {
                                my_SelectedHandle = h;
                            }
                        }
                        if (my_SelectedHandle == null)
                        {   //Check if the mouse is still inside the Tool when no handle was selected.
                            if (My_SelectedTool.IsMouseOver(pt))
                            {
                                //Mouse down inside a Tool but not in a Handle ->Tool Move Mode
                                MyEditState = EditState.EditStateMovingTool;
                                MouseStart = pt;
                            }
                        }
                        else
                        {
                            //Mouse down inside a Handle -> Handle Move Mode
                            MyEditState = EditState.EditStateMovingHandle;
                            my_SelectedHandle.Selected = true;
                            MouseStart = pt;
                        }
                    }
                }
            }
        }

        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!myLoaded) { return; }
            Point pt = e.GetPosition(canvas1);
            switch (MyDrawMode)
            {
                case DrawMode.Edit:
                    foreach (MeasurementTool mt in Tools)
                    {
                        if (mt.IsMouseOver(pt))
                        {
                            mt.ShowHandles();
                        }
                        else
                        {
                            if (My_SelectedTool != mt) mt.HideHandles();
                        }
                    }
                    if (MyEditState == EditState.EditStateSelectHandle) //Highlight a Handle of the selected Tool when the mouse is over it.
                    {
                        if (My_SelectedTool != null)
                        {
                            foreach (Handle h in My_SelectedTool.ToolHandles)
                            {
                                if (h.IsMouseOver(pt))
                                {
                                    h.Highlighted = true;
                                }
                                else
                                {
                                    h.Highlighted = false;
                                }
                            }
                        }
                        return;
                    }
                    if (MyEditState == EditState.EditStateMovingTool) //Move the entire tool
                    {
                        if (myMouseDown == true)
                        {
                            if (My_SelectedTool != null)
                            {
                                My_SelectedTool.Move(pt - MouseStart);
                                MouseStart = pt;
                            }
                        }
                        return;
                    }
                    if (MyEditState == EditState.EditStateMovingHandle) //Move 1 handle and update the Tool
                    {
                        if (My_SelectedTool != null && my_SelectedHandle != null)
                        {
                            my_SelectedHandle.SetCenter(pt);
                            My_SelectedTool.Update();
                        }
                        return;
                    }
                    break;
                case DrawMode.Line:
                    if (MyLine != null)
                    {
                        if (MyLine.DrawingState == 1)
                        {
                            MyLine.MouseMove(pt);
                        }
                    }
                    break;
                case DrawMode.Rectangle:
                    if (myRect != null)
                    {
                        if (myRect.DrawingState == 1)
                        {
                            myRect.MouseMove(pt);
                        }
                    }
                    break;
                case DrawMode.Ellipse:
                    if (myEllipse != null)
                    {
                        if (myEllipse.DrawingState == 1)
                        {
                            myEllipse.MouseMove(pt);
                        }
                    }
                    break;
                case DrawMode.Circle:
                    if (myCircle != null)
                    {
                        if (myCircle.DrawingState == 3)
                        {
                            myCircle.MouseMove(pt);
                        }
                    }
                    break;
                case DrawMode.Circle2:
                    if (myCircle2 != null)
                    {
                        if (myCircle2.DrawingState == 1 || myCircle2.DrawingState == 2)
                        {
                            myCircle2.MouseMove(pt);
                        }
                    }
                    break;
                case DrawMode.Angle:
                    if (myAngle != null)
                    {
                        if (myAngle.DrawingState == 1 || myAngle.DrawingState == 2)
                        {
                            myAngle.MouseMove(pt);
                        }
                    }
                    break;
                case DrawMode.Angle2:
                    if (myAngle2 != null)
                    {
                        if (myAngle2.DrawingState == 1 || myAngle2.DrawingState == 3)
                        {
                            myAngle2.MouseMove(pt);
                        }
                    }
                    break;
                case DrawMode.Caliper:
                    if (myCaliper != null)
                    {
                        if (myCaliper.DrawingState == 1 || myCaliper.DrawingState == 2)
                        {
                            myCaliper.MouseMove(pt);
                        }
                    }
                    break;
            }
        }

        private void canvas1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!myLoaded) { return; }
            if (myMouseDown == true) myMouseDown = false;
            Point pt = e.GetPosition(canvas1);
            switch (MyDrawMode)
            {
                case DrawMode.Edit:
                    if (MyEditState == EditState.EditStateMovingTool)
                    {   //End of Tool Moving
                        if (My_SelectedTool != null)
                        {
                            My_SelectedTool.HideHandles();
                            My_SelectedTool.Highlighted = false;
                            my_Handles = null;
                            MyEditState = EditState.EditStateSelectTool;
                        }
                        return;
                    }
                    if (MyEditState == EditState.EditStateMovingHandle)
                    {
                        //End of Handle Moving
                        if (my_SelectedHandle != null)
                        {
                            my_SelectedHandle.Highlighted = false;
                            my_SelectedHandle.Selected = false;
                            my_SelectedHandle.Update();
                            MyEditState = EditState.EditStateSelectHandle;
                        }
                        return;
                    }
                    My_SelectedTool = null;
                    foreach (MeasurementTool mt in Tools)
                    {
                        if (mt.IsMouseOver(pt))
                        {
                            mt.ShowHandles();
                            My_SelectedTool = mt;
                            mt.Highlighted = true;
                            my_Handles = mt.ToolHandles;
                            MyEditState = EditState.EditStateSelectHandle;
                        }
                        else
                        {
                            mt.Highlighted = false;
                            mt.HideHandles();
                        }
                    }
                    break;
                case DrawMode.Line:
                    if (MyLine == null)
                    {
                        MyLine = new LineTool(MyParent.Settings);
                        MyLine.Draw(canvas1);
                        Tools.Add(MyLine);
                    }
                    MyLine.MouseUp(pt);
                    if (MyLine.Finished)
                    {
                        MyLine = null;  //ready for next Line measurement
                    }
                    break;
                case DrawMode.Rectangle:
                    if (myRect == null)
                    {
                        myRect = new RectangleTool(MyParent.Settings);
                        myRect.Draw(canvas1);
                        Tools.Add(myRect);
                    }
                    myRect.MouseUp(pt);
                    if (myRect.Finished)
                    {
                        myRect = null; //ready for next Ellipse measurement
                    }
                    break;
                case DrawMode.Ellipse:
                    if (myEllipse == null)
                    {
                        myEllipse = new EllipseTool(MyParent.Settings);
                        myEllipse.Draw(canvas1);
                        Tools.Add(myEllipse);
                    }
                    myEllipse.MouseUp(pt);
                    if (myEllipse.Finished)
                    {
                        myEllipse = null;  //ready for next Ellipse measurement
                    }
                    break;
                case DrawMode.Circle:
                    if (myCircle == null)
                    {
                        myCircle = new CircleTool(MyParent.Settings);
                        myCircle.Draw(canvas1);
                        Tools.Add(myCircle);
                    }
                    myCircle.MouseUp(pt);
                    if (myCircle.Finished)
                    {
                        myCircle.HideHandles();
                        myCircle = null;  //ready for next Circle measurement
                    }
                    break;
                case DrawMode.Circle2:
                    if (myCircle2 == null)
                    {
                        myCircle2 = new Circle2Tool(MyParent.Settings);
                        myCircle2.Draw(canvas1);
                        Tools.Add(myCircle2);
                    }
                    myCircle2.MouseUp(pt);
                    if (myCircle2.Finished)
                    {
                        myCircle2.HideHandles();
                        myCircle2 = null;  //ready for next Circle2 measurement
                    }
                    break;
                case DrawMode.Angle:
                    if (myAngle == null)
                    {
                        myAngle = new AngleTool(MyParent.Settings);
                        myAngle.Draw(canvas1);
                        Tools.Add(myAngle);
                    }
                    myAngle.MouseUp(pt);
                    if (myAngle.Finished)
                    {
                        myAngle.HideHandles();
                        myAngle = null; //Ready for next Angle measurement
                    }
                    break;
                case DrawMode.Angle2:
                    if (myAngle2 == null)
                    {
                        myAngle2 = new Angle2Tool(MyParent.Settings);
                        myAngle2.Draw(canvas1);
                        Tools.Add(myAngle2);
                    }
                    myAngle2.MouseUp(pt);
                    if (myAngle2.Finished)
                    {
                        myAngle2.HideHandles();
                        myAngle2 = null; //Ready for next Angle measurement
                    }
                    break;
                case DrawMode.Caliper:
                    if (myCaliper == null)
                    {
                        myCaliper = new CaliperTool(MyParent.Settings);
                        myCaliper.Draw(canvas1);
                        Tools.Add(myCaliper);
                    }
                    myCaliper.MouseUp(pt);
                    if (myCaliper.Finished)
                    {
                        myCaliper.HideHandles();
                        myCaliper = null; //Ready for next Angle measurement
                    }
                    break;
            }
        }

        #endregion

        public void SetImage(ImageBorder img)
        {
            SetImage(img.Image, img.Filename);
        }

        public void SetImage(BitmapImage image, string file)
        {
            if (image != null)
            {
                myImage = image;
                InitialScale = canvas1.ActualHeight / myImage.PixelHeight;
                canvas1.Height = canvas1.ActualHeight;
                canvas1.Width = InitialScale * myImage.PixelWidth;
                canvas1.Children.Clear();
                canvas1.Background = new ImageBrush(myImage);
            }
            myLoaded = false;
            Title = "Image Viewer";
            TxtImageName.Text = file;
            myLoaded = true;
        }

        private void SaveImage(string filename)
        {
            BitmapEncoder MyEncoder;
            RenderTargetBitmap renderbmp = ConvertToBitmap(canvas1);
            try
            {
                switch (MyParent.Settings.ImageFormatIndex)
                {
                    case 1:
                        MyEncoder = new BmpBitmapEncoder();
                        break;
                    case 2:
                        MyEncoder = new JpegBitmapEncoder();
                        break;

                    case 3:
                        MyEncoder = new GifBitmapEncoder();
                        break;

                    case 4:
                        MyEncoder = new TiffBitmapEncoder();
                        break;

                    case 5:
                        MyEncoder = new PngBitmapEncoder();
                        break;

                    default:
                        //Should not occur
                        return;
                }
                MyEncoder.Frames.Add(BitmapFrame.Create(renderbmp));
                // Create a FileStream to write the image to the file.
                if (File.Exists(filename)) { File.Delete(filename); }
                using (FileStream sw = new FileStream(filename, FileMode.Create))
                {
                    MyEncoder.Save(sw);
                }
            }
            catch
            {
                MessageBox.Show("The Image could not be saved.", "ImageTools error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private RenderTargetBitmap ConvertToBitmap(UIElement uiElement)
        {
            uiElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size sz = uiElement.DesiredSize;
            Rect rec = new Rect(sz);
            uiElement.Arrange(rec);
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)(rec.Width), (int)(rec.Height), 96, 96, PixelFormats.Default);
            bmp.Render(uiElement);
            return bmp;
        }

        public BitmapImage ToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream mystream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(mystream);
                mystream.Position = 0;
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = mystream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }
    }

    public enum DrawMode
    {
        None,
        Line,
        Rectangle,
        Ellipse,
        Circle,
        Circle2,
        Angle,
        Angle2,
        Caliper,
        Edit
    }

    public enum EditState
    {
        None,
        EditStateSelectTool,
        EditStateSelectHandle,
        EditStateMovingTool,
        EditStateMovingHandle
    }
}
