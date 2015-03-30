using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Teltec.FileSystem.Tests
{
	[TestClass]
	public class PathNodesWindowsTest
	{
		[TestMethod]
		public void TestPathNodesParent()
		{
			PathNodes nodes1 = new PathNodes(@"C:");
			CollectionAssert.AreEqual(new string[] { }, nodes1.Parents.Select(p => p.Path).ToArray());

			PathNodes nodes2 = new PathNodes(@"C:\");
			CollectionAssert.AreEqual(new string[] { }, nodes2.Parents.Select(p => p.Path).ToArray());
			
			PathNodes nodes3 = new PathNodes(@"C:\.txt");
			CollectionAssert.AreEqual(new string[] { @"C:\" }, nodes3.Parents.Select(p => p.Path).ToArray());
			
			PathNodes nodes4 = new PathNodes(@"C:\TODO.txt");
			CollectionAssert.AreEqual(new string[] { @"C:\" }, nodes4.Parents.Select(p => p.Path).ToArray());
			
			PathNodes nodes5 = new PathNodes(@"C:\Users\TODO.txt");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\Users\" }, nodes5.Parents.Select(p => p.Path).ToArray());
			
			PathNodes nodes6 = new PathNodes(@"C:\Users\johndoe\");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\Users\" }, nodes6.Parents.Select(p => p.Path).ToArray());
			
			PathNodes nodes7 = new PathNodes(@"C:\Users\johndoe\Desktop\");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\Users\", @"C:\Users\johndoe\" }, nodes7.Parents.Select(p => p.Path).ToArray());
			
			PathNodes nodes8 = new PathNodes(@"C:\Users\johndoe\Desktop\TODO.txt");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\Users\", @"C:\Users\johndoe\", @"C:\Users\johndoe\Desktop\" }, nodes8.Parents.Select(p => p.Path).ToArray());
			
			PathNodes nodes9 = new PathNodes(@"Users\johndoe\Desktop\");
			CollectionAssert.AreEqual(new string[] { @"Users\", @"Users\johndoe\" }, nodes9.Parents.Select(p => p.Path).ToArray());
			
			PathNodes nodes10 = new PathNodes(@"TODO.txt");
			CollectionAssert.AreEqual(new string[] { }, nodes10.Parents.Select(p => p.Path).ToArray());
		}
	}

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
