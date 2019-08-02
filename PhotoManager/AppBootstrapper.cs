using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KsWare.CaliburnMicro.Common;
using BootstrapperBase = KsWare.CaliburnMicro.Common.BootstrapperBase;

namespace KsWare.PhotoManager
{
	public class AppBootstrapper : BootstrapperBase
	{
		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			var startupTasks =
				GetAllInstances(typeof(StartupTask))
					.Cast<ExportedDelegate>()
					.Select(exportedDelegate => (StartupTask) exportedDelegate.CreateDelegate(typeof(StartupTask)));

			startupTasks.Apply(s => s());

			DisplayRootViewFor<IShell>();
		}
	}
}