using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PianoKeyboard.Extensions
{
    public class WinMMManager
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
        public static extern uint midiOutOpen(out IntPtr lphMidiOut, int uDeviceID, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);
        [DllImport("winmm.dll")]
        public static extern uint midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);
        [DllImport("winmm.dll")]
        public static extern uint midiOutClose(IntPtr hMidiOut);

        public struct MidiInCaps
        {
            public ushort wMid;
            public ushort wPid;
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint dwSupport;
        }

        [DllImport("winmm.dll")]
        public static extern uint midiInGetNumDevs();
        [DllImport("winmm.dll")]
        public static extern uint midiInGetDevCaps(uint uDevID, out MidiInCaps pmic, int cbmic);
        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInOpen(out IntPtr lphMidiIn, int uDeviceId, MidiInProcDelegate dwCallback, IntPtr dwInstance, MidiOpenFlags dwFlags);
        public delegate void MidiInProcDelegate(IntPtr hMidiIn, MidiInMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);
        [DllImport("winmm.dll")]
        public static extern uint midiInStart(IntPtr hMidiIn);
        [DllImport("winmm.dll")]
        public static extern MMRESULT midiInClose(IntPtr hMidiIn);
        [DllImport("winmm.dll")]
        public static extern uint midiInStop(IntPtr hMidiIn);

        public enum MMRESULT : uint
        {
            MMSYSERR_BASE = 0,
            MMSYSERR_NOERROR = MMSYSERR_BASE + 0
        }

        public enum MidiOpenFlags : uint
        {
            CALLBACK_FUNCTION = 0x30000,
            CALLBACK_TYPEMASK = 0x70000,
            CALLBACK_NULL = 0x00000
        }

        public enum MidiInMessage : uint
        {
            MIM_OPEN = 0x3C1,
            MIM_CLOSE = 0x3C2,
            MIM_NOTE = 0x3C3
        }

        public enum MidiInNoteEvent : int
        {
            MINE_ON = 0x9,
            MINE_OFF = 0x8
        }

        public struct output
        {
            public int index { get; set; }
            public string szPname { get; set; }
        }

        public struct input
        {
            public int index { get; set; }
            public string szPname { get; set; }
        }

        public struct channelColorMap
        {
            public byte ch;
            public Color color;
        }

        public IntPtr hMidiOut;
        public IntPtr hMidiIn;

        private List<output> midiOutputList = new List<output>();
        private int defaultOutputIndex = -1;
        private List<input> midiInputList = new List<input>();
        private int defaultInputIndex = -1;

        private List<channelColorMap> channelColorList = new List<channelColorMap>{
            new channelColorMap{ ch = 0x00, color = Color.FromArgb(0xFF, 0x71, 0x0E, 0xF0) },
            new channelColorMap{ ch = 0x01, color = Color.FromArgb(0xFF, 0x03, 0x94, 0xCD) },
            new channelColorMap{ ch = 0x02, color = Color.FromArgb(0xFF, 0x2C, 0xD0, 0x8D) },
            new channelColorMap{ ch = 0x03, color = Color.FromArgb(0xFF, 0x9B, 0xEA, 0x15) },
            new channelColorMap{ ch = 0x04, color = Color.FromArgb(0xFF, 0x13, 0x4B, 0xE3) },
            new channelColorMap{ ch = 0x05, color = Color.FromArgb(0xFF, 0x93, 0xC4, 0xFE) },
            new channelColorMap{ ch = 0x06, color = Color.FromArgb(0xFF, 0x96, 0x77, 0x4A) },
            new channelColorMap{ ch = 0x07, color = Color.FromArgb(0xFF, 0x76, 0x3A, 0xC4) },
            new channelColorMap{ ch = 0x08, color = Color.FromArgb(0xFF, 0x89, 0x95, 0x99) },
            new channelColorMap{ ch = 0x09, color = Color.FromArgb(0xFF, 0x01, 0x72, 0xEA) },
            new channelColorMap{ ch = 0x0A, color = Color.FromArgb(0xFF, 0x28, 0xC8, 0xC6) },
            new channelColorMap{ ch = 0x0B, color = Color.FromArgb(0xFF, 0xA4, 0x5E, 0x87) },
            new channelColorMap{ ch = 0x0C, color = Color.FromArgb(0xFF, 0xC2, 0x4D, 0x22) },
            new channelColorMap{ ch = 0x0D, color = Color.FromArgb(0xFF, 0x90, 0x76, 0xA3) },
            new channelColorMap{ ch = 0x0E, color = Color.FromArgb(0xFF, 0xB2, 0x98, 0xFD) },
            new channelColorMap{ ch = 0x0F, color = Color.FromArgb(0xFF, 0x3E, 0xB2, 0xA3) }
        };

        public bool isOutputActive = false;
        public bool isInputActive = false;

        private MidiInProcDelegate midiInProc;
        private MainWindow window;

        public WinMMManager(MainWindow _window, bool useDefaultOut = true, bool useDefaultIn = true)
        {
            midiInProc = new MidiInProcDelegate(MIDIInEvent);
            window = _window;

            uint midiOutNumDevs = midiOutGetNumDevs();
            for (uint i = 0; i < midiOutNumDevs; i++)
            {
                MidiOutCaps midiOutCaps = new MidiOutCaps();
                midiOutGetDevCaps(i, out midiOutCaps, Marshal.SizeOf(typeof(MidiOutCaps)));

                midiOutputList.Add(new output { index = Convert.ToInt32(i), szPname = midiOutCaps.szPname });
            }

            uint midiInNumDevs = midiInGetNumDevs();
            for (uint i = 0; i < midiInNumDevs; i++)
            {
                MidiInCaps midiInCaps = new MidiInCaps();
                midiInGetDevCaps(i, out midiInCaps, Marshal.SizeOf(typeof(MidiInCaps)));
                
                midiInputList.Add(new input { index = Convert.ToInt32(i), szPname = midiInCaps.szPname });
            }

            if (useDefaultOut)
            {
                midiOutOpen(out hMidiOut, defaultOutputIndex, IntPtr.Zero, IntPtr.Zero, uint.MinValue);
                midiOutShortMsg(hMidiOut, 0x0000C0);
                isOutputActive = true;
            }

            if (useDefaultIn)
            {
                midiInOpen(out hMidiIn, defaultInputIndex, midiInProc, IntPtr.Zero, MidiOpenFlags.CALLBACK_FUNCTION);
                midiInStart(hMidiIn);
                isInputActive = true;
            }
        }

        public void ChangeOutput(int outputIndex)
        {
            isOutputActive = false;
            if (!Equals(hMidiOut, IntPtr.Zero))
            {
                midiOutClose(hMidiOut);
            }

            midiOutOpen(out hMidiOut, outputIndex, IntPtr.Zero, IntPtr.Zero, uint.MinValue);
            midiOutShortMsg(hMidiOut, 0x0000C0);
            isOutputActive = true;
        }

        public void ChangeInput(int inputIndex)
        {
            isInputActive = false;
            if (!Equals(hMidiIn, IntPtr.Zero))
            {
                midiInStop(hMidiIn);
                midiInClose(hMidiIn);
            }

            midiInOpen(out hMidiIn, inputIndex, midiInProc, IntPtr.Zero, MidiOpenFlags.CALLBACK_FUNCTION);
            midiInStart(hMidiIn);
            isInputActive = true;
        }

        public List<output> GetOutputList()
        {
            return midiOutputList;
        }

        public List<input> GetInputList()
        {
            return midiInputList;
        }

        public void NoteOn(byte key, byte vel = 0x7F)
        {
            if (!isOutputActive || !isOutputActive) return;
            if (hMidiOut == IntPtr.Zero) return;
            byte[] vals = new byte[4];
            vals[0] = 0x90;
            vals[1] = key;
            vals[2] = vel;
            vals[3] = 0x00;
            midiOutShortMsg(hMidiOut, BitConverter.ToUInt32(vals, 0));
        }

        public void NoteOff(byte key)
        {
            if (!isOutputActive || !isOutputActive) return;
            if (hMidiOut == IntPtr.Zero) return;
            byte[] vals = new byte[4];
            vals[0] = 0x80;
            vals[1] = key;
            vals[2] = 0x00;
            vals[3] = 0x00;
            midiOutShortMsg(hMidiOut, BitConverter.ToUInt32(vals, 0));
        }

        public void MIDIInEvent(IntPtr lphMidiIn, MidiInMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            if (hMidiOut == null) return;
            if (wMsg == MidiInMessage.MIM_NOTE)
            {
                byte[] p1 = BitConverter.GetBytes(dwParam1.ToInt32());
                byte cmd = Convert.ToByte(p1[0] >> 4);
                byte ch = Convert.ToByte(p1[0] & 0xf);
                NoteConverter.NoteMap note = window.noteConverter.GetNote(p1[1]);
                byte vel = p1[2];
                KeyboardRender.ColorMap color = new KeyboardRender.ColorMap(channelColorList[ch].color);

                switch ((MidiInNoteEvent)cmd) {
                    case MidiInNoteEvent.MINE_ON:
                        NoteOn(note.note, vel);
                        window.Dispatcher.Invoke(new Action(() => {
                            window.keyboardRender.ChangeKeyColor(note, true, color);
                        }));
                        break;
                    case MidiInNoteEvent.MINE_OFF:
                        NoteOff(note.note);
                        window.Dispatcher.Invoke(new Action(() => {
                            window.keyboardRender.ChangeKeyColor(note, false);
                        }));
                        break;
                }
            }
        }
    }
}
