using System.IO;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageTools
{
    public class ImageBorder : Border
    {
        private string my_Filename;
        private readonly BitmapImage bmp;
        private readonly Image img;
        private readonly TextBox txt;
        private Size my_Size;
        private MainWindow my_Main;

        public ImageBorder(string ImgFile, MainWindow Main)
        {
            my_Filename = ImgFile;
            my_Main = Main;
            my_Size = new Size(Main.Settings.ThumbWidth, Main.Settings.ThumbHeight);
            //Set the border parameters
            BorderBrush = Brushes.Blue;
            BorderThickness = new Thickness(2.0);
            Background = Main.Settings.ThumbBackground;
            if (Main.Settings.ThumbRoundedCorners)
            {
                CornerRadius = new CornerRadius(8.0);
            }
            else
            {
                CornerRadius = new CornerRadius(0.0);
            }
            Margin = new Thickness(Main.Settings.ThumbSpacing);
            Padding = new Thickness(5.0);
            //Made a grid with 2 rows.
            Grid gr = new Grid();
            RowDefinition rowdef= new RowDefinition();
            rowdef.Height = new GridLength(0.0, GridUnitType.Auto);
            gr.RowDefinitions.Add(rowdef);
            rowdef = new RowDefinition();
            rowdef.Height = new GridLength(0.0, GridUnitType.Star);
            gr.RowDefinitions.Add(rowdef);
            //Make the Image and TextBox.
            img = new Image();            
            txt = new TextBox();
            txt.Width = my_Size.Width;
            txt.Height = 25.0;
            txt.TextWrapping = TextWrapping.Wrap;
            txt.HorizontalContentAlignment = HorizontalAlignment.Center;
            txt.Background = Brushes.Beige;
            txt.FontSize = Main.Settings.ThumbFontSize;
            try
            {
                FileStream fs = new FileStream(ImgFile, FileMode.Open, FileAccess.Read);
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = fs;
                bmp.EndInit();
                fs.Close();
                img.Width = my_Size.Width;
                img.Height = my_Size.Height - 25;
                img.Source = bmp;
                GC.Collect();
                img.ContextMenu = GetContextMenu();
                txt.Text = Path.GetFileNameWithoutExtension(ImgFile);
            }
            catch
            {
                //Do nothing
            }
            //Place the Image and TextBox in the grid
            txt.SetValue(Grid.RowProperty, 0);
            img.SetValue(Grid.RowProperty, 1);
            gr.Children.Add(img);
            gr.Children.Add(txt);
            //Add the grid into the border
            Child = gr;
            PreviewMouseLeftButtonUp += ImageBorder_PreviewMouseLeftButtonUp;
        }

        public void Update()
        {
            my_Size.Width = my_Main.Settings.ThumbWidth;
            my_Size.Height = my_Main.Settings.ThumbHeight;
            Background = my_Main.Settings.ThumbBackground;
            if (my_Main.Settings.ThumbRoundedCorners)
            {
                CornerRadius = new CornerRadius(8.0);
            }
            else
            {
                CornerRadius = new CornerRadius(0.0);
            }
            Margin = new Thickness(my_Main.Settings.ThumbSpacing);
            img.Width = my_Size.Width;
            img.Height = my_Size.Height - 25;
            txt.Width = my_Size.Width;
            txt.FontSize = my_Main.Settings.ThumbFontSize;
        }

        public Size size
        {
            get { return my_Size; }
            set
            {
                my_Size = value;
                img.Width = my_Size.Width;
                img.Height = my_Size.Height - 25;
                txt.Width = my_Size.Width;
            }
        }

        public string Filename
        {
            get { return my_Filename; }
        }

        public BitmapImage Image
        {
            get { return bmp; }
        }

        private ContextMenu GetContextMenu()
        {
            ContextMenu cm = new ContextMenu();
            MenuItem mi = new MenuItem();
            mi.Header = "Copy";
            mi.Click += MenuCopy_click;
            cm.Items.Add(mi);
            mi = new MenuItem();
            mi.Header = "Measure";
            mi.Click += MenuMeasure_click;
            cm.Items.Add(mi);
            mi = new MenuItem();
            mi.Header = "Edit";
            mi.Click += MenuEdit_click;
            cm.Items.Add(mi);
            Separator sep = new Separator();
            cm.Items.Add(sep);
            mi = new MenuItem();
            mi.Header = "Delete";
            mi.Click += MnuDelete_Click;
            cm.Items.Add(mi);
            return cm;
        }

        private void MenuCopy_click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(bmp);
        }

        private void MenuMeasure_click(object sender, RoutedEventArgs e)
        {
            my_Main.ShowViewer(this);
        }

        private void MenuEdit_click(object sender, RoutedEventArgs e)
        {
            my_Main.ShowEditor(this);
        }

        private void MnuDelete_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(my_Filename))
            {
                File.Delete(my_Filename);
                my_Main.RemoveImgBorder(this);
            }
        }

        private void ImageBorder_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            my_Main.SetSelected(this);
        }
    }
}
