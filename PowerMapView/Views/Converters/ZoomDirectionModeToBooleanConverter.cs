using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PowerMapView.Views.Converters
{
	public class ZoomDirectionModeToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			ZoomDirectionMode valueMode;
			if ((value == null) || !Enum.TryParse(value.ToString(), true, out valueMode))
				return DependencyProperty.UnsetValue;

			ZoomDirectionMode parameterMode;
			if ((parameter == null) || !Enum.TryParse(parameter.ToString(), true, out parameterMode))
				return DependencyProperty.UnsetValue;

			return (valueMode == parameterMode);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (!(value is bool))
				return DependencyProperty.UnsetValue;

			ZoomDirectionMode parameterMode;
			if ((parameter == null) || !Enum.TryParse(parameter.ToString(), true, out parameterMode))
				return DependencyProperty.UnsetValue;

			return ((bool)value)
				? parameterMode
				: ZoomDirectionMode.None;
		}
	}
}