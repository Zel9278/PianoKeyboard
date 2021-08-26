using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace PianoKeyboard
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public Extensions.NoteConverter noteConverter = new Extensions.NoteConverter();
        public Extensions.WinMMManager winMM = new Extensions.WinMMManager(false);
        public Extensions.KeyboardRender keyboardRender;
        public Extensions.KeyLayout keyLayout;

        private SettingWindow setting;
        private AboutBox about;

        public MainWindow()
        {
            InitializeComponent();

            keyboardRender = new Extensions.KeyboardRender(this);
            keyLayout = new Extensions.KeyLayout(this);

            setting = new SettingWindow(this);
            about = new AboutBox();

            KeyDown += keyLayout.KeyDownHandler;
            KeyUp += keyLayout.KeyUpHandler;

            Keyboard.MouseLeave += keyboardRender.MouseLeaveHandler;
            Keyboard.MouseDown += keyboardRender.MouseDownHandler;
            Keyboard.MouseMove += keyboardRender.MouseMoveHandler;
            Keyboard.MouseUp += keyboardRender.MouseUpHandler;

            Closing += (s, e) =>
            {
                Application.Current.Shutdown();
            };
        }

        public void NoteOnHandler(Extensions.NoteConverter.noteMap note)
        {
            keyboardRender.ChangeKeyColor(note, true);
            winMM.NoteOn(note.note);
        }

        public void NoteOffHandler(Extensions.NoteConverter.noteMap note)
        {
            keyboardRender.ChangeKeyColor(note, false);
            winMM.NoteOff(note.note);
        }

        private void SettingItem_Click(object sender, RoutedEventArgs e)
        {
            setting.ShowDialog();
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            about.ShowDialog();
        }
    }
}
