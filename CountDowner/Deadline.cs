using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml.Linq;
using Reactive.Bindings;

namespace CountDowner
{
	public class Deadline : INotifyPropertyChanged
	{
		public Deadline(string description, IObservable<DateTime> timer) : this(description, DateTime.UtcNow, timer) { }
		public Deadline(XElement element, IObservable<DateTime> timer) : this((string)element.Element(nameof(Description)), (DateTime)element.Element(nameof(Value)), timer) { }
		Deadline(string description, DateTime value, IObservable<DateTime> timer)
		{
			Description = new ReactiveProperty<string>(description);
			Description.SetValidateNotifyError(x => string.IsNullOrWhiteSpace(x) ? "説明を空白にすることはできません。" : null);
			Value = new ReactivePropertySlim<DateTime>(value);
			Remaining = Value.CombineLatest(timer, (x, y) => x - y).Select(x => x.Ticks >= 0 ? x : (TimeSpan?)null).ToReadOnlyReactivePropertySlim();
		}

		public ReactiveProperty<string> Description { get; }
		public ReactivePropertySlim<DateTime> Value { get; }
		public ReadOnlyReactivePropertySlim<TimeSpan?> Remaining { get; }

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { }
			remove { }
		}

		public XElement Serialize() => new XElement(nameof(Deadline), new XElement(nameof(Description), Description.Value), new XElement(nameof(Value), Value.Value));
	}
}
