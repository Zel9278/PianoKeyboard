using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PianoKeyboard
{
    /// <summary>
    /// ReadmeWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ReadmeWindow : Window
    {
        public ReadmeWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                e.Cancel = true;
                Visibility = Visibility.Collapsed;
            };
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = (Hyperlink)sender;
            Process.Start(hyperlink.NavigateUri.ToString());
        }
    }
}
