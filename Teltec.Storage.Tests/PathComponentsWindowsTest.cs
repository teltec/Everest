using Microsoft.VisualStudio.TestTools.UnitTesting;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Tests
{
	[TestClass]
	public class PathComponentsWindowsTest
	{
		[TestMethod]
		public void TestParse()
		{
			PathComponents path = new PathComponents(@"C:\Users\johndoe\Desktop\TODO.txt");

			Assert.IsTrue(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual("C", path.Drive);
			CollectionAssert.AreEqual(new string[] { "Users", "johndoe", "Desktop" }, path.Directories);
			Assert.AreEqual("TODO.txt", path.FileName);
			Assert.AreEqual("TODO", path.FileNameWithoutExtension);
			Assert.AreEqual("txt", path.Extension);
		}

		[TestMethod]
		public void TestParseWithoutDirectories()
		{
			PathComponents path = new PathComponents(@"C:\TODO.txt");

			Assert.IsTrue(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual("C", path.Drive);
			CollectionAssert.AreEqual(null, path.Directories);
			Assert.AreEqual("TODO.txt", path.FileName);
		}

		[TestMethod]
		public void TestParseWithoutFilenameButWithExtension()
		{
			PathComponents path = new PathComponents(@"C:\.txt");

			Assert.IsTrue(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual("C", path.Drive);
			CollectionAssert.AreEqual(null, path.Directories);
			Assert.AreEqual(".txt", path.FileName);
			Assert.AreEqual(string.Empty, path.FileNameWithoutExtension);
			Assert.AreEqual("txt", path.Extension);
		}

		[TestMethod]
		public void TestParseWithoutFilename()
		{
			PathComponents path = new PathComponents(@"C:\Users\johndoe\Desktop\");

			Assert.IsTrue(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsFalse(path.HasFileName);
			Assert.IsFalse(path.HasExtension);

			Assert.AreEqual("C", path.Drive);
			CollectionAssert.AreEqual(new string[] { "Users", "johndoe", "Desktop" }, path.Directories);
			Assert.AreEqual(string.Empty, path.FileName);
			Assert.AreEqual(string.Empty, path.FileNameWithoutExtension);
			Assert.AreEqual(string.Empty, path.Extension);
		}

		[TestMethod]
		public void TestParseRelative()
		{
			PathComponents path = new PathComponents(@"Users\johndoe\Desktop\TODO.txt");

			Assert.IsFalse(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual(string.Empty, path.Drive);
			CollectionAssert.AreEqual(new string[] { "Users", "johndoe", "Desktop" }, path.Directories);
			Assert.AreEqual("TODO.txt", path.FileName);
			Assert.AreEqual("TODO", path.FileNameWithoutExtension);
			Assert.AreEqual("txt", path.Extension);
		}

		[TestMethod]
		public void TestParseRelativeWithoutFilename()
		{
			PathComponents path = new PathComponents(@"Users\johndoe\Desktop\");

			Assert.IsFalse(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsFalse(path.HasFileName);
			Assert.IsFalse(path.HasExtension);

			Assert.AreEqual(string.Empty, path.Drive);
			CollectionAssert.AreEqual(new string[] { "Users", "johndoe", "Desktop" }, path.Directories);
			Assert.AreEqual(string.Empty, path.FileName);
			Assert.AreEqual(string.Empty, path.FileNameWithoutExtension);
			Assert.AreEqual(string.Empty, path.Extension);
		}

		[TestMethod]
		public void TestParseFilenameAlone()
		{
			PathComponents path = new PathComponents(@"TODO.txt");

			Assert.IsFalse(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual(string.Empty, path.Drive);
			CollectionAssert.AreEqual(null, path.Directories);
			Assert.AreEqual("TODO.txt", path.FileName);
			Assert.AreEqual("TODO", path.FileNameWithoutExtension);
			Assert.AreEqual("txt", path.Extension);
		}

		[TestMethod]
		public void TestParseDriveAlone()
		{
			PathComponents path = new PathComponents(@"C:\");

			Assert.IsTrue(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsFalse(path.HasFileName);
			Assert.IsFalse(path.HasExtension);

			Assert.AreEqual("C", path.Drive);
			CollectionAssert.AreEqual(null, path.Directories);
			Assert.AreEqual(string.Empty, path.FileName);
			Assert.AreEqual(string.Empty, path.FileNameWithoutExtension);
			Assert.AreEqual(string.Empty, path.Extension);
		}

		[TestMethod]
		public void TestParseDriveAloneWithoutSeparator()
		{
			PathComponents path = new PathComponents(@"C:");

			Assert.IsTrue(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsFalse(path.HasFileName);
			Assert.IsFalse(path.HasExtension);

			Assert.AreEqual("C", path.Drive);
			CollectionAssert.AreEqual(null, path.Directories);
			Assert.AreEqual(string.Empty, path.FileName);
			Assert.AreEqual(string.Empty, path.FileNameWithoutExtension);
			Assert.AreEqual(string.Empty, path.Extension);
		}
	}
}
