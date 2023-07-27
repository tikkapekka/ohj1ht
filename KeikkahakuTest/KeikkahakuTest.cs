using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Transactions;
using System.Globalization;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net.Http;
using NUnit.Framework;
using static Keikkahaku;

	[TestFixture]
	[DefaultFloatingPointTolerance(0.000001)]
	public  class TestKeikkahaku
	{
		[Test]
		public  void TestTeeTaulu124()
		{
			List<List<string>> listat = new(){ new List<string> { "Yö", "Dingo", "Popeda" },
			new List<string> { "Pori", "myös Pori", "Tampere" }};
			string[,] taulu = TeeTaulu(listat);
			Assert.AreEqual( "Tampere", taulu[2,1] , "in method TeeTaulu, line 128");
			Assert.AreEqual( new string[,]{{ "Yö", "Pori" }, { "Dingo", "myös Pori" }, { "Popeda", "Tampere" }}, taulu , "in method TeeTaulu, line 129");
			List<List<string>> listat2 = new(){ new List<string> { "A", "B", "C", "D" },
			new List<string> { "E", "F", "G", "H" },
			new List<string> { "I", "J", "K", "L" }};
			string[,] taulu2 = TeeTaulu(listat2);
			Assert.AreEqual( new string[,]{{ "A", "E", "I" }, { "B", "F", "J" }, { "C", "G", "K" }, { "D", "H", "L" }}, taulu2 , "in method TeeTaulu, line 134");
			listat2.Add(new List<string> { "1", "2" });
			try
			{
			TeeTaulu(listat2);
			Assert.Fail("Did not throw System.ArgumentOutOfRangeException in method TeeTaulu on line 135");
			}
			catch (System.ArgumentOutOfRangeException)
			{
			}
		}
	}

