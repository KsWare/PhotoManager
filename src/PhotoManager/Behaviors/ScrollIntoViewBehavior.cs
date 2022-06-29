using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace KsWare.PhotoManager.Behaviors
{
	public class ScrollIntoViewBehavior : Behavior<ListBox> // TODO specify a less specific control
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
		}

		private void AssociatedObject_SelectionChanged(object sender,
			SelectionChangedEventArgs e)
		{
			var listBox = sender as ListBox;
			if (listBox?.SelectedItem == null) return;
			listBox.Dispatcher.BeginInvoke(
				(Action) (() =>
				{
					listBox.UpdateLayout();
					if (listBox.SelectedItem !=
					    null)
						listBox.ScrollIntoView(
							listBox.SelectedItem);
				}));
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.SelectionChanged -=
				AssociatedObject_SelectionChanged;
		}
	}
}