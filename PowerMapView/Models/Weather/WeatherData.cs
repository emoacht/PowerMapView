using System.Runtime.Serialization;

namespace PowerMapView.Models.Weather
{
	[DataContract]
	public class WeatherDataGroup
	{
		[DataMember(Name = "cnt")]
		public int Count { get; set; }

		[DataMember(Name = "list")]
		public WeatherData[] Data { get; set; }
	}

	[DataContract]
	public class WeatherData
	{
		/// <summary>
		/// City geo location
		/// </summary>
		[DataMember(Name = "coord")]
		public Coordinates Location { get; set; }

		/// <summary>
		/// Sys information
		/// </summary>
		[DataMember(Name = "sys")]
		public SysInfo Sys { get; set; }

		/// <summary>
		/// Weather information
		/// </summary>
		[DataMember(Name = "weather")]
		public WeatherInfo[] Weather { get; set; }

		/// <summary>
		/// Main information
		/// </summary>
		[DataMember(Name = "main")]
		public MainInfo Main { get; set; }

		/// <summary>
		/// Wind information
		/// </summary>
		[DataMember(Name = "wind")]
		public WindInfo Wind { get; set; }

		/// <summary>
		/// Clouds information
		/// </summary>
		[DataMember(Name = "clouds")]
		public CloudsInfo Clouds { get; set; }

		/// <summary>
		/// Rain information
		/// </summary>
		[DataMember(Name = "rain")]
		public RainInfo Rain { get; set; }

		/// <summary>
		/// Snow information
		/// </summary>
		[DataMember(Name = "snow")]
		public SnowInfo Snow { get; set; }

		/// <summary>
		/// Data receiving time, unix time, GMT
		/// </summary>
		[DataMember(Name = "dt")]
		public int DataTime { get; set; }

		/// <summary>
		/// City identification
		/// </summary>
		[DataMember(Name = "id")]
		public int CityId { get; set; }

		/// <summary>
		/// City name
		/// </summary>
		[DataMember(Name = "name")]
		public string CityName { get; set; }
	}

	[DataContract]
	public class Coordinates
	{
		/// <summary>
		/// City geo location, lon
		/// </summary>
		[DataMember(Name = "lon")]
		public double Longitude { get; set; }

		/// <summary>
		/// City geo location, lat
		/// </summary>
		[DataMember(Name = "lat")]
		public double Latitude { get; set; }
	}

	[DataContract]
	public class SysInfo
	{
		//public int type { get; set; }
		//public int id { get; set; }
		//public float message { get; set; }

		/// <summary>
		/// Country (GB, JP etc.)
		/// </summary>
		[DataMember(Name = "country")]
		public string Country { get; set; }

		/// <summary>
		/// Sunrise time, unix, UTC
		/// </summary>
		[DataMember(Name = "sunrise")]
		public int SunriseTime { get; set; }

		/// <summary>
		/// Sunset time, unix, UTC
		/// </summary>
		[DataMember(Name = "sunset")]
		public int SunsetTime { get; set; }
	}

	[DataContract]
	public class WeatherInfo
	{
		/// <summary>
		/// Weather condition id
		/// </summary>
		[DataMember(Name = "id")]
		public int Id { get; set; }

		/// <summary>
		/// Group of weather parameters (Rain, Snow, Extreme etc.)
		/// </summary>
		[DataMember(Name = "main")]
		public string Main { get; set; }

		/// <summary>
		/// Weather condition within the group
		/// </summary>
		[DataMember(Name = "description")]
		public string Description { get; set; }

		/// <summary>
		/// Weather icon id
		/// </summary>
		[DataMember(Name = "icon")]
		public string Icon { get; set; }
	}

	[DataContract]
	public class MainInfo
	{
		/// <summary>
		/// Temperature, Kelvin (subtract 273.15 to convert to Celsius)
		/// </summary>
		[DataMember(Name = "temp")]
		public double Temp { get; set; }

		/// <summary>
		/// Atmospheric pressure (on the sea level, if there is no sea_level or grnd_level data), hPa
		/// </summary>
		[DataMember(Name = "pressure")]
		public double Pressure { get; set; }

		/// <summary>
		/// Humidity, %
		/// </summary>
		[DataMember(Name = "humidity")]
		public int Humidity { get; set; }

		/// <summary>
		/// Minimum temperature at the moment. This is deviation from current temp that is possible for large cities 
		/// and megalopolises geographically expanded (use these parameter optionally)
		/// </summary>
		[DataMember(Name = "temp_min")]
		public double MinTemp { get; set; }

		/// <summary>
		/// Maximum temperature at the moment. This is deviation from current temp that is possible for large cities 
		/// and megalopolises geographically expanded (use these parameter optionally)
		/// </summary>
		[DataMember(Name = "temp_max")]
		public double MaxTemp { get; set; }
	}

	[DataContract]
	public class WindInfo
	{
		/// <summary>
		/// Wind speed, mps
		/// </summary>
		[DataMember(Name = "speed")]
		public double Speed { get; set; }

		/// <summary>
		/// Wind direction, degrees (meteorological)
		/// </summary>
		[DataMember(Name = "deg")]
		public double Deg { get; set; }

		/// <summary>
		/// Wind gust, mps
		/// </summary>
		[DataMember(Name = "gust")]
		public double Gust { get; set; }
	}

	[DataContract]
	public class CloudsInfo
	{
		/// <summary>
		/// Cloudiness, %
		/// </summary>
		[DataMember(Name = "all")]
		public int All { get; set; }
	}

	[DataContract]
	public class RainInfo
	{
		/// <summary>
		/// Precipitation volume for last 3 hours, mm
		/// </summary>
		[DataMember(Name = "3h")]
		public double Volume { get; set; }
	}

	[DataContract]
	public class SnowInfo
	{
		/// <summary>
		/// Snow volume for last 3 hours, mm
		/// </summary>
		[DataMember(Name = "3h")]
		public double Volume { get; set; }
	}
}