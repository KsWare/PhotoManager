using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using KsWare.PhotoManager.Tools;
using KsWare.PhotoManager.Extensions;
using KsWare.PhotoManager.Settings;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace KsWare.PhotoManager.MyPhotoTable
{
	public class PhotoTableViewModel : PropertyChangedBase
	{
		private ObservableCollection<PhotoTableItemViewModel> _items = new ObservableCollection<PhotoTableItemViewModel>();
		private string[] _supportedExtensions = {".jpg", ".jpeg", ".png", ".bmp", ".emf", ".exif", ".gif", ".ico", ".tif", ".tiff", ".wmf" };
		private int _size = 256;

		private UserSettings UserSettings => UserSettings.Instance;

		public PhotoTableViewModel()
		{
			if(DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
			//			Load(@"E:\Fotos\2019-06-22 Import DMC-FZ7");
//			Load(@"E:\Fotos\2019-07-06 Import DC-FZ1000 II")
			if (UserSettings.DefaultFolder != null)
			{
				Task.Run(() => Load(UserSettings.DefaultFolder)).ConfigureAwait(false);
			}
			else
			{
				Task.Run(OpenFolder).ConfigureAwait(false);
			}
			
		}

		private void OpenFolder()
		{
			//TODO maybe use another FolderDialog, but for the first, this one does the job
			string folder = null;
			ApplicationWrapper.Dispatcher.Invoke(() =>
			{
				using (var dlg = new CommonOpenFileDialog
				{
					IsFolderPicker = true,
					EnsurePathExists = true
				})
				{
					var result = dlg.ShowDialog();
					if (result != CommonFileDialogResult.Ok) return;
					folder = dlg.FileNames.FirstOrDefault();
				}
			});
			if(folder==null) return;
			UserSettings.DefaultFolder = folder;
			UserSettings.Save();
			Load(UserSettings.DefaultFolder);
		}

		public int Size { get => _size; set => Set(ref _size, value);}

		private void Load(string path)
		{
			ApplicationWrapper.Dispatcher.Invoke(() => Items=null);
			var directory = new DirectoryInfo(path);
			var files = directory.GetFiles();

			var items=new ObservableCollection<PhotoTableItemViewModel>();
			foreach (var file in files)
			{
				var ext = Path.GetExtension(file.Name)?.ToLower();
				if(!_supportedExtensions.Contains(ext)) continue;
				items.Add(new PhotoTableItemViewModel(file));
			}
			ApplicationWrapper.Dispatcher.Invoke(() => Items = items);
			foreach (var item in items)
			{
				item.BeginInitialize();
			}
		}

		public void ViewRefresh()
		{
			var items = Items;
			Items = null;
			Dispatcher.CurrentDispatcher.Invoke(() => Items = items);
		}

		public ObservableCollection<PhotoTableItemViewModel> Items { get => _items; set => Set(ref _items, value); }
	}
}
