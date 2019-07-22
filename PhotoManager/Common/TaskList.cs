using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KsWare.PhotoManager.Extensions;

namespace KsWare.PhotoManager.Common
{
	internal class TaskList : List<Task>
	{
		private readonly ApplicationDispatcherExtender UiThread = ApplicationDispatcher.Instance.Do;

		public void RunOnUiThread(Action action) => 
			Add(UiThread.RunAsync(action));
		public void RunOnUiThread<T1>(T1 p1, Action action) => 
			Add(UiThread.RunAsync(p1, action));

		public void RunOnUiThread<T1, T2>(T1 p1, T2 p2, Action action) =>
			Add(UiThread.RunAsync(p1, p2, action));

		public Task<TResult> Add<TResult>(Task<TResult> task)
		{
			base.Add(task);
			return task;
		}

		public Task<TResult> RunOnUiThread<TResult>(Func<TResult> func) => 
			Add(UiThread.RunAsync(func));

		public Task<TResult> RunOnUiThread<T1, TResult>(T1 p1, Func<T1, TResult> func) =>
			Add(UiThread.RunAsync(p1, func));

		public Task<TResult> RunOnUiThread<T1, T2, TResult>(T1 p1, T2 p2, Func<T1, T2, TResult> func) =>
			Add(UiThread.RunAsync(p1, p2, func));
	}
}