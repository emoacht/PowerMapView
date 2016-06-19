using System;
using System.Linq;
using System.Text;

using PowerMapView.Models.Weather;

namespace PowerMapView.ViewModels
{
	public class SiteViewModel : CanvasItemViewModel
	{
		private readonly Site _site;

		public SiteViewModel(int index)
		{
			if ((index < 0) || (Site.Sites.Count <= index))
				throw new ArgumentOutOfRangeException(nameof(index));

			_site = Site.Sites[index];
		}

		#region Data

		/// <summary>
		/// City identification for OpenWeatherMap
		/// </summary>
		public int Id => _site.Id;

		/// <summary>
		/// City name
		/// </summary>
		public string Name => _site.NameJa;

		/// <summary>
		/// Power company name for this city
		/// </summary>
		public string PowerCompanyName => _site.PowerCompanyName;

		public PowerCompanyViewModel PowerCompany
		{
			get
			{
				if (_powerCompany == null)
					_powerCompany = MainPageViewModel.CompanyCollection.First(x => string.Equals(x.Name, _site.PowerCompanyName, StringComparison.Ordinal));

				return _powerCompany;
			}
		}
		private PowerCompanyViewModel _powerCompany;

		/// <summary>
		/// Temperature in Kelvin
		/// </summary>
		public double TempKelvin
		{
			get { return _tempKelvin.GetValueOrDefault(); }
			set
			{
				if (SetProperty(ref _tempKelvin, value))
					TempCelsius = Math.Round((value - 273.15) * 10D) / 10D;
			}
		}
		private double? _tempKelvin;

		/// <summary>
		/// Temperature in Celsius
		/// </summary>
		public double TempCelsius
		{
			get { return _tempCelsius.GetValueOrDefault(); }
			set
			{
				if (SetProperty(ref _tempCelsius, value))
					RaisePropertyChanged(nameof(Description));
			}
		}
		private double? _tempCelsius;

		/// <summary>
		/// Humidity
		/// </summary>
		public double Humidity
		{
			get { return _humidity.GetValueOrDefault(); }
			set
			{
				if (SetProperty(ref _humidity, value))
					RaisePropertyChanged(nameof(Description));
			}
		}
		private double? _humidity;

		public string Description
		{
			get
			{
				if (!_tempCelsius.HasValue || !_humidity.HasValue)
					return null;

				var sb = new StringBuilder();
				sb.AppendLine($"気温 {TempCelsius}C");
				sb.AppendLine($"湿度 {Humidity}%");
				sb.Append($"{PowerCompanyName}管内");

				if ((PowerCompany != null) && (0 < PowerCompany.UsageAmount))
				{
					sb.AppendLine();
					sb.AppendLine($"ピーク供給力 {PowerCompany.PeakSupply}万kW");
					sb.AppendLine($"最新の使用量 {PowerCompany.UsageAmount}万kW");
					sb.Append($"最新の使用率 {PowerCompany.UsagePercentage:f1}%");
				}

				return sb.ToString();
			}
		}

		#endregion
	}
}