using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoKeyboard.Extensions
{
    public class NoteConverter
    {
        public struct noteMap
        {
            public byte note;
            public string noteName;
            public int noteNumber;
        }

        public static List<noteMap> noteList = new List<noteMap>();
        public static string[] baseKeyNameList = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        public static List<string> keyNameList;

        public NoteConverter()
        {
            keyNameList = makeOctaves();

            for (byte noteNum = 0; noteNum < keyNameList.Count; noteNum++)
            {
                noteList.Add(new noteMap { note = Convert.ToByte(noteNum), noteName = keyNameList[noteNum], noteNumber = noteNum });
            }
        }

        private List<string> makeOctaves()
        {
            List<string> res = new List<string>();

            for (int oct = -1; oct < 10; oct++)
            {
                int baseKeyNamesLength = (oct == 9) ? baseKeyNameList.Length - 4 : baseKeyNameList.Length;

                for (int key = 0; key < baseKeyNamesLength; key++)
                {
                    res.Add(baseKeyNameList[key] + oct);
                }
            }

            return res;
        }

        public byte ToByte(string noteName)
        {
            return noteList.Find(a => a.noteName == noteName.ToUpper()).note;
        }

        public byte ToByte(int noteNumber)
        {
            return noteList.Find(a => a.noteNumber == noteNumber).note;
        }

        public string ToString(byte note)
        {
            return noteList.Find(a => a.note == note).noteName;
        }

        public string ToString(int noteNumber)
        {
            return noteList.Find(a => a.noteNumber == noteNumber).noteName;
        }

        public int ToInt(byte note)
        {
            return noteList.Find(a => a.note == note).noteNumber;
        }

        public int ToInt(string noteName)
        {
            return noteList.Find(a => a.noteName == noteName.ToUpper()).noteNumber;
        }

        public noteMap GetNote(byte note)
        {
            return noteList.Find(a => a.note == note);
        }

        public noteMap GetNote(string noteName)
        {
            return noteList.Find(a => a.noteName == noteName.ToUpper());
        }

        public noteMap GetNote(int noteNumber)
        {
            return noteList.Find(a => a.noteNumber == noteNumber);
        }
    }
}