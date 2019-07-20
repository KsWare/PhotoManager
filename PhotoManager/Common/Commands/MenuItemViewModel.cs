using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Action = System.Action;

namespace KsWare.PhotoManager.Common.Commands
{
	public class MenuItemViewModel : UiCommandViewModel, IMenuItemViewModel
	{
		private string _toolTip;
		private BindableCollection<IMenuItemViewModel> _subItems;

		public MenuItemViewModel(string displayName, Action executeCallback, Func<bool> canExecuteCallback = null) : base(displayName, executeCallback, canExecuteCallback)
		{
		}

		public MenuItemViewModel(string displayName, Func<Task> executeCallback, Func<bool> canExecuteCallback = null) : base(displayName, executeCallback, canExecuteCallback)
		{
		}

		private MenuItemViewModel() : base(null, null,null)
		{
		}

		public bool IsSeparator { get; protected set; }
		public string ToolTip { get => _toolTip; set => Set(ref _toolTip, value);}

		public BindableCollection<IMenuItemViewModel> SubItems { get => _subItems; set => Set(ref _subItems, value);}
	}
}