using System;
using System.Drawing;
using NUnit.Framework;
using SpawnEditor;

namespace SpawnEditor.Tests
{
	public class SpawnPointTests
	{
		[Test]
		public void IsSameArea_DetectsOverlap()
		{
            SpawnPoint sp = new SpawnPoint(Guid.NewGuid(), WorldMap.Trammel, 0, 0, 10, 10);

            Assert.IsTrue(sp.IsSameArea(5, 5), "Point inside bounds should match area");
            Assert.IsTrue(sp.IsSameArea(3, 3, 1), "Range overlap should be detected");
            Assert.IsFalse(sp.IsSameArea(20, 20), "Point outside bounds should not match");
		}

		[Test]
		public void BoundsSetter_UpdatesCentreCoordinates()
		{
            SpawnPoint sp = new SpawnPoint(Guid.NewGuid(), WorldMap.Felucca, 0, 0, 10, 10);

            sp.Bounds = new Rectangle(10, 20, 4, 6);

            Assert.AreEqual(12, sp.CentreX);
            Assert.AreEqual(23, sp.CentreY);
		}
	}
}

