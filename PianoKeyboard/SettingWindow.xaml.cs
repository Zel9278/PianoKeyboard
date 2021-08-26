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

namespace PianoKeyboard
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        MainWindow window;

        public SettingWindow(MainWindow _window)
        {
            InitializeComponent();

            window = _window;

            OutputComboBox.ItemsSource = _window.winMM.GetOutputList();
            OutputComboBox.SelectedItem = OutputComboBox.Items[0];

            Closing += (s, e) =>
            {
                e.Cancel = true;
                Visibility = Visibility.Collapsed;
            };
        }

        private void OutputComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (OutputComboBox.SelectedItem == null) return;
            Extensions.WinMMManager.output midiOutCaps = (Extensions.WinMMManager.output)OutputComboBox.SelectedItem;
            window.winMM.ChangeOutput(midiOutCaps.index);
        }
    }
}
