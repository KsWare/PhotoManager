using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
		private SelectionMode _selectionMode = SelectionMode.Extended;
		private BindableCollection<PhotoTableItemViewModel> _selectedItems = new BindableCollection<PhotoTableItemViewModel>();
		private bool _isMoreAsOneItemSelected;
		private int _selectedItemsCount;

		public PhotoTableItemViewModel SelectedItem { get => _selectedItem; set => Set(ref _selectedItem, value); }

		public IObservableCollection<PhotoTableItemViewModel> SelectedItems => _selectedItems;

		public int Size { get => _size; set => Set(ref _size, value); }

		public bool ShowDisplayNames { get => _showDisplayNames; set => Set(ref _showDisplayNames, value); }

		public ObservableCollection<PhotoTableItemViewModel> Items { get => _items; set => Set(ref _items, value); }
		public SelectionMode SelectionMode { get => _selectionMode; set => Set(ref _selectionMode, value);}

		public void ListBox_SelectionChanged(SelectionChangedEventArgs e)
		{
			_selectedItems.RemoveRange(e.RemovedItems.Cast<PhotoTableItemViewModel>());
			_selectedItems.AddRange(e.AddedItems.Cast<PhotoTableItemViewModel>());
		}

		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
			if (_settingsManager.User.DefaultFolder != null)
				Task.Run(() => Load(_settingsManager.User.DefaultFolder)).ConfigureAwait(false);
			else
				Task.Run(MenuFileOpenFolder).ConfigureAwait(false);
			_selectedItems.CollectionChanged+=SelectedItems_CollectionChanged;
		}

		private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SelectedItemsCount = _selectedItems.Count;
			IsMoreAsOneItemSelected = _selectedItems.Count > 1;
		}

		public bool IsMoreAsOneItemSelected { get => _isMoreAsOneItemSelected; private set => Set(ref _isMoreAsOneItemSelected, value);}
		public int SelectedItemsCount { get => _selectedItemsCount; private set => Set(ref _selectedItemsCount, value);}


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
			_shell.ShowImageViewer().OpenImage(SelectedItem.FilePath);
		}

		public void ContextMenuView(object selectedItem)
		{
			//_shell.ShowImageViewer().OpenImage(selectedItem.FilePath);
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
				//TODO use correct way to instantiate a PhotoTableItemViewModel
//				var item = _serviceLocator.GetInstance<PhotoTableItemViewModel>(new Dictionary<string, object> { { "Parent", this }, { "FileInfo", file } });
				var item = _serviceLocator.CreateInstanceDirect<PhotoTableItemViewModel>(this, file);
//				item.Setup(this, file); 
				items.Add(item);
			}

			ApplicationWrapper.Dispatcher.Invoke(() => Items = items);
			foreach (var item in items) item.BeginInitialize();
		}

	}
}