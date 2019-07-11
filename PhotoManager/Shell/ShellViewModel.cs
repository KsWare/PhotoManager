using Caliburn.Micro;
using KsWare.PhotoManager.MyPhotoTable;
using PhotoManager;

namespace KsWare.PhotoManager.Shell {
	public class ShellViewModel : PropertyChangedBase, IShell
	{
		private PropertyChangedBase _mainContent;

		public ShellViewModel()
		{
			_mainContent=new PhotoTableViewModel();
		}

		public PropertyChangedBase MainContent { get => _mainContent; set => Set(ref _mainContent, value);}

	}
}