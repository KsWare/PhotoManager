using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using KsWare.PhotoManager.Common;
using KsWare.PhotoManager.Common.Commands;
using KsWare.PhotoManager.Communication;
using KsWare.PhotoManager.Shell;
using KsWare.PhotoManager.Tools;
using Microsoft.VisualBasic.FileIO;
using Action = System.Action;

namespace KsWare.PhotoManager.MyPhotoTable
{
	[Export]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class PhotoTableItemViewModel : PropertyChangedBase, IMessageSink, IChild<PhotoTableViewModel>
	{
		[Import] private ImageLoader _imageLoader;
		[Import(typeof(IShell))] private ShellViewModel _shell;
		private DateTime _dateTaken;

		private string _displayName;
		private FileInfo _fileInfo;
		private string _filePath;
		private bool _isPrioritizedLoad;
		private BitmapSource _previewImage;
		private bool _showDisplayName;
		private Point _contextMenuMousePosition;
		private bool _isSelected;
		private BindableCollection<MenuItemViewModel> _contextMenuItems = new BindableCollection<MenuItemViewModel>();

		[ImportingConstructor]
		public PhotoTableItemViewModel(
			[Import("Parent")] PhotoTableViewModel parent, 
			[Import("FileInfo")] FileInfo file)
		{
			ClickCommand = new SimpleCommand(OnClick); //TODO revise
			Parent = parent;
			_fileInfo = file;
			_filePath = file.FullName;
			Parent.PropertyChanged += PhotoTableViewModel_PropertyChanged;

			_contextMenuItems.AddRange(new []
			{
				new MenuItemViewModel("View", ContextMenuView),
				new MenuItemViewModel("Edit", ContextMenuEdit),
				new MenuItemViewModel("Edit with...", ContextMenuEditWith),
				new MenuItemViewModel("Delete", ContextMenuDelete),
				new MenuItemViewModel("View", ContextMenuView),
				new MenuItemSeparatorViewModel(),
				new MenuItemViewModel("More...",ContextMenuSystem){ToolTip="Opens the system context menu."},
			});
			/*
			 * 		<!-- 	<MenuItem Header="View" FontWeight="Bold" cal:Message.Attach="ContextMenuView"/> -->
		<!-- 	<MenuItem Header="Edit" cal:Message.Attach="ContextMenuEdit"/> -->
		<!-- 	<MenuItem Header="Edit with..." cal:Message.Attach="ContextMenuEditWith"/> -->
		<!-- 	<MenuItem Header="Delete" cal:Message.Attach="ContextMenuDelete"/> -->
		<!-- 	<Separator/> -->
		<!-- 	<MenuItem Header="More" cal:Message.Attach="ContextMenuSystem"/> -->
			 */
		}

		public string FilePath { get => _filePath; set => Set(ref _filePath, value); }

		public string DisplayName { get => _displayName; set => Set(ref _displayName, value); }

		public BitmapSource PreviewImage { get => _previewImage; set => Set(ref _previewImage, value); }

		public DateTime DateTaken { get => _dateTaken; set => Set(ref _dateTaken, value); }

		public bool IsPrioritizedLoad { get => _isPrioritizedLoad; set => Set(ref _isPrioritizedLoad, value); }

		public bool ShowDisplayName { get => _showDisplayName; set => Set(ref _showDisplayName, value); }

		public ICommand ClickCommand { get; }

		public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value);}

//		public void Setup(PhotoTableViewModel photoTableViewModel, FileInfo file)
//		{
//			Parent = photoTableViewModel;
//			_fileInfo = file;
//			_filePath = file.FullName;
//			Parent.PropertyChanged += PhotoTableViewModel_PropertyChanged;
//		}

		public void ContextMenuView()
		{
			_shell.ShowImageViewer().OpenImage(FilePath);
		}

		public void ContextMenuEdit()
		{
			var psi = new ProcessStartInfo();
			//TODO remove hard coded filename
			psi.FileName = @"C:\Program Files\Skylum\Aurora HDR 2019\Aurora HDR 2019.exe";

			if (Parent.IsMoreAsOneItemSelected)
			{
				psi.Arguments = string.Join(" ", Parent.SelectedItems.Select(i => $"\"{i.FilePath}\""));
				
			}
			else
			{
				psi.Arguments =$"\"{FilePath}\"";
			}

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

		private void OnClick()
		{
			PreviewImage = null;
			_imageLoader.Add(FilePath, 100, this);
		}

		private void OnImageLoaded(BitmapSource imageSource)
		{
			IsPrioritizedLoad = false;
			PreviewImage = imageSource;
			ShowDisplayName = Parent.ShowDisplayNames;
		}

		public void OnDataContextAssigned()
		{
			if (PreviewImage == null)
			{
				IsPrioritizedLoad = true;
				_imageLoader.Prioritize(FilePath, true);
			}
		}

		public void OnDataContextReleased(string reason)
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
				ApplicationWrapper.Dispatcher.Invoke(() =>
				{
					DateTaken = _fileInfo.LastWriteTime; //TODO use re
					DisplayName = _fileInfo.Name;
				});
			});

			_imageLoader.Add(FilePath, 256, this);
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

		public BindableCollection<MenuItemViewModel> ContextMenuItems { get => _contextMenuItems; private set => Set(ref _contextMenuItems, value);}
	}
}