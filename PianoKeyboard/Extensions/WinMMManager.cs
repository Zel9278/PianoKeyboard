using System;
using System.Collections.Generic;
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
        public static extern uint midiOutEvents(uint uDevID, out MidiOutCaps pmic, int cbmic);

        [DllImport("winmm.dll")]
        public static extern uint midiOutOpen(out IntPtr lphMidiOut, int uDeviceID, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);

        [DllImport("winmm.dll")]
        public static extern uint midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

        [DllImport("winmm.dll")]
        public static extern uint midiOutClose(IntPtr hMidiOut);

        [DllImport("winmm.dll")]
        public static extern uint midiOutReset(IntPtr hMidiOut);

        public struct output
        {
            public int index { get; set; }
            public string szPname { get; set; }
        }

        public IntPtr hMidiOut;

        private List<output> midiOutputList = new List<output>();
        private int defaultOutputIndex = -1;

        public WinMMManager(bool useDefault = true)
        {
            uint midiOutNumDevs = midiOutGetNumDevs();

            for (uint i = 0; i < midiOutNumDevs; i++)
            {
                MidiOutCaps midiOutCaps = new MidiOutCaps();
                midiOutGetDevCaps(i, out midiOutCaps, Marshal.SizeOf(typeof(MidiOutCaps)));

                midiOutputList.Add(new output { index = Convert.ToInt32(i), szPname = midiOutCaps.szPname });
            }

            if (useDefault)
            {
                midiOutOpen(out hMidiOut, defaultOutputIndex, IntPtr.Zero, IntPtr.Zero, uint.MinValue);
                midiOutShortMsg(hMidiOut, 0x0000C0);
            }
        }

        public void ChangeOutput(int outputIndex)
        {
            midiOutClose(hMidiOut);
            midiOutOpen(out hMidiOut, outputIndex, IntPtr.Zero, IntPtr.Zero, uint.MinValue);
            midiOutShortMsg(hMidiOut, 0x0000C0);
        }

        public List<output> GetOutputList()
        {
            return midiOutputList;
        }

        public void NoteOn(byte key)
        {
            if (hMidiOut == null) return;
            byte[] vals = new byte[4];
            vals[0] = 0x90;
            vals[1] = key;
            vals[2] = 0x7F;
            vals[3] = 0x00;
            midiOutShortMsg(hMidiOut, BitConverter.ToUInt32(vals, 0));
        }

        public void NoteOff(byte key)
        {
            if (hMidiOut == null) return;
            byte[] vals = new byte[4];
            vals[0] = 0x90;
            vals[1] = key;
            vals[2] = 0x00;
            vals[3] = 0x00;
            midiOutShortMsg(hMidiOut, BitConverter.ToUInt32(vals, 0));
        }
    }
}
