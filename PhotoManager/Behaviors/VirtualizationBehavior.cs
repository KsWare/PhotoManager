using System.Diagnostics;
using System.Windows;
using System.Windows.Interactivity;
using KsWare.PhotoManager.Common;

namespace KsWare.PhotoManager.Behaviors
{
	public class VirtualizationBehavior: Behavior<FrameworkElement>
	{
		private IDataContextObserver _prevVM;

		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.DataContextChanged+=AssociatedObjectOnDataContextChanged;
			AssociatedObject.RequestBringIntoView += (s, e) => Debug.WriteLine("RequestBringIntoView");
			AssociatedObject.Unloaded+=AssociatedObjectOnUnloaded;
			if (AssociatedObject.DataContext!=null)
			{
				OnDataContextChanged(AssociatedObject.DataContext,null, "DataContextChanged");
			}
		}

		private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs e)
		{
			OnDataContextChanged(null,_prevVM, "Unloaded");
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.DataContextChanged -= AssociatedObjectOnDataContextChanged;
			_prevVM?.OnDataContextReleased("Detaching");
			_prevVM = null;
		}

		private void AssociatedObjectOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			OnDataContextChanged(e.NewValue, e.OldValue, "OnDataContextChanged");
		}

		private void OnDataContextChanged(object eNewValue, object eOldValue, string reason)
		{
			if (eNewValue != null && eNewValue is IDataContextObserver newVM)
			{
				_prevVM = newVM;
				newVM.OnDataContextAssigned();
			}
			else if (eOldValue != null && eOldValue is IDataContextObserver oldVM)
			{
				oldVM.OnDataContextReleased(reason);
				_prevVM = null;
			}
		}
	}
}
