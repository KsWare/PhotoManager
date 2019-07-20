using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Action = System.Action;

namespace KsWare.PhotoManager.Common.Commands
{
	public class CommandViewModel : PropertyChangedBase, ICommand
	{
		public CommandViewModel(Action action, Func<bool> canExecute = null)
		{
			ExecuteCallback=action;
			CanExecuteCallback = canExecute;
		}

		public CommandViewModel(Func<Task> asyncAction, Func<bool> canExecute = null)
		{
			ExecuteAsyncCallback = asyncAction;
			CanExecuteCallback = canExecute;
		}

		public Func<bool> CanExecuteCallback { get; set; }

		public Action ExecuteCallback { get; set; }

		public Func<Task> ExecuteAsyncCallback { get; set; }

		public bool CanExecute(object parameter) => CanExecuteCallback == null || CanExecuteCallback();

		public async void Execute(object parameter)
		{
			if (ExecuteAsyncCallback != null) await ExecuteAsyncCallback().ConfigureAwait(true);
			else if (ExecuteCallback != null) ExecuteCallback();
		}

		public async Task ExecuteAsync(object parameter)
		{
			if (ExecuteAsyncCallback != null) await ExecuteAsyncCallback().ConfigureAwait(false);
			else if(ExecuteCallback !=null) await Task.Run(ExecuteCallback).ConfigureAwait(false);
		}

		public event EventHandler CanExecuteChanged;
	}
}