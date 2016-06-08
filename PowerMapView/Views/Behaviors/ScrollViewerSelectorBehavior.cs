using Microsoft.Xaml.Interactivity;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using WinRTXamlToolkit.Controls.Extensions;

namespace PowerMapView.Views.Behaviors
{
	public class ScrollViewerSelectorBehavior : DependencyObject, IBehavior
	{
		public DependencyObject AssociatedObject { get; private set; }

		private ScrollViewer AssociatedViewer
		{
			get { return (ScrollViewer)this.AssociatedObject; }
		}

		private Selector AssociatedSelector
		{
			get { return _associatedSelector ?? (_associatedSelector = this.AssociatedObject.GetFirstDescendantOfType<Selector>()); }
		}
		private Selector _associatedSelector;

		private CompositeDisposable disposer;

		public void Attach(DependencyObject associatedObject)
		{
			this.AssociatedObject = associatedObject as ScrollViewer;

			if (this.AssociatedObject == null)
			{
				Debug.WriteLine("Associated object is not ScrollViewer!");
				return;
			}

			AssociatedViewer.Loaded += OnLoaded;

			disposer = new CompositeDisposable();
		}

		public void Detach()
		{
			if (this.AssociatedObject == null)
				return;

			AssociatedViewer.Loaded -= OnLoaded;

			disposer.Dispose();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			ViewerSize = new Size(AssociatedViewer.ActualWidth, AssociatedViewer.ActualHeight);

			// ViewChanged event
			disposer.Add(
				Observable.FromEventPattern<ScrollViewerViewChangedEventArgs>(
					h => AssociatedViewer.ViewChanged += h,
					h => AssociatedViewer.ViewChanged -= h)
				.Throttle(TimeSpan.FromMilliseconds(100)) // 100 msec throttling
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(e_ => SaveInViewerCenterPosition()));

			// SizeChanged event
			disposer.Add(
				Observable.FromEvent<SizeChangedEventHandler, SizeChangedEventArgs>(
					h => (sender_, e_) => h(e_),
					h => AssociatedViewer.SizeChanged += h,
					h => AssociatedViewer.SizeChanged -= h)
				.Subscribe(e_ => RestoreInViewerCenterPosition()));

			// Tapped event
			StartListenTap();

			// Manipulation events (alternative)
			StartListenManipulation();

			//// Pointer events (alternative)
			//StartListenPointer();
		}

		#region Common

		public Size ViewerSize
		{
			get { return (Size)GetValue(ViewerSizeProperty); }
			set { SetValue(ViewerSizeProperty, value); }
		}
		public static readonly DependencyProperty ViewerSizeProperty =
			DependencyProperty.Register(
				"ViewerSize",
				typeof(Size),
				typeof(ScrollViewerSelectorBehavior),
				new PropertyMetadata(default(Size)));

		public float ViewerZoomFactor
		{
			get { return (float)GetValue(ViewerZoomFactorProperty); }
			set { SetValue(ViewerZoomFactorProperty, value); }
		}
		public static readonly DependencyProperty ViewerZoomFactorProperty =
			DependencyProperty.Register(
				"ViewerZoomFactor",
				typeof(float),
				typeof(ScrollViewerSelectorBehavior),
				new PropertyMetadata(0F));

		public Point InSelectorCenterPosition
		{
			get { return (Point)GetValue(InSelectorCenterPositionProperty); }
			set { SetValue(InSelectorCenterPositionProperty, value); }
		}
		public static readonly DependencyProperty InSelectorCenterPositionProperty =
			DependencyProperty.Register(
				"InSelectorCenterPosition",
				typeof(Point),
				typeof(ScrollViewerSelectorBehavior),
				new PropertyMetadata(default(Point)));

