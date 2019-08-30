using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace KsWare.PhotoManager.Tools
{
	[Export, PartCreationPolicy(CreationPolicy.Shared)]
	public class ImageLoader : IDisposable
	{
		private List<ImageWorker> _queue=new List<ImageWorker>();
		private List<ImageWorker> _loadQueue = new List<ImageWorker>();
		private ManualResetEvent _signal=new ManualResetEvent(false);
		private Thread _loaderThread;
		private bool _isDisposed;
		private bool _isDisposing;
		private readonly object SyncRoot=new object();

		public ImageLoader()
		{
			_loaderThread=new Thread(ImageLoaderProc){IsBackground = true, Name = "ImageLoader"};
			_loaderThread.Start();
		}

		public int MaxWorkers { get; set; } = 8;

		public ImageWorker Add(string filePath, int maxSize, IMessageSink messageSink)
		{
			lock (SyncRoot)
			{
				var w = new ImageWorker(filePath, maxSize, messageSink,this);
				_queue.Add(w);
				_signal.Set();
				return w;
			}
		}

		public void Dispose()
		{
			if(_isDisposed || _isDisposing) return;
			_isDisposing = true;
//			_queue.Dispose();
//			_loadQueue.Dispose();
//			_processQueue.Dispose();
			_isDisposed = true;
		}

		private void ImageLoaderProc()
		{
			try
			{

				lock (SyncRoot)
				{
					while (!_isDisposed && !_isDisposing)
					{
						Monitor.Exit(SyncRoot);
						_signal.WaitOne(-1, true);
						Monitor.Enter(SyncRoot);
						if (_isDisposed || _isDisposing) return;
						if (_loadQueue.Count < MaxWorkers && _queue.Count > 0)
						{
							var worker = _queue.FirstOrDefault(w => w.IsPrioritized) ?? _queue.First();
							_queue.Remove(worker);
							_loadQueue.Add(worker);
							worker.BeginLoad();
						}

						if (_queue.Count == 0 || _loadQueue.Count == MaxWorkers) _signal.Reset();
					}
				}
			}
			catch (ThreadAbortException ex) { }
			catch (Exception ex)
			{
				Debug.WriteLine("ImageLoader thread exited unexpectedly!" + "\n" + ex.StackTrace);
			}
		}

		public void OnLoaded(ImageWorker worker)
		{
			lock (SyncRoot)
			{
				_loadQueue.Remove(worker);
				worker.Dispose();
				_signal.Set();
			}
		}

		public void Prioritize(string filePath, bool value)
		{
			lock (SyncRoot)
			{
				var worker = _queue.FirstOrDefault(w => w.FilePath == filePath);
				worker?.Prioritize(value);
				var c=_queue.Count(w => w.IsPrioritized);
				c += _loadQueue.Count(w => w.IsPrioritized);
				Debug.WriteLine($"Prioritized items: {c}");
			}
		}

		public void Stop()
		{
			lock (SyncRoot)
			{
				_queue.Clear();
			}
		}
	}
}
