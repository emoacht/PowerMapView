using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PowerMapView.Views.Controls
{
	public class CanvasListView : ListView
	{
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			var container = (ListViewItem)element;

			container.SetBinding(
				Canvas.LeftProperty,
				new Binding
				{
					Path = new PropertyPath("Left"),
					Mode = BindingMode.OneWay
				});

			container.SetBinding(
				Canvas.TopProperty,
				new Binding
				{
					Path = new PropertyPath("Top"),
					Mode = BindingMode.OneWay
				});

			container.SetBinding(
				Canvas.ZIndexProperty,
				new Binding
				{
					Path = new PropertyPath("ZIndex"),
					Mode = BindingMode.OneWay
				});

			base.PrepareContainerForItemOverride(element, item);
		}
	}
}