		public bool IsViewerInitiating
		{
			get { return (bool)GetValue(IsViewerInitiatingProperty); }
			set { SetValue(IsViewerInitiatingProperty, value); }
		}
		public static readonly DependencyProperty IsViewerInitiatingProperty =
			DependencyProperty.Register(
				"IsViewerInitiating",
				typeof(bool),
				typeof(ScrollViewerSelectorBehavior),
				new PropertyMetadata(
					false, // Default must be false.
					(d, e) =>
					{
						if (!(bool)e.NewValue)
							((ScrollViewerSelectorBehavior)d).RestoreInViewerCenterPosition();
					}));

		private bool isInitial = true;

		private void RestoreInViewerCenterPosition()
		{
			if ((ViewerZoomFactor == 0F) || (InSelectorCenterPosition == default(Point)))
				return;

			var inViewerCenterPosition = new Point(AssociatedViewer.ActualWidth / 2D, AssociatedViewer.ActualHeight / 2D);

			RestoreInViewerPosition(inViewerCenterPosition, InSelectorCenterPosition, ViewerZoomFactor, isInitial);

			isInitial = false;

			Debug.WriteLine("Restored center position and zoom factor. {0}", InSelectorCenterPosition);
		}

		private void RestoreInViewerPosition(Point inViewerPosition, Point inSelectorPosition, float viewerZoomFactor, bool checkesItemInViewport = false)
		{
			var offsetX = inSelectorPosition.X * viewerZoomFactor - inViewerPosition.X;
			var offsetY = inSelectorPosition.Y * viewerZoomFactor - inViewerPosition.Y;

			if (checkesItemInViewport)
			{
				var left = offsetX / viewerZoomFactor;
				var right = left + AssociatedViewer.ActualWidth / viewerZoomFactor;
				var top = offsetY / viewerZoomFactor;
				var bottom = top + AssociatedViewer.ActualHeight / viewerZoomFactor;

				var existsItemInViewport = AssociatedSelector.GetDescendantsOfType<SelectorItem>()
					.Select(item => item.TransformToVisual(AssociatedSelector).TransformPoint(default(Point)))
					.Any(position => (left < position.X) && (position.X < right) && (top < position.Y) && (position.Y < bottom));

				if (!existsItemInViewport)
					return;
			}

			try
			{
				AssociatedViewer.ChangeView(offsetX, offsetY, viewerZoomFactor);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to restore position and zoom factor.\r\n{0}", ex);
			}
		}

		private void SaveInViewerCenterPosition()
		{
			var inViewerCenterPosition = new Point(AssociatedViewer.ActualWidth / 2D, AssociatedViewer.ActualHeight / 2D);

			InSelectorCenterPosition = ConvertToInSelectorPosition(inViewerCenterPosition, AssociatedViewer.ZoomFactor);

			ViewerZoomFactor = AssociatedViewer.ZoomFactor;

			Debug.WriteLine("Saved center position and zoom factor. {0}", InSelectorCenterPosition);
		}

		private Point ConvertToInSelectorPosition(Point inViewerPosition, float viewerZoomFactor)
		{
			var selectorPosition = AssociatedSelector.TransformToVisual(AssociatedViewer).TransformPoint(default(Point));

			var offsetX = (inViewerPosition.X - selectorPosition.X) / viewerZoomFactor;
			var offsetY = (inViewerPosition.Y - selectorPosition.Y) / viewerZoomFactor;

			return new Point(offsetX, offsetY);
		}

		#endregion

		#region Zoom(Tap)

		public ZoomDirectionMode ZoomDirection
		{
			get { return (ZoomDirectionMode)GetValue(ZoomDirectionProperty); }
			set { SetValue(ZoomDirectionProperty, value); }
		}
		public static readonly DependencyProperty ZoomDirectionProperty =
			DependencyProperty.Register(
				"ZoomDirection",
				typeof(ZoomDirectionMode),
				typeof(ScrollViewerSelectorBehavior),
				new PropertyMetadata(default(ZoomDirectionMode)));

		private void StartListenTap()
		{
			disposer.Add(
				Observable.FromEventPattern<TappedEventHandler, TappedRoutedEventArgs>(
					h => h.Invoke,
					h => AssociatedViewer.Tapped += h,
					h => AssociatedViewer.Tapped -= h)
				.Subscribe(x => OnTapped(x.Sender, x.EventArgs)));
		}

