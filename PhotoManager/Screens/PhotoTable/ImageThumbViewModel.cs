using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using KsWare.PhotoManager.Common;
using KsWare.PhotoManager.Common.Commands;
using KsWare.PhotoManager.Communication;
using KsWare.PhotoManager.Shell;
using KsWare.PhotoManager.Tools;
using Microsoft.VisualBasic.FileIO;

namespace KsWare.PhotoManager.Screens.PhotoTable
{
	[Export]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class ImageThumbViewModel : PropertyChangedBase, IMessageSink, IChild<PhotoTableViewModel>, IDataContextObserver
	{
		[Import] private ImageLoader _imageLoader;
		[Import(typeof(IShell))] private ShellViewModel _shell;

		private DateTime _dateTaken;
		private string _displayName;
		private System.IO.FileInfo _fileInfo;
		private string _filePath;
		private bool _isPrioritizedLoad;
		private BitmapSource _previewImage;
		private bool _showDisplayName;
		private Point _contextMenuMousePosition;
		private bool _isSelected;
		private Lazy<BindableCollection<MenuItemViewModel>> _contextMenuItems;

		[ImportingConstructor]
		public ImageThumbViewModel(
			[Import("Parent")] PhotoTableViewModel parent, 
			[Import("FileInfo")] System.IO.FileInfo file)
		{
			Parent = parent;
			_fileInfo = file;
			_filePath = file.FullName;
			Parent.PropertyChanged += PhotoTableViewModel_PropertyChanged;
			_contextMenuItems=new Lazy<BindableCollection<MenuItemViewModel>>(() =>
			{
				var items = new BindableCollection<MenuItemViewModel>();
				InitContextMenu();
				return items;
			});
		}

		public string FilePath { get => _filePath; set => Set(ref _filePath, value); }

		public string DisplayName { get => _displayName; set => Set(ref _displayName, value); }

		public BitmapSource PreviewImage { get => _previewImage; set => Set(ref _previewImage, value); }

		public DateTime DateTaken { get => _dateTaken; set => Set(ref _dateTaken, value); }

		public bool IsPrioritizedLoad { get => _isPrioritizedLoad; set => Set(ref _isPrioritizedLoad, value); }

		public bool ShowDisplayName { get => _showDisplayName; set => Set(ref _showDisplayName, value); }

		public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value);}

//		public void Setup(PhotoTableViewModel photoTableViewModel, FileInfo file)
//		{
//			Parent = photoTableViewModel;
//			_fileInfo = file;
//			_filePath = file.FullName;
//			Parent.PropertyChanged += PhotoTableViewModel_PropertyChanged;
//		}

		public Task ContextMenuView()
		{
			return _shell.ShowImageViewer().OpenImageAsync(FilePath);
		}

		public void ContextMenuEdit()
		{
			var psi = new ProcessStartInfo();
			//TODO remove hard coded filename
			psi.FileName = @"C:\Program Files\Skylum\Aurora HDR 2019\Aurora HDR 2019.exe";

			psi.Arguments = Parent.IsMoreAsOneItemSelected 
				? string.Join(" ", Parent.SelectedItems.Select(i => $"\"{i.FilePath}\"")) 
				: $"\"{FilePath}\"";

			Process.Start(psi);
		}

		public void ContextMenuEditWith()
		{
			WindowShell.ShowOpenWithDialog(FilePath);
		}
		public void ContextMenuEditRename()
		{
			//TODO implement ContextMenuEditRename
		}

		public void ContextMenuDelete()
		{
			if (Parent.IsMoreAsOneItemSelected)
			{
				foreach (var item in Parent.SelectedItems.ToArray())
				{
					FileSystem.DeleteFile(item.FilePath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
					Parent.SelectedItems.Remove(item);
					Parent.Items.Remove(item);
				}
			}
		}

		public void ContextMenuSystem()
		{
			WindowShell.OpenContextMenu(Parent.SelectedItems.Select(i=>i.FilePath).ToList(), _contextMenuMousePosition);
		}

		public void OnContextMenuOpened()
		{
			_contextMenuMousePosition = WindowShell.GetMousePosition();
		}

		private void PhotoTableViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(PhotoTableViewModel.ShowDisplayNames):
					ShowDisplayName = PreviewImage == null || Parent.ShowDisplayNames;
					break;
			}
		}

		private void OnImageLoaded(BitmapSource imageSource)
		{
			IsPrioritizedLoad = false;
			PreviewImage = imageSource;
			ShowDisplayName = Parent.ShowDisplayNames;
		}

		void IDataContextObserver.OnDataContextAssigned()
		{
			if (PreviewImage == null)
			{
				IsPrioritizedLoad = true;
				_imageLoader.Prioritize(FilePath, true);
			}
		}

		void IDataContextObserver.OnDataContextReleased(string reason)
		{
			if (PreviewImage == null)
			{
				IsPrioritizedLoad = false;
				_imageLoader.Prioritize(FilePath, false);
			}
		}

		public void BeginInitialize()
		{
			Task.Run(() =>
			{
				var date = _fileInfo.LastWriteTime; //TODO use correct date
				ApplicationWrapper.Dispatcher.Invoke(() =>
				{
					DateTaken = date; 
					DisplayName = _fileInfo.Name;
				});
			});

		}

		public Task InitializeAsync()
		{
			return Task.Run(Function);

			void Function()
			{
				_imageLoader.Add(FilePath, 256, this);
				var date = _fileInfo.LastWriteTime; //TODO use correct date
				OnUIThread(() =>
				{
					DateTaken = date;
					DisplayName = _fileInfo.Name;
				});
			}
		}

		#region IMessageSink

		IMessage IMessageSink.SyncProcessMessage(IMessage msg)
		{
			if (msg is ImageLoadedMessage imageLoaded) OnImageLoaded(imageLoaded.ImageSource);

			return msg;
		}

		IMessageCtrl IMessageSink.AsyncProcessMessage(IMessage msg, IMessageSink replySink)
		{
			throw new NotImplementedException();
		}

		IMessageSink IMessageSink.NextSink { get; }

		#endregion

		object IChild.Parent { get => Parent; set => Parent = (PhotoTableViewModel) value; }

		public PhotoTableViewModel Parent { get; set; }

		public BindableCollection<MenuItemViewModel> ContextMenuItems => _contextMenuItems.Value;

		private void InitContextMenu()
		{
			ContextMenuItems.Clear();
			ContextMenuItems.AddRange(new[]
			{
				new MenuItemViewModel("View", ContextMenuView),
				new MenuItemViewModel("Edit", ContextMenuEdit),
				new MenuItemViewModel("Edit with...", ContextMenuEditWith),
				new MenuItemViewModel("Delete", ContextMenuDelete),
				new MenuItemViewModel("View", ContextMenuView),
				new MenuItemSeparatorViewModel(),
				new MenuItemViewModel("More...", ContextMenuSystem) {ToolTip = "Opens the system context menu."},
			});
		}

	}
}