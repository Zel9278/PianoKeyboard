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

public class NativeMethods
{
    public struct MidiOutCaps
    {
        public ushort wMid;
        public ushort wPid;
        public uint vDriverVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;
        public uint dwSupport;
    }

    [DllImport("winmm.dll")]
    public static extern uint midiOutGetNumDevs();

    [DllImport("winmm.dll")]
    public static extern uint midiOutGetDevCaps(uint uDevID, out MidiOutCaps pmic, int cbmic);

    [DllImport("winmm.dll")]
    public static extern uint midiOutEvents(uint uDevID, out MidiOutCaps pmic, int cbmic);

    [DllImport("winmm.dll")]
    public static extern uint midiOutOpen(out IntPtr lphMidiOut, int uDeviceID, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);

    [DllImport("winmm.dll")]
    public static extern uint midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

    [DllImport("winmm.dll")]
    public static extern uint midiOutClose(IntPtr hMidiOut);

    [DllImport("winmm.dll")]
    public static extern uint midiOutReset(IntPtr hMidiOut);
}

namespace PianoKeyboard
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public struct keyCode
        {
            public Key jp;
            public Key en_us;
        }

        public struct keyMap
        {
            public keyCode keyCode;
            public byte note;
            public Rectangle Key;
            public bool isSharp;
        }

        public struct output
        {
            public int index { get; set; }
            public string szPname { get; set; }
        }

        private SettingWindow setting = new SettingWindow();
        private Extensions.NoteConverter noteConverter = new Extensions.NoteConverter();

        public static IntPtr hMidiOut;

        private List<Key> activeKeyList = new List<Key>();
        private List<output> midiOutputList = new List<output>();

        private int octave = 0;
        private bool isMousePushed = false;
        private keyMap nowKey;

        private List<keyMap> keyList = new List<keyMap>();
        
        public MainWindow()
        {
            InitializeComponent();

            uint midiOutNumDevs = NativeMethods.midiOutGetNumDevs();
            Console.WriteLine("midi # of devs: {0}", midiOutNumDevs);

            if (midiOutNumDevs == 0)
            {
                MessageBox.Show("This PC does not have a MIDI output device.", "PianoKeyboard",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            for (uint i = 0; i < midiOutNumDevs; i++)
            {
                NativeMethods.MidiOutCaps midiOutCaps = new NativeMethods.MidiOutCaps();
                NativeMethods.midiOutGetDevCaps(i, out midiOutCaps, Marshal.SizeOf(typeof(NativeMethods.MidiOutCaps)));
                Console.WriteLine("#{0}: {1}", i, midiOutCaps.szPname);

                midiOutputList.Add(new output { index = Convert.ToInt32(i), szPname = midiOutCaps.szPname });
            }

            setting.OutputComboBox.ItemsSource = midiOutputList;
            setting.OutputComboBox.SelectedItem = setting.OutputComboBox.Items[0];

            Console.WriteLine(CultureInfo.CurrentCulture.KeyboardLayoutId);
            Console.WriteLine(noteConverter.ToByte("c4").ToString());

            /*
                jp106/109 / 1041

                 2   4 5 6   8 9   - ^
                q w e r t y u i o p @ [

                 A#  C#D#  F#G#A#  C#D#
                A B C D E F G A B C D E

                 s   f g h   k l   :
                z x c v b n m , . / \

                 A#  C#D#  F#G#A#  C#
                A B C D E F G A B C D

                en101/102 / 1033

                 2   4 5 6   8 9   - =
                q w e r t y u i o p [ ]

                 A#  C#D#  F#G#A#  C#D#
                A B C D E F G A B C D E

                 s   f g h   k l ;
                z x c v b n m , . /

                 A#  C#D#  F#G#A#
                A B C D E F G A B C
            */

            /*MouseLeave += (s, e) =>
            {
                isMousePushed = false;

                if (activeKeyList.Any(x => x == nowKey.KeyCode))
                {
                    activeKeyList.Remove(nowKey.KeyCode);
                }
                KeyHandler(nowKey.KeyCode, false);
            };

            MouseDown += (s, e) =>
            {
                if (e.Source.ToString() != "System.Windows.Shapes.Rectangle") return;
                Rectangle item = (Rectangle)e.Source;
                keyMap key = keyList.Find(a => a.Key == item);

                isMousePushed = true;

                nowKey = key;

                if (activeKeyList.Any(x => x == key.KeyCode)) return;
                activeKeyList.Add(key.KeyCode);
                KeyHandler(key.KeyCode, true);
            };

            MouseUp += (s, e) =>
            {
                if (e.Source.ToString() != "System.Windows.Shapes.Rectangle") return;
                Rectangle item = (Rectangle)e.Source;
                keyMap key = keyList.Find(a => a.Key == item);

                isMousePushed = false;

                if (activeKeyList.Any(x => x == nowKey.KeyCode))
                {
                    activeKeyList.Remove(nowKey.KeyCode);
                }

                KeyHandler(nowKey.KeyCode, false);
            };

            MouseMove += (s, e) =>
            {
                if (!isMousePushed) return;
                Rectangle item = (Rectangle)e.Source;
                keyMap key = keyList.Find(a => a.Key == item);
                
                if (nowKey.KeyCode != key.KeyCode)
                {
                    if (activeKeyList.Any(x => x == nowKey.KeyCode))
                    {
                        activeKeyList.Remove(nowKey.KeyCode);
                    }
                    KeyHandler(nowKey.KeyCode, false);

                    nowKey = key;

                    if (activeKeyList.Any(x => x == nowKey.KeyCode)) return;
                    activeKeyList.Add(nowKey.KeyCode);
                    KeyHandler(nowKey.KeyCode, true);
                }
            };*/

            Closing += (s, e) =>
            {
                Application.Current.Shutdown();
            };
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (activeKeyList.Any(x => x == e.Key)) return;
            activeKeyList.Add(e.Key);
            KeyHandler(e.Key, true);
        }

        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            if (activeKeyList.Any(x => x == e.Key))
            {
                activeKeyList.Remove(e.Key);
            }
            KeyHandler(e.Key, false);
        }

        private void KeyHandler(Key key, bool isNotePush)
        {
            /*switch (key)
            {
                case Key.Left:
                    octave -= 1;
                    break;
                case Key.Right:
                    octave += 1;
                    break;
            }

            if (!keyList.Any(a => a.KeyCode == key)) return;
            keyMap currentKey = keyList.Find(a => a.KeyCode == key);
            MIDIHandler(currentKey.note, isNotePush);

            if (isNotePush)
            {
                LinearGradientBrush whiteBrush = new LinearGradientBrush();
                whiteBrush.StartPoint = new Point(0.5, 0);
                whiteBrush.EndPoint = new Point(0.5, 1);
                whiteBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 1));
                whiteBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 0.95));
                LinearGradientBrush blackBrush = new LinearGradientBrush();
                blackBrush.StartPoint = new Point(0.5, 0);
                blackBrush.EndPoint = new Point(0.5, 1);
                blackBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 1));
                blackBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 0.95));
                currentKey.Key.Fill = currentKey.isSharp ? blackBrush : whiteBrush;
            }
            else
            {
                LinearGradientBrush whiteBrush = new LinearGradientBrush();
                whiteBrush.StartPoint = new Point(0.5, 0);
                whiteBrush.EndPoint = new Point(0.5, 1);
                whiteBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 1));
                whiteBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 0.9));
                LinearGradientBrush blackBrush = new LinearGradientBrush();
                blackBrush.StartPoint = new Point(0.5, 0);
                blackBrush.EndPoint = new Point(0.5, 1);
                blackBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 1));
                blackBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 0.9));
                currentKey.Key.Fill = currentKey.isSharp ? blackBrush : whiteBrush;            
            }*/
        }

        private void MIDIHandler(byte key, bool isNotePush)
        {
            byte[] vals = new byte[4];

            if (isNotePush) {
                vals[0] = 0x90;
                vals[1] = (byte)(key + (octave * 6));
                vals[2] = 0x7F;
                vals[3] = 0x00;
                NativeMethods.midiOutShortMsg(hMidiOut, BitConverter.ToUInt32(vals, 0));

            }
            else
            {
                vals[0] = 0x90;
                vals[1] = (byte)(key + (octave * 6));
                vals[2] = 0x00;
                vals[3] = 0x00;
                NativeMethods.midiOutShortMsg(hMidiOut, BitConverter.ToUInt32(vals, 0));
            }
        }

        private void SettingItem_Click(object sender, RoutedEventArgs e)
        {
            setting.ShowDialog();
        }
    }
}
