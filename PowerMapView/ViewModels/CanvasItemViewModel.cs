using Windows.Foundation;

namespace PowerMapView.ViewModels
{
	public class CanvasItemViewModel : ViewModelBase
	{
		public double Left
		{
			get { return _left; }
			set { SetProperty(ref _left, value); }
		}
		private double _left;

		public double Top
		{
			get { return _top; }
			set { SetProperty(ref _top, value); }
		}
		private double _top;

		public int ZIndex
		{
			get { return _zIndex; }
			set { SetProperty(ref _zIndex, value); }
		}
		private int _zIndex = 0;

		public Point Location
		{
			get { return new Point(Left, Top); }
			set
			{
				Left = value.X;
				Top = value.Y;
			}
		}

		public bool IsLoaded { get; set; }
	}
}