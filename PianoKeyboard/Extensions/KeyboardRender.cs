using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PianoKeyboard.Extensions
{
    public class KeyboardRender
    {
        public struct KeyboardMap
        {
            public Rectangle key;
            public NoteConverter.noteMap note;
        }

        private MainWindow window;
        private NoteConverter noteConverter;
        private WinMMManager winMM;

        private double whiteHeight = 100;
        private double whiteWidth = 15;
        private double blackHeight = 50;
        private double blackWidth = 8.75;

        private bool isMouseDown = false;
        private NoteConverter.noteMap nowKey;

        public List<KeyboardMap> keyList = new List<KeyboardMap>();

        public KeyboardRender(MainWindow _window)
        {
            window = _window;
            noteConverter = window.noteConverter;
            winMM = window.winMM;

            List<NoteConverter.noteMap> whiteNoteList = noteConverter.GetNoteList().FindAll(note => note.isSharp == false);
            List<NoteConverter.noteMap> noteList = noteConverter.GetNoteList();

            for (int noteNumber = 0; noteNumber < whiteNoteList.Count; noteNumber++)
            {
                NoteConverter.noteMap note = whiteNoteList[noteNumber];

                LinearGradientBrush whiteBrush = new LinearGradientBrush();
                whiteBrush.StartPoint = new Point(0.5, 0);
                whiteBrush.EndPoint = new Point(0.5, 1);
                whiteBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 1));
                whiteBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 0.9));

                Rectangle key = new Rectangle();
                key.Name = note.noteName;
                key.Height = whiteHeight;
                key.Width = whiteWidth;
                key.Fill = whiteBrush;
                key.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                key.HorizontalAlignment = HorizontalAlignment.Left;

                Thickness margin = new Thickness();
                margin.Left = noteNumber * whiteWidth;
                key.Margin = margin;

                keyList.Add(new KeyboardMap{ key = key, note = note });
                window.Keyboard.Children.Add(key);
            }

            for (int noteNumber = 0; noteNumber < noteList.Count; noteNumber++)
            {
                NoteConverter.noteMap note = noteList[noteNumber];
                if (note.isSharp) {
                    LinearGradientBrush blackBrush = new LinearGradientBrush();
                    blackBrush.StartPoint = new Point(0.5, 0);
                    blackBrush.EndPoint = new Point(0.5, 1);
                    blackBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 1));
                    blackBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 0.85));

                    Rectangle key = new Rectangle();
                    key.Name = note.noteName;
                    key.Height = blackHeight;
                    key.Width = blackWidth;
                    key.Fill = blackBrush;
                    key.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                    key.HorizontalAlignment = HorizontalAlignment.Left;
                    key.VerticalAlignment = VerticalAlignment.Top;

                    Thickness margin = new Thickness();
                    margin.Left = noteNumber * blackWidth;
                    key.Margin = margin;

                    keyList.Add(new KeyboardMap { key = key, note = note });
                    window.Keyboard.Children.Add(key);
                }
            }
        }

        public void ChangeKeyColor(NoteConverter.noteMap note, bool isNoteOn)
        {
            KeyboardMap keyboard = keyList.Find(k => k.note.noteName == note.noteName);
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0.5, 0);
            brush.EndPoint = new Point(0.5, 1);

            double black = isNoteOn ? 0.9 : 0.85;
            double white = isNoteOn ? 0.95 : 0.9;

            if (note.isSharp)
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), 1));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), black));
                keyboard.key.Fill = brush;
            }
            else
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E), 1));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4), white));
                keyboard.key.Fill = brush;
            }
        }

        public void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (!isMouseDown) return;
            isMouseDown = false;

            window.NoteOffHandler(nowKey);
        }

        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (e.Source.ToString() != "System.Windows.Shapes.Rectangle") return;
            isMouseDown = true;

            Rectangle key = (Rectangle)e.Source;
            nowKey = noteConverter.GetNote(key.Name);

            window.NoteOnHandler(nowKey);
        }

        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (e.Source.ToString() != "System.Windows.Shapes.Rectangle") return;
            if (!isMouseDown) return;
            Rectangle key = (Rectangle)e.Source;

            if (nowKey.noteName != key.Name)
            {
                window.NoteOffHandler(nowKey);

                nowKey = noteConverter.GetNote(key.Name);

                window.NoteOnHandler(nowKey);
            }
        }

        public void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (e.Source.ToString() != "System.Windows.Shapes.Rectangle") return;
            isMouseDown = false;

            window.NoteOffHandler(nowKey);
        }
    }
}



/*
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
 */