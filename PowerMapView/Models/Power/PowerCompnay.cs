using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PowerMapView.Models.Power
{
	[DataContract]
	public class PowerCompany
	{
		static internal IReadOnlyList<PowerCompany> Companies
		{
			get { return _companies; }
		}
		private static PowerCompany[] _companies;


		#region Data

		/// <summary>
		/// Power company name
		/// </summary>
		[DataMember(Order = 0)]
		public string Name { get; set; }

		/// <summary>
		/// Url of Usage situation data
		/// </summary>
		[DataMember(Order = 1)]
		public string Url { get; set; }

		/// <summary>
		/// Update interval of usage situation data (min)
		/// </summary>
		[DataMember(Order = 2)]
		public int Interval { get; set; }

		/// <summary>
		/// Longitude of power company's head office
		/// </summary>
		[DataMember(Order = 3)]
		public double Longitude { get; set; }

		/// <summary>
		/// Latitude of power company's head office
		/// </summary>
		[DataMember(Order = 4)]
		public double Latitude { get; set; }

		#endregion


		#region Load/Save

		private const string companiesFileName = "PowerCompanies.json";
		private const string companiesFilePath = "ms-appx:///Models/Power/PowerCompanies.json";

		public static async Task LoadFromApplicationUriAsync()
		{
			try
			{
				_companies = await FileService.LoadFromApplicationUriAsync<PowerCompany[]>(companiesFilePath);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to load power companies file.\r\n{0}", ex);
			}
		}

		public static async Task CreateSaveToLocalAsync()
		{
			var source = new[]
			{
				new PowerCompany()
				{
					Name = "北海道電力",
					Url = @"http://denkiyoho.hepco.co.jp/data/juyo_hokkaidou.csv",
					Interval = 5,
					Longitude = 141.357672,
					Latitude = 43.061531,
				},
				new PowerCompany()
				{
					Name = "東北電力",
					Url = @"http://setsuden.tohoku-epco.co.jp/common/demand/juyo_02_[yyyyMMdd].csv", // Date part must be replaced with current date.
					Interval = 5,
					Longitude = 140.878367,
					Latitude = 38.265633,
				},
				new PowerCompany()
				{
					Name = "東京電力",
					Url = @"http://www.tepco.co.jp/forecast/html/images/juyo-j.csv",
					Interval = 5,
					Longitude = 139.758420,
					Latitude = 35.670214,
				},			
				new PowerCompany()
				{
					Name = "北陸電力",
					Url = @"http://www.rikuden.co.jp/denki-yoho/csv/juyo_05_[yyyyMMdd].csv", // Date part must be replaced with current date.
					Interval = 5,
					Longitude = 137.215333,
					Latitude = 36.702751,
				},
				new PowerCompany()
				{
					Name = "中部電力",
					Url = @"http://denki-yoho.chuden.jp/denki_yoho_content_data/juyo_cepco003.csv",
					Interval = 5,
					Longitude = 136.913677,
					Latitude = 35.169967,
				},
				new PowerCompany()
				{
					Name = "関西電力",
					Url = @"http://www.kepco.co.jp/yamasou/juyo1_kansai.csv",
					Interval = 5,
					Longitude = 135.492568,
					Latitude = 34.692641,
				},
				new PowerCompany()
				{
					Name = "中国電力",
					Url = @"http://www.energia.co.jp/jukyuu/sys/juyo_07_[yyyyMMdd].csv", // Date part must be replaced with current date.
					Interval = 5,
					Longitude = 132.455922,
					Latitude = 34.387803,
				},
				new PowerCompany()
				{
					Name = "四国電力",
					Url = @"http://www.yonden.co.jp/denkiyoho/juyo_shikoku.csv",
					Interval = 5,
					Longitude = 134.050183,
					Latitude = 34.347726,
				},
				new PowerCompany()
				{
					Name = "九州電力",
					Url = @"http://www.kyuden.co.jp/power_usages/csv/juyo-hourly-[yyyyMMdd].csv", // Date part must be replaced with current date.
					Interval = 5,
					Longitude = 130.404586,
					Latitude = 33.583509,
				},
				new PowerCompany()
				{
					Name = "沖縄電力",
					Url = @"http://www.okiden.co.jp/denki/juyo_10_[yyyyMMdd].csv", // Date part must be replaced with current date.
					Interval = 5,
					Longitude = 127.716202,
					Latitude = 26.270959,
				}
			};

			await FileService.SaveToLocalAsync(companiesFileName, source);
		}

		#endregion
	}
}