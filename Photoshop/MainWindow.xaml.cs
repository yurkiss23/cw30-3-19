using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Photoshop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Path { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.comboColors.SelectedIndex = 0;
            this.comboSize.SelectedIndex = 0;
        }

        private void ComboColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string colorToUse = (this.comboColors.SelectedItem as StackPanel).Tag.ToString();
            this.myInkCanvas.DefaultDrawingAttributes.Color = (Color)ColorConverter.ConvertFromString(colorToUse);
        }

        private void ComboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sizeToUse = Int32.Parse((this.comboSize.SelectedItem as StackPanel).Tag.ToString());
            this.myInkCanvas.DefaultDrawingAttributes.Height
                = this.myInkCanvas.DefaultDrawingAttributes.Width = sizeToUse;
        }

        private void RadioButtonClick(object sender, RoutedEventArgs e)
        {
            switch ((sender as RadioButton).Content.ToString())
            {
                case "Ink Mode":
                    this.myInkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    break;
                case "Erase Mode":
                    this.myInkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    break;
                case "Select Mode":
                    this.myInkCanvas.EditingMode = InkCanvasEditingMode.Select;
                    break;
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            this.myInkCanvas.Background = new SolidColorBrush(Colors.White);
            this.myInkCanvas.Strokes = new StrokeCollection();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter= "Файли проекта(*.bin) | *.bin";
            if (sfd.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                {
                    this.myInkCanvas.Strokes.Save(fs);
                }
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Файли проекта(*.bin) | *.bin";
                if (ofd.ShowDialog() == true)
                {
                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                    {
                        StrokeCollection stroke = new StrokeCollection(fs);
                        this.myInkCanvas.Strokes = stroke;
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("Error file not found {0}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Other Error: {0}", ex.Message);
            }
        }

        private void BtnSaveImg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter= "Файли зображень(*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                if (sfd.ShowDialog() == true)
                {
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)myInkCanvas.ActualWidth, (int)myInkCanvas.ActualHeight, 0, 0, PixelFormats.Default);
                    rtb.Render(myInkCanvas);
                    JpegBitmapEncoder jEnc = new JpegBitmapEncoder();
                    jEnc.Frames.Add(BitmapFrame.Create(rtb));
                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        jEnc.Save(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Other Error: {0}", ex.Message);
            }
        }

        private void BtnLoadImg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Файли зображень(*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                if (ofd.ShowDialog() == true)
                {
                    BitmapImage img = new BitmapImage(new Uri(ofd.FileName));
                    myInkCanvas.Background = new ImageBrush(img);
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("Error file not found {0}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Other Error: {0}", ex.Message);
            }
        }

        private void TabAlbum_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(DriveInfo drive in DriveInfo.GetDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Tag = drive;
                item.Header = drive.ToString();
                item.Items.Add("*");
                TrwDrv.Items.Add(item);
            }
        }

        private void trwDrv_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();
            DirectoryInfo dir;
            if (item.Tag is DriveInfo)
            {
                DriveInfo drive = (DriveInfo)item.Tag;
                dir = drive.RootDirectory;
            }
            else
            {
                dir = (DirectoryInfo)item.Tag;
            }

            Path = dir.FullName.ToString();

            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeViewItem nItem = new TreeViewItem();
                    nItem.Tag = subDir;
                    nItem.Header = subDir.ToString();
                    nItem.Items.Add("*");
                    item.Items.Add(nItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void trwDrv_Selected(object sender, RoutedEventArgs e)
        {
            wpGallery.Children.Clear();
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            try
            {
                if (item.Tag is DriveInfo)
                {
                    Path = ((DriveInfo)item.Tag).RootDirectory.FullName.ToString();
                }
                else
                {
                    Path = ((DirectoryInfo)item.Tag).FullName.ToString();
                }
                foreach (string file in Directory.GetFiles(Path))
                {
                    if (file.Substring(file.LastIndexOf('.'), 4).ToLower() == ".jpg")
                    {
                        Image img = new Image();
                        img.Stretch = Stretch.Uniform;
                        img.Source = new BitmapImage(new Uri(file));
                        wpGallery.Children.Add(img);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void btnMirrorV_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mirror Vertical");
        }

        private void btnMirrorH_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mirror Horizontal");
        }

        private void btnFlipR_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Flip Right");
        }

        private void btnFlipL_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Flip Left");
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (viewList.SelectedIndex)
            {
                //case 0:
                //    MessageBox.Show("List");
                //    break;
                case 0:
                    //MessageBox.Show("Small");
                    wpGallery.ItemWidth = 40;
                    break;
                case 1:
                    //MessageBox.Show("Middle");
                    wpGallery.ItemWidth = 80;
                    break;
                case 2:
                    //MessageBox.Show("Big");
                    wpGallery.ItemWidth = 120;
                    break;
            }
        }
    }
}
