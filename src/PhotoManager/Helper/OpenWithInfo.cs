namespace KsWare.PhotoManager.Helper
{
	public class OpenWithInfo
	{
		public static OpenWithInfo ParseCommand(string command)
		{
			// HKEY_CLASSES_ROOT\.png\OpenWithProgids:paint.net.1
			// HKEY_CLASSES_ROOT\paint.net.1\shell\open\command
			// "C:\Program Files\paint.net\PaintDotNet.exe" "%1"

			if (string.IsNullOrWhiteSpace(command)) return null;

			string fileName =null;
			var commandLine = command;
			if (commandLine.StartsWith("\""))
			{
				var p = commandLine.IndexOf('"', 1);
				fileName= commandLine.Substring(1, p - 1);
				commandLine = commandLine.Substring(p+1);
			}
			else
			{
				var p = commandLine.IndexOf(' ');
				if (p == -1)
				{
					fileName = commandLine;
					commandLine = "";
				}
				else
				{
					fileName = commandLine.Substring(0, p);
					commandLine = commandLine.Substring(p);
				}
			}

			return new OpenWithInfo
			{
				FileName= fileName,
				Parameter = commandLine.Trim()
			};
		}

		public string FileName { get; set; }
		public string Parameter { get; set; }
		public bool IsInProcServer32 { get; set; }
	}
}