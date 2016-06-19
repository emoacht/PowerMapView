using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PowerMapView.Models.Power;

namespace PowerMapView.ViewModels
{
	public class PowerCompanyViewModel : ViewModelBase
	{
		private readonly PowerCompany _companyLocation;
		private readonly PowerMonitor.PowerCompany _companyData;

		public PowerCompanyViewModel(int index)
		{
			if ((index < 0)
				|| (PowerCompany.Companies.Count <= index)
				|| (PowerMonitor.PowerCompany.Companies.Count <= index))
				throw new ArgumentOutOfRangeException(nameof(index));

			_companyLocation = PowerCompany.Companies[index];
			_companyData = PowerMonitor.PowerCompany.Companies[index];

			if (!string.Equals(_companyLocation.Name, _companyData.Name, StringComparison.Ordinal))
				throw new InvalidOperationException();
		}

		#region Data

		/// <summary>
		/// Power company name
		/// </summary>
		public string Name => _companyLocation.Name;

		/// <summary>
		/// Longitude of power company's head office
		/// </summary>
		public double Longitude => _companyLocation.Longitude;

		/// <summary>
		/// Latitude of power company's head office
		/// </summary>
		public double Latitude => _companyLocation.Latitude;

		/// <summary>
		/// Supply capacity during peak hours of current date
		/// </summary>
		public double PeakSupply
		{
			get { return _peakSupply; }
			private set { SetProperty(ref _peakSupply, value); }
		}
		private double _peakSupply;

		/// <summary>
		/// The latest usage amount
		/// </summary>
		public double UsageAmount
		{
			get { return _usageAmount; }
			private set { SetProperty(ref _usageAmount, value); }
		}
		private double _usageAmount;

		/// <summary>
		/// The latest usage percentage
		/// </summary>
		public double UsagePercentage
		{
			get { return _usagePercentage; }
			private set { SetProperty(ref _usagePercentage, value); }
		}
		private double _usagePercentage;

		/// <summary>
		/// Time when the latest usage amount is recorded
		/// </summary>
		public DateTimeOffset DataTime
		{
			get { return _dataTime; }
			private set { SetProperty(ref _dataTime, value); }
		}
		private DateTimeOffset _dataTime;

		#endregion

		#region Update

		/// <summary>
		/// Last check time
		/// </summary>
		public DateTimeOffset CheckTimeLast => _companyData.CheckTime;

		/// <summary>
		/// Next check time
		/// </summary>
		public DateTimeOffset CheckTimeNext => CheckTimeLast.AddMinutes((double)_companyData.Interval / 2D);

		public async Task UpdateAsync()
		{
			if (!await _companyData.CheckAsync())
				return;

			PeakSupply = _companyData.Data.PeakSupply;
			UsageAmount = _companyData.Data.UsageAmount;
			UsagePercentage = _companyData.Data.UsagePercentage;
			DataTime = _companyData.Data.DataTime;

			Debug.WriteLine($"{Name}: {UsageAmount} ({DataTime:HH:mm}) / {PeakSupply} -> {UsagePercentage:f1}%");
		}

		#endregion
	}
}