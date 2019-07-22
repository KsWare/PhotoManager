using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using KsWare.PhotoManager.Tools;
using Action = System.Action;

namespace KsWare.PhotoManager.Extensions
{
	public class ApplicationDispatcher
	{
		public static readonly ApplicationDispatcher Instance = new ApplicationDispatcher();

		private readonly ApplicationDispatcherExtender _extender = new ApplicationDispatcherExtender();

		public ApplicationDispatcherExtender Do => _extender;
		private ApplicationDispatcher() { }

	}

	public class ApplicationDispatcherExtender
	{
		public void Run(Action action) => ApplicationWrapper.Dispatcher.Invoke(action);
		public void Run<T1>(T1 p1, Action<T1> action) => ApplicationWrapper.Dispatcher.Invoke(action, p1);
		public void Run<T1, T2>(T1 p1, T2 p2, Action<T1,T2> action) => ApplicationWrapper.Dispatcher.Invoke(action, p1, p2);

		public TResult Run<TResult>(Func<TResult> func) => ApplicationWrapper.Dispatcher.Invoke(func);
		public TResult Run<T1, TResult>(T1 p1, Func<T1, TResult> func) => ApplicationWrapper.Dispatcher.Invoke(()=>func(p1));
		public TResult Run<T1,T2, TResult>(T1 p1, T2 p2, Func<T1,T2, TResult> func) => ApplicationWrapper.Dispatcher.Invoke(() => func(p1,p2));

		public Task RunAsync(Action action) => ApplicationWrapper.Dispatcher.BeginInvoke(action).Task;
		public Task RunAsync<T1>(T1 p1, Action action) => ApplicationWrapper.Dispatcher.BeginInvoke(action, p1).Task;
		public Task RunAsync<T1,T2>(T1 p1,T2 p2, Action action) => ApplicationWrapper.Dispatcher.BeginInvoke(action, p1, p2).Task;
		
		public Task<TResult> RunAsync<TResult>(Func<TResult> func) => ApplicationWrapper.Dispatcher.InvokeAsync(func).Task;
		public Task<TResult> RunAsync<T1, TResult>(T1 p1, Func<T1, TResult> func) => ApplicationWrapper.Dispatcher.InvokeAsync(() => func(p1)).Task;
		public Task<TResult> RunAsync<T1,T2, TResult>(T1 p1, T2 p2, Func<T1,T2, TResult> func) => ApplicationWrapper.Dispatcher.InvokeAsync(() => func(p1, p2)).Task;

	}
}