		private void OnTapped(object sender, TappedRoutedEventArgs e)
		{
			try
			{
				var inViewerPosition = e.GetPosition((FrameworkElement)sender);
				var inSelectorPosition = ConvertToInSelectorPosition(inViewerPosition, AssociatedViewer.ZoomFactor);

				switch (ZoomDirection)
				{
					case ZoomDirectionMode.ZoomIn:
						e.Handled = true;
						ZoomIn(inViewerPosition, inSelectorPosition);
						break;

					case ZoomDirectionMode.ZoomOut:
						e.Handled = true;
						ZoomOut(inViewerPosition, inSelectorPosition);
						break;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to handle zoom tapped event.\r\n{0}", ex);
			}
		}

		private const double zoomNotchFactor = 0.18F;

		private void ZoomIn(Point inViewerPosition, Point inSelectorPosition)
		{
			var factor = Math.Min(9F, AssociatedViewer.ZoomFactor * (float)(1D + zoomNotchFactor));

			RestoreInViewerPosition(inViewerPosition, inSelectorPosition, factor);

			Debug.WriteLine("Zoomed in: Factor:{0} InViewerPosition: {1} inSelectorPosition: {2}", factor, inViewerPosition, inSelectorPosition);
		}

		private void ZoomOut(Point inViewerPosition, Point inSelectorPosition)
		{
			var factor = Math.Max(1F, AssociatedViewer.ZoomFactor / (float)(1D + zoomNotchFactor));

			RestoreInViewerPosition(inViewerPosition, inSelectorPosition, factor);

			Debug.WriteLine("Zoomed in: Factor:{0} InViewerPosition: {1} InSelectorPosition: {2}", factor, inViewerPosition, inSelectorPosition);
		}

		#endregion

		#region Move/Zoom(Manipulation)

		private void StartListenManipulation()
		{
			var manipulationStarted = Observable.FromEvent<ManipulationStartedEventHandler, ManipulationStartedRoutedEventArgs>(
					h => (sender, e) => h(e),
					h => AssociatedViewer.ManipulationStarted += h,
					h => AssociatedViewer.ManipulationStarted -= h)
				.Where(e => e.PointerDeviceType == PointerDeviceType.Mouse);

			var manipulationDelta = Observable.FromEvent<ManipulationDeltaEventHandler, ManipulationDeltaRoutedEventArgs>(
					h => (sender, e) => h(e),
					h => AssociatedViewer.ManipulationDelta += h,
					h => AssociatedViewer.ManipulationDelta -= h);

			var manipulationCompleted = Observable.FromEvent<ManipulationCompletedEventHandler, ManipulationCompletedRoutedEventArgs>(
					h => (sender, e) => h(e),
					h => AssociatedViewer.ManipulationCompleted += h,
					h => AssociatedViewer.ManipulationCompleted -= h);

			disposer.Add(manipulationDelta
				.SkipUntil(manipulationStarted.Do(e => OnManipulationStarted(e)))
				.TakeUntil(manipulationCompleted.Do(e => OnManipulationCompleted(e)))
				.Throttle(TimeSpan.FromMilliseconds(10)) // 10 msec throttling
				.ObserveOn(SynchronizationContext.Current)
				.Repeat()
				.Subscribe(e => OnManipulationDelta(e)));
		}

		private Point startPosition;
		private double startHorizontalOffset;
		private double startVerticalOffset;
		private float startZoomFactor;

		private void OnManipulationStarted(ManipulationStartedRoutedEventArgs e)
		{
			Debug.WriteLine("Manipulation Started!");
			e.Handled = true;

			try
			{
				startPosition = e.Position;
				startHorizontalOffset = AssociatedViewer.HorizontalOffset;
				startVerticalOffset = AssociatedViewer.VerticalOffset;
				startZoomFactor = AssociatedViewer.ZoomFactor;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to handle manipulation started event.\r\n{0}", ex);
			}
		}

		private void OnManipulationCompleted(ManipulationCompletedRoutedEventArgs e)
		{
			Debug.WriteLine("Manipulation Completed!");
			e.Handled = true;
		}

		private void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
		{
			e.Handled = true;

			try
			{
				float? factor = null;

				var offsetX = startHorizontalOffset - e.Cumulative.Translation.X;
				var offsetY = startVerticalOffset - e.Cumulative.Translation.Y;

				var currentScale = e.Cumulative.Scale;
				if (currentScale != 1F)
				{
					factor = startZoomFactor * currentScale;

					offsetX = offsetX * currentScale + startPosition.X * (currentScale - 1F);
					offsetY = offsetY * currentScale + startPosition.Y * (currentScale - 1F);
				}

				AssociatedViewer.ChangeView(offsetX, offsetY, factor);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to reflect changes of ManipulationDalta.\r\n{0}", ex);
			}
		}

		#endregion

		#region Move(Pointer)

		private void StartListenPointer()
		{
			var pointerPressed = Observable.FromEvent<PointerEventHandler, PointerRoutedEventArgs>(
					h => (sender, e) => h(e),
					h => AssociatedSelector.PointerPressed += h,
					h => AssociatedSelector.PointerPressed -= h)
				.Where(e => e.Pointer.PointerDeviceType == PointerDeviceType.Mouse);

			var pointerMoved = Observable.FromEvent<PointerEventHandler, PointerRoutedEventArgs>(
					h => (sender, e) => h(e),
					h => AssociatedSelector.PointerMoved += h,
					h => AssociatedSelector.PointerMoved -= h);

			// PointerReleased and other events seems not to fire from ScrollViewer if its content is ItemsControl.
			var pointerUnpressed = Observable.Merge(
				Observable.FromEvent<PointerEventHandler, PointerRoutedEventArgs>(
					h => (sender, e) => h(e),
					h => AssociatedSelector.PointerReleased += h,
					h => AssociatedSelector.PointerReleased -= h),
				Observable.FromEvent<PointerEventHandler, PointerRoutedEventArgs>(
					h => (sender, e) => h(e),
					h => AssociatedSelector.PointerCanceled += h,
					h => AssociatedSelector.PointerCanceled -= h),
				Observable.FromEvent<PointerEventHandler, PointerRoutedEventArgs>(
					h => (sender, e) => h(e),
					h => AssociatedSelector.PointerCaptureLost += h,
					h => AssociatedSelector.PointerCaptureLost -= h));

			disposer.Add(pointerMoved
				.SkipUntil(pointerPressed.Do(e => OnPointerPressed(e)))
				.TakeUntil(pointerUnpressed.Do(e => OnPointerUnpressed(e)))
				.Throttle(TimeSpan.FromMilliseconds(10)) // 10 msec throttling
				.ObserveOn(SynchronizationContext.Current)
				.Repeat()
				.Subscribe(e => OnPointerMoved(e)));
		}

		private double startHorizontalOffsetPosition;
		private double startVerticalOffsetPosition;

		private void OnPointerPressed(PointerRoutedEventArgs e)
		{
			Debug.WriteLine("Pointer pressed.");
			e.Handled = true;

			var startPosition = e.GetCurrentPoint(AssociatedViewer).Position;

			startHorizontalOffsetPosition = AssociatedViewer.HorizontalOffset + startPosition.X;
			startVerticalOffsetPosition = AssociatedViewer.VerticalOffset + startPosition.Y;
		}

		private void OnPointerUnpressed(PointerRoutedEventArgs e)
		{
			Debug.WriteLine("Pointer unpressed.");
			e.Handled = true;
		}

		private void OnPointerMoved(PointerRoutedEventArgs e)
		{
			e.Handled = true;

			var position = e.GetCurrentPoint(AssociatedViewer).Position;

			var offsetX = startHorizontalOffsetPosition - position.X;
			var offsetY = startVerticalOffsetPosition - position.Y;

			AssociatedViewer.ChangeView(offsetX, offsetY, null);
		}

		#endregion
	}
}