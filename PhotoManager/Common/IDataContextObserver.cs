namespace KsWare.PhotoManager.Common
{
	public interface IDataContextObserver
	{
		void OnDataContextAssigned();
		void OnDataContextReleased(string reason);
	}
}