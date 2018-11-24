using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CountDowner
{
	public class UtcDateTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is DateTime dateTime) || targetType != typeof(string))
				return value;
			if (parameter is string format)
				return dateTime.ToLocalTime().ToString(format, culture);
			else
				return dateTime.ToLocalTime().ToString(culture);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is string s) || targetType != typeof(DateTime))
				return value;
			if (!(parameter is string format ?
				DateTime.TryParseExact(s, format, culture, DateTimeStyles.AssumeLocal, out var result) :
				DateTime.TryParse(s, culture, DateTimeStyles.AssumeLocal, out result)))
				return DependencyProperty.UnsetValue;
			return result.ToUniversalTime();
		}
	}
}
