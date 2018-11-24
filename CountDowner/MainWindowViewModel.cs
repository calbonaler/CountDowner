using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Xml.Linq;
using Reactive.Bindings;

namespace CountDowner
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		public MainWindowViewModel()
		{
			_timer = new ReactiveTimer(TimeSpan.FromMilliseconds(10));
			var dateTimeSequence = _timer.Select(x => DateTime.UtcNow);
			UpdateFrequency = new ReactiveProperty<double>(_timer.Interval.TotalMilliseconds, ReactivePropertyMode.DistinctUntilChanged);
			UpdateFrequency.SetValidateNotifyError(x => x <= 0 ? "負の値は有効な更新頻度ではありません。" : null);
			UpdateFrequency.Subscribe(x => _timer.Interval = TimeSpan.FromMilliseconds(x));
			Deadlines = new ObservableCollection<Deadline>();
			if (!string.IsNullOrEmpty(Properties.Settings.Default.SerializedDeadlines))
			{
				var deadlines = XElement.Parse(Properties.Settings.Default.SerializedDeadlines);
				foreach (var element in deadlines.Elements())
					Deadlines.Add(new Deadline(element, dateTimeSequence));
			}

			WindowLeft = ReactiveProperty.FromObject(Properties.Settings.Default, x => x.WindowLeft);
			WindowTop = ReactiveProperty.FromObject(Properties.Settings.Default, x => x.WindowTop);
			WindowWidth = ReactiveProperty.FromObject(Properties.Settings.Default, x => x.WindowWidth);
			WindowHeight = ReactiveProperty.FromObject(Properties.Settings.Default, x => x.WindowHeight);
			WindowState = ReactiveProperty.FromObject(Properties.Settings.Default, x => x.WindowState);

			NewDeadlineCommand = new ReactiveCommand();
			NewDeadlineCommand.Subscribe(() => Deadlines.Add(new Deadline("(新規)", dateTimeSequence)));
			RemoveDeadlineCommand = new ReactiveCommand<Deadline>();
			RemoveDeadlineCommand.Select(Deadlines.Remove).Subscribe();
			MoveUpDeadlineCommand = new ReactiveCommand<Deadline>();
			MoveUpDeadlineCommand.Select(Deadlines.IndexOf).Where(x => x > 0).Subscribe(x => Deadlines.Move(x, x - 1));
			MoveDownDeadlineCommand = new ReactiveCommand<Deadline>();
			MoveDownDeadlineCommand.Select(Deadlines.IndexOf).Where(x => x < Deadlines.Count - 1).Subscribe(x => Deadlines.Move(x, x + 1));
			SaveConfigurationCommand = new ReactiveCommand();
			SaveConfigurationCommand.Subscribe(() =>
			{
				Properties.Settings.Default.SerializedDeadlines = new XElement(nameof(Deadlines), Deadlines.Select(x => x.Serialize())).ToString();
				Properties.Settings.Default.Save();
			});

			_timer.Start();
		}

		ReactiveTimer _timer;

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { }
			remove { }
		}

		public ReactiveProperty<double> UpdateFrequency { get; }
		public ReactiveProperty<double> WindowLeft { get; }
		public ReactiveProperty<double> WindowTop { get; }
		public ReactiveProperty<double> WindowWidth { get; }
		public ReactiveProperty<double> WindowHeight { get; }
		public ReactiveProperty<WindowState> WindowState { get; }
		public ObservableCollection<Deadline> Deadlines { get; }

		public ReactiveCommand NewDeadlineCommand { get; }
		public ReactiveCommand<Deadline> RemoveDeadlineCommand { get; }
		public ReactiveCommand<Deadline> MoveUpDeadlineCommand { get; }
		public ReactiveCommand<Deadline> MoveDownDeadlineCommand { get; }
		public ReactiveCommand SaveConfigurationCommand { get; }
	}
}
