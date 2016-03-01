using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnitTest {
	[TestFixture]
	internal class TreePathTest {
		[Test]
		public void b000_TreePathGetSizeTest () {
			{
				TreePath treePath = new TreePath();
				Assert.AreEqual(0, treePath.GetSize());
			}
			{
				TreePathNode node = new TreePathNode(Vector3.zero);
				TreePath treePath = new TreePath(node);
				Assert.AreEqual(1, treePath.GetSize());
			}
			{
				TreePathNode node = new TreePathNode(Vector3.zero);
				node.AddChild(new TreePathNode(Vector3.one));
				node.AddChild(new TreePathNode(Vector3.right));
				TreePath treePath = new TreePath(node);
				Assert.AreEqual(3, treePath.GetSize());
			}
			{
				TreePathNode node = new TreePathNode(Vector3.zero);
				node.AddChild(new TreePathNode(Vector3.one));
				node.AddChild(new TreePathNode(Vector3.right));
				node.AddChild(new TreePathNode(Vector3.left));
				node.AddChild(new TreePathNode(Vector3.up));
				TreePath treePath = new TreePath(node);
				Assert.AreEqual(5, treePath.GetSize());
			}
			{
				TreePathNode node = new TreePathNode(Vector3.zero);
				node.AddChild(new TreePathNode(Vector3.one));
				node.AddChild(new TreePathNode(Vector3.right));
				node.AddChild(new TreePathNode(Vector3.left));

				TreePathNode l1 = new TreePathNode(Vector3.down);
				l1.AddChild(new TreePathNode(Vector3.up));
				l1.AddChild(new TreePathNode(Vector3.forward));

				TreePathNode l2 = new TreePathNode(Vector3.back);
				l2.AddChild(new TreePathNode(Vector3.left));

				l1.AddChild(l2);
				node.AddChild(l1);

				TreePath treePath = new TreePath(node);
				Assert.AreEqual(9, treePath.GetSize());

				treePath.ResetRoot(node);
				Assert.AreEqual(9, treePath.GetSize());

				treePath.Clear();
				Assert.AreEqual(0, treePath.GetSize());
			}
		}

		[Test]
		public void a000_TreeNodeOccupyEndpointTest () {
			{
				TreePathNode node = new TreePathNode(Vector3.zero);
				Assert.True (node.OccupyEndpoint(Vector3.zero));
			}
			{
				TreePathNode node = new TreePathNode(Vector3.one);
				Assert.True (node.OccupyEndpoint(Vector3.one));
				Assert.False (node.OccupyEndpoint(Vector3.zero));
			}
		}

		[Test]
		public void a000_TreeNodeEqualTest () {
			{
				TreePathNode node1 = new TreePathNode(Vector3.zero);
				TreePathNode node2 = new TreePathNode(Vector3.zero);
				Assert.True (node1.Equals(node2));
				Assert.False (node1 == node2);
			}
			{
				TreePathNode node1 = new TreePathNode(Vector3.zero);
				TreePathNode node2 = new TreePathNode(Vector3.one);
				Assert.True (node1 != node2);
				Assert.False (node1 == node2);
				Assert.True (!node1.Equals(node2));
			}
		}
	}
}
