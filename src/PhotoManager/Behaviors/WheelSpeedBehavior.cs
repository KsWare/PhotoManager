using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace KsWare.PhotoManager.Behaviors
{
	public class WheelSpeedBehavior : Behavior<ListBox> // TODO specify a less specific control
	{
		private ScrollViewer _scrollViewer;
		private FrameworkElement _firstContainer;

		public ScrollUnit ScrollUnit { get; set; } = ScrollUnit.Item;

		public double ScrollDelta { get; set; } = 3;

		public bool IgnoreSystemSettings { get; set; } = true;

		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
			_scrollViewer = FindChild<ScrollViewer>(AssociatedObject, 3);
		}

		private T FindChild<T>(DependencyObject o, int maxDeep = -1) where T : class => FindChild<T>(o, maxDeep, 0);

		private T FindChild<T>(DependencyObject o, int maxDeep, int deep) where T : class
		{
			var count = VisualTreeHelper.GetChildrenCount(o);
			for (int i = 0; i < count; i++)
			{
				if (VisualTreeHelper.GetChild(o, i) is T child) return child;
			}

			if (maxDeep >= 0 && deep == maxDeep) return null;
			for (int i = 0; i < count; i++)
			{
				o = VisualTreeHelper.GetChild(AssociatedObject, i);
				var child = FindChild<T>(o, maxDeep, deep + 1);
				if (child != null) return child;
			}

			return null;
		}

		protected override void OnDetaching()
		{
			_scrollViewer = null;
			AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
			base.OnDetaching();
		}

		private void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Handled) return;
			if (_scrollViewer == null) _scrollViewer = FindChild<ScrollViewer>(AssociatedObject, 3);
			if (_scrollViewer == null) return;

			if (AssociatedObject.Items.IsEmpty) return;
			var item = AssociatedObject.Items[0];
			if (_firstContainer == null)
				_firstContainer = AssociatedObject.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
			if (_firstContainer == null) return;


			if (_scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
			{
				double delta = 0;
				switch (ScrollUnit)
				{
					case ScrollUnit.Pixel:
						delta = ScrollDelta;
						break;
					case ScrollUnit.Item:
						delta = _firstContainer.ActualWidth * ScrollDelta;
						break;
					case ScrollUnit.Page:
						var deltaItems = Math.Ceiling(_scrollViewer.ViewportWidth / _firstContainer.ActualWidth);
						if (deltaItems >= 0 && deltaItems < 1) deltaItems = 1;
						else if (deltaItems <= 0 && deltaItems > -1) deltaItems = -1;
						delta = deltaItems * _firstContainer.ActualWidth * ScrollDelta;
						break;
				}

				var mouseDelta = GetMouseDelta(e.Delta);
				_scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + delta * mouseDelta * -1);
				e.Handled = true;
			}
			else if (_scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
			{
				double delta = 0;
				switch (ScrollUnit)
				{
					case ScrollUnit.Pixel:
						delta = ScrollDelta;
						break;
					case ScrollUnit.Item:
						delta = _firstContainer.ActualHeight * ScrollDelta;
						break;
					case ScrollUnit.Page:
						var deltaItems = Math.Ceiling(_scrollViewer.ViewportHeight / _firstContainer.ActualHeight);
						if (deltaItems >= 0 && deltaItems < 1) deltaItems = 1;
						else if (deltaItems <= 0 && deltaItems > -1) deltaItems = -1;
						delta = deltaItems * _firstContainer.ActualHeight * ScrollDelta;
						break;
				}

				var mouseDelta = GetMouseDelta(e.Delta);
				_scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + delta * mouseDelta * -1);
				e.Handled = true;
			}
		}

		private double GetMouseDelta(int delta) => IgnoreSystemSettings ? Math.Sign(delta) : (double)delta / 40;
	}

	public enum ScrollUnit
	{
		None,
		Pixel,
		Item,
		Page
	}
}