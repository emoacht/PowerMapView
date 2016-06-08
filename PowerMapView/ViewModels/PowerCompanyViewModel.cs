using PowerMapView.Models;
using PowerMapView.Models.Power;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using PowerMapView.Helper;

namespace PowerMapView.ViewModels
{
	public class PowerCompanyViewModel : ViewModelBase
	{
		public static List<PowerCompanyViewModel> CompanyCollection
		{
			get { return _companyCollection ?? (_companyCollection = new List<PowerCompanyViewModel>()); }
		}
		private static List<PowerCompanyViewModel> _companyCollection;
		
		private readonly int companyIndex;

		public PowerCompanyViewModel(int index)
		{
			this.companyIndex = index;
		}
		
		#region Data

		/// <summary>
		/// Power company name
		/// </summary>
		public string Name
		{
			get { return PowerCompany.Companies[companyIndex].Name; }
		}

		/// <summary>
		/// Today's peak supply
		/// </summary>
		public double PeakSupply
		{
			get { return _peakSupply; }
			set
			{
				_peakSupply = value;
				RaisePropertyChanged();
			}
		}
		private double _peakSupply;

		/// <summary>
		/// Latest usage amount
		/// </summary>
		public double UsageAmount
		{
			get { return _usageAmount; }
			set
			{
				_usageAmount = value;
				RaisePropertyChanged();

				if (0 < PeakSupply)
					UsagePercentage = value / PeakSupply * 100;
			}
		}
		private double _usageAmount;

		/// <summary>
		/// Latest usage percentage
		/// </summary>
		public double UsagePercentage
		{
			get { return _usagePercentage; }
			private set
			{
				_usagePercentage = value;
				RaisePropertyChanged();
			}
		}
		private double _usagePercentage;
		
		/// <summary>
		/// Data time of latest usage amount
		/// </summary>
		public DateTimeOffset DataTime
		{
			get { return _dataTime; }
			set
			{
				_dataTime = value;
				RaisePropertyChanged();
			}
		}
		private DateTimeOffset _dataTime;

		#endregion
		
		#region Update

		/// <summary>
		/// Index number of header row for today's peak supply
		/// </summary>
		/// <remarks>Default value means not checked yet or does not exist.</remarks>
		private int PeakSupplyHeaderIndex = -1;

		/// <summary>
		/// Index number of header row of actual usage raws
		/// </summary>
		/// <remarks>Default value means not checked yet or does not exist.</remarks>
		private int ActualUsageHeaderIndex = -1;

		/// <summary>
		/// Starting part of header row of today's peak supply
		/// </summary>
		private const string PeakSupplyHeaderStart = "ピーク時供給力";

		/// <summary>
		/// Starting part of header row of actual usage rows (This must be searched from last)
		/// </summary>
		private const string ActualUsageHeaderStart = "DATE,TIME,当日実績";

		/// <summary>
		/// Last update time (Not UPDATE time in csv file)
		/// </summary>
		public DateTime UpdateTimeLast
		{
			get { return _updateTimeLast; }
			private set
			{
				if (_updateTimeLast == value)
					return;

				_updateTimeLast = value;
				UpdateTimeNext = GetInterval(value);
			}
		}
		private DateTime _updateTimeLast;

		/// <summary>
		/// Next update time
		/// </summary>
		public DateTime UpdateTimeNext { get; private set; }

		private int failureCount;
		private const int failureCountLimit = 3;

		private int nodataCount;
		private const int nodataCountLimit = 20;

		private DateTime GetInterval(DateTime baseTime)
		{
			if ((failureCount < failureCountLimit) && (nodataCount < nodataCountLimit))
				return baseTime.AddMinutes(PowerCompany.Companies[companyIndex].Interval / 2);

			var oneHourLater = baseTime.AddHours(1);
			var tomorrowMidnight = baseTime.Date.AddDays(1);

			return (oneHourLater < tomorrowMidnight) ? oneHourLater : tomorrowMidnight;
		}

		public async Task UpdateAsync()
		{
			var targetUrl = PowerCompany.Companies[companyIndex].Url.Replace("[yyyyMMdd]", DateTimeOffset.Now.ToJst().ToString("yyyyMMdd"));
			var csv = await DataAccess.GetPowerDataAsync(targetUrl);

			UpdateTimeLast = DateTime.Now;

			if (string.IsNullOrEmpty(csv))
			{
				failureCount++;
				return;
			}
			failureCount = 0;

			var records = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
			
			if ((PeakSupplyHeaderIndex < 0) ||
				(ActualUsageHeaderIndex < 0))
			{
				var peakSupplyHeaderIndex = records.FindIndex(x => x.StartsWith(PeakSupplyHeaderStart));
				var actualUsageHeaderIndex = records.FindLastIndex(x => x.StartsWith(ActualUsageHeaderStart)); // Search from last

				if ((peakSupplyHeaderIndex < 0) || (actualUsageHeaderIndex < 0))
					return;

				PeakSupplyHeaderIndex = peakSupplyHeaderIndex;
				ActualUsageHeaderIndex = actualUsageHeaderIndex;
			}

			// Find peak supply.
			var supplyFields = records[PeakSupplyHeaderIndex + 1].Split(',');
			if (supplyFields.Length > 0)
			{
				double supplyBuff;
				if (double.TryParse(supplyFields[0], out supplyBuff))
					PeakSupply = Math.Round(supplyBuff);
			}

			// Find usage amount and data time.
			nodataCount++;
			var currentDate = DateTimeOffset.Now.ToJst().Date;

			foreach (var usageRecord in records.Skip(ActualUsageHeaderIndex + 1))
			{
				if (string.IsNullOrWhiteSpace(usageRecord))
					break;

				var usageFields = usageRecord.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (usageFields.Length < 3)
					break;

				// Find data time.
				DateTimeOffset dataTimeBuff;
				if (DateTimeOffset.TryParse(string.Format("{0} {1}", usageFields[0], usageFields[1]), out dataTimeBuff))
				{
					dataTimeBuff = dataTimeBuff.ToJst();

					// Check if data date is today.
					if (dataTimeBuff.Date != currentDate)
						break;

					DataTime = dataTimeBuff;
				}

				// Find usage amount.
				double usageBuff;
				if (double.TryParse(usageFields[2], out usageBuff))
				{
					if (usageBuff <= 0)
						break;

					nodataCount = 0;
					UsageAmount = usageBuff;
				}
			}

			Debug.WriteLine("{0}: {1}, {2}, {3:f1}", Name, PeakSupply, UsageAmount, UsagePercentage);
		}

		#endregion
	}
}