using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Input;
using KsWare.CaliburnMicro.Commands;

namespace KsWare.PhotoManager.Screens.PhotoTable.Commands
{
	[Export]
	public class OpenFolderInExplorerCommand : UserCommand<object>
	{
		public override void Do()
		{
			var folderPath = FolderPath?.Invoke();
			if (folderPath == null) return;
			if (Keyboard.Modifiers == ModifierKeys.Control)
				Process.Start("explorer.exe", $"/select, \"{folderPath}\"");
			else
				Process.Start("explorer.exe", folderPath);
		}

		protected override bool CanDo() => !string.IsNullOrWhiteSpace(FolderPath?.Invoke());
		public Func<string> FolderPath { get; set; }
	}
}
