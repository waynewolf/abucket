using UnityEngine;
using System.Collections;
using NUnit.Framework;

namespace UnitTest {

	[TestFixture]
	internal class UtilsTest {

		[Test]
		public void AngleLargeThanPi() {
			Vector2 from = Vector2.right;

			Vector2 to = new Vector2 (1f, 1f);
			Assert.False (Utils.AngleLargeThanPi (from, to));

			to = Vector2.up;
			Assert.False (Utils.AngleLargeThanPi (from, to));

			to = new Vector2 (-1f, 1f);
			Assert.False (Utils.AngleLargeThanPi (from, to));

			to = Vector2.left;
			Assert.False (Utils.AngleLargeThanPi (from, to));

			to = new Vector2 (-1f, -1f);
			Assert.True (Utils.AngleLargeThanPi (from, to));

			to = Vector2.down;
			Assert.True (Utils.AngleLargeThanPi (from, to));

			to = new Vector2(1f, -1f);
			Assert.True (Utils.AngleLargeThanPi (from, to));

			to = Vector2.right;
			Assert.False (Utils.AngleLargeThanPi (from, to));
		}

		[Test]
		public void AngleBetween() {
			Vector2 from = Vector2.right;
			
			Vector2 to = new Vector2 (1f, 1f);
			Assert.AreEqual (45f, Utils.AngleBetween (from, to));
			
			to = Vector2.up;
			Assert.AreEqual (90f, Utils.AngleBetween (from, to));

			to = new Vector2 (-1f, 1f);
			Assert.AreEqual (135f, Utils.AngleBetween (from, to));
			
			to = Vector2.left;
			Assert.AreEqual (180f, Utils.AngleBetween (from, to));
			
			to = new Vector2 (-1f, -1f);
			Assert.AreEqual (225f, Utils.AngleBetween (from, to));
			
			to = Vector2.down;
			Assert.AreEqual (270f, Utils.AngleBetween (from, to));
			
			to = new Vector2(1f, -1f);
			Assert.AreEqual (315f, Utils.AngleBetween (from, to));
			
			to = Vector2.right;
			Assert.AreEqual (0f, Utils.AngleBetween (from, to));
		}

		[Test]
		public void AngleFromX() {
			Vector2 to = new Vector2 (1f, 1f);
			Assert.AreEqual (45f, Utils.AngleFromX(to));
			
			to = Vector2.up;
			Assert.AreEqual (90f, Utils.AngleFromX(to));
			
			to = new Vector2 (-1f, 1f);
			Assert.AreEqual (135f, Utils.AngleFromX(to));
			
			to = Vector2.left;
			Assert.AreEqual (180f, Utils.AngleFromX(to));
			
			to = new Vector2 (-1f, -1f);
			Assert.AreEqual (225f, Utils.AngleFromX(to));
			
			to = Vector2.down;
			Assert.AreEqual (270f, Utils.AngleFromX(to));
			
			to = new Vector2(1f, -1f);
			Assert.AreEqual (315f, Utils.AngleFromX(to));
			
			to = Vector2.right;
			Assert.AreEqual (0f, Utils.AngleFromX(to));
		}
	}

}