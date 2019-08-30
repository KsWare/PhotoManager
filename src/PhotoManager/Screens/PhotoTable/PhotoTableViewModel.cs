using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using KsWare.CaliburnMicro.Commands;
using KsWare.CaliburnMicro.Common;
using KsWare.CaliburnMicro.DragDrop;
using KsWare.CaliburnMicro.Shared;
using KsWare.PhotoManager.Resources;
using KsWare.PhotoManager.Screens.About;
using KsWare.PhotoManager.Screens.ImageViewer;
using KsWare.PhotoManager.Settings;
using KsWare.PhotoManager.Shell;
using KsWare.PhotoManager.Tools;
using KsWare.Presentation.StaticWrapper;
using Microsoft.WindowsAPICodePack.Dialogs;
using Action = System.Action;

namespace KsWare.PhotoManager.Screens.PhotoTable
{
	[Export]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class PhotoTableViewModel : Screen, IPartImportsSatisfiedNotification, ICustomDropTarget
	{
		[Import] private ImageLoader _imageLoader;
		[Import] private IServiceLocator _serviceLocator;
		[Import] private SettingsManager _settingsManager;
		[Import(typeof(IShell))] private ShellViewModel _shell;
		[Import] private IApplication _application;
		[Import] private AboutViewModel _aboutViewModel;
		private ApplicationDispatcherExtender UiThread = ApplicationDispatcher.Do;

		private readonly string[] _supportedExtensions = ImageTools.SupportedExtensions.Select(x => x.Key).ToArray();

		private IObservableCollection<ImageThumbViewModel> _items = new BindableCollection<ImageThumbViewModel>();

		private ImageThumbViewModel _selectedItem;
		private bool _showDisplayNames;
		private int _size = 256;
		private SelectionMode _selectionMode = SelectionMode.Extended;
		private IObservableCollection<ImageThumbViewModel> _selectedItems = new BindableCollection<ImageThumbViewModel>();
		private bool _isMoreAsOneItemSelected;
		private int _selectedItemsCount;

		public ImageThumbViewModel SelectedItem { get => _selectedItem; set => Set(ref _selectedItem, value); }

		public IObservableCollection<ImageThumbViewModel> SelectedItems => _selectedItems;

		public int Size { get => _size; set => Set(ref _size, value); }

		public bool ShowDisplayNames { get => _showDisplayNames; set => Set(ref _showDisplayNames, value); }

		public IObservableCollection<ImageThumbViewModel> Items { get => _items; set => Set(ref _items, value); }
		public SelectionMode SelectionMode { get => _selectionMode; set => Set(ref _selectionMode, value); }

		public void ListBox_SelectionChanged(SelectionChangedEventArgs e)
		{
			_selectedItems.RemoveRange(e.RemovedItems.Cast<ImageThumbViewModel>());
			_selectedItems.AddRange(e.AddedItems.Cast<ImageThumbViewModel>());
		}

		void IPartImportsSatisfiedNotification.OnImportsSatisfied()
		{
			if (_settingsManager.User.DefaultFolder != null)
				Task.Run(() => LoadAsync(_settingsManager.User.DefaultFolder).Wait()).ConfigureAwait(false);
			else
				Task.Run(MenuFileOpenFolder).ConfigureAwait(false);
			_selectedItems.CollectionChanged += SelectedItems_CollectionChanged;
			InitMenu();
		}

		private void InitMenu()
		{
			MenuItems.Clear();;
			MenuItems.Add(new MenuItemViewModel("_File", new[]
			{
				new MenuItemViewModel("_Open Folder...", MenuFileOpenFolder),
				new MenuItemViewModel("Open _File...", MenuFileOpenFile),
				new MenuItemSeparatorViewModel(),
				new MenuItemViewModel("_Exit", MenuFileExit),
			}));
//			MenuItems.Add(new MenuItemViewModel("_Edit", new[]
//				{
//				}));
			MenuItems.Add(new MenuItemViewModel("_View", new[]
			{
				new MenuItemViewModel("Open Image", MenuViewOpenImage),
				new MenuItemViewModel("Refresh", MenuViewRefresh),
			}));
			MenuItems.Add(new MenuItemViewModel("_Window", new[]
			{
				new MenuItemViewModel("_Color Test", MenuWindowColorTest)
			}));
			MenuItems.Add(new MenuItemViewModel("_Help", new[]
			{
				new MenuItemViewModel("_About...", MenuHelpAbout),
			}));
		}

		public IList<IMenuItemViewModel> MenuItems { get; } = new BindableCollection<IMenuItemViewModel>();

		private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SelectedItemsCount = _selectedItems.Count;
			IsMoreAsOneItemSelected = _selectedItems.Count > 1;
		}

