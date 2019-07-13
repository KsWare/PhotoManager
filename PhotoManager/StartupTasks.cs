using System.ComponentModel.Composition;
using KsWare.PhotoManager.Common;

namespace KsWare.PhotoManager
{
	public delegate void StartupTask();

	public class StartupTasks
	{
		private readonly IServiceLocator _serviceLocator;

		[ImportingConstructor]
		public StartupTasks(IServiceLocator serviceLocator)
		{
			_serviceLocator = serviceLocator;
		}

		[Export(typeof(StartupTask))]
		public void ApplyViewLocatorOverride()
		{
			var viewLocator = _serviceLocator.GetInstance<IViewLocator>();
			Caliburn.Micro.ViewLocator.GetOrCreateViewType = viewLocator.GetOrCreateViewType;
		}
	}
}