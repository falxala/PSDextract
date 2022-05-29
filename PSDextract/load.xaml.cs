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
using System.Windows.Shapes;

namespace PSD
{
    /// <summary>
    /// load.xaml の相互作用ロジック
    /// </summary>
    public partial class load : Window
    {
        public load()
        {
            InitializeComponent();
        }

        private void ProgressBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
