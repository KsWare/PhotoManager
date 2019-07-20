using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace KsWare.PhotoManager.Common
{
	//TODO MefServiceLocator cleanup and test 
	//[Export(typeof(IServiceLocator)), PartCreationPolicy(CreationPolicy.Shared)]
	public class MefServiceLocator : IServiceLocator
	{

		private readonly DebugCompositionContainer _container;

		//[ImportingConstructor]
		public MefServiceLocator(DebugCompositionContainer container)
		{
			_container = container;
		}

		public T GetInstance<T>()
		{
			var instance = _container.GetExportedValue<T>();
			if (instance != null) { return instance; }

			throw new CompositionException(
				$"Could not locate any instances of contract {typeof(T)}.\nTrace\n{_container.GetFailedExportsTrace()}");
		}

		//TODO implement and test GetInstance<T,T1>(string p1Name, T1 p1)
		public T GetInstance<T, T1>(string p1Name, T1 p1)
		{
			_container.ComposeExportedValue(p1Name, p1);
			var instance = _container.GetExportedValue<T>();
			if (instance != null) { return instance; }

			throw new CompositionException(
				$"Could not locate any instances of contract {typeof(T)}.\nTrace\n{_container.GetFailedExportsTrace()}");
		}

		//TODO implement and test GetInstance<T, T1, T2>(string p1Name, T1 p1, string p2Name, T2 p2)
		public T GetInstance<T, T1, T2>(string p1Name, T1 p1, string p2Name, T2 p2)
		{
			_container.ComposeExportedValue<T1>(p1Name, p1);
			_container.ComposeExportedValue<T2>(p2Name, p2);
			var instance = _container.GetExportedValue<T>();
			if (instance != null) { return instance; }

			throw new CompositionException(
				$"Could not locate any instances of contract {typeof(T)}.\nTrace\n{_container.GetFailedExportsTrace()}");
		}

//        public T GetInstance<T>(IDictionary<string, object> parameter)
//        {
//	        foreach (var p in parameter)
//				_container.ComposeExportedValue(p.Key, p.Value);
//
//			var instance = _container.GetExportedValue<T>();
//	        if (instance != null)
//	        {
//		        return instance;
//	        }
//
//	        throw new CompositionException($"Could not locate any instances of contract {typeof(T)}.\nTrace\n{_container.GetFailedExportsTrace()}");
//        }

		//TODO test (implementation is not working)
		public T GetInstance<T>(IDictionary<string, object> parameter)
		{
			var batch = new CompositionBatch();
			foreach (var p in parameter)
				batch.AddExportedValue(p.Key, p.Value);
			_container.Compose(batch);
			var instance = _container.GetExportedValue<T>();
			if (instance != null) { return instance; }

			throw new CompositionException(
				$"Could not locate any instances of contract {typeof(T)}.\nTrace\n{_container.GetFailedExportsTrace()}");
		}

		public T CreateInstanceDirect<T>(params object[] args)
		{ //DRAFT
			var instance = (T) Activator.CreateInstance(typeof(T), args);
			_container.ComposeParts(instance);
			return instance;
		}
	}
}