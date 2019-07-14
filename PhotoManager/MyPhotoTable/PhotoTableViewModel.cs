using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using KsWare.PhotoManager.Common;
using KsWare.PhotoManager.Extensions;
using KsWare.PhotoManager.MyImageViewer;
using KsWare.PhotoManager.Resources;
using KsWare.PhotoManager.Settings;
using KsWare.PhotoManager.Shell;
using KsWare.PhotoManager.Tools;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace KsWare.PhotoManager.MyPhotoTable
{
	[Export]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class PhotoTableViewModel : Screen, IPartImportsSatisfiedNotification
	{
		[Import] private ImageLoader _imageLoader;
		[Import] private IServiceLocator _serviceLocator;
		[Import] private SettingsManager _settingsManager;
		[Import(typeof(IShell))] private ShellViewModel _shell;

		private readonly string[] _supportedExtensions = ImageTools.SupportedExtensions.Select(x => x.Key).ToArray();

		private ObservableCollection<PhotoTableItemViewModel> _items =
			new ObservableCollection<PhotoTableItemViewModel>();

		private PhotoTableItemViewModel _selectedItem;
		private bool _showDisplayNames;
		private int _size = 256;

		public PhotoTableItemViewModel SelectedItem { get => _selectedItem; set => Set(ref _selectedItem, value); }

		public int Size { get => _size; set => Set(ref _size, value); }

		public bool ShowDisplayNames { get => _showDisplayNames; set => Set(ref _showDisplayNames, value); }

		public ObservableCollection<PhotoTableItemViewModel> Items { get => _items; set => Set(ref _items, value); }

		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
			if (_settingsManager.User.DefaultFolder != null)
				Task.Run(() => Load(_settingsManager.User.DefaultFolder)).ConfigureAwait(false);
			else
				Task.Run(MenuFileOpenFolder).ConfigureAwait(false);
		}

		public void MenuFileOpenFolder()
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

		public void MenuFileOpenFile()
		{
			ApplicationWrapper.Dispatcher.BeginInvoke(() => _shell.ShowImageViewer());
			ApplicationWrapper.Dispatcher.BeginInvoke(() => ((ImageViewerViewModel) _shell.ActiveItem).MenuFileOpen());
		}

		public void MenuViewColorTest()
		{
			_shell.ActivateItem(new ColorTestViewModel());
		}

		public void MenuViewOpenImage()
		{
			var imageViewer = _shell.ShowImageViewer();
			imageViewer.OpenImage(SelectedItem.FilePath);
		}

		public void ViewRefresh()
		{
			var items = Items;
			Items = null;
			Dispatcher.CurrentDispatcher.Invoke(() => Items = items);
		}

		public void ShowImageViewer(PhotoTableItemViewModel item)
		{
			var imageViewer = _shell.ShowImageViewer();
			imageViewer.OpenImage(item.FilePath);
		}

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
			foreach (var item in items) item.BeginInitialize();
		}

	}
}