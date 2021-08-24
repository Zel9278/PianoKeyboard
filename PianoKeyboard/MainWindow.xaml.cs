using System;
using System.Collections.Generic;
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
        public struct keyMap
        {
            public Key KeyCode;
            public byte note;
            public Rectangle Key;
            public bool isSharp;
        }

        private IntPtr hMidiOut;
        private const int MIDI_MAPPER = 1;
        private List<Key> activeKeyList = new List<Key>();
        private int octave = 0;
        private bool isPushed = false;
        private bool isMousePushed = false;
        private keyMap nowKey;

        private List<keyMap> keyList = new List<keyMap>();
        
        public MainWindow()
        {
            InitializeComponent();

            NativeMethods.midiOutOpen(out hMidiOut, MIDI_MAPPER, IntPtr.Zero, IntPtr.Zero, uint.MinValue);
            NativeMethods.midiOutShortMsg(hMidiOut, 0x0000C0);

            keyList.Add(new keyMap { KeyCode = Key.Z, note = 0x3C, Key = C, isSharp = false });
            keyList.Add(new keyMap { KeyCode = Key.X, note = 0x3E, Key = D, isSharp = false });
            keyList.Add(new keyMap { KeyCode = Key.C, note = 0x40, Key = E, isSharp = false });
            keyList.Add(new keyMap { KeyCode = Key.V, note = 0x41, Key = F, isSharp = false });
            keyList.Add(new keyMap { KeyCode = Key.B, note = 0x43, Key = G, isSharp = false });
            keyList.Add(new keyMap { KeyCode = Key.N, note = 0x45, Key = A, isSharp = false });
            keyList.Add(new keyMap { KeyCode = Key.M, note = 0x47, Key = B, isSharp = false });
            keyList.Add(new keyMap { KeyCode = Key.OemComma, note = 0x48, Key = C2, isSharp = false });

            keyList.Add(new keyMap { KeyCode = Key.S, note = 0x3D, Key = CS, isSharp = true });
            keyList.Add(new keyMap { KeyCode = Key.D, note = 0x3F, Key = DS, isSharp = true });
            keyList.Add(new keyMap { KeyCode = Key.G, note = 0x42, Key = FS, isSharp = true });
            keyList.Add(new keyMap { KeyCode = Key.H, note = 0x44, Key = GS, isSharp = true });
            keyList.Add(new keyMap { KeyCode = Key.J, note = 0x46, Key = AS, isSharp = true });

            MouseEnter += (s, e) =>
            {
            };

            MouseLeave += (s, e) =>
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
            switch (key)
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
            }
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
    }
}
