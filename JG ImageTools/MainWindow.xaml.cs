using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageTools
{
    public partial class MainWindow : Window
    {
        private List<ImageBorder> Borders;
        private ImageBorder SelectedImageBorder;
        private SettingForm Setter;
        private ImgViewer Viewer;
        private PictureControl Editor;

        [DllImport("mpr.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WNetGetConnection([MarshalAs(UnmanagedType.LPWStr)] string localName, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder remoteName, ref int length);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SplashScreen splashScreen = new SplashScreen();
            //splashScreen.Show();
            Setter = new SettingForm(this);
            Init();
            //splashScreen.Close();
        }

        public SettingForm Settings
        {
            get { return Setter; }
        }

        #region "TreeView Code"

        private void Init()
        {
            Cursor = Cursors.AppStarting;
            //Make the MyComputer Item
            TrvFolders.Items.Clear();
            TreeViewItem TVItem = MakeTreeViewItem(Environment.CurrentDirectory + "\\Images\\Computer.jpg", "My Computer", ""); //New TreeViewItem() ;
            TVItem.Items.Clear();
            TrvFolders.Items.Add(TVItem);
            //Make all the Drive items in MyComputer
            foreach (string s in Directory.GetLogicalDrives())
            {
                if (GetUncPath(s) != "")
                {
                    TVItem = MakeTreeViewItem(Environment.CurrentDirectory + "\\Images\\Drive.jpg", GetUncPath(s), s); //New TreeViewItem();
                    ((TreeViewItem)TrvFolders.Items[0]).Items.Add(TVItem);
                }
            }
            //Expand the TreeView to the MainStartFolder
            ((TreeViewItem)TrvFolders.Items[0]).IsExpanded = true;
            string[] dirs = Setter.StartFolder.Split('\\');
            string fol = "";
            TreeViewItem tvi = (TreeViewItem)TrvFolders.Items[0];
            foreach (string dir in dirs)
            {
                fol = fol + dir + "\\";
                for (int I = 0; I < tvi.Items.Count; I++)
                {
                    if ((string)((TreeViewItem)tvi.Items[I]).Tag == fol | (string)((TreeViewItem)tvi.Items[I]).Tag + "\\" == fol)
                    {
                        tvi = (TreeViewItem)tvi.Items[I];
                        Expand(tvi, fol);
                        try
                        {
                            tvi.IsExpanded = true;
                        }
                        catch
                        {
                            //Do nothing
                        }
                        break;
                    }
                }
            }
            tvi.IsSelected = true;
            try
            {
                if (!Directory.Exists(Setter.StartFolder))
                {
                    Setter.StartFolder = "C:\\";
                }
            }
            catch
            {
                Setter.StartFolder = "C:\\";
            }
            ShowImages(Setter.StartFolder);
            Cursor = Cursors.Arrow;
        }

        private TreeViewItem MakeTreeViewItem(string imagefile, string header, string tag)
        {
            TreeViewItem TVItem = new TreeViewItem();
            TreeViewItem dummyNode = new TreeViewItem();
            dummyNode.Tag = "dummyNode";
            StackPanel pnl = new StackPanel();
            pnl.Orientation = Orientation.Horizontal;
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(imagefile));
            Label txtHeader = new Label();
            txtHeader.Padding = new Thickness(1.0);
            txtHeader.Content = header;
            pnl.Children.Add(img);
            pnl.Children.Add(txtHeader);
            TVItem.Header = pnl;
            TVItem.Tag = tag;
            TVItem.Items.Add(dummyNode);
            TVItem.Expanded += Trv_Expand;
            TVItem.Selected += Trv_Select;
            return TVItem;
        }

        private string GetUncPath(string LocalPath)
        {
            string result = "";
            int bsize = 259;
            string DriveName = LocalPath.Substring(0, LocalPath.IndexOf(":") + 1);
            if (DriveName != "")
            {
                DriveInfo dInfo = new DriveInfo(DriveName);
                if (dInfo.DriveType == DriveType.Network)
                {
                    StringBuilder sb = new StringBuilder(bsize);
                    int err = WNetGetConnection(DriveName, sb, ref bsize);
                    if (err == 0)
                    {
                        string[] names = sb.ToString().Split('\\');
                        result = names[3] + " (" + LocalPath + ")";
                    }
                }
                else
                {
                    if (dInfo.IsReady)
                    {
                        try
                        {
                            result = dInfo.VolumeLabel + " (" + LocalPath + ")";
                        }
                        catch
                        {
                            Exception ex;
                            result = LocalPath;
                        }
                    }
                }
            }
            return result;
        }

        private void Trv_Expand(object sender, RoutedEventArgs e)
        {
            TreeViewItem ExpandedItem = (TreeViewItem)sender;
            Expand(ExpandedItem, ExpandedItem.Tag.ToString());
        }

        private void Expand(TreeViewItem item, string folder)
        {
            TreeViewItem TVSubItem;
            if (item.Items.Count == 1 && (string)((TreeViewItem)item.Items[0]).Tag == "dummyNode")
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(folder))
                    {
                        TVSubItem = MakeTreeViewItem(Environment.CurrentDirectory + "\\Images\\Folder.jpg", Path.GetFileName(s), s); //New TreeViewItem();
                        item.Items.Add(TVSubItem);
                    }
                }
                catch
                {
                    //Do nothing
                }
            }
        }

        private void Trv_Select(object sender, RoutedEventArgs e)
        {
            ShowImages((string)((TreeViewItem)sender).Tag);
            e.Handled = true;
        }

        #endregion

        private void ShowImages(string dir)
        {
            ImageBorder bord;
            string f_Ext = "";
            string t_Ext = "";
            List<string> files = new List<string>();
            Thumbnails.Children.Clear();
            Borders = new List<ImageBorder>();
            Cursor = Cursors.Wait;
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                {
                    if (Path.GetFileNameWithoutExtension(f).Length > 0)
                    {
                        files.Add(f);
                    }
                }
            }
            catch
            {
                // MessageBox.Show("Access denied!", "TreeView Error.", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            }
            foreach (string f in files)
            {
                f_Ext = Path.GetExtension(f).ToLower();
                if (f_Ext == ".bmp" || f_Ext == ".jpg" || f_Ext == ".tif" || f_Ext == ".tiff" || f_Ext == ".png" || f_Ext == ".heic")
                {
                    bord = new ImageBorder(f, this);
                    Borders.Add(bord);
                    Thumbnails.Children.Add(bord);
                }
            }
            Setter.StartFolder = dir;
            if(Borders.Count > 0)
            {
                SetSelected(Borders[0]);
            }
            Cursor = Cursors.Arrow;
        }

        public void SetSelected(ImageBorder imgborder)
        {
            SelectedImageBorder = imgborder;
            for (int i = 0; i < Borders.Count; i++)
            {
                if (Borders[i].Equals(SelectedImageBorder))
                {
                    Borders[i].Background = Setter.ThumbSelectedBackground;
                }
                else
                {
                    Borders[i].Background = Setter.ThumbBackground;
                }
            }
        }

        private void Thumbnails_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (SelectedImageBorder != null)
                {
                    if (Editor == null) { Editor = new PictureControl(this); }
                    Editor.Show();
                    Editor.SetImage(SelectedImageBorder);
                    Setter.ImageFileName = SelectedImageBorder.Filename;
                }
                Editor.Focus();
            }
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Viewer != null) { Viewer.Close(); }
            if (Editor != null) { Editor.Close(); }
            if (Setter != null)
            {
                Setter.SaveSettings();
                Setter.Close();
            }
            Environment.Exit(0);
        }

        private void MnuEditor_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedImageBorder != null)
            {
                if (SelectedImageBorder.Image != null)
                {
                    ShowEditor(SelectedImageBorder);
                }
            }
        }

        public void ShowEditor(ImageBorder imgborder)
        {
            if (Editor == null) 
            { 
                Editor = new PictureControl(this);
            }
            if (imgborder != null)
            {
                if (imgborder.Image != null)
                {
                    Editor.Show();
                    Editor.SetImage(imgborder);
                    Setter.ImageFileName = imgborder.Filename;
                }
            }
            Editor.Focus();
        }

        public void ShowEditor(BitmapImage image, string filename)
        {
            if (Editor == null) 
            { 
                Editor = new PictureControl(this);
            }
            if (image != null)
            {
                Editor.Show();
                Editor.SetImage(image, filename);
                Setter.ImageFileName = filename;
            }
            Editor.Focus();
        }

        private void MnuViewer_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedImageBorder != null)
            {
                ShowViewer(SelectedImageBorder);
            }
            else
            {
                ShowViewer(null);
            }
        }

        public void ShowViewer(ImageBorder imgborder)
        {
            if (Viewer == null)
            {
                Viewer = new ImgViewer(this);
            }
            if (imgborder != null)
            {
                if (imgborder.Image != null)
                {
                    Viewer.Show();
                    Viewer.canvas1.Children.Clear();
                    Viewer.SetImage(imgborder);
                    Setter.ImageFileName = imgborder.Filename;
                }
            }
            Viewer.Focus();
        }

        public void ShowViewer(BitmapImage image, string filename)
        {
            if (Viewer == null)
            {
                Viewer = new ImgViewer(this);
            }
            if (image != null)
            {
                Viewer.Show();
                Viewer.canvas1.Children.Clear();
                Viewer.SetImage(image, filename);
                Setter.ImageFileName = filename;
            }
            Viewer.Focus();
        }

        private void MnuOptions_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        public void ShowSettings()
        {
            if (Setter == null) { Setter = new SettingForm(this); }  //Should never occur!!
            Setter.Show();
        }

        public void RemoveImgBorder(ImageBorder bord)
        {
            if (Thumbnails.Children.Contains(bord))
            {
                Thumbnails.Children.Remove(bord);
            }
        }

        public void Update()
        {
            for (int I = 0; I < Borders.Count; I++)
            {
                Borders[I].Update();
            }
            //if (Viewer != null) { Viewer.Update(); }
        }

        private void MnuReset_Click(object sender, RoutedEventArgs e)
        {
            Init();
        }
    }
}
