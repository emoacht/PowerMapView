using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PowerMapView.Models.Weather
{
	[DataContract]
	public class Site
	{
		internal static IReadOnlyList<Site> Sites
		{
			get { return _sites; }
		}
		private static Site[] _sites;

		#region Data

		[DataMember(Order = 0)]
		public string NameJa { get; set; }

		[DataMember(Order = 1)]
		public string NameEn { get; set; }

		[DataMember(Order = 2)]
		public int Id { get; set; }

		[DataMember(Order = 3)]
		public double Longitude { get; set; }

		[DataMember(Order = 4)]
		public double Latitude { get; set; }

		[DataMember(Order = 5)]
		public string Area { get; set; }

		[DataMember(Order = 6)]
		public string PowerCompanyName { get; set; }

		#endregion

		#region Load/Save

		private const string citiesFileName = "Sites.json";
		private const string citiesFilePath = "ms-appx:///Models/Weather/Sites.json";

		internal static async Task LoadFromApplicationUriAsync()
		{
			try
			{
				_sites = await FileService.LoadFromApplicationUriAsync<Site[]>(citiesFilePath);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to load power companies file.\r\n{0}", ex);
			}
		}

		internal static async Task CreateSaveAsync()
		{
			var source = new List<Site>
			{
				new Site
				{
					NameJa="東京",
					NameEn="Tokyo",
					Id = 1850147,
					Longitude = 0,
					Latitude = 0,
					Area = "関東・甲信",
					PowerCompanyName="東京電力",
				},
			};

			await FileService.SaveToLocalAsync(citiesFileName, source);
		}

		#endregion
	}
}