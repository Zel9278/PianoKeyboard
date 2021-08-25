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
        public SettingWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                e.Cancel = true;
                Visibility = Visibility.Collapsed;
            };
        }

        private void OutputComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (OutputComboBox.SelectedItem == null) return;
            MainWindow.output midiOutCaps = (MainWindow.output)OutputComboBox.SelectedItem;

            Console.WriteLine("Selected: {0} - {1}", midiOutCaps.index, midiOutCaps.szPname);

            NativeMethods.midiOutClose(MainWindow.hMidiOut);

            NativeMethods.midiOutOpen(out MainWindow.hMidiOut, midiOutCaps.index, IntPtr.Zero, IntPtr.Zero, uint.MinValue);
            NativeMethods.midiOutShortMsg(MainWindow.hMidiOut, 0x0000C0);
        }
    }
}
