using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KsWare.PhotoManager.Extensions
{
	public static class DispatcherExtension
	{
		public static DispatcherOperation BeginInvoke(this Dispatcher dispatcher, Action callback, DispatcherPriority priority = DispatcherPriority.Normal)
			=> dispatcher.BeginInvoke(callback,priority);
		
	}
}
