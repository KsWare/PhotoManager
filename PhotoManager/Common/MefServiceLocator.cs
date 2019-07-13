using System;

namespace KsWare.PhotoManager.Common
{
    //[Export(typeof(IServiceLocator)), PartCreationPolicy(CreationPolicy.Shared)]
    public class MefServiceLocator : IServiceLocator
    {
        private readonly DebugCompositionContainer _container;

        //[ImportingConstructor]
        public MefServiceLocator(DebugCompositionContainer container)
        {
            _container = container;
        }

        public T GetInstance<T>() where T : class
        {
            var instance = _container.GetExportedValue<T>();
            if (instance != null)
            {
                return instance;
            }

            throw new Exception($"Could not locate any instances of contract {typeof(T)}.\nTrace\n{_container.GetFailedExportsTrace()}");
        }
    }
}