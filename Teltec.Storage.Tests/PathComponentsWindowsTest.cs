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

			Assert.AreEqual(path.Drive, "C");
			CollectionAssert.AreEqual(path.Directories, new string[] { "Users", "johndoe", "Desktop" });
			Assert.AreEqual(path.FileName, "TODO.txt");
			Assert.AreEqual(path.FileNameWithoutExtension, "TODO");
			Assert.AreEqual(path.Extension, "txt");
		}

		[TestMethod]
		public void TestParseWithoutDirectories()
		{
			PathComponents path = new PathComponents(@"C:\TODO.txt");

			Assert.IsTrue(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual(path.Drive, "C");
			CollectionAssert.AreEqual(path.Directories, null);
			Assert.AreEqual(path.FileName, "TODO.txt");
		}

		[TestMethod]
		public void TestParseWithoutFilenameButWithExtension()
		{
			PathComponents path = new PathComponents(@"C:\.txt");

			Assert.IsTrue(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual(path.Drive, "C");
			CollectionAssert.AreEqual(path.Directories, null);
			Assert.AreEqual(path.FileName, ".txt");
			Assert.AreEqual(path.FileNameWithoutExtension, string.Empty);
			Assert.AreEqual(path.Extension, "txt");
		}

		[TestMethod]
		public void TestParseWithoutFilename()
		{
			PathComponents path = new PathComponents(@"C:\Users\johndoe\Desktop\");

			Assert.IsTrue(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsFalse(path.HasFileName);
			Assert.IsFalse(path.HasExtension);

			Assert.AreEqual(path.Drive, "C");
			CollectionAssert.AreEqual(path.Directories, new string[] { "Users", "johndoe", "Desktop" });
			Assert.AreEqual(path.FileName, string.Empty);
			Assert.AreEqual(path.FileNameWithoutExtension, string.Empty);
			Assert.AreEqual(path.Extension, string.Empty);
		}

		[TestMethod]
		public void TestParseRelative()
		{
			PathComponents path = new PathComponents(@"Users\johndoe\Desktop\TODO.txt");

			Assert.IsFalse(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual(path.Drive, string.Empty);
			CollectionAssert.AreEqual(path.Directories, new string[] { "Users", "johndoe", "Desktop" });
			Assert.AreEqual(path.FileName, "TODO.txt");
			Assert.AreEqual(path.FileNameWithoutExtension, "TODO");
			Assert.AreEqual(path.Extension, "txt");
		}

		[TestMethod]
		public void TestParseRelativeWithoutFilename()
		{
			PathComponents path = new PathComponents(@"Users\johndoe\Desktop\");

			Assert.IsFalse(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsFalse(path.HasFileName);
			Assert.IsFalse(path.HasExtension);

			Assert.AreEqual(path.Drive, string.Empty);
			CollectionAssert.AreEqual(path.Directories, new string[] { "Users", "johndoe", "Desktop" });
			Assert.AreEqual(path.FileName, string.Empty);
			Assert.AreEqual(path.FileNameWithoutExtension, string.Empty);
			Assert.AreEqual(path.Extension, string.Empty);
		}

		[TestMethod]
		public void TestParseFilenameAlone()
		{
			PathComponents path = new PathComponents(@"TODO.txt");

			Assert.IsFalse(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual(path.Drive, string.Empty);
			CollectionAssert.AreEqual(path.Directories, null);
			Assert.AreEqual(path.FileName, "TODO.txt");
			Assert.AreEqual(path.FileNameWithoutExtension, "TODO");
			Assert.AreEqual(path.Extension, "txt");
		}

		[TestMethod]
		public void TestParseDriveAlone()
		{
			PathComponents path = new PathComponents(@"C:\");

			Assert.IsTrue(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsFalse(path.HasFileName);
			Assert.IsFalse(path.HasExtension);

			Assert.AreEqual(path.Drive, "C");
			CollectionAssert.AreEqual(path.Directories, null);
			Assert.AreEqual(path.FileName, string.Empty);
			Assert.AreEqual(path.FileNameWithoutExtension, string.Empty);
			Assert.AreEqual(path.Extension, string.Empty);
		}

		[TestMethod]
		public void TestParseDriveAloneWithoutSeparator()
		{
			PathComponents path = new PathComponents(@"C:");

			Assert.IsTrue(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsFalse(path.HasFileName);
			Assert.IsFalse(path.HasExtension);

			Assert.AreEqual(path.Drive, "C");
			CollectionAssert.AreEqual(path.Directories, null);
			Assert.AreEqual(path.FileName, string.Empty);
			Assert.AreEqual(path.FileNameWithoutExtension, string.Empty);
			Assert.AreEqual(path.Extension, string.Empty);
		}
	}
}
