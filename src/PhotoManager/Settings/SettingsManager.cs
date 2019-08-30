using System.ComponentModel.Composition;

namespace KsWare.PhotoManager.Settings
{
	[Export, PartCreationPolicy(CreationPolicy.Shared)]
	public class SettingsManager : IPartImportsSatisfiedNotification
	{
		public UserSettings User { get; private set; }

		public AppSettings App { get; private set;}

		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
			User=UserSettings.Load();
		}
	}
}
