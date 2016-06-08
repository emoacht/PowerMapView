using PowerMapView.Models.Weather;
using System;

namespace PowerMapView.ViewModels
{
	public class SiteViewModel : CanvasItemViewModel
	{
		private readonly int siteIndex;
		private int companyIndex = -1;

		public SiteViewModel(int index)
		{
			this.siteIndex = index;
		}

		#region Data

		/// <summary>
		/// City identification for OpenWeatherMap
		/// </summary>
		public int Id
		{
			get { return Site.Sites[siteIndex].Id; }
		}

		/// <summary>
		/// City name
		/// </summary>
		public string Name
		{
			get { return Site.Sites[siteIndex].NameJa; }
		}

		/// <summary>
		/// Power company name for this city
		/// </summary>
		public string PowerCompanyName
		{
			get { return Site.Sites[siteIndex].PowerCompanyName; }
		}

		public PowerCompanyViewModel PowerCompany
		{
			get
			{
				if (companyIndex < 0)
				{
					companyIndex = PowerCompanyViewModel.CompanyCollection.FindIndex(x => x.Name == PowerCompanyName);

					if (companyIndex < 0)
						return null; // Do not make this happen when binding. Otherwise, binding will not be made.
				}

				return PowerCompanyViewModel.CompanyCollection[companyIndex];
			}
		}

		/// <summary>
		/// Temperature in Kelvin
		/// </summary>
		public double TempKelvin
		{
			get { return _tempKelvin.HasValue ? _tempKelvin.Value : 0; }
			set
			{
				_tempKelvin = value;
				RaisePropertyChanged();

				TempCelsius = Math.Round((value - 273.15) * 10) / 10;
			}
		}
		private double? _tempKelvin;

		/// <summary>
		/// Temperature in Celsius
		/// </summary>
		public double TempCelsius
		{
			get { return _tempCelsius.HasValue ? _tempCelsius.Value : 0; }
			set
			{
				_tempCelsius = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => Description);
			}
		}
		private double? _tempCelsius;

		/// <summary>
		/// Humidity
		/// </summary>
		public double Humidity
		{
			get { return _humidity.HasValue ? _humidity.Value : 0; }
			set
			{
				_humidity = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => Description);
			}
		}
		private double? _humidity;

		public string Description
		{
			get
			{
				if (!_tempCelsius.HasValue || !_humidity.HasValue)
					return null;

				var description = String.Format("気温 {0}C", TempCelsius) + Environment.NewLine +
					String.Format("湿度 {0}%", Humidity) + Environment.NewLine +
					String.Format("{0}管内", PowerCompanyName);

				if ((PowerCompany != null) && (0 < PowerCompany.UsageAmount))
				{
					description += Environment.NewLine +
						String.Format("ピーク供給力 {0}万kW", PowerCompany.PeakSupply) + Environment.NewLine +
						String.Format("最新の使用量 {0}万kW", PowerCompany.UsageAmount) + Environment.NewLine +
						String.Format("最新の使用率 {0:f1}%", PowerCompany.UsagePercentage);
				}

				return description;
			}
		}

		#endregion
	}
}