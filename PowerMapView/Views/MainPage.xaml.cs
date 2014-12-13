using PowerMapView.Common;
using PowerMapView.ViewModels;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace PowerMapView.Views
{
	public sealed partial class MainPage : Page
	{
		public NavigationHelper NavigationHelper
		{
			get { return this.navigationHelper; }
		}
		private NavigationHelper navigationHelper;

		private readonly MainPageViewModel mainPageViewModel;

		public MainPage()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += navigationHelper_LoadState;
			this.navigationHelper.SaveState += navigationHelper_SaveState;

			mainPageViewModel = new MainPageViewModel();
			this.DataContext = mainPageViewModel;

			this.Loaded += OnLoaded;
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			await mainPageViewModel.InitiateStartAsync();
		}

		private void PageHeader_Tapped(object sender, TappedRoutedEventArgs e)
		{
			e.Handled = true;

			if (TopAppBar != null)
				TopAppBar.IsOpen = true;
		}

		private void CanvasItem_Tapped(object sender, TappedRoutedEventArgs e)
		{
			try
			{
				e.Handled = true;

				var item = sender as FrameworkElement;
				if (item == null)
					return;

				var flyoutBase = FlyoutBase.GetAttachedFlyout(item);
				if (flyoutBase == null)
					return;

				flyoutBase.ShowAt(item);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to handle canvas item tapped event and show flyout.\r\n{0}", ex);
			}
		}


		#region NavigationHelper

		private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
		}

		private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			navigationHelper.OnNavigatedFrom(e);
		}

		#endregion
	}
}