using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using KsWare.PhotoManager.Tools;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using NUnit.Framework;

namespace KsWare.PhotoManagerTests.Tools
{
	[TestFixture]
	public class ImageToolsTests
	{
		[Test]
		public void ResizeImageTest()
		{
			//Debug.WriteLine(Environment.CurrentDirectory);
			var image = Image.FromFile(GetFullName("Resources\\1920x1200.png"));
			var bitmap = ImageTools.ResizeImage(image, 192, height: 120);
			Assert.That(bitmap.Width, Is.EqualTo(192));
			Assert.That(bitmap.Height, Is.EqualTo(120));
		}

		[Test]
		public void CreatePreviewTest()
		{
			var bitmap = ImageTools.CreatePreview(GetFullName("Resources\\1920x1200.png"), 192);
			Assert.That(bitmap.Width, Is.EqualTo(192));
			Assert.That(bitmap.Height, Is.EqualTo(120));
		}

		private string GetFullName(string relativePath)
		{
			var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			return Path.Combine(assemblyFolder, relativePath);
		}

		[Test]
		public void ShellFilePropertiesTest()
		{
			using (var file = ShellFile.FromFilePath(GetFullName("Resources\\exif.jpg")))
			{
				var dpc = file.Properties.DefaultPropertyCollection;
				foreach (var p in dpc)
				{
					Debug.WriteLine($"{p.CanonicalName,-60} {p.FormatForDisplay(PropertyDescriptionFormatOptions.None),-30} {p.ValueType.Name,-30} {p.Description.DisplayName}");
				}
			}
		}

		[Test]
		public void ShellFilePropertiesTest2()
		{
			using (var file = ShellFile.FromFilePath(@"E:\Fotos\2019-08-09 Import RX10M4\S4003017.JPG"))
			{
				var dpc = file.Properties.DefaultPropertyCollection;
				foreach (var p in dpc)
				{
					Debug.WriteLine($"{p.CanonicalName,-60} {p.FormatForDisplay(PropertyDescriptionFormatOptions.None),-30} {p.ValueType.Name,-30} {p.Description.DisplayName}");
				}
			}
		}
	}
}
