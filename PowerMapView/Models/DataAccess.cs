using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

using PowerMapView.Models.Weather;

namespace PowerMapView.Models
{
	public static class DataAccess
	{
		#region Site

		private const string weatherDataEndPoint = @"http://api.openweathermap.org/data/2.5/group?id=";

		internal static async Task<WeatherData[]> GetWeatherDataAsync(int[] ids)
		{
			var requestUrl = weatherDataEndPoint + string.Join(",", ids);

			string json = string.Empty;

			try
			{
				json = await DataAccess.GetStringAsync(requestUrl);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to get json from weather map.\r\n{ex}");
			}

			if (string.IsNullOrEmpty(json))
				return null;

			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms))
			{
				await sw.WriteAsync(json);
				await sw.FlushAsync();
				ms.Seek(0, SeekOrigin.Begin);

				var serializer = new DataContractJsonSerializer(typeof(WeatherDataGroup));
				var group = serializer.ReadObject(ms) as WeatherDataGroup;
				return group?.Data;
			}
		}

		#endregion

		#region Company

		internal static async Task<string> GetPowerDataAsync(string targetUrl)
		{
			try
			{
				return await DataAccess.GetStringAsync(targetUrl, "shift-jis");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to get csv from power company.\r\n{ex}");
				return string.Empty;
			}
		}

		#endregion

		#region Access

		private const int retryCountMax = 3; // Maximum retry count
		private static readonly TimeSpan retryLength = TimeSpan.FromSeconds(3); // Waiting time length before retry
		private static readonly TimeSpan timeoutLength = TimeSpan.FromSeconds(20); // Timeout length

		private static async Task<string> GetStringAsync(string targetUrl, string encodingName = "utf-8")
		{
			if (string.IsNullOrWhiteSpace(targetUrl))
				throw new ArgumentNullException(nameof(targetUrl));

			Uri targetUri;
			if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out targetUri))
				throw new ArgumentException("Url seems invalid.");

			return await GetStringAsync(targetUri, encodingName);
		}

		private static async Task<string> GetStringAsync(Uri targetUri, string encordingName = "utf-8")
		{
			if (targetUri == null)
				throw new ArgumentNullException(nameof(targetUri));
			if (string.IsNullOrWhiteSpace(encordingName))
				throw new ArgumentNullException(nameof(encordingName));

			int retryCount = 0;

			using (var cts = new CancellationTokenSource(timeoutLength))
			using (var filter = new HttpBaseProtocolFilter())
			{
				filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;

				using (var client = new HttpClient(filter))
				{
					while (true)
					{
						retryCount++;

						try
						{
							try
							{
								var response = await client.GetAsync(targetUri).AsTask(cts.Token).ConfigureAwait(false);

								response.EnsureSuccessStatusCode();

								var buff = await response.Content.ReadAsBufferAsync();

								using (var stream = buff.AsStream())
								using (var reader = new StreamReader(stream, Encoding.GetEncoding(encordingName)))
								{
									return await reader.ReadToEndAsync();
								}
							}
							catch (OperationCanceledException)
							{
								throw;
							}
							catch
							{
								if (retryCount >= retryCountMax)
									throw;
							}

							await Task.Delay(retryLength, cts.Token);
						}
						catch (Exception ex)
						{
							if ((ex.GetType() == typeof(OperationCanceledException)) ||
								(ex.GetType() == typeof(TaskCanceledException)))
								throw new TimeoutException("Timed out.");

							throw;
						}
					}
				}
			}
		}

		#endregion
	}
}