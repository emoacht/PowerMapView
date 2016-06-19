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
		internal static IReadOnlyList<PowerCompany> Companies => _companies;
		private static PowerCompany[] _companies;

		#region Data

		/// <summary>
		/// Power company name
		/// </summary>
		[DataMember(Order = 0)]
		public string Name { get; set; }

		/// <summary>
		/// Longitude of power company's head office
		/// </summary>
		[DataMember(Order = 1)]
		public double Longitude { get; set; }

		/// <summary>
		/// Latitude of power company's head office
		/// </summary>
		[DataMember(Order = 2)]
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
				Debug.WriteLine($"Failed to load power companies file.\r\n{ex}");
			}
		}

		public static async Task CreateSaveToLocalAsync()
		{
			var source = new PowerCompany[]
			{
				new PowerCompany()
				{
					Name = "北海道電力",
					Longitude = 141.357672,
					Latitude = 43.061531,
				},
				new PowerCompany()
				{
					Name = "東北電力",
					Longitude = 140.878367,
					Latitude = 38.265633,
				},
				new PowerCompany()
				{
					Name = "東京電力",
					Longitude = 139.758420,
					Latitude = 35.670214,
				},
				new PowerCompany()
				{
					Name = "北陸電力",
					Longitude = 137.215333,
					Latitude = 36.702751,
				},
				new PowerCompany()
				{
					Name = "中部電力",
					Longitude = 136.913677,
					Latitude = 35.169967,
				},
				new PowerCompany()
				{
					Name = "関西電力",
					Longitude = 135.492568,
					Latitude = 34.692641,
				},
				new PowerCompany()
				{
					Name = "中国電力",
					Longitude = 132.455922,
					Latitude = 34.387803,
				},
				new PowerCompany()
				{
					Name = "四国電力",
					Longitude = 134.050183,
					Latitude = 34.347726,
				},
				new PowerCompany()
				{
					Name = "九州電力",
					Longitude = 130.404586,
					Latitude = 33.583509,
				},
				new PowerCompany()
				{
					Name = "沖縄電力",
					Longitude = 127.716202,
					Latitude = 26.270959,
				}
			};

			await FileService.SaveToLocalAsync(companiesFileName, source);
		}

		#endregion
	}
}