using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Action = System.Action;

namespace KsWare.PhotoManager.Common.Commands
{
	public class UiCommandViewModel : CommandViewModel, IHaveDisplayName
	{
		private string _displayName;
		private object _icon;

		public UiCommandViewModel(string displayName, Action executeCallback, Func<bool> canExecuteCallback) : base(executeCallback, canExecuteCallback)
		{
			_displayName = displayName;
		}

		public UiCommandViewModel(string displayName, Func<Task> executeCallback, Func<bool> canExecuteCallback) : base(executeCallback, canExecuteCallback)
		{
			_displayName = displayName;
		}

		public string DisplayName { get => _displayName; set => Set(ref _displayName, value); }

		public object Icon { get => _icon; set => Set(ref _icon, value);}
	}
}