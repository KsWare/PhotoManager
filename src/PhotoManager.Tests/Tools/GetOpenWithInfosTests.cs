using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KsWare.PhotoManager.Tools;
using NUnit.Framework;

namespace KsWare.PhotoManagerTests.Tools
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
