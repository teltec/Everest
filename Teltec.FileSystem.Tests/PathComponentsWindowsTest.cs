/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Teltec.FileSystem.Tests
{
	[TestClass]
	public class PathNodesWindowsTest
	{
		[TestMethod]
		public void TestPathNodesNodesSelection()
		{
			PathNodes nodes1 = new PathNodes(@"C:");
			CollectionAssert.AreEqual(new string[] { @"C:\" }, nodes1.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"C:" }, nodes1.Nodes.Select(p => p.Name).ToArray());

			PathNodes nodes2 = new PathNodes(@"C:\");
			CollectionAssert.AreEqual(new string[] { @"C:\" }, nodes2.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"C:" }, nodes2.Nodes.Select(p => p.Name).ToArray());

			PathNodes nodes3 = new PathNodes(@"C:\.txt");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\.txt" }, nodes3.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"C:", @".txt" }, nodes3.Nodes.Select(p => p.Name).ToArray());

			PathNodes nodes4 = new PathNodes(@"C:\FOO.txt");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\FOO.txt" }, nodes4.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"C:", @"FOO.txt" }, nodes4.Nodes.Select(p => p.Name).ToArray());

			PathNodes nodes5 = new PathNodes(@"C:\Users\FOO.txt");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\Users\", @"C:\Users\FOO.txt" }, nodes5.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"C:", @"Users", @"FOO.txt" }, nodes5.Nodes.Select(p => p.Name).ToArray());

			PathNodes nodes6 = new PathNodes(@"C:\Users\johndoe\");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\Users\", @"C:\Users\johndoe\" }, nodes6.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"C:", @"Users", "johndoe" }, nodes6.Nodes.Select(p => p.Name).ToArray());

			PathNodes nodes7 = new PathNodes(@"C:\Users\johndoe\Desktop\");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\Users\", @"C:\Users\johndoe\", @"C:\Users\johndoe\Desktop\" }, nodes7.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"C:", @"Users", @"johndoe", @"Desktop" }, nodes7.Nodes.Select(p => p.Name).ToArray());

			PathNodes nodes8 = new PathNodes(@"C:\Users\johndoe\Desktop\FOO.txt");
			CollectionAssert.AreEqual(new string[] { @"C:\", @"C:\Users\", @"C:\Users\johndoe\", @"C:\Users\johndoe\Desktop\", @"C:\Users\johndoe\Desktop\FOO.txt" }, nodes8.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"C:", @"Users", @"johndoe", @"Desktop", @"FOO.txt" }, nodes8.Nodes.Select(p => p.Name).ToArray());

			PathNodes nodes9 = new PathNodes(@"Users\johndoe\Desktop\");
			CollectionAssert.AreEqual(new string[] { @"Users\", @"Users\johndoe\", @"Users\johndoe\Desktop\" }, nodes9.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"Users", @"johndoe", @"Desktop" }, nodes9.Nodes.Select(p => p.Name).ToArray());

			PathNodes nodes10 = new PathNodes(@"FOO.txt");
			CollectionAssert.AreEqual(new string[] { @"FOO.txt" }, nodes10.Nodes.Select(p => p.Path).ToArray());
			CollectionAssert.AreEqual(new string[] { @"FOO.txt" }, nodes10.Nodes.Select(p => p.Name).ToArray());
		}
	}

	[TestClass]
	public class PathComponentsWindowsTest
	{
		[TestMethod]
		public void TestParse()
		{
			PathComponents path = new PathComponents(@"C:\Users\johndoe\Desktop\FOO.txt");

			Assert.IsTrue(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual("C", path.Drive);
			CollectionAssert.AreEqual(new string[] { "Users", "johndoe", "Desktop" }, path.Directories);
			Assert.AreEqual("FOO.txt", path.FileName);
			Assert.AreEqual("FOO", path.FileNameWithoutExtension);
			Assert.AreEqual("txt", path.Extension);
		}

		[TestMethod]
		public void TestParseWithoutDirectories()
		{
			PathComponents path = new PathComponents(@"C:\FOO.txt");

			Assert.IsTrue(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual("C", path.Drive);
			CollectionAssert.AreEqual(null, path.Directories);
			Assert.AreEqual("FOO.txt", path.FileName);
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
			PathComponents path = new PathComponents(@"Users\johndoe\Desktop\FOO.txt");

			Assert.IsFalse(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual(string.Empty, path.Drive);
			CollectionAssert.AreEqual(new string[] { "Users", "johndoe", "Desktop" }, path.Directories);
			Assert.AreEqual("FOO.txt", path.FileName);
			Assert.AreEqual("FOO", path.FileNameWithoutExtension);
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
			PathComponents path = new PathComponents(@"FOO.txt");

			Assert.IsFalse(path.HasDrive);
			Assert.IsFalse(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual(string.Empty, path.Drive);
			CollectionAssert.AreEqual(null, path.Directories);
			Assert.AreEqual("FOO.txt", path.FileName);
			Assert.AreEqual("FOO", path.FileNameWithoutExtension);
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
