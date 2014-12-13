using PowerMapView.Models;
using PowerMapView.Models.Power;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
		public DateTime DataTime
		{
			get { return _dataTime; }
			set
			{
				_dataTime = value;
				RaisePropertyChanged();
			}
		}
		private DateTime _dataTime;

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
		private const string ActualUsageHeaderStart = "DATE,TIME,";

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
			var targetUrl = PowerCompany.Companies[companyIndex].Url.Replace("[yyyyMMdd]", DateTime.Now.ToString("yyyyMMdd"));
			var csv = await DataAccess.GetPowerDataAsync(targetUrl);

			UpdateTimeLast = DateTime.Now;

			if (String.IsNullOrEmpty(csv))
			{
				failureCount++;
				return;
			}
			failureCount = 0;

			var responseRows = csv.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

			if ((PeakSupplyHeaderIndex < 0) ||
				(ActualUsageHeaderIndex < 0))
			{
				var responseRowList = responseRows.Take(responseRows.Length - 1).ToList();

				var peakSupplyHeaderIndex = responseRowList
					.FindIndex(x => x.StartsWith(PeakSupplyHeaderStart));
				var actualUsageHeaderIndex = responseRowList
					.FindLastIndex(x => x.StartsWith(ActualUsageHeaderStart)); // Search from last

				if ((peakSupplyHeaderIndex < 0) || (actualUsageHeaderIndex < 0))
					return;

				PeakSupplyHeaderIndex = peakSupplyHeaderIndex;
				ActualUsageHeaderIndex = actualUsageHeaderIndex;
			}

			// Find peak supply.
			var supplyRowFields = responseRows[PeakSupplyHeaderIndex + 1].Split(',');
			if (supplyRowFields.Any())
			{
				double numBuff;
				if (double.TryParse(supplyRowFields[0], out numBuff))
					PeakSupply = Math.Round(numBuff);
			}

			// Find usage amount and data time.
			nodataCount++;
			var currentDate = DateTime.Now.ToString("yyyy/M/d");
			var rowIndex = ActualUsageHeaderIndex;			

			do
			{
				rowIndex++;

				var usageRow = responseRows[rowIndex];
				if (String.IsNullOrWhiteSpace(usageRow))
					break;

				var usageRowFields = usageRow.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (usageRowFields.Length < 3)
					break;

				// Check if data date is today.
				if (usageRowFields[0] != currentDate)
					break;

				// Find usage amount.
				double numBuff;
				if (double.TryParse(usageRowFields[2], out numBuff))
				{
					if (numBuff <= 0)
						break;

					nodataCount = 0;
					UsageAmount = Math.Round(numBuff);					
				}

				// Find data time.
				DateTime timeBuff;
				if (DateTime.TryParse(String.Format("{0} {1}", usageRowFields[0], usageRowFields[1]), out timeBuff))
				{
					DataTime = timeBuff;
				}
			}
			while (rowIndex <= responseRows.Length - 2);

			Debug.WriteLine("{0}: {1}, {2}, {3:f1}", Name, PeakSupply, UsageAmount, UsagePercentage);
		}

		#endregion
	}
}