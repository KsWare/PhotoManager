using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KsWare.PhotoManager.Helper;
using NUnit.Framework;

namespace KsWare.PhotoManagerTests.Tools
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