		public bool IsMoreAsOneItemSelected
		{
			get => _isMoreAsOneItemSelected;
			private set => Set(ref _isMoreAsOneItemSelected, value);
		}

		public int SelectedItemsCount
		{
			get => _selectedItemsCount;
			private set => Set(ref _selectedItemsCount, value);
		}


		public async Task MenuFileOpenFolder()
		{
			string AskForFolder()
			{
				//TODO maybe use another FolderDialog, but for the first, this one does the job
				using (var dlg = new CommonOpenFileDialog {IsFolderPicker = true, EnsurePathExists = true})
				{
					var result = dlg.ShowDialog();
					if (result != CommonFileDialogResult.Ok) return null;
					return dlg.FileNames.FirstOrDefault();
				}
			}

			await Task.Run(() =>
			{
				var folder = ApplicationDispatcher.Do.Run(AskForFolder);
				if (folder == null) return;

				_settingsManager.User.DefaultFolder = folder;
				_settingsManager.User.Save();

				_imageLoader.Stop();
			}).ConfigureAwait(false);
			await LoadAsync(_settingsManager.User.DefaultFolder).ConfigureAwait(false);
		}

		public void MenuFileOpenFile()
		{
			ApplicationDispatcher.Do.RunAsync(() => _shell.ShowImageViewer());
			ApplicationDispatcher.Do.RunAsync(() => ((ImageViewerViewModel) _shell.ActiveItem).MenuFileOpen());
		}
		public void MenuFileExit()
		{
			_application.Shutdown();
		}

		public void MenuWindowColorTest()
		{
			_shell.ActivateItem(new ColorTestViewModel()); //TODO Testcode
		}
		public void MenuHelpAbout()
		{
			_shell.ActivateItem(_aboutViewModel);
		}

		public void MenuViewRefresh()
		{
			
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
			//TODO ViewRefresh()
		}

		public void ShowImageViewer(ImageThumbViewModel item)
		{
			var imageViewer = _shell.ShowImageViewer();
			imageViewer.OpenImage(item.FilePath);
		}

		private Task LoadAsync(string path)
		{
			var tasks=new TaskList();

			tasks.RunOnUiThread((Action) (() => Items = null));

			var directory = new DirectoryInfo(path);
			var files = directory.GetFiles();

			var items = new List<ImageThumbViewModel>();
			foreach (var file in files)
			{
				var ext = Path.GetExtension(file.Name)?.ToLower();
				if (!_supportedExtensions.Contains(ext)) continue;
				//TODO use correct way to instantiate a ImageThumbViewModel
				//var item = _serviceLocator.GetInstance<ImageThumbViewModel>(new Dictionary<string, object> { { "Parent", this }, { "FileInfo", file } });
				var item = _serviceLocator.CreateInstanceDirect<ImageThumbViewModel>(this, file);
				//item.Setup(this, file); 
				items.Add(item);
			}
			tasks.RunOnUiThread((Action) (() => Items = new BindableCollection<ImageThumbViewModel>(items)));
			tasks.AddRange(items.Select(i => i.InitializeAsync()));

			return Task.WhenAll(tasks);
		}

		void ICustomDropTarget.OnDrop(object sender, DragEventArgs e)
		{
			var files = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (files.Length == 1)
			{
				if (Directory.Exists(files[0]))
				{
					Task.Run(() => LoadAsync(files[0]));
				}
				else if (File.Exists(files[0]))
				{
					Task.Run(() => LoadAsync(Path.GetDirectoryName(files[0])));
				}
			}
		}

		void ICustomDropTarget.OnDragEnter(object sender, DragEventArgs e)
		{

		}

		void ICustomDropTarget.OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
		{

		}

		void ICustomDropTarget.OnDragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (files.Length == 1)
				{
					e.Effects = DragDropEffects.Copy;
				}
				
			}
		}

		void ICustomDropTarget.OnDragLeave(object sender, DragEventArgs e)
		{

		}

	}
}