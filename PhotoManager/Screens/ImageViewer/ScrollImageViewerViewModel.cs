using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KsWare.CaliburnMicro.Common;
using KsWare.CaliburnMicro.Tools;
using KsWare.PhotoManager.Shell;
using KsWare.PhotoManager.Tools;
using Microsoft.Win32;

namespace KsWare.PhotoManager.Screens.ImageViewer
{
	[Export]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class ScrollImageViewerViewModel : Screen, IImageViewerViewModel
	{
		[Import(typeof(IShell))] private ShellViewModel _shell;
		[Import] private IServiceLocator _serviceLocator;

		private static readonly string[] SupportExtensions =
			ImageTools.SupportedExtensions.Select(x => x.Key.ToLower()).ToArray();


		private int _currentIndex;
		private IObservableCollection<ImageViewModel> _items = new BindableCollection<ImageViewModel>();
		private ImageViewModel _currentItem;


		public IObservableCollection<ImageViewModel> Items { get => _items; set => Set(ref _items, value);}

		public ImageViewModel SelectedItem { get => _currentItem; set => Set(ref _currentItem, value);}

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
			_currentItem = _items[_currentIndex];
			OnCurrentItemChanged();
		}

		public void GotoNextImage()
		{
			if (_currentIndex == _items.Count - 1)
				return;
			_currentIndex++;

			_currentItem = _items[_currentIndex];
			OnCurrentItemChanged();
		}

		public void OpenImage(string path) => AsyncHelper.RunSync(() => OpenImageAsync(path));

		public async Task OpenImageAsync(string path)
		{
			//TODO reuse existing folder
			var fn = Path.GetFullPath(path);
			var folder = Path.GetDirectoryName(fn);

			var files = await AsyncHelper.RunAsync(() =>
			{
				return Directory.GetFiles(folder)
					.Where(f => SupportExtensions.Contains(Path.GetExtension(f).ToLower()))
					.ToList();
			}).ConfigureAwait(true);

			_items.Clear();
			_items.AddRange(files.Select(f=>_serviceLocator.CreateInstanceDirect<ImageViewModel>(f)));
			_currentItem = _items.First(f => f.FullName.Equals(fn, StringComparison.OrdinalIgnoreCase));
			_currentIndex = _items.IndexOf(_currentItem);
			OnCurrentItemChanged();
		}

		public void MenuViewPhotoTable()
		{
			_shell.ShowPhotoTable();
		}

		private void OnCurrentItemChanged()
		{
			NotifyOfPropertyChange(nameof(SelectedItem));
		}
	}
}