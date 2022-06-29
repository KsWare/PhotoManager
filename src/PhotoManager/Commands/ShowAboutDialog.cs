using System.ComponentModel.Composition;
using Caliburn.Micro;
using KsWare.CaliburnMicro.Commands;
using KsWare.CaliburnMicro.Common;
using KsWare.PhotoManager.Screens.About;
using KsWare.PhotoManager.Shell;

namespace KsWare.PhotoManager.Commands
{
	[Export]
	public class ShowAboutDialog : UserCommand<object>
	{
		[Import] private IWindowManager _windowManager;
		[Import(typeof(IShell))] private ShellViewModel _shell;
		[Import] private AboutViewModel _aboutViewModel;
		public override void Do()
		{
//			_shell.ActivateItem(_aboutViewModel);
			_windowManager.ShowDialog(_aboutViewModel);
		}
	}
}
