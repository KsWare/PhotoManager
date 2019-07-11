﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace KsWare.PhotoManager.Tools
{
	public static class ApplicationWrapper
	{
		public static Dispatcher Dispatcher
		{
			get
			{
				if (Application.Current != null)
				{
					return Application.Current.Dispatcher;
				}
				else
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
