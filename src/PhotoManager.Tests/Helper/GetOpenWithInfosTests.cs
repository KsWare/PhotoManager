using KsWare.PhotoManager.Helper;
using NUnit.Framework;

namespace KsWare.PhotoManagerTests.Helper
{
	[TestFixture]
	public class GetOpenWithInfosTests
	{
		[Test]
		public void DoTest()
		{
			var info1 = GetOpenWithInfos.Do(".png");
			var info2 = GetOpenWithInfos.Do("foobar.png");
		}
	}
}
