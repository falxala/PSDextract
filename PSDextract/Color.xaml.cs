using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static PSD.MainWindow;

namespace PSD
{
    /// <summary>
    /// Color.xaml の相互作用ロジック
    /// </summary>
    public partial class Colorw : Window
    {
        public static ObservableCollection<Psd> psds = new ObservableCollection<Psd>();
        public Colorw(Window window)
        {
            InitializeComponent();
            MyListView.DataContext = psds;
            TextBox1.DataContext = psds;
            psds.CollectionChanged += Col_CollectionChanged;
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void MyListBox_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var rgb = ColorTranslator.FromHtml((MyListView.SelectedItem as Psd).Field0);
            MainWindow.brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(rgb.A, rgb.R, rgb.G, rgb.B));
            MainWindow.brush.Freeze();
            TextBox1.Text = brush.ToString();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(TextBox1.Text);
        }

        private void Col_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            TextBox1.Text = (e.NewItems[0] as Psd).Field1;
        }

        private void Grid0_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            string str;
            html h = new html();
            var list = new List<string>();
            foreach(var item in MyListView.Items)
            {
               list.Add( h._ColorTable((item as Psd).Field1.Remove(1, 2), 1, (item as Psd).Field1.Remove(1,2)));
            }
            string a = "";
            foreach(var item in list)
            {
                a += item;
            }
            str = h._html(h._Body(a));

            Save(str);
        }

        private void Save(string str)
        {
            //SaveFileDialogクラスのインスタンスを作成
            SaveFileDialog sfd = new SaveFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            sfd.FileName = "新しいファイル.html";
            //はじめに表示されるフォルダを指定する
            sfd.InitialDirectory = @"C:\";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            sfd.Filter = "HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            sfd.FilterIndex = 2;
            //タイトルを設定する
            sfd.Title = "保存先のファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;
            //既に存在するファイル名を指定したとき警告する
            //デフォルトでTrueなので指定する必要はない
            sfd.OverwritePrompt = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            sfd.CheckPathExists = true;

            //ダイアログを表示する
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.Stream stream;
                stream = sfd.OpenFile();
                if (stream != null)
                {
                    //ファイルに書き込む
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                    sw.Write(str);
                    //閉じる
                    sw.Close();
                    stream.Close();
                }
            }
            
        }
    }
}
