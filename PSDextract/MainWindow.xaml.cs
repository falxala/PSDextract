using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Threading;
using ImageMagick;

namespace PSD
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Psd> list = new ObservableCollection<Psd>();
        ObservableCollection<Psd> list2 = new ObservableCollection<Psd>();
        Colorw Colorwindow;
        Histogram histogram1;

        private System.Windows.Point _start;

        public MainWindow()
        {
            InitializeComponent();
            EnableDragDrop(Window1);
            Colorwindow = new Colorw(this);
            Colorwindow.Show();
            histogram1 = new Histogram();
            histogram1.Show();
            Colorwindow.Visibility = Visibility.Collapsed; 
            histogram1.Visibility= Visibility.Collapsed;
        }
        private void EnableDragDrop(Control control)
        {
            //ドラッグ＆ドロップを受け付けられるようにする
            control.AllowDrop = true;

            //ドラッグが開始された時のイベント処理（マウスカーソルをドラッグ中のアイコンに変更）
            control.PreviewDragOver += (s, e) =>
            {
                //ファイルがドラッグされたとき、カーソルをドラッグ中のアイコンに変更し、そうでない場合は何もしない。
                e.Effects = (e.Data.GetDataPresent(DataFormats.FileDrop)) ? DragDropEffects.Copy : e.Effects = DragDropEffects.None;
                e.Handled = true;
            };

            //ドラッグ＆ドロップが完了した時の処理（ファイル名を取得し、ファイルの中身をTextプロパティに代入）
            control.PreviewDrop += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) // ドロップされたものがファイルかどうか確認する。
                {
                    string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                    for (int i = 0; i < paths.Count(); i++)
                    {
                        using (var mymagick = new ImageMagick.MagickImage(paths[i]))
                        {
                            mymagick.Thumbnail(500, 500);
                            mymagick.Format = ImageMagick.MagickFormat.Tif;
                            AddItem(list,paths[i], ByteArrayToImage(mymagick.ToByteArray()));
                        }
                    }
                }
            };

        }
        /// <summary>
        /// バイトからBitmapimageへ変換
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public BitmapImage ByteArrayToImage(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            BitmapImage imageSource = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                stream.Seek(0, SeekOrigin.Begin);
                imageSource.BeginInit();
                imageSource.StreamSource = stream;
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.EndInit();
            }
            return imageSource;
        }
        public class Psd
        {
            public Int64 Id { get; set; }
            public string Field0 { get; set; }
            public string Field1 { get; set; }
            public BitmapImage Field2 { get; set; }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddItem(ObservableCollection<Psd> list,string text,BitmapImage image)
        {
            // 追加する項目のIdの値を計算する
            Int64 newId;
            if (list.Count > 0)
            {
                // 既存の項目のIdの最大値をを求めて、それに+1する
                var query = from p in list select p.Id;
                newId = query.Max() + 1;
            }
            else
            {
                newId = 1;
            }
            Psd item = new Psd { Id = newId,Field0 = System.IO.Path.GetFileName(text),  Field1 = text, Field2 = image }; // 追加する項目の内容を設定する
            list.Add(item); // listに項目を追加する
        }

        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            MyListView.DataContext = list;
            MyListView_Copy.DataContext = list2;
        }

        private async void MyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            load loading = new load();
            loading.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            loading.Show();
            loading.Topmost = true;
            await Task.Delay(100);

            Image1.Source = null;
            list2.Clear();

            using (var myMagicks = new ImageMagick.MagickImageCollection((MyListView.SelectedItem as Psd).Field1))
            {
                Image1.Source = (MyListView.SelectedItem as Psd).Field2;
                await Task.Run(() =>
                {
                    for (int i = 0; i < myMagicks.Count; i++)
                    {

                        myMagicks[i].ColorAlpha(ImageMagick.MagickColors.Transparent);
                        myMagicks[i].Format = MagickFormat.Bmp;
                        this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                        {
                            AddItem(list2, i.ToString(), ByteArrayToImage(myMagicks[i].ToByteArray()));
                            loading.ProgressBar1.Value = (float)i / myMagicks.Count * 100;
                        }));
                    }
                });
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            MyListView_Copy.Focus();
            MyListView_Copy.SelectedIndex = 0;
            MyListView_Copy.SelectedItem = 1;
            loading.Close();

        }

        private void MyListBox_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyListView_Copy.Items.Count != 0)
            {
                Image1.Source = (MyListView_Copy.SelectedItem as Psd).Field2;

                histogram1.plot1.Plot.Clear();
                histogram1.plot1.Refresh();
                AccessPixels((MyListView_Copy.SelectedItem as Psd).Field2);
                histogram1.plot1.Render();
            }
        }

        public BitmapSource bmp = new BitmapImage();
        public static SolidColorBrush brush = new SolidColorBrush();
        private void Image1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Image1.IsMouseCaptured)
            {
                var matrix = Image1.RenderTransform.Value;

                Vector v = _start - e.GetPosition(Border1);
                matrix.Translate(-v.X, -v.Y);
                Image1.RenderTransform = new MatrixTransform(matrix);
                _start = e.GetPosition(Border1);
            }
            else
            {

                System.Drawing.Point p = System.Windows.Forms.Cursor.Position;
                // 矩形領域
                System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle((int)p.X - 5, (int)p.Y - 5, 10, 10);
                Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(new System.Drawing.Point(rectangle.X, rectangle.Y), new System.Drawing.Point(0, 0), bitmap.Size);
                // グラフィックスの解放
                graphics.Dispose();
                bmp = BitmaoToImage(bitmap);
                var rgb = bitmap.GetPixel(5, 5);
                brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(rgb.A, rgb.R, rgb.G, rgb.B));
                ColorPicker.SelectedColor = System.Windows.Media.Color.FromArgb(rgb.A, rgb.R, rgb.G, rgb.B);
            }
        }

        BitmapSource BitmaoToImage(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            System.Windows.Media.Imaging.BitmapSource bitmapsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
            return bitmapsource;
        }

        private void Image1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
                Image1.RenderTransform = new MatrixTransform(1, 0, 0, 1, 0, 0);
        }

        private void Image1_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Cross;
        }

        private void Image1_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void Window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Colorwindow.Close();
            histogram1.Close();
        }

        private void Image1_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // スケールの値を変えることでホイールを動かした時の拡大率を制御できます
            const double scale = 1.2;
            var pos = e.GetPosition((UIElement)sender);
            var matrix = Image1.RenderTransform.Value;
            if (e.Delta > 0)
            {
                // 拡大処理
                matrix.ScaleAt(scale, scale, pos.X, pos.Y);
            }
            else
            {
                // 縮小処理
                matrix.ScaleAt(1.0 / scale, 1.0 / scale, pos.X, pos.Y);
            }

            Image1.RenderTransform = new MatrixTransform(matrix);
        }

        private void Image1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image1.CaptureMouse();
            _start = e.GetPosition(Border1);
        }

        private void Image1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Colorw.psds.Add(new Psd { Id = 0, Field0 = brush.ToString(), Field1 = brush.ToString(), Field2 = null });
        }

        private void Image1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image1.ReleaseMouseCapture();
        }

        private void Window1_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public class ChartData
        {
            public double[] A { get; set; }
            public double[] R { get; set; }
            public double[] G { get; set; }
            public double[] B { get; set; }
        }

        private unsafe ChartData AccessPixels(in BitmapImage bitmapimageOriginal)
        {
            histogram1.plot1.Plot.Clear();
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bitmapimageOriginal, PixelFormats.Pbgra32, null, 0);
            bitmap.Freeze();
            byte[] originalPixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];//bgrαの4種 4*8bit=32bit
            int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(originalPixels, stride, 0);

            ChartData data = new ChartData();
            data.A = new double[256];
            data.R = new double[256];
            data.G = new double[256];
            data.B = new double[256];

            //並列処理
            Parallel.For(0, bitmap.PixelHeight, y =>
            {
                for (int x = 0; x < bitmap.PixelWidth; ++x)
                {
                    int offset = (bitmap.PixelWidth * y + x) * 4;
                    data.B[originalPixels[0 + offset]]++;
                    data.G[originalPixels[1 + offset]]++;
                    data.R[originalPixels[2 + offset]]++;
                    data.A[originalPixels[3 + offset]]++;
                }
            });

            var MaxA = data.A.Max();
            var MaxR = data.R.Max();
            var MaxG = data.G.Max();
            var MaxB = data.B.Max();

            Parallel.For(0, 256, w =>
            {
                data.R[w] = data.R[w] / MaxR;
                data.G[w] = data.G[w] / MaxG;
                data.B[w] = data.B[w] / MaxB;
                data.A[w] = data.A[w] / MaxA;
            });

            /*
            var Alpha = histogram1.plot1.Plot.AddBar(data.A, System.Drawing.Color.Black);
            Alpha.BarWidth = 1;
            Alpha.FillColor = System.Drawing.Color.FromArgb(100, System.Drawing.Color.Black);
            Alpha.BorderLineWidth = 0;
            */

            // plot histograms
            var Red = histogram1.plot1.Plot.AddBar(data.R, System.Drawing.Color.Red);
            Red.BarWidth = 1;
            Red.FillColor = System.Drawing.Color.FromArgb(100, System.Drawing.Color.Red);
            Red.BorderLineWidth = 0;

            var Green = histogram1.plot1.Plot.AddBar(data.G, System.Drawing.Color.Green);
            Green.BarWidth = 1;
            Green.FillColor = System.Drawing.Color.FromArgb(100, System.Drawing.Color.Green);
            Green.BorderLineWidth = 0;

            var Blue = histogram1.plot1.Plot.AddBar(data.B, System.Drawing.Color.Blue);
            Blue.BarWidth = 1;
            Blue.FillColor = System.Drawing.Color.FromArgb(100, System.Drawing.Color.Blue);
            Blue.BorderLineWidth = 0;

            return data;

        }

        private unsafe ImageSource AccessPixels2(in BitmapImage bitmapimageOriginal,double num)
        {
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bitmapimageOriginal, PixelFormats.Pbgra32, null, 0);
            bitmap.Freeze();
            byte[] originalPixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];//bgrαの4種 4*8bit=32bit
            int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(originalPixels, stride, 0);

            //並列処理
            Parallel.For(0, bitmap.PixelHeight, y =>
            {
                for (int x = 0; x < bitmap.PixelWidth; ++x)
                {
                    int offset = (bitmap.PixelWidth * y + x) * 4;
                    originalPixels[0 + offset] = (byte)(originalPixels[0 + offset] / num);
                    originalPixels[1 + offset] = (byte)(originalPixels[1 + offset] / num);
                    originalPixels[2 + offset] = (byte)(originalPixels[2 + offset] / num);
                    originalPixels[3 + offset] = (byte)(originalPixels[3 + offset]);
                }
            });

            BitmapSource originalBitmap = BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, 96, 96, PixelFormats.Pbgra32, null, originalPixels, stride);//変更データ(配列)をBitmapSourceに変換
            return originalBitmap;

        }

        private void plot1_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Checked_1(object sender, RoutedEventArgs e)
        {
            histogram1.Visibility = Visibility.Visible;

        }

        private void MenuItem_Checked_2(object sender, RoutedEventArgs e)
        {

            Colorwindow.Visibility = Visibility.Visible;

        }

        private void Hist_Unchecked(object sender, RoutedEventArgs e)
        {
            histogram1.Visibility = Visibility.Hidden;
        }

        private void Colorhist_Unchecked(object sender, RoutedEventArgs e)
        {
            Colorwindow.Visibility = Visibility.Hidden;
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyListView_Copy.SelectedIndex != -1)
                image2.Source = AccessPixels2((MyListView_Copy.SelectedItem as Psd).Field2, slider1.Value);
        }
    }
}