using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using KsWare.PhotoManager.Tools;
using Microsoft.Win32;

namespace KsWare.PhotoManager.MyImageViewer
{
	[Export,PartCreationPolicy(CreationPolicy.NonShared)]
	public class ImageViewerViewModel : PropertyChangedBase
	{
		private string _currentFilePath;
		private List<string> _files;
		private int _currentIndex;
		private object _imageSource;
		private string _fileName;
		private string _fullName;

		public void MenuFileOpen()
		{
			var filter = "*" + string.Join("; *", ImageTools.SupportedExtensions.Select(x => x.Key));
			var supportExtensions = ImageTools.SupportedExtensions.Select(x => x.Key.ToLower()).ToArray();
			var dlg=new OpenFileDialog { 
				Filter = $"Supported image files|{filter}",
				FilterIndex = 1
			};
			if(dlg.ShowDialog()!=true) return;

			_currentFilePath = Path.GetFullPath(dlg.FileName);
			var folder = Path.GetDirectoryName(_currentFilePath);
			_files = Directory.GetFiles(folder)
				.Where(f=> supportExtensions.Contains(Path.GetExtension(f).ToLower()))
					.ToList();
			_currentFilePath = _files.First(f => f.Equals(_currentFilePath, StringComparison.OrdinalIgnoreCase));
			_currentIndex = _files.IndexOf(_currentFilePath);
			OnCurrentFileChanged();
		}

		public object ImageSource { get => _imageSource; set => Set(ref _imageSource, value);}

		public void GotoPreviousImage()
		{
			if (_currentIndex == 0)
			{
				return;
			}
			else
			{
				_currentIndex--;
			}
			_currentFilePath = _files[_currentIndex];
			OnCurrentFileChanged();

		}
		public void GotoNextImage()
		{
			if (_currentIndex == _files.Count-1)
			{
				return;
			}
			else
			{
				_currentIndex++;
			}

			_currentFilePath = _files[_currentIndex];
			OnCurrentFileChanged();
		}

		private void OnCurrentFileChanged()
		{
			ImageSource = new Uri(_currentFilePath);
			FileName = Path.GetFileName(_currentFilePath);
			FullName = _currentFilePath;
		}
		
		public string FileName { get => _fileName; set => Set(ref _fileName, value);}
		public string FullName { get => _fullName; set => Set(ref _fullName, value);}
	}
}
