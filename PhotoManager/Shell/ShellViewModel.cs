using System.ComponentModel.Composition;
using Caliburn.Micro;
using KsWare.PhotoManager.Common;
using KsWare.PhotoManager.MyPhotoTable;

namespace KsWare.PhotoManager.Shell
{
	[Export(typeof(IShell)), PartCreationPolicy(CreationPolicy.Shared)]
	public class ShellViewModel : Screen, IShell, IPartImportsSatisfiedNotification
	{
		[Import] private IServiceLocator _serviceLocator;
		private PropertyChangedBase _mainContent;

		public PropertyChangedBase MainContent { get => _mainContent; set => Set(ref _mainContent, value); }

		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
		}

		protected override void OnInitialize()
		{
			base.OnInitialize();
			MainContent = _serviceLocator.GetInstance<PhotoTableViewModel>();
		}

	}
}