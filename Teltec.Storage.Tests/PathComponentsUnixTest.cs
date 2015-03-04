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

			Assert.AreEqual(path.Drive, "/home");
			CollectionAssert.AreEqual(path.Directories, new string[] { "johndoe", "Desktop" });
			Assert.AreEqual(path.FileName, "TODO.txt");
			Assert.AreEqual(path.FileNameWithoutExtension, "TODO");
			Assert.AreEqual(path.Extension, "txt");
		}
	}
}
