using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using KsWare.PhotoManager.Common;
using KsWare.PhotoManager.Communication;
using KsWare.PhotoManager.Tools;

namespace KsWare.PhotoManager.MyPhotoTable
{
	[Export,PartCreationPolicy(CreationPolicy.NonShared)]
	public class PhotoTableItemViewModel : PropertyChangedBase, IMessageSink
	{
		[Import] private ImageLoader _imageLoader;

		private string _displayName;
		private BitmapSource _previewImage;
		private DateTime _dateTaken;
		private string _filePath;
		private PhotoTableViewModel _photoTableViewModel;
		private FileInfo _fileInfo;
		private IMessageSink _nextSink;
		private bool _isPrioritizedLoad;
		private bool _showDisplayName;

		public PhotoTableItemViewModel()
		{
			ClickCommand = new SimpleCommand(OnClick); //TODO revise
		}

		public void Setup(PhotoTableViewModel photoTableViewModel, FileInfo file)
		{
			_photoTableViewModel = photoTableViewModel;
			_fileInfo = file;
			_filePath = file.FullName;
			_photoTableViewModel.PropertyChanged += PhotoTableViewModel_PropertyChanged;
		}

		private void PhotoTableViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(PhotoTableViewModel.ShowDisplayNames):
					ShowDisplayName = PreviewImage == null || _photoTableViewModel.ShowDisplayNames;
					break;
			}
		}

		public string FilePath { get => _filePath; set => Set(ref _filePath, value); }

		public string DisplayName { get => _displayName; set => Set(ref _displayName, value); }

		public BitmapSource PreviewImage { get => _previewImage; set => Set(ref _previewImage, value); }

		public DateTime DateTaken { get => _dateTaken; set => Set(ref _dateTaken, value); }

		public bool IsPrioritizedLoad { get => _isPrioritizedLoad; set => Set(ref _isPrioritizedLoad, value); }

		public bool ShowDisplayName { get => _showDisplayName; set => Set(ref _showDisplayName, value); }

		public ICommand ClickCommand { get; }

		private void OnClick()
		{
			PreviewImage = null;
			_imageLoader.Add(FilePath, 100, this);
		}

		private void OnImageLoaded(BitmapSource imageSource)
		{
			IsPrioritizedLoad = false;
			PreviewImage = imageSource;
			ShowDisplayName = _photoTableViewModel.ShowDisplayNames;
		}

		#region IMessageSink

		IMessage IMessageSink.SyncProcessMessage(IMessage msg)
		{
			if (msg is ImageLoadedMessage imageLoaded) { OnImageLoaded(imageLoaded.ImageSource); }

			return msg;
		}

		IMessageCtrl IMessageSink.AsyncProcessMessage(IMessage msg, IMessageSink replySink)
		{
			throw new NotImplementedException();
		}

		IMessageSink IMessageSink.NextSink => _nextSink;

		#endregion

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
					DateTaken = _fileInfo.LastWriteTime; //TODO
					DisplayName = _fileInfo.Name;
				});
			});

			_imageLoader.Add(FilePath, 256, this);
		}
	}
}
