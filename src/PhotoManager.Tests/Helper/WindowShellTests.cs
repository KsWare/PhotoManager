using KsWare.PhotoManager.Helper;
using NUnit.Framework;

namespace KsWare.PhotoManagerTests.Helper
{
	[TestFixture]
	public class WindowShellTests
	{
		[Test]
		public void GetOpenWithInfosTest()
		{
			WindowShell.GetOpenWithInfo(".png");
			WindowShell.GetOpenWithInfo("foobar.png");
		}
	}
}
