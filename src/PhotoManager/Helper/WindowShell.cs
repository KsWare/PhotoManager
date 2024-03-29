﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using KsWare.Presentation.StaticWrapper;
using Point = System.Windows.Point;

namespace KsWare.PhotoManager.Helper
{
	public static class WindowShell
	{
		private static IApplication ApplicationWrapper => KsWare.Presentation.StaticWrapper.AssemblyBootstrapper.ApplicationWrapper;
		public static void ShowOpenWithDialog(string path)
		{
			var args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
			args += ",OpenAs_RunDLL " + path;
			Process.Start("rundll32.exe", args);
		}

		public static IEnumerable<OpenWithInfo> GetOpenWithInfo(string fileName) =>
			GetOpenWithInfos.Do(fileName);

		public static void OpenContextMenu(string filePath, Point? position) => 
			OpenContextMenu(new[]{new FileInfo(filePath)}, position);

		public static void OpenContextMenu(IEnumerable<string> filePaths, Point? position) => 
			OpenContextMenu(filePaths.Select(f => new FileInfo(f)).ToArray(), position);

		public static void OpenContextMenu(IList<FileInfo> files, Point? position)
		{
			var windowHandle = ApplicationWrapper.Get.MainWindowHandle;
			var scm = new ShellContextMenu();
			scm.ShowContextMenu(files, position.HasValue ? position.Value : GetMousePosition());
		}


		public static Point GetMousePosition()
		{
			var window = ApplicationWrapper.MainWindow;
			return window.PointToScreen(Mouse.GetPosition(window));
		}
	}
}
