using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace PowerMapView.Models
{
	public class FileService
	{
		public static async Task<T> LoadFromApplicationUriAsync<T>(String filePath)
		{
			Uri fileUri;
			if (!Uri.TryCreate(filePath, UriKind.Absolute, out fileUri))
				return default(T);

			return await LoadFromApplicationUriAsync<T>(fileUri);
		}

		public static async Task<T> LoadFromApplicationUriAsync<T>(Uri fileUri)
		{
			var file = await StorageFile.GetFileFromApplicationUriAsync(fileUri);

			try
			{
				using (var stream = await file.OpenStreamForReadAsync())
				{
					var serializer = new DataContractJsonSerializer(typeof(T));
					return (T)serializer.ReadObject(stream);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to parse json.\r\n{0}", ex);
				throw;
			}
		}

		public static async Task<T> LoadFromLocalAsync<T>(string fileName)
		{
			var localFolder = ApplicationData.Current.LocalFolder;

			var file = await localFolder.TryGetItemAsync(fileName) as StorageFile;
			if (file == null)
				return default(T);

			using (var stream = await file.OpenStreamForReadAsync())
			{
				var serializer = new DataContractJsonSerializer(typeof(T));
				return (T)serializer.ReadObject(stream);
			}
		}

		public static async Task SaveToLocalAsync<T>(string fileName, T source)
		{
			var localFolder = ApplicationData.Current.LocalFolder;

			var file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
			using (var stream = await file.OpenStreamForWriteAsync())
			{
				var serializer = new DataContractJsonSerializer(typeof(T));
				serializer.WriteObject(stream, source);

				await stream.FlushAsync();
			}
		}
	}
}