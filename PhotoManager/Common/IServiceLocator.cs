namespace KsWare.PhotoManager.Common
{
    public interface IServiceLocator
    {
        T GetInstance<T>() where T : class;
    }
}