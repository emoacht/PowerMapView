using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace PowerMapView.Models
{
	public class SettingsService
	{
		#region Method (Public)

		public static T GetValue<T>([CallerMemberName] string propertyName = null)
		{
			return GetValue<T>(DataContainer.Local, propertyName);
		}

		public static bool TryGetValue<T>(out T propertyValue, [CallerMemberName] string propertyName = null)
		{
			return TryGetValue<T>(out propertyValue, DataContainer.Local, propertyName);
		}

		public static void SetValue<T>(T propertyValue, [CallerMemberName] string propertyName = null)
		{
			SetValue<T>(propertyValue, DataContainer.Local, propertyName);
		}

		#endregion

		#region Method (Private)

		private enum DataContainer
		{
			Local = 0,
			Roaming,
		}

		private static T GetValue<T>(DataContainer container = default(DataContainer), [CallerMemberName] string propertyName = null)
		{
			try
			{
				var values = (container == DataContainer.Local)
					? ApplicationData.Current.LocalSettings.Values
					: ApplicationData.Current.RoamingSettings.Values;

				if (values.ContainsKey(propertyName))
				{
					return (T)values[propertyName];
				}
				else
				{
					return default(T);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to get property value.\r\n{0}", ex);
				return default(T);
			}
		}

		private static bool TryGetValue<T>(out T propertyValue, DataContainer container = default(DataContainer), [CallerMemberName] string propertyName = null)
		{
			try
			{
				var values = (container == DataContainer.Local)
					? ApplicationData.Current.LocalSettings.Values
					: ApplicationData.Current.RoamingSettings.Values;

				if (!values.ContainsKey(propertyName))
				{
					propertyValue = default(T);
					return false;
				}
				else
				{
					propertyValue = (T)values[propertyName];
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to try to get property value.\r\n{0}", ex);
				propertyValue = default(T);
				return false;
			}
		}

		private static void SetValue<T>(T propertyValue, DataContainer container = default(DataContainer), [CallerMemberName] string propertyName = null)
		{
			try
			{
				var settings = (container == DataContainer.Local)
					? ApplicationData.Current.LocalSettings
					: ApplicationData.Current.RoamingSettings;

				if (settings.Values.ContainsKey(propertyName))
				{
					settings.Values[propertyName] = propertyValue;
				}
				else
				{
					settings.Values.Add(propertyName, propertyValue);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to set property value.\r\n{0}", ex);
			}
		}

		#endregion
	}
}