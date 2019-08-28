using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using KsWare.CaliburnMicro.Common;
using KsWare.CaliburnMicro.Shared;
using KsWare.PhotoManager.Shell;
using KsWare.PhotoManager.Tools;

namespace KsWare.PhotoManager.Screens.ImageViewer
{
	public class ImageViewModel : PropertyChangedBase, IDataContextObserver
	{
		[Import] private ImageLoader _imageLoader;
		[Import(typeof(IShell))] private ShellViewModel _shell;

		private object _imageSource;
		private string _fileName;
		private string _fullName;

		[ImportingConstructor]
		public ImageViewModel([Import("File")] string file)
		{
			_fullName = file;
			_fileName = System.IO.Path.GetFileName(_fullName);
		}

		public object ImageSource { get => _imageSource; private set => Set(ref _imageSource, value); }
		public string FileName { get => _fileName; private set => Set(ref _fileName, value); }
		public string FullName { get => _fullName; private set => Set(ref _fullName, value); }

		void IDataContextObserver.OnDataContextAssigned()
		{
			if(ImageSource!=null) return;
			ImageSource=new Uri(_fullName);
		}

		void IDataContextObserver.OnDataContextReleased(string reason)
		{
			ImageSource = null;
		}
	}
}
