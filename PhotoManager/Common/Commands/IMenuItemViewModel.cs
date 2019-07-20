using System.ComponentModel;
using System.Windows.Input;
using Caliburn.Micro;

namespace KsWare.PhotoManager.Common.Commands
{
	public interface IMenuItemViewModel : IHaveDisplayName, ICommand, INotifyPropertyChanged
	{
		bool IsSeparator { get; }
		string ToolTip { get; set; }
		BindableCollection<IMenuItemViewModel> SubItems { get; set; }
	}
}