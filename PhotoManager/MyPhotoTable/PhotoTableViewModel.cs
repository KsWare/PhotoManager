using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using KsWare.PhotoManager.Common;
using KsWare.PhotoManager.Tools;
using KsWare.PhotoManager.Resources;
using KsWare.PhotoManager.Settings;
using KsWare.PhotoManager.Shell;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace KsWare.PhotoManager.MyPhotoTable
{
	[Export, PartCreationPolicy(CreationPolicy.NonShared)]
	public class PhotoTableViewModel : PropertyChangedBase, IPartImportsSatisfiedNotification
	{
		[Import] private ImageLoader _imageLoader;
		[Import] private SettingsManager _settingsManager;
		[Import(typeof(IShell))] private ShellViewModel _shellViewModel;
		[Import] private IServiceLocator _serviceLocator;

		private ObservableCollection<PhotoTableItemViewModel> _items =
			new ObservableCollection<PhotoTableItemViewModel>();

		private readonly string[] _supportedExtensions = ImageTools.SupportedExtensions.Select(x => x.Key).ToArray();
		private int _size = 256;
		private bool _showDisplayNames;

		public void FileOpenFolder()
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
			if (folder == null) return;
			_settingsManager.User.DefaultFolder = folder;
			_settingsManager.User.Save();

			_imageLoader.Stop();

			Load(_settingsManager.User.DefaultFolder);
		}

		public bool CanFileOpenFolder() => true;

		public void ViewColorTest()
		{
			_shellViewModel.MainContent = new ColorTestViewModel(); //TODO use ViewChanger
		}

		public int Size { get => _size; set => Set(ref _size, value); }

		public bool ShowDisplayNames { get => _showDisplayNames; set => Set(ref _showDisplayNames, value); }

		private void Load(string path)
		{
			ApplicationWrapper.Dispatcher.Invoke(() => Items = null);
			var directory = new DirectoryInfo(path);
			var files = directory.GetFiles();

			var items = new ObservableCollection<PhotoTableItemViewModel>();
			foreach (var file in files)
			{
				var ext = Path.GetExtension(file.Name)?.ToLower();
				if (!_supportedExtensions.Contains(ext)) continue;
				var item = _serviceLocator.GetInstance<PhotoTableItemViewModel>();
				item.Setup(this, file);
				items.Add(item);
			}

			ApplicationWrapper.Dispatcher.Invoke(() => Items = items);
			foreach (var item in items) { item.BeginInitialize(); }
		}

		public void ViewRefresh()
		{
			var items = Items;
			Items = null;
			Dispatcher.CurrentDispatcher.Invoke(() => Items = items);
		}

		public ObservableCollection<PhotoTableItemViewModel> Items { get => _items; set => Set(ref _items, value); }

		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
			if (_settingsManager.User.DefaultFolder != null)
			{
				Task.Run(() => Load(_settingsManager.User.DefaultFolder)).ConfigureAwait(false);
			}
			else { Task.Run(FileOpenFolder).ConfigureAwait(false); }
		}
	}
}