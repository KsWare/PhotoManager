using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using KsWare.PhotoManager.Communication;
using KsWare.Presentation.StaticWrapper;
using Path = System.IO.Path;

namespace KsWare.PhotoManager.Helper
{
	public class ImageWorker : IDisposable
	{
		private IApplicationDispatcher ApplicationDispatcher => KsWare.Presentation.StaticWrapper.AssemblyBootstrapper.ApplicationDispatcher;

		private readonly int _maxSize;
		private readonly IMessageSink _messageSink;
		private Thread _thread;
		private bool _disposing;
		private bool _disposed;
		private ImageLoader _imageLoader;

		//TODO revise
		public ImageWorker(string filePath, int maxSize, IMessageSink messageSink, ImageLoader imageLoader)
		{
			FilePath = filePath;
			_maxSize = maxSize;
			_messageSink = messageSink;
			_imageLoader = imageLoader;
		}

		public bool IsPrioritized { get; private set; }
		public string FilePath { get; }

		public void BeginLoad()
		{
			_thread=new Thread(LoadProc){Name = "ImageWorker", IsBackground = true, Priority = ThreadPriority.BelowNormal};
			_thread.Start();
		}

		private void LoadProc()
		{
			try
			{
				var cacheFilePath = GetCacheFilePath(FilePath, _maxSize, ".jpg");
				if (File.Exists(cacheFilePath))
				{
					var imageSource = ImageHelper.CreateBitmapSource(cacheFilePath);
					var msg = new ImageLoadedMessage(imageSource);
					ApplicationDispatcher.Do.Invoke(new Func<IMessage, IMessage>(_messageSink.SyncProcessMessage), msg);
				}
				else
				{
					var previewImage = ImageHelper.CreatePreview(FilePath, _maxSize);
					previewImage.SaveAsJpeg(cacheFilePath,75);
					Debug.WriteLine($"Preview Image created ({previewImage.Width}x{previewImage.Height} {previewImage.PixelFormat})");
					var imageSource = ImageHelper.CreateBitmapSource2(previewImage);
					var msg = new ImageLoadedMessage(imageSource);
					ApplicationDispatcher.Do.Invoke(new Func<IMessage, IMessage>(_messageSink.SyncProcessMessage), msg);					
				}
			}
			catch (OutOfMemoryException ex)
			{
				Debug.WriteLine("ImageWorker: Out of memory!");
			}
			catch (ThreadAbortException ex){
					
			}
			catch (Exception ex){
				Debug.WriteLine("ImageWorker thread exited unexpectedly!" + "\n" + ex.StackTrace);
			}
			finally
			{
				_imageLoader.OnLoaded(this);
			}
		}

		private string GetCacheFilePath(string filePath, int size, string newExtension)
		{
			var d = Path.GetDirectoryName(filePath);
			var n = Path.GetFileNameWithoutExtension(filePath);
			var d1 = Path.Combine(d, ".thumbs");
			Directory.CreateDirectory(d1);
			var f1 = Path.Combine(d1, $"{n}.{size}{newExtension}");
			return f1;
		}

		public void Dispose()
		{
			if(_disposing||_disposed) return;
			_disposing = true;
			
			_disposed = true;
			_disposing = false;
		}

		public void Prioritize(bool value)
		{
			IsPrioritized = value;
		}


	}
}