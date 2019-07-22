using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace KsWare.PhotoManager.Screens.FileInfo
{
	[Export]
	public class FileInfoViewModel : Screen, IPartImportsSatisfiedNotification
	{
		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
			
		}
	}
}
