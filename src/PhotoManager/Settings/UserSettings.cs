using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace KsWare.PhotoManager.Settings
{
	public class UserSettings
	{
		public static UserSettings Load()
		{
			UserSettings settings;
			if (!File.Exists(FileName))
			{
				settings = new UserSettings();
			}
			else
			{
				using (var sr = new StreamReader(File.OpenRead(FileName), Encoding.UTF8))
				{
					var json = sr.ReadToEnd();
					settings = JsonConvert.DeserializeObject<UserSettings>(json);
				}				
			}
			return settings;
		}

		public void Save()
		{
			Directory.CreateDirectory(FolderName);
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			using (var sw = new StreamWriter(File.Open(FileName,FileMode.Create,FileAccess.Write), Encoding.UTF8))
			{
				sw.Write(json);
			}
		}

		private static string Version => "0.9";

		private static string FolderName => Path.Combine(Environment.ExpandEnvironmentVariables(@"%localappdata%"),
			"KsWare", "PhotoManager", Version);
		private static string FileName => Path.Combine(FolderName, "User.settings");

		public string DefaultFolder { get; set; }
	}
}
