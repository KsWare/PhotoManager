using System.ComponentModel.Composition;
using KsWare.CaliburnMicro.Common;
using KsWare.CaliburnMicro.Shared;
using KsWare.PhotoManager.Shell;

namespace KsWare.PhotoManager
{
	public class StartupTasks : StartupTasksBase
	{
		[ImportingConstructor]
		public StartupTasks(IServiceLocator serviceLocator) :base(serviceLocator)
		{
		}

		[Export(typeof(StartupTask))]
		public void ApplyViewLocatorOverride()
		{
			var viewLocator = ServiceLocator.GetInstance<IViewLocator>();
			Caliburn.Micro.ViewLocator.GetOrCreateViewType = viewLocator.GetOrCreateViewType;
		}

		[Export(typeof(StartupTask))]
		public void ShowPhotoTable()
		{
			var shell = (ShellViewModel)ServiceLocator.GetInstance<IShell>();
			shell.ShowPhotoTable();
		}
	}
}