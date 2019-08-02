using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using KsWare.CaliburnMicro.DragDrop;
using KsWare.PhotoManager.Screens.ImageViewer;
using KsWare.PhotoManager.Screens.PhotoTable;

namespace KsWare.PhotoManager.Shell
{
	[Export(typeof(IShell)), PartCreationPolicy(CreationPolicy.Shared)]
	public sealed class ShellViewModel : Conductor<object>, IShell, IPartImportsSatisfiedNotification, ICustomDropTarget
	{
		[Import] private PhotoTableViewModel _photoTable;
		[Import] private ScrollImageViewerViewModel _imageViewer;

		public PhotoTableViewModel ShowPhotoTable()
		{
			ActivateItem(_photoTable);
			return _photoTable;
		}

		public IImageViewerViewModel ShowImageViewer()
		{
			ActivateItem(_imageViewer);
			return _imageViewer;
		}

		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
		}

		void ICustomDropTarget.OnDrop(object sender, DragEventArgs e)
		{
			if (ActiveItem is ICustomDropTarget dropTarget)
			{
				dropTarget.OnDrop(sender, e);
				return;
			}
		}

		void ICustomDropTarget.OnDragEnter(object sender, DragEventArgs e)
		{
			if (ActiveItem is ICustomDropTarget dropTarget)
			{
				dropTarget.OnDragEnter(sender, e);
				return;
			}
		}

		void ICustomDropTarget.OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
		{
			if (ActiveItem is ICustomDropTarget dropTarget)
			{
				dropTarget.OnGiveFeedback(sender, e);
				return;
			}
		}

		void ICustomDropTarget.OnDragOver(object sender, DragEventArgs e)
		{
			if (ActiveItem is ICustomDropTarget dropTarget)
			{
				dropTarget.OnDragOver(sender, e);
				return;
			}
		}

		void ICustomDropTarget.OnDragLeave(object sender, DragEventArgs e)
		{
			if (ActiveItem is ICustomDropTarget dropTarget)
			{
				dropTarget.OnDragLeave(sender, e);
				return;
			}
		}
	}
}