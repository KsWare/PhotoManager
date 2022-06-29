using System.Threading.Tasks;

namespace KsWare.PhotoManager.Screens.ImageViewer
{
	public interface IImageViewerViewModel
	{
		void OpenImage(string filePath);
		Task OpenImageAsync(string filePath);
	}
}