using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace KsWare.PhotoManager.Tools
{
	public static class GetOpenWithInfos
	{
		public static IEnumerable<OpenWithInfo> Do(string fileName)
		{
			var list = new List<OpenWithInfo>();
			var ext = Path.GetExtension(fileName);
			var defaultId = Registry.GetValue($@"HKEY_CLASSES_ROOT\{ext}", "", null) as string;
			// Photoshop.PNGFile.9
			ReadProgId(defaultId, list);


			var extensionKey = Registry.ClassesRoot.OpenSubKey(ext);
			ReadOpenWithList(extensionKey, list);
			OpenWithProgids(extensionKey,list);
			return list;
		}

		private static void OpenWithProgids(RegistryKey extensionKey, List<OpenWithInfo> results)
		{
			var key = extensionKey.OpenSubKey("OpenWithProgids");
			var valueNames = key.GetValueNames();
			foreach (var valueName in valueNames)
			{
				ReadProgId(valueName, results);
			}
		}

		private static void ReadProgId(string progId, List<OpenWithInfo> results)
		{
			if(string.IsNullOrWhiteSpace(progId)) return;
			if (progId.StartsWith("App"))
			{

			}
			else
			{
				
			}

			var name = Registry.GetValue($@"HKEY_CLASSES_ROOT\{progId}", "", null);
			var openCommand = Registry.GetValue($@"HKEY_CLASSES_ROOT\{progId}\shell\open\command", "", null) as string;
			// "C:\Program Files\paint.net\PaintDotNet.exe" "%1"
			if (!string.IsNullOrEmpty(openCommand))
			{
				results.Add(OpenWithInfo.ParseCommand(openCommand));
				return;
			}

			var delegateExecute = Registry.GetValue($@"HKEY_CLASSES_ROOT\{progId}\shell\open\command", "DelegateExecute", null) as string;
			if (!string.IsNullOrEmpty(delegateExecute))
			{
				var inProcServer32 = Registry.GetValue($@"HKEY_CLASSES_ROOT\CLSID\{delegateExecute}\InProcServer32", "", null) as string;
				// %SystemRoot%\system32\twinui.dll
				if (!string.IsNullOrEmpty(inProcServer32))
				{
					results.Add(new OpenWithInfo
					{
						FileName = inProcServer32,
						IsInProcServer32 = true
					});
					return;
				}

			}
		}

		private static void ReadOpenWithList(RegistryKey extensionKey, List<OpenWithInfo> results)
		{
			//TODO implement ReadOpenWithList
		}
	}
}