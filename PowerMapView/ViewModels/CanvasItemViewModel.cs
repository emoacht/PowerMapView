using Windows.Foundation;

namespace PowerMapView.ViewModels
{
	public class CanvasItemViewModel : ViewModelBase
	{
		public double Left
		{
			get { return _left; }
			set
			{
				_left = value;
				RaisePropertyChanged();
			}
		}
		private double _left;

		public double Top
		{
			get { return _top; }
			set
			{
				_top = value;
				RaisePropertyChanged();
			}
		}
		private double _top;

		public int ZIndex
		{
			get { return _zIndex; }
			set
			{
				_zIndex = value;
				RaisePropertyChanged();
			}
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