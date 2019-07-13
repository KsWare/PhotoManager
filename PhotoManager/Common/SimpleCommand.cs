using System;
using System.Windows.Input;

namespace KsWare.PhotoManager.Common
{
	public class SimpleCommand : ICommand
	{
		private readonly Action _executeAction;

		public SimpleCommand(Action executeAction)
		{
			_executeAction = executeAction;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			_executeAction();
		}

		public event EventHandler CanExecuteChanged;
	}
}