using System.ComponentModel.Composition;
using Caliburn.Micro;
using KsWare.PhotoManager.Common;
using KsWare.PhotoManager.MyImageViewer;
using KsWare.PhotoManager.MyPhotoTable;

namespace KsWare.PhotoManager.Shell
{
	[Export(typeof(IShell)), PartCreationPolicy(CreationPolicy.Shared)]
	public sealed class ShellViewModel : Conductor<object>, IShell, IPartImportsSatisfiedNotification
	{
		[Import] private PhotoTableViewModel _photoTableViewModel;
		[Import] private ImageViewerViewModel _imageViewer;

		public PhotoTableViewModel ShowPhotoTable()
		{
			ActivateItem(_photoTableViewModel);
			return _photoTableViewModel;
		}

		public ImageViewerViewModel ShowImageViewer()
		{
			ActivateItem(_imageViewer);
			return _imageViewer;
		}

		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
		}
	}
}