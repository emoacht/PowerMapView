using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PowerMapView.Views.Controls
{
	public class CanvasItemGrid : Grid
	{
		public CanvasItemGrid()
		{
			this.Loaded += (sender, e) => IsLoaded = true;
		}

		public bool IsLoaded
		{
			get { return (bool)GetValue(IsLoadedProperty); }
			set { SetValue(IsLoadedProperty, value); }
		}
		public static readonly DependencyProperty IsLoadedProperty =
			DependencyProperty.Register(
				"IsLoaded",
				typeof(bool),
				typeof(CanvasItemGrid),
				new PropertyMetadata(false));
	}
}