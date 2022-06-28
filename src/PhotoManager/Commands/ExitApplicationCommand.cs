using System.ComponentModel.Composition;
using KsWare.CaliburnMicro.Commands;
using KsWare.Presentation.StaticWrapper;

namespace KsWare.PhotoManager.Commands
{
	[Export]
	public class ExitApplicationCommand : UserCommand<object>
	{
		[Import] private IApplication _application;

		public override void Do()
		{
			_application.Shutdown();
		}
	}
}
