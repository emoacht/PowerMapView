using PowerMapView.Models;
using PowerMapView.Models.Power;
using PowerMapView.Models.Weather;
using PowerMapView.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace PowerMapView.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
		public ObservableCollection<SiteViewModel> SiteCollection
		{
			get { return _siteCollection ?? (_siteCollection = new ObservableCollection<SiteViewModel>()); }
		}
		private ObservableCollection<SiteViewModel> _siteCollection;

		public MainPageViewModel()
		{
			CheckConnectionAvailable();
			NetworkInformation.NetworkStatusChanged += (sender) => CheckConnectionAvailable();
		}

		internal async Task InitiateStartAsync()
		{
			await InitiateAsync();

			var startTasks = new Task[]
			{
				StartCompanyUpdateAsync(),
				StartSiteUpdateAsync(),				
			};

			await Task.WhenAll(startTasks);
		}

		private async Task InitiateAsync()
		{
			IsMainViewerInitiating = true;

			await PowerCompany.LoadFromApplicationUriAsync();

			int companyIndex = 0;
			foreach (var company in PowerCompany.Companies)
			{
				PowerCompanyViewModel.CompanyCollection.Add(new PowerCompanyViewModel(companyIndex++));
			}

			await Site.LoadFromApplicationUriAsync();

			CheckAreaCoordinates(Site.Sites, MainViewerSize);

			RaisePropertyChanged(() => MainViewerZoomFactor);
			RaisePropertyChanged(() => MainCanvasCenterPosition);

			int siteIndex = 0;
			foreach (var site in Site.Sites)
			{
				SiteCollection.Add(new SiteViewModel(siteIndex++) { Location = ConvertPositionFromReal(site.Longitude, site.Latitude) });
			}

			// Wait for all railway members being loaded. Otherwise, subsequent restoring position 
			// may not work correctly.
			while (SiteCollection.Any(x => !x.IsLoaded))
			{
				await Task.Delay(TimeSpan.FromMilliseconds(50));
			}

			IsMainViewerInitiating = false;
		}

		#region Site

		private readonly DispatcherTimer SiteUpdateTimer = new DispatcherTimer();

		private async Task StartSiteUpdateAsync()
		{
			await SiteUpdateAsync();

			CompanyUpdateTimer.Interval = TimeSpan.FromMinutes(10); // 10 min
			CompanyUpdateTimer.Tick += async (sender, e) =>
			{
				CompanyUpdateTimer.Stop();
				await SiteUpdateAsync();
				CompanyUpdateTimer.Start();
			};
			CompanyUpdateTimer.Start();
		}

		private async Task SiteUpdateAsync()
		{
			if (!IsConnectionAvailable)
				return;

			await UpdateWeatherDataAsync();

			Debug.WriteLine("Updated sites. {0:HH:mm:ss}", DateTime.Now);
		}

		public async Task UpdateWeatherDataAsync()
		{
			var idsByPowerCompany = SiteCollection
				.GroupBy(x => x.PowerCompanyName) // SiteViewModel.PowerCompany will be null in case of Okinawa power company.
				.Select(x => x.Select(y => y.Id).ToArray())
				.ToArray();

			foreach (var ids in idsByPowerCompany)
			{
				var dataByPowerCompany = await DataAccess.GetWeatherDataAsync(ids);
				if (dataByPowerCompany == null)
					continue;

				foreach (var data in dataByPowerCompany)
				{
					var matchingSite = SiteCollection.FirstOrDefault(x => x.Id == data.CityId);
					if (matchingSite == null)
						continue;

					matchingSite.TempKelvin = data.Main.Temp;
					matchingSite.Humidity = data.Main.Humidity;
				}
			}
		}

		#endregion

		#region Company

		private readonly DispatcherTimer CompanyUpdateTimer = new DispatcherTimer();

		private async Task StartCompanyUpdateAsync()
		{
			await CompanyUpdateAsync();

			CompanyUpdateTimer.Interval = TimeSpan.FromSeconds(10); // 10 sec
			CompanyUpdateTimer.Tick += async (sender, e) =>
			{
				CompanyUpdateTimer.Stop();
				await CompanyUpdateAsync();
				CompanyUpdateTimer.Start();
			};
			CompanyUpdateTimer.Start();
		}

		private async Task CompanyUpdateAsync()
		{
			if (!IsConnectionAvailable)
				return;

			var updateTasks = PowerCompanyViewModel.CompanyCollection
				.Where(x => x.UpdateTimeNext <= DateTime.Now)
				.Select(async x => await x.UpdateAsync());

			await Task.WhenAll(updateTasks);

			Debug.WriteLine("Updated companies. {0:HH:mm:ss}", DateTime.Now);
		}

		#endregion

		#region Viewer

		public Size MainViewerSize { get; set; }

		public float MainViewerZoomFactor
		{
			get
			{
				var realZoomFactor = SettingsService.GetValue<double>("RealZoomFactor");
				return ConvertZoomFactorFromReal(realZoomFactor);
			}
			set
			{
				var realZoomFactor = ConvertZoomFactorToReal(value);
				SettingsService.SetValue(realZoomFactor, "RealZoomFactor");
				RaisePropertyChanged();
			}
		}

		public Point MainCanvasCenterPosition
		{
			get
			{
				var realCenterPosition = SettingsService.GetValue<Point>("RealCenterPosition");
				return ConvertPositionFromReal(realCenterPosition);
			}
			set
			{
				var realCenterPosition = ConvertPositionToReal(value);
				SettingsService.SetValue(realCenterPosition, "RealCenterPosition");
				RaisePropertyChanged();
			}
		}

		public bool IsMainViewerInitiating
		{
			get { return _isMainViewerInitiating; }
			set
			{
				_isMainViewerInitiating = value;
				RaisePropertyChanged();
			}
		}
		private bool _isMainViewerInitiating;

		public ZoomDirectionMode ZoomDirection // AppBar's elements cannot be binded to Page's properties.
		{
			get { return _zoomDirection; }
			set
			{
				_zoomDirection = value;
				RaisePropertyChanged();
			}
		}
		private ZoomDirectionMode _zoomDirection;

		#endregion

		#region Conversion

		private Rect sitesArea;
		private const double sitesAreaPadding = 0.4; // To add vacant space around real sites area
		private double inCanvasZoomFactor;
		private double inCanvasPaddingLeft;
		private double inCanvasPaddingTop;

		internal void CheckAreaCoordinates(IEnumerable<Site> sites, Size viewerSize)
		{
			if (viewerSize == default(Size))
				return;

			double left = 180D; // Starting value is the greatest in longitude (East longitude, International Date Line).
			double right = 0D;
			double top = 0D;
			double bottom = 90D; // Starting value is the greatest in latitude (North latitude, North pole).

			foreach (var site in sites)
			{
				left = Math.Min(left, site.Longitude);
				right = Math.Max(right, site.Longitude);
				top = Math.Max(top, site.Latitude);
				bottom = Math.Min(bottom, site.Latitude);
			}

			sitesArea = new Rect(
				left - sitesAreaPadding,
				top + sitesAreaPadding,
				right - left + sitesAreaPadding * 2,
				top - bottom + sitesAreaPadding * 2); // Height is latitude and so top is greater than bottom.

			inCanvasZoomFactor = Math.Min(viewerSize.Width / sitesArea.Width, viewerSize.Height / sitesArea.Height);
			inCanvasPaddingLeft = (viewerSize.Width - sitesArea.Width * inCanvasZoomFactor) / 2;
			inCanvasPaddingTop = (viewerSize.Height - sitesArea.Height * inCanvasZoomFactor) / 2;
		}

		internal Point ConvertPositionFromReal(Point realPosition)
		{
			// If argument is invalid or if not checked yet, return invalid value.
			if ((realPosition == default(Point)) || (inCanvasZoomFactor == 0D))
				return default(Point);

			return ConvertPositionFromReal(realPosition.X, realPosition.Y);
		}

		private Point ConvertPositionFromReal(double x, double y)
		{
			return new Point(
				(x - sitesArea.X) * inCanvasZoomFactor + inCanvasPaddingLeft,
				-(y - sitesArea.Y) * inCanvasZoomFactor + inCanvasPaddingTop);
		}

		internal Point ConvertPositionToReal(Point inCanvasPosition)
		{
			// If argument is invalid or if not checked yet, return invalid value.
			if ((inCanvasPosition == default(Point)) || (inCanvasZoomFactor == 0D))
				return default(Point);

			return ConvertPositionToReal(inCanvasPosition.X, inCanvasPosition.Y);
		}

		private Point ConvertPositionToReal(double x, double y)
		{
			return new Point(
				(x - inCanvasPaddingLeft) / inCanvasZoomFactor + sitesArea.X,
				-(y - inCanvasPaddingTop) / inCanvasZoomFactor + sitesArea.Y);
		}

		internal float ConvertZoomFactorFromReal(double realZoomFactor)
		{
			// If argument is invalid or if not checked yet, return invalid value.
			if ((realZoomFactor == 0D) || (inCanvasZoomFactor == 0D))
				return 0F;

			return (float)realZoomFactor / (float)inCanvasZoomFactor;
		}

		internal double ConvertZoomFactorToReal(float viewerZoomFactor)
		{
			// If argument is invalid or if not checked yet, return invalid value.
			if ((viewerZoomFactor == 0D) || (inCanvasZoomFactor == 0D))
				return 0D;

			return viewerZoomFactor * (float)inCanvasZoomFactor;
		}

		#endregion

		#region Internet Connection

		internal bool IsConnectionAvailable
		{
			get { return _isConnectionAvailable; }
			set
			{
				if (_isConnectionAvailable == value)
					return;

				_isConnectionAvailable = value;
				RaisePropertyChanged();
			}
		}
		private bool _isConnectionAvailable;

		private async void CheckConnectionAvailable()
		{
			var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
			if (dispatcher.HasThreadAccess)
			{
				CheckConnectionAvailableBase();
			}
			else
			{
				await dispatcher.RunAsync(CoreDispatcherPriority.Normal, CheckConnectionAvailableBase);
			}
		}

		private void CheckConnectionAvailableBase()
		{
			var profile = NetworkInformation.GetInternetConnectionProfile();

			IsConnectionAvailable = (profile != null) &&
				(profile.GetNetworkConnectivityLevel() >= NetworkConnectivityLevel.InternetAccess);
		}

		#endregion
	}
}