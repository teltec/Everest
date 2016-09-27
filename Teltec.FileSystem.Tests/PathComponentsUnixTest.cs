/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Teltec.FileSystem.Tests
{
	[TestClass]
	public class PathComponentsUnixTest
	{
		[TestMethod]
		public void TestParse()
		{
			PathComponents path = new PathComponents("/home/johndoe/Desktop/FOO.txt");

			Assert.IsTrue(path.HasDrive);
			Assert.IsTrue(path.HasDirectories);
			Assert.IsTrue(path.HasFileName);
			Assert.IsTrue(path.HasExtension);

			Assert.AreEqual("/home", path.Drive);
			CollectionAssert.AreEqual(new string[] { "johndoe", "Desktop" }, path.Directories);
			Assert.AreEqual("FOO.txt", path.FileName);
			Assert.AreEqual("FOO", path.FileNameWithoutExtension);
			Assert.AreEqual("txt", path.Extension);
		}
	}
}
