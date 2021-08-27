using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;

namespace PianoKeyboard.Extensions
{
    public class KeyLayout
    {
        private MainWindow window;
        private int keyLayoutId;

        private int transpose = 0;

        public struct KeyCode
        {
            public int rawNoteNum;
            public Key jp;
            public Key en_us;
        }

        public List<KeyCode> keyCodes = new List<KeyCode>{
            new KeyCode{ rawNoteNum = 45, jp = Key.Z, en_us = Key.Z },
            new KeyCode{ rawNoteNum = 46, jp = Key.S, en_us = Key.S },
            new KeyCode{ rawNoteNum = 47, jp = Key.X, en_us = Key.X },

            new KeyCode{ rawNoteNum = 48, jp = Key.C, en_us = Key.C },
            new KeyCode{ rawNoteNum = 49, jp = Key.F, en_us = Key.F },
            new KeyCode{ rawNoteNum = 50, jp = Key.V, en_us = Key.V },
            new KeyCode{ rawNoteNum = 51, jp = Key.G, en_us = Key.G },
            new KeyCode{ rawNoteNum = 52, jp = Key.B, en_us = Key.B },

            new KeyCode{ rawNoteNum = 53, jp = Key.N, en_us = Key.N },
            new KeyCode{ rawNoteNum = 54, jp = Key.J, en_us = Key.J },
            new KeyCode{ rawNoteNum = 55, jp = Key.M, en_us = Key.M },
            new KeyCode{ rawNoteNum = 56, jp = Key.K, en_us = Key.K },
            new KeyCode{ rawNoteNum = 57, jp = Key.OemComma, en_us = Key.OemComma },
            new KeyCode{ rawNoteNum = 58, jp = Key.L, en_us = Key.L },
            new KeyCode{ rawNoteNum = 59, jp = Key.OemPeriod, en_us = Key.OemPeriod },

            new KeyCode{ rawNoteNum = 60, jp = Key.OemQuestion, en_us = Key.OemQuestion },
            new KeyCode{ rawNoteNum = 61, jp = Key.Oem1 },
            new KeyCode{ rawNoteNum = 62, jp = Key.OemBackslash },


            new KeyCode{ rawNoteNum = 57, jp = Key.Q, en_us = Key.Q },
            new KeyCode{ rawNoteNum = 58, jp = Key.D2, en_us = Key.D2 },
            new KeyCode{ rawNoteNum = 59, jp = Key.W, en_us = Key.W },

            new KeyCode{ rawNoteNum = 60, jp = Key.E, en_us = Key.E },
            new KeyCode{ rawNoteNum = 61, jp = Key.D4, en_us = Key.D4 },
            new KeyCode{ rawNoteNum = 62, jp = Key.R, en_us = Key.R },
            new KeyCode{ rawNoteNum = 63, jp = Key.D5, en_us = Key.D5 },
            new KeyCode{ rawNoteNum = 64, jp = Key.T, en_us = Key.T },

            new KeyCode{ rawNoteNum = 65, jp = Key.Y, en_us = Key.Y },
            new KeyCode{ rawNoteNum = 66, jp = Key.D7, en_us = Key.D7 },
            new KeyCode{ rawNoteNum = 67, jp = Key.U, en_us = Key.U },
            new KeyCode{ rawNoteNum = 68, jp = Key.D8, en_us = Key.D8 },
            new KeyCode{ rawNoteNum = 69, jp = Key.I, en_us = Key.I },
            new KeyCode{ rawNoteNum = 70, jp = Key.D9, en_us = Key.D9 },
            new KeyCode{ rawNoteNum = 71, jp = Key.O, en_us = Key.O },

            new KeyCode{ rawNoteNum = 72, jp = Key.P, en_us = Key.P },
            new KeyCode{ rawNoteNum = 73, jp = Key.OemMinus, en_us = Key.OemMinus },
            new KeyCode{ rawNoteNum = 74, jp = Key.Oem3, en_us = Key.OemOpenBrackets },
            new KeyCode{ rawNoteNum = 75, jp = Key.OemQuotes, en_us = Key.OemPlus },
            new KeyCode{ rawNoteNum = 76, jp = Key.OemOpenBrackets, en_us = Key.OemCloseBrackets }
        };

        private List<NoteConverter.noteMap> activeKeyList = new List<NoteConverter.noteMap>();

        public KeyLayout(MainWindow _window)
        {
            window = _window;
            keyLayoutId = CultureInfo.CurrentCulture.KeyboardLayoutId;
        }

        public void KeyDownHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if ((transpose + 12) > 60) break;
                    resetKeyList();
                    transpose += 12;
                    window.TransposeItam.Content = $"Transpose: {transpose}";
                    break;

                case Key.Down:
                    if ((transpose - 12) < -48) break;
                    resetKeyList();
                    transpose -= 12;
                    window.TransposeItam.Content = $"Transpose: {transpose}";
                    break;

                case Key.Right:
                    if ((transpose + 1) > 60) break;
                    resetKeyList();
                    transpose += 1;
                    window.TransposeItam.Content = $"Transpose: {transpose}";
                    break;

                case Key.Left:
                    if ((transpose - 1) < -48) break;
                    resetKeyList();
                    transpose -= 1;
                    window.TransposeItam.Content = $"Transpose: {transpose}";
                    break;
            }

            if (!keyCodes.Any(n => ((keyLayoutId == 1041) ? n.jp : n.en_us) == e.Key)) return;
            KeyCode key = keyCodes.Find(n => ((keyLayoutId == 1041) ? n.jp : n.en_us) == e.Key);
            int noteNum = key.rawNoteNum + transpose;
            if (noteNum < 0 || noteNum > 127) return;
            NoteConverter.noteMap note = window.noteConverter.GetNote(noteNum);
            if (activeKeyList.Any(x => x.noteName == note.noteName)) return;
            window.NoteOnHandler(note);
            activeKeyList.Add(note);
        }

        public void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (!keyCodes.Any(n => ((keyLayoutId == 1041) ? n.jp : n.en_us) == e.Key)) return;
            KeyCode key = keyCodes.Find(n => ((keyLayoutId == 1041) ? n.jp : n.en_us) == e.Key);
            int noteNum = key.rawNoteNum + transpose;
            if (noteNum < 0 || noteNum > 127) return;
            NoteConverter.noteMap note = window.noteConverter.GetNote(noteNum);
            if (activeKeyList.Any(x => x.noteName == note.noteName))
            {
                activeKeyList.Remove(note);
            }
            window.NoteOffHandler(note);
        }

        private void resetKeyList()
        {
            for (int key = 0; key < activeKeyList.Count; key++)
            {
                NoteConverter.noteMap activeKey = activeKeyList[key];
                window.NoteOffHandler(activeKey);
            }
        }
    }
}