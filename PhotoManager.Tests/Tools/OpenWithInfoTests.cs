using KsWare.PhotoManager.Tools;
using NUnit.Framework;

namespace KsWare.PhotoManagerTests.Tools
{
	[TestFixture]
	public class OpenWithInfoTests
	{
		[Test]
		public void ParseCommandTest()
		{
			var sut = OpenWithInfo.ParseCommand(@"""C:\Program Files\paint.net\PaintDotNet.exe"" ""%1""");
			Assert.That(() => sut.FileName, Is.EqualTo(@"C:\Program Files\paint.net\PaintDotNet.exe"));
			Assert.That(() => sut.Parameter, Is.EqualTo(@"""%1"""));

			sut = OpenWithInfo.ParseCommand(@"""C:\Program Files\paint.net\PaintDotNet.exe"" ""%1""");
			Assert.That(() => sut.FileName, Is.EqualTo(@"C:\Program Files\paint.net\PaintDotNet.exe"));
			Assert.That(() => sut.Parameter, Is.EqualTo(@"""%1"""));

			sut = OpenWithInfo.ParseCommand(@"C:\Programme\paint.net\PaintDotNet.exe ""%1""");
			Assert.That(() => sut.FileName, Is.EqualTo(@"C:\Programme\paint.net\PaintDotNet.exe"));
			Assert.That(() => sut.Parameter, Is.EqualTo(@"""%1"""));

			sut = OpenWithInfo.ParseCommand(@"C:\Programme\paint.net\PaintDotNet.exe %1");
			Assert.That(() => sut.FileName, Is.EqualTo(@"C:\Programme\paint.net\PaintDotNet.exe"));
			Assert.That(() => sut.Parameter, Is.EqualTo(@"%1"));

			sut = OpenWithInfo.ParseCommand(@"""C:\Program Files\paint.net\PaintDotNet.exe""");
			Assert.That(() => sut.FileName, Is.EqualTo(@"C:\Program Files\paint.net\PaintDotNet.exe"));
			Assert.That(() => sut.Parameter, Is.EqualTo(@""));

			sut = OpenWithInfo.ParseCommand(@"C:\Programme\paint.net\PaintDotNet.exe");
			Assert.That(() => sut.FileName, Is.EqualTo(@"C:\Programme\paint.net\PaintDotNet.exe"));
			Assert.That(() => sut.Parameter, Is.EqualTo(@""));
		}
	}
}
