using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using KsWare.CaliburnMicro.DragDrop;
using KsWare.CaliburnMicro.Shared;
using KsWare.PhotoManager.Shell;
using KsWare.PhotoManager.Tools;
using Microsoft.Win32;

namespace KsWare.PhotoManager.Screens.ImageViewer
{
	[Export]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class ImageViewerViewModel : Screen, ICustomDropTarget
	{
		[Import(typeof(IShell))] private ShellViewModel _shell;

		private static readonly string[] SupportExtensions =
			ImageTools.SupportedExtensions.Select(x => x.Key.ToLower()).ToArray();


		private string _currentFilePath;
		private int _currentIndex;
		private string _fileName;
		private List<string> _files;
		private string _fullName;
		private object _imageSource;

		public object ImageSource { get => _imageSource; set => Set(ref _imageSource, value); }
		public string FileName { get => _fileName; set => Set(ref _fileName, value); }
		public string FullName { get => _fullName; set => Set(ref _fullName, value); }

		public void MenuFileOpen()
		{
			var filter = "*" + string.Join("; *", ImageTools.SupportedExtensions.Select(x => x.Key));

			var dlg = new OpenFileDialog
			{
				Filter = $"Supported image files|{filter}",
				FilterIndex = 1
			};
			if (dlg.ShowDialog() != true) return;

			OpenImage(Path.GetFullPath(dlg.FileName));
		}

		public void GotoPreviousImage()
		{
			if (_currentIndex == 0)
				return;
			_currentIndex--;
			_currentFilePath = _files[_currentIndex];
			OnCurrentFileChanged();
		}

		public void GotoNextImage()
		{
			if (_currentIndex == _files.Count - 1)
				return;
			_currentIndex++;

			_currentFilePath = _files[_currentIndex];
			OnCurrentFileChanged();
		}

		public void OpenImage(string path)
		{
			//TODO reuse existing folder
			_currentFilePath = Path.GetFullPath(path);
			var folder = Path.GetDirectoryName(_currentFilePath);
			_files = Directory.GetFiles(folder)
				.Where(f => SupportExtensions.Contains(Path.GetExtension(f).ToLower()))
				.ToList();
			_currentFilePath = _files.First(f => f.Equals(_currentFilePath, StringComparison.OrdinalIgnoreCase));
			_currentIndex = _files.IndexOf(_currentFilePath);
			OnCurrentFileChanged();
		}

		public void MenuViewPhotoTable()
		{
			_shell.ShowPhotoTable();
		}

		private void OnCurrentFileChanged()
		{
			ImageSource = new Uri(_currentFilePath);
			FileName = Path.GetFileName(_currentFilePath);
			FullName = _currentFilePath;
		}

		void ICustomDropTarget.OnDrop(object sender, DragEventArgs e)
		{
			var files = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (files.Length == 1)
			{
				if (Directory.Exists(files[0]))
				{

					foreach (var file in Directory.EnumerateFiles(files[0]))
					{
						if (SupportExtensions.Contains(Path.GetExtension(file)))
						{
							Task.Run(() => OpenImage(file));
							break;
						}
					}
				}
				else if (File.Exists(files[0]))
				{
					Task.Run(() => OpenImage(files[0]));
				}
			}
			else
			{
				//TODO open multiple files
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
					// TODO check supported file
					e.Effects = DragDropEffects.Copy;
				}
				else
				{
					// TODO support multiple files
				}

			}
		}

		void ICustomDropTarget.OnDragLeave(object sender, DragEventArgs e)
		{

		}
	}
}