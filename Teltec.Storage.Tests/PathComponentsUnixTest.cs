using Microsoft.VisualStudio.TestTools.UnitTesting;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Tests
{
	[TestClass]
	public class PathComponentsUnixTest
	{
		[TestMethod]
		public void TestParse()
		{
			PathComponents path = new PathComponents("/home/johndoe/Desktop/TODO.txt");

			Assert.IsTrue(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual("/home", path.Drive);
			CollectionAssert.AreEqual(new string[] { "johndoe", "Desktop" }, path.Directories);
			Assert.AreEqual("TODO.txt", path.FileName);
			Assert.AreEqual("TODO", path.FileNameWithoutExtension);
			Assert.AreEqual("txt", path.Extension);
		}
	}
}
