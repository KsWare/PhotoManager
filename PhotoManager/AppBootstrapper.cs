using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KsWare.PhotoManager.Common;

namespace KsWare.PhotoManager {
	public class AppBootstrapper : BootstrapperBase
	{
		private DebugCompositionContainer _container;

		public AppBootstrapper()
		{
			Initialize();
		}

		protected override void BuildUp(object instance)
		{
			_container.SatisfyImportsOnce(instance);
		}

		protected override void Configure()
		{
			var catalog =
				new AggregateCatalog(
					AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>());

			_container = new DebugCompositionContainer(catalog);

			var batch = new CompositionBatch();

			batch.AddExportedValue<IWindowManager>(new WindowManager());
			batch.AddExportedValue<IEventAggregator>(new EventAggregator());
//			batch.AddExportedValue(_container); // TODO Warning: A CompositionContainer should never import itself, or a part that has a reference to it. Such a reference could allow an untrusted part to gain access all the parts in the container.
			batch.AddExportedValue<IServiceLocator>(new MefServiceLocator(_container));
			batch.AddExportedValue(catalog);

			_container.Compose(batch);
		}

		protected override IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
		}

		protected override object GetInstance(Type serviceType, string key)
		{
			var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
			var exports = _container.GetExportedValues<object>(contract);

			if (exports.Any())
			{
				return exports.First();
			}

			throw new Exception($"Could not locate any instances of contract {contract}.\nTrace:\n{_container.GetFailedExportsTrace()}");
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			var startupTasks =
				GetAllInstances(typeof(StartupTask))
				.Cast<ExportedDelegate>()
				.Select(exportedDelegate => (StartupTask)exportedDelegate.CreateDelegate(typeof(StartupTask)));

			startupTasks.Apply(s => s());

			DisplayRootViewFor<IShell>();
		}
    }
}