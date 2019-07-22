using System.ComponentModel.Composition;
using Caliburn.Micro;
using KsWare.PhotoManager.Screens.ImageViewer;
using KsWare.PhotoManager.Screens.PhotoTable;

namespace KsWare.PhotoManager.Shell
{
	[Export(typeof(IShell)), PartCreationPolicy(CreationPolicy.Shared)]
	public sealed class ShellViewModel : Conductor<object>, IShell, IPartImportsSatisfiedNotification
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
	}
}