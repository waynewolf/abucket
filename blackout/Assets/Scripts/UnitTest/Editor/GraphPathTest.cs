using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace UnitTest {
	[TestFixture]
	internal class GraphPathTest {
		private GameObject _tempObjHolder = null;

		[TestFixtureSetUp]
		public void Setup() {
			_tempObjHolder = new GameObject("*GraphPathTest*");
		}

		[TestFixtureTearDown]
		public void TearDown() {
			GameObject testHolder = GameObject.Find ("*GraphPathTest*");
			if (testHolder != null)
				UnityEngine.Object.DestroyImmediate(testHolder);
		}

		Wire[] PrepareWireArray(int count) {
			Wire[] wires = new Wire[count];
			
			for (int i = 0; i < count; i++) {
				GameObject wireGameObject = new GameObject("Wire" + i, typeof(Wire));
				wireGameObject.transform.SetParent(_tempObjHolder.transform);
				wires[i] = wireGameObject.GetComponent<Wire>();
				Assert.NotNull(wires[i]);
			}

			return wires;
		}

		[Test]
		public void a000_PositionCheckTest() {
			Wire[] wires = PrepareWireArray (1);
			wires[0]._direction = Wire.Direction.XZ_X;
			wires[0].transform.position = new Vector3(0, 0, 0);
			Assert.False (wires[0].CheckPosition());

			wires[0]._direction = Wire.Direction.XZ_X;
			wires[0].transform.position = new Vector3(0, 0.2f, 0.5f);
			Assert.False (wires[0].CheckPosition());

			wires[0]._direction = Wire.Direction.XZ_X;
			wires[0].transform.position = new Vector3(0.3f, 0.2f, 0);
			Assert.False (wires[0].CheckPosition());

			wires[0]._direction = Wire.Direction.XZ_X;
			wires[0].transform.position = new Vector3(0.5f, 0, 0);
			Assert.True (wires[0].CheckPosition());

			wires[0]._direction = Wire.Direction.XZ_Z;
			wires[0].transform.position = new Vector3(0, 0, 0);
			Assert.False (wires[0].CheckPosition());

			wires[0]._direction = Wire.Direction.XZ_Z;
			wires[0].transform.position = new Vector3(1.2f, 0, 0);
			Assert.False (wires[0].CheckPosition());

			wires[0]._direction = Wire.Direction.XZ_Z;
			wires[0].transform.position = new Vector3(0, 0, -1.3f);
			Assert.False (wires[0].CheckPosition());

			wires[0]._direction = Wire.Direction.XZ_Z;
			wires[0].transform.position = new Vector3(0, 0, 1.5f);
			Assert.True (wires[0].CheckPosition());
		}

		[Test]
		public void a010_NodeNumberTest() {
			{
				Wire[] wires = PrepareWireArray (1);
				wires[0].transform.position = new Vector3(0.5f, 0, 0);
				wires[0]._direction = Wire.Direction.XZ_X;
				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				Assert.AreEqual(2, nodes.Count);
			}
			{
				Wire[] wires = PrepareWireArray (2);
				wires[0].transform.position = new Vector3(0.5f, 0, 1);
				wires[0]._direction = Wire.Direction.XZ_X;
				wires[1].transform.position = new Vector3(8.5f, 0, 8);
				wires[1]._direction = Wire.Direction.XZ_X;
				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				Assert.AreEqual(4, nodes.Count);
			}
			{
				Wire[] wires = PrepareWireArray (2);
				wires[0].transform.position = new Vector3(0.5f, 0, 1);
				wires[0]._direction = Wire.Direction.XZ_X;
				wires[1].transform.position = new Vector3(1.5f, 0, 1);
				wires[1]._direction = Wire.Direction.XZ_X;
				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				Assert.AreEqual(3, nodes.Count);
			}
			{
				Wire[] wires = PrepareWireArray (2);
				wires[0].transform.position = new Vector3(0.5f, 0, 1);
				wires[0]._direction = Wire.Direction.XZ_X;
				wires[1].transform.position = new Vector3(1.5f, 0, 2);
				wires[1]._direction = Wire.Direction.XZ_X;
				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				Assert.AreEqual(4, nodes.Count);
			}
			{
				Wire[] wires = PrepareWireArray (2);
				wires[0].transform.position = new Vector3(0.5f, 0, 1);
				wires[0]._direction = Wire.Direction.XZ_X;
				wires[1].transform.position = new Vector3(2.5f, 0, 1);
				wires[1]._direction = Wire.Direction.XZ_X;
				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				Assert.AreEqual(4, nodes.Count);
			}
			{
				Wire[] wires = PrepareWireArray (3);
				wires[0].transform.position = new Vector3(-0.5f, 0, 1);
				wires[0]._direction = Wire.Direction.XZ_X;
				wires[1].transform.position = new Vector3(0.5f, 0, 1);
				wires[1]._direction = Wire.Direction.XZ_X;
				wires[2].transform.position = new Vector3(1.5f, 0, 1);
				wires[2]._direction = Wire.Direction.XZ_X;
				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				Assert.AreEqual(4, nodes.Count);
			}
			{
				Wire[] wires = PrepareWireArray (4);
				wires[0].transform.position = new Vector3(0.5f, 0, 1);
				wires[0]._direction = Wire.Direction.XZ_X;
				wires[1].transform.position = new Vector3(1.5f, 0, 1);
				wires[1]._direction = Wire.Direction.XZ_X;
				wires[2].transform.position = new Vector3(2.5f, 0, 1);
				wires[2]._direction = Wire.Direction.XZ_X;
				wires[3].transform.position = new Vector3(5.5f, 0, 5);
				wires[3]._direction = Wire.Direction.XZ_X;
				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				Assert.AreEqual(6, nodes.Count);
			}
			{
				Wire[] wires = PrepareWireArray (6);
				wires[0].transform.position = new Vector3(0.5f, 0, 1);
				wires[0]._direction = Wire.Direction.XZ_X;
				wires[1].transform.position = new Vector3(1.5f, 0, 1);
				wires[1]._direction = Wire.Direction.XZ_X;
				wires[2].transform.position = new Vector3(2.5f, 0, 1);
				wires[2]._direction = Wire.Direction.XZ_X;
				wires[3].transform.position = new Vector3(5.5f, 0, 5);
				wires[3]._direction = Wire.Direction.XZ_X;
				wires[4].transform.position = new Vector3(5, 0, 5.5f);
				wires[4]._direction = Wire.Direction.XZ_Z;
				wires[5].transform.position = new Vector3(5, 0, 6.5f);
				wires[5]._direction = Wire.Direction.XZ_Z;
				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				Assert.AreEqual(8, nodes.Count);
			}
		}

		[Test]
		public void a020_BuildTest() {
			{
				Wire[] wires = PrepareWireArray (6);
				wires[0].transform.position = new Vector3(0.5f, 0, 1);
				wires[0]._direction = Wire.Direction.XZ_X;
				wires[1].transform.position = new Vector3(1.5f, 0, 1);
				wires[1]._direction = Wire.Direction.XZ_X;
				wires[2].transform.position = new Vector3(2.5f, 0, 1);
				wires[2]._direction = Wire.Direction.XZ_X;
				wires[3].transform.position = new Vector3(5.5f, 0, 5);
				wires[3]._direction = Wire.Direction.XZ_X;
				wires[4].transform.position = new Vector3(5, 0, 5.5f);
				wires[4]._direction = Wire.Direction.XZ_Z;
				wires[5].transform.position = new Vector3(5, 0, 6.5f);
				wires[5]._direction = Wire.Direction.XZ_Z;
				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				//          7
				//          |
				//          6
				//          |
				// 0-1-2-3, 4-5

				Assert.AreEqual(1, nodes[0]._adjacentNodes.Count);
				Assert.AreEqual(1, nodes[0]._adjacentNodes[0]);

				Assert.AreEqual(2, nodes[1]._adjacentNodes.Count);
				Assert.AreEqual(0, nodes[1]._adjacentNodes[0]);
				Assert.AreEqual(2, nodes[1]._adjacentNodes[1]);

				Assert.AreEqual(2, nodes[2]._adjacentNodes.Count);
				Assert.AreEqual(1, nodes[2]._adjacentNodes[0]);
				Assert.AreEqual(3, nodes[2]._adjacentNodes[1]);

				Assert.AreEqual(1, nodes[3]._adjacentNodes.Count);
				Assert.AreEqual(2, nodes[3]._adjacentNodes[0]);

				Assert.AreEqual(2, nodes[4]._adjacentNodes.Count);
				Assert.AreEqual(5, nodes[4]._adjacentNodes[0]);
				Assert.AreEqual(6, nodes[4]._adjacentNodes[1]);

				Assert.AreEqual(1, nodes[5]._adjacentNodes.Count);
				Assert.AreEqual(4, nodes[5]._adjacentNodes[0]);

				Assert.AreEqual(2, nodes[6]._adjacentNodes.Count);
				Assert.AreEqual(4, nodes[6]._adjacentNodes[0]);
				Assert.AreEqual(7, nodes[6]._adjacentNodes[1]);

				Assert.AreEqual(1, nodes[7]._adjacentNodes.Count);
				Assert.AreEqual(6, nodes[7]._adjacentNodes[0]);
			}

			{
				Wire[] wires = PrepareWireArray (8);
				wires[0].transform.position = new Vector3(0.5f, 0, 1);
				wires[0]._direction = Wire.Direction.XZ_X;
				wires[1].transform.position = new Vector3(1.5f, 0, 1);
				wires[1]._direction = Wire.Direction.XZ_X;
				wires[2].transform.position = new Vector3(2.5f, 0, 1);
				wires[2]._direction = Wire.Direction.XZ_X;
				wires[3].transform.position = new Vector3(5.5f, 0, 5);
				wires[3]._direction = Wire.Direction.XZ_X;
				wires[4].transform.position = new Vector3(5, 0, 5.5f);
				wires[4]._direction = Wire.Direction.XZ_Z;
				wires[5].transform.position = new Vector3(5, 0, 6.5f);
				wires[5]._direction = Wire.Direction.XZ_Z;
				wires[6].transform.position = new Vector3(4.5f, 0, 6);
				wires[6]._direction = Wire.Direction.XZ_X;
				wires[7].transform.position = new Vector3(5.5f, 0, 6);
				wires[7]._direction = Wire.Direction.XZ_X;

				GraphPath graphPath = GraphPath.Build(wires);
				UniqueList<GraphPathNode> nodes = graphPath.GetInternalNodes_UnitTest();
				//          7
				//          |
				//        8-6-9
				//          |
				// 0-1-2-3, 4-5

				Assert.AreEqual(10, nodes.Count);

				Assert.AreEqual(1, nodes[0]._adjacentNodes.Count);
				Assert.AreEqual(1, nodes[0]._adjacentNodes[0]);
				
				Assert.AreEqual(2, nodes[1]._adjacentNodes.Count);
				Assert.AreEqual(0, nodes[1]._adjacentNodes[0]);
				Assert.AreEqual(2, nodes[1]._adjacentNodes[1]);
				
				Assert.AreEqual(2, nodes[2]._adjacentNodes.Count);
				Assert.AreEqual(1, nodes[2]._adjacentNodes[0]);
				Assert.AreEqual(3, nodes[2]._adjacentNodes[1]);
				
				Assert.AreEqual(1, nodes[3]._adjacentNodes.Count);
				Assert.AreEqual(2, nodes[3]._adjacentNodes[0]);
				
				Assert.AreEqual(2, nodes[4]._adjacentNodes.Count);
				Assert.AreEqual(5, nodes[4]._adjacentNodes[0]);
				Assert.AreEqual(6, nodes[4]._adjacentNodes[1]);
				
				Assert.AreEqual(1, nodes[5]._adjacentNodes.Count);
				Assert.AreEqual(4, nodes[5]._adjacentNodes[0]);
				
				Assert.AreEqual(4, nodes[6]._adjacentNodes.Count);
				Assert.AreEqual(4, nodes[6]._adjacentNodes[0]);
				Assert.AreEqual(7, nodes[6]._adjacentNodes[1]);
				Assert.AreEqual(8, nodes[6]._adjacentNodes[2]);
				Assert.AreEqual(9, nodes[6]._adjacentNodes[3]);

				Assert.AreEqual(1, nodes[7]._adjacentNodes.Count);
				Assert.AreEqual(6, nodes[7]._adjacentNodes[0]);

				Assert.AreEqual(1, nodes[8]._adjacentNodes.Count);
				Assert.AreEqual(6, nodes[8]._adjacentNodes[0]);

				Assert.AreEqual(1, nodes[9]._adjacentNodes.Count);
				Assert.AreEqual(6, nodes[9]._adjacentNodes[0]);
			}
		}

		[Test]
		public void a030_GetNodeByIdTest() {
			Wire[] wires = PrepareWireArray (8);
			wires[0].transform.position = new Vector3(0.5f, 0, 1);
			wires[0]._direction = Wire.Direction.XZ_X;
			wires[1].transform.position = new Vector3(1.5f, 0, 1);
			wires[1]._direction = Wire.Direction.XZ_X;
			wires[2].transform.position = new Vector3(2.5f, 0, 1);
			wires[2]._direction = Wire.Direction.XZ_X;
			wires[3].transform.position = new Vector3(5.5f, 0, 5);
			wires[3]._direction = Wire.Direction.XZ_X;
			wires[4].transform.position = new Vector3(5, 0, 5.5f);
			wires[4]._direction = Wire.Direction.XZ_Z;
			wires[5].transform.position = new Vector3(5, 0, 6.5f);
			wires[5]._direction = Wire.Direction.XZ_Z;
			wires[6].transform.position = new Vector3(4.5f, 0, 6);
			wires[6]._direction = Wire.Direction.XZ_X;
			wires[7].transform.position = new Vector3(5.5f, 0, 6);
			wires[7]._direction = Wire.Direction.XZ_X;
			
			GraphPath graphPath = GraphPath.Build(wires);
			//          7
			//          |
			//        8-6-9
			//          |
			// 0-1-2-3, 4-5

			Assert.AreEqual(0, graphPath.GetNodeById(0)._id);
			Assert.AreEqual(1, graphPath.GetNodeById(1)._id);
			Assert.AreEqual(2, graphPath.GetNodeById(2)._id);
			Assert.AreEqual(3, graphPath.GetNodeById(3)._id);
			Assert.AreEqual(4, graphPath.GetNodeById(4)._id);
			Assert.AreEqual(5, graphPath.GetNodeById(5)._id);
			Assert.AreEqual(6, graphPath.GetNodeById(6)._id);
			Assert.AreEqual(7, graphPath.GetNodeById(7)._id);
			Assert.AreEqual(8, graphPath.GetNodeById(8)._id);
			Assert.AreEqual(9, graphPath.GetNodeById(9)._id);
		}

		[Test]
		public void a040_GetNodeByEndpointTest() {
			Wire[] wires = PrepareWireArray (8);
			wires[0].transform.position = new Vector3(0.5f, 0, 1);
			wires[0]._direction = Wire.Direction.XZ_X;
			wires[1].transform.position = new Vector3(1.5f, 0, 1);
			wires[1]._direction = Wire.Direction.XZ_X;
			wires[2].transform.position = new Vector3(2.5f, 0, 1);
			wires[2]._direction = Wire.Direction.XZ_X;
			wires[3].transform.position = new Vector3(5.5f, 0, 5);
			wires[3]._direction = Wire.Direction.XZ_X;
			wires[4].transform.position = new Vector3(5, 0, 5.5f);
			wires[4]._direction = Wire.Direction.XZ_Z;
			wires[5].transform.position = new Vector3(5, 0, 6.5f);
			wires[5]._direction = Wire.Direction.XZ_Z;
			wires[6].transform.position = new Vector3(4.5f, 0, 6);
			wires[6]._direction = Wire.Direction.XZ_X;
			wires[7].transform.position = new Vector3(5.5f, 0, 6);
			wires[7]._direction = Wire.Direction.XZ_X;
			
			GraphPath graphPath = GraphPath.Build(wires);
			//          7
			//          |
			//        8-6-9
			//          |
			// 0-1-2-3, 4-5
			
			Assert.AreEqual(0, graphPath.GetNodeByEndpoint(new Vector3(0, 0, 1))._id);
			Assert.AreEqual(1, graphPath.GetNodeByEndpoint(new Vector3(1, 0, 1))._id);
			Assert.AreEqual(2, graphPath.GetNodeByEndpoint(new Vector3(2, 0, 1))._id);
			Assert.AreEqual(3, graphPath.GetNodeByEndpoint(new Vector3(3, 0, 1))._id);
			Assert.AreEqual(4, graphPath.GetNodeByEndpoint(new Vector3(5, 0, 5))._id);
			Assert.AreEqual(5, graphPath.GetNodeByEndpoint(new Vector3(6, 0, 5))._id);
			Assert.AreEqual(6, graphPath.GetNodeByEndpoint(new Vector3(5, 0, 6))._id);
			Assert.AreEqual(7, graphPath.GetNodeByEndpoint(new Vector3(5, 0, 7))._id);
			Assert.AreEqual(8, graphPath.GetNodeByEndpoint(new Vector3(4, 0, 6))._id);
			Assert.AreEqual(9, graphPath.GetNodeByEndpoint(new Vector3(6, 0, 6))._id);
			Assert.IsNull(graphPath.GetNodeByEndpoint(new Vector3(10, 0, 6)));
			Assert.NotNull(graphPath.GetNodeByEndpoint(new Vector3(5.4f, 0, 7)));
			Assert.IsNull(graphPath.GetNodeByEndpoint(new Vector3(5.6f, 0, 7)));
		}

		[Test]
		public void a050_IsDirectConnectedTest() {
			Wire[] wires = PrepareWireArray (8);
			wires[0].transform.position = new Vector3(0.5f, 0, 1);
			wires[0]._direction = Wire.Direction.XZ_X;
			wires[1].transform.position = new Vector3(1.5f, 0, 1);
			wires[1]._direction = Wire.Direction.XZ_X;
			wires[2].transform.position = new Vector3(2.5f, 0, 1);
			wires[2]._direction = Wire.Direction.XZ_X;
			wires[3].transform.position = new Vector3(5.5f, 0, 5);
			wires[3]._direction = Wire.Direction.XZ_X;
			wires[4].transform.position = new Vector3(5, 0, 5.5f);
			wires[4]._direction = Wire.Direction.XZ_Z;
			wires[5].transform.position = new Vector3(5, 0, 6.5f);
			wires[5]._direction = Wire.Direction.XZ_Z;
			wires[6].transform.position = new Vector3(4.5f, 0, 6);
			wires[6]._direction = Wire.Direction.XZ_X;
			wires[7].transform.position = new Vector3(5.5f, 0, 6);
			wires[7]._direction = Wire.Direction.XZ_X;
			
			GraphPath graphPath = GraphPath.Build(wires);
			//          7
			//          |
			//        8-6-9
			//          |
			// 0-1-2-3, 4-5

			Assert.True (graphPath.IsDirectConnected(graphPath.GetNodeById(0), graphPath.GetNodeById(1)));
			Assert.True (graphPath.IsDirectConnected(graphPath.GetNodeById(1), graphPath.GetNodeById(2)));
			Assert.True (graphPath.IsDirectConnected(graphPath.GetNodeById(2), graphPath.GetNodeById(3)));
			Assert.False(graphPath.IsDirectConnected(graphPath.GetNodeById(3), graphPath.GetNodeById(4)));
			Assert.True (graphPath.IsDirectConnected(graphPath.GetNodeById(4), graphPath.GetNodeById(5)));
			Assert.False(graphPath.IsDirectConnected(graphPath.GetNodeById(5), graphPath.GetNodeById(6)));
			Assert.True (graphPath.IsDirectConnected(graphPath.GetNodeById(6), graphPath.GetNodeById(7)));
			Assert.True (graphPath.IsDirectConnected(graphPath.GetNodeById(6), graphPath.GetNodeById(8)));
			Assert.True (graphPath.IsDirectConnected(graphPath.GetNodeById(6), graphPath.GetNodeById(9)));
			Assert.False(graphPath.IsDirectConnected(graphPath.GetNodeById(6), graphPath.GetNodeById(5)));
			Assert.False(graphPath.IsDirectConnected(graphPath.GetNodeById(6), graphPath.GetNodeById(1)));
			Assert.False(graphPath.IsDirectConnected(graphPath.GetNodeById(7), graphPath.GetNodeById(8)));
			Assert.False(graphPath.IsDirectConnected(graphPath.GetNodeById(8), graphPath.GetNodeById(9)));
			Assert.False(graphPath.IsDirectConnected(graphPath.GetNodeById(8), graphPath.GetNodeById(8)));
		}

		[Test]
		public void a060_BreadthFirstTraversalTest() {
			Wire[] wires = PrepareWireArray (8);
			wires[0].transform.position = new Vector3(0.5f, 0, 1);
			wires[0]._direction = Wire.Direction.XZ_X;
			wires[1].transform.position = new Vector3(1.5f, 0, 1);
			wires[1]._direction = Wire.Direction.XZ_X;
			wires[2].transform.position = new Vector3(2.5f, 0, 1);
			wires[2]._direction = Wire.Direction.XZ_X;
			wires[3].transform.position = new Vector3(5.5f, 0, 5);
			wires[3]._direction = Wire.Direction.XZ_X;
			wires[4].transform.position = new Vector3(5, 0, 5.5f);
			wires[4]._direction = Wire.Direction.XZ_Z;
			wires[5].transform.position = new Vector3(5, 0, 6.5f);
			wires[5]._direction = Wire.Direction.XZ_Z;
			wires[6].transform.position = new Vector3(4.5f, 0, 6);
			wires[6]._direction = Wire.Direction.XZ_X;
			wires[7].transform.position = new Vector3(5.5f, 0, 6);
			wires[7]._direction = Wire.Direction.XZ_X;
			
			GraphPath graphPath = GraphPath.Build(wires);
			//          7
			//          |
			//        8-6-9
			//          |
			// 0-1-2-3, 4-5

			List<GraphPathNode> path = graphPath.BreadthFirstTraversal(graphPath.GetNodeById(0));
			Assert.AreEqual(4, path.Count);
			Assert.AreEqual(0, path[0]._id);
			Assert.AreEqual(1, path[1]._id);
			Assert.AreEqual(2, path[2]._id);
			Assert.AreEqual(3, path[3]._id);

			List<GraphPathNode> path2 = graphPath.BreadthFirstTraversal(graphPath.GetNodeById(4));
			
			Assert.AreEqual(6, path2.Count);
			Assert.AreEqual(4, path2[0]._id);
			Assert.AreEqual(5, path2[1]._id);
			Assert.AreEqual(6, path2[2]._id);
			Assert.AreEqual(7, path2[3]._id);
			Assert.AreEqual(8, path2[4]._id);
			Assert.AreEqual(9, path2[5]._id);
		}

		[Test]
		public void b000_BuildTreeTest() {
			Wire[] wires = PrepareWireArray (8);
			wires[0].transform.position = new Vector3(0.5f, 0, 1);
			wires[0]._direction = Wire.Direction.XZ_X;
			wires[1].transform.position = new Vector3(1.5f, 0, 1);
			wires[1]._direction = Wire.Direction.XZ_X;
			wires[2].transform.position = new Vector3(2.5f, 0, 1);
			wires[2]._direction = Wire.Direction.XZ_X;
			wires[3].transform.position = new Vector3(5.5f, 0, 5);
			wires[3]._direction = Wire.Direction.XZ_X;
			wires[4].transform.position = new Vector3(5, 0, 5.5f);
			wires[4]._direction = Wire.Direction.XZ_Z;
			wires[5].transform.position = new Vector3(5, 0, 6.5f);
			wires[5]._direction = Wire.Direction.XZ_Z;
			wires[6].transform.position = new Vector3(4.5f, 0, 6);
			wires[6]._direction = Wire.Direction.XZ_X;
			wires[7].transform.position = new Vector3(5.5f, 0, 6);
			wires[7]._direction = Wire.Direction.XZ_X;
			
			GraphPath graphPath = GraphPath.Build(wires);
			//          7
			//          |
			//        8-6-9
			//          |
			// 0-1-2-3, 4-5

			{
				int[] correctResult = new int[] {
					2, 1, 0, 3
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(2));
				Assert.AreEqual(4, tree.GetSize());

				int cursor = 0;
				tree.DepthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					4, 5, 6, 7, 8, 9
				};

				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(4));

				int cursor = 0;
				tree.DepthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					4, 5, 6, 7, 8, 9
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(4));
				
				int cursor = 0;
				tree.BreadthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					5, 4, 6, 7, 8, 9
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(5));
				
				int cursor = 0;
				tree.DepthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					5, 4, 6, 7, 8, 9
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(5));
				
				int cursor = 0;
				tree.BreadthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					6, 4, 5, 7, 8, 9
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(6));
				
				int cursor = 0;
				tree.DepthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}
			{
				int[] correctResult = new int[] {
					6, 4, 7, 8, 9, 5
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(6));
				
				int cursor = 0;
				tree.BreadthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					7, 6, 4, 5, 8, 9
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(7));
				
				int cursor = 0;
				tree.DepthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					7, 6, 4, 8, 9, 5
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(7));
				
				int cursor = 0;
				tree.BreadthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					8, 6, 4, 5, 7, 9
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(8));
				
				int cursor = 0;
				tree.DepthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}
			
			{
				int[] correctResult = new int[] {
					8, 6, 4, 7, 9, 5
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(8));
				
				int cursor = 0;
				tree.BreadthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					9, 6, 4, 5, 7, 8
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(9));
				
				int cursor = 0;
				tree.DepthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}

			{
				int[] correctResult = new int[] {
					9, 6, 4, 7, 8, 5
				};
				
				TreePath tree = graphPath.BuildTree(graphPath.GetNodeById(9));
				Assert.AreEqual(6, tree.GetSize());

				int cursor = 0;
				tree.BreadthFirstTraversal((TreePathNode node) => {
					Assert.AreEqual(correctResult[cursor++], node._id);
				});
			}
		}
	}
}
