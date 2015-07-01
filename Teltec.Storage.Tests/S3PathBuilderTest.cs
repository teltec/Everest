using Microsoft.VisualStudio.TestTools.UnitTesting;
using Teltec.Storage.Implementations.S3;

namespace Teltec.Storage.Tests
{
	[TestClass]
	public class S3PathBuilderTest
	{
		[TestMethod]
		public void TestBuildLocalPath()
		{
			S3PathBuilder pathBuilder = new S3PathBuilder();
			pathBuilder.RemoteRootDirectory = "TELTEC_BKP/HOSTNAME";
			pathBuilder.LocalRootDirectory = null;

			string version = null;
			string localPath = null;

			localPath = pathBuilder.BuildLocalPath("TELTEC_BKP/HOSTNAME/c:/teste/a.txt:/20150701143445/a.txt", out version);
			Assert.AreEqual(localPath, @"c:\teste\a.txt");
			Assert.AreEqual(version, @"20150701143445");

			localPath = pathBuilder.BuildLocalPath("TELTEC_BKP/HOSTNAME/c:/a.txt:/20150701143446/a.txt", out version);
			Assert.AreEqual(localPath, @"c:\a.txt");
			Assert.AreEqual(version, @"20150701143446");

			localPath = pathBuilder.BuildLocalPath("TELTEC_BKP/HOSTNAME/c:/teste/sub_teste/a.txt:/20150701143447/a.txt", out version);
			Assert.AreEqual(localPath, @"c:\teste\sub_teste\a.txt");
			Assert.AreEqual(version, @"20150701143447");
		}
	}
}
