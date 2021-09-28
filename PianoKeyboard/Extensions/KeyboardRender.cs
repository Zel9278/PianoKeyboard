using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PianoKeyboard.Extensions
{
    public class KeyboardRender
    {
        public struct KeyboardMap
        {
            public Rectangle key;
            public NoteConverter.NoteMap note;
        }

        public struct ColorMap
        {
            public Color black;
            public Color white;
            public ColorMap(Color Black, Color White) { black = Black; white = White; }
            public ColorMap(Color color) { black = color; white = color; }
        }

        public struct ActiveKeyMap
        {
            public KeyboardMap key;
            public ColorMap color;
        }

        private MainWindow window;
        private NoteConverter noteConverter;

        private double whiteHeight = 100;
        private double whiteWidth = 15;
        private double blackHeight = 60;
        private double blackWidth = 8.75;

        private bool isMouseDown = false;
        private NoteConverter.NoteMap nowKey;

        public List<KeyboardMap> keyList = new List<KeyboardMap>();
        public List<ActiveKeyMap> activeKeyList = new List<ActiveKeyMap>();

        public KeyboardRender(MainWindow _window)
        {
            window = _window;
            noteConverter = window.noteConverter;

            List<NoteConverter.NoteMap> noteList = noteConverter.GetNoteList();

            int whiteNoteNumber = 0;

            for (int noteNumber = 0; noteNumber < noteList.Count; noteNumber++)
            {
                NoteConverter.NoteMap note = noteList[noteNumber];
                LinearGradientBrush brush = new LinearGradientBrush();
                Rectangle key = new Rectangle();
                Thickness margin = new Thickness();

                brush.StartPoint = new Point(0.5, 0);
                brush.EndPoint = new Point(0.5, 1);

                key.Name = note.noteName;
                key.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                key.HorizontalAlignment = HorizontalAlignment.Left;
                key.VerticalAlignment = VerticalAlignment.Top;

                if (note.isSharp)
                {
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 1));
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 0.85));

                    key.Height = blackHeight;
                    key.Width = blackWidth;
                    margin.Left = noteNumber * blackWidth;

                    window.BlackGrid.Children.Add(key);
                }
                else
                {
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 1));
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 0.9));

                    key.Height = whiteHeight;
                    key.Width = whiteWidth;
                    margin.Left = whiteNoteNumber * whiteWidth;

                    whiteNoteNumber += 1;

                    window.WhiteGrid.Children.Add(key);
                }

                key.Fill = brush;
                key.Margin = margin;
                keyList.Add(new KeyboardMap { key = key, note = note });
            }

            window.KeyboardScroll.ScrollToHorizontalOffset(window.Width / 2);
        }

        public void ChangeKeyColor(NoteConverter.NoteMap note, bool isNoteOn, ColorMap color = new ColorMap())
        {
            Color blackColor = Equals(color.black, default(ColorMap).black) ? Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E) : color.black;
            Color whiteColor = Equals(color.white, default(ColorMap).white) ? Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4) : color.white;

            double blackOffset = isNoteOn ? 0.9 : 0.85;
            double whiteOffset = isNoteOn ? 0.95 : 0.9;

            if(!noteConverter.GetNoteList().Any(n => Equals(n, note))) return;
            KeyboardMap keyboard = keyList.Find(k => Equals(k.note, note));
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0.5, 0);
            brush.EndPoint = new Point(0.5, 1);

            if (note.isSharp)
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 1));
                brush.GradientStops.Add(new GradientStop(blackColor, blackOffset));
            }
            else
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 1));
                brush.GradientStops.Add(new GradientStop(whiteColor, whiteOffset));
            }

            keyboard.key.Fill = brush;
        }

        public void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (!isMouseDown) return;
            isMouseDown = false;

            window.NoteOffHandler(0x80, nowKey);
        }

        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            isMouseDown = true;

            Rectangle key = (Rectangle)e.Source;
            nowKey = noteConverter.GetNote(key.Name);

            window.NoteOnHandler(0x90, nowKey);
        }

        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!isMouseDown) return;
            Rectangle key = (Rectangle)e.Source;

            if (!Equals(nowKey.noteName, key.Name))
            {
                window.NoteOffHandler(0x80, nowKey);

                nowKey = noteConverter.GetNote(key.Name);

                window.NoteOnHandler(0x90, nowKey);
            }
        }

        public void MouseUpHandler(object sender, MouseEventArgs e)
        {
            isMouseDown = false;

            window.NoteOffHandler(0x80, nowKey);
        }
    }
}