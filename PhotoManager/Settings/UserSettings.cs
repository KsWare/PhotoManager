using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace KsWare.PhotoManager.Settings
{
	public class UserSettings
	{

		public static UserSettings Instance;

		public static void Save()
		{
			Directory.CreateDirectory(FolderName);
			string json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
			using (var sw = new StreamWriter(File.OpenWrite(FileName), Encoding.UTF8))
			{
				sw.Write(json);
			}
		}

		public static void Load()
		{
			if (!File.Exists(FileName))
			{
				Instance = new UserSettings();
				return;
			}

			using (var sr = new StreamReader(File.OpenRead(FileName), Encoding.UTF8))
			{
				var json = sr.ReadToEnd();
				Instance = JsonConvert.DeserializeObject<UserSettings>(json);
			}

		}

		private static string Version => "0.9";

		private static string FolderName => Path.Combine(Environment.ExpandEnvironmentVariables(@"%localappdata%"),
			"KsWare", "PhotoManager", Version);
		private static string FileName => Path.Combine(FolderName, "User.settings");

		public string DefaultFolder { get; set; }




	}
}
