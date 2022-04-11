using System;
using Sanford.Multimedia.Midi;

namespace PianoKeyboard.Extensions
{
	public class MIDISequencer
	{
		public Sequencer _sequencer;
		public string MidiPath;
		private MainWindow window;

		public MIDISequencer(MainWindow _window)
		{
			window = _window;
		}

		public void SetMIDIFile(string _MidiPath)
		{
			MidiPath = _MidiPath;
		}

		public void Start()
		{
			_sequencer = new Sequencer();
			_sequencer.Sequence = new Sequence(MidiPath);
			_sequencer.ChannelMessagePlayed += OnChannelMessage;
			_sequencer.Stopped += OnStop;
			_sequencer.Start();
		}

		private void OnStop(object sender, StoppedEventArgs e)
		{
		}

		private void OnChannelMessage(object sender, ChannelMessageEventArgs e)
		{
			NoteConverter.NoteMap note = window.noteConverter.GetNote((byte)e.Message.Data1);
			KeyboardRender.ColorMap color = new KeyboardRender.ColorMap(window.winMM.channelColorList[e.Message.MidiChannel].color);
			window.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (e.Message.Data2 == 0 || (e.Message.Command != ChannelCommand.NoteOn))
				{
					window.NoteOffHandler(0x80, note);
					return;
				}
				window.NoteOnHandler(0x90, note, (byte)e.Message.Data2, color);
			}));
		}
	}
}
