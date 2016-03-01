using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class TreePathNode : IEquatable<TreePathNode> {
	public Vector3 _endPoint;			// The position of this TreeNode occupies, used to identify TreeNode
	public int _id;						// Used for debugging
	UniqueList<TreePathNode> _children;
	
	public TreePathNode(Vector3 endPoint) {
		_children = new UniqueList<TreePathNode>();
		_endPoint = endPoint;
		_id = 0;
	}
	
	public TreePathNode() {
		_children = new UniqueList<TreePathNode>();
		_endPoint = Vector3.zero;
		_id = 0;
	}

	public TreePathNode(GraphPathNode graphPathNode) {
		_children = new UniqueList<TreePathNode>();
		_endPoint = graphPathNode._endPoint;
		_id = graphPathNode._id;
	}
	
	public bool OccupyEndpoint(Vector3 endPoint) {
		if (MathUtil.Vector3EqualInt(_endPoint, endPoint))
			return true;
		return false;
	}
	
	public override bool Equals(System.Object other) {
		if (other == null)
			return false;
		
		TreePathNode treeNode = other as TreePathNode;
		if (treeNode == null)
			return false;
		else   
			return Equals(treeNode);
	}
	
	public override int GetHashCode() {
		return _endPoint.GetHashCode();
	}
	
	public bool Equals(TreePathNode other) {
		if (other == null) 
			return false;
		
		if (MathUtil.Vector3EqualInt(_endPoint, other._endPoint))
			return true;
		
		return false;
	}
	
	public int GetChildrenNumber() {
		return _children.Count;
	}
	
	public TreePathNode GetChild(int index) {
		if (index < 0 || index >= GetChildrenNumber())
			return null;
		return _children[index];
	}

	public TreePathNode[] GetChildren() {
		return _children.ToArray();
	}

	public bool AddChild(TreePathNode childNode) {
		return _children.Add(childNode);
	}
}

public class TreePath {
	TreePathNode _root;
	int _size;

	public TreePath() {
		Clear ();
	}

	public TreePath(TreePathNode rootNode) {
		ResetRoot(rootNode);
	}

	public bool AddChild(TreePathNode parent, TreePathNode child) {
		if (parent == null || child == null)
			return false;
		return parent.AddChild(child);
	}

	public TreePathNode[] GetChildren (TreePathNode parent){
		return parent.GetChildren();
	}

	public bool ResetRoot(TreePathNode rootNode) {
		_root = rootNode;
		RecalculateSize();
		return true;
	}

	public void Clear() {
		_size = 0;
		_root = null;
	}

	public int GetSize() {
		return _size;
	}

	void RecalculateSize() {
		int size = 0;
		DepthFirstTraversal (_root, (TreePathNode notUsedParameter) => { size++; });
		_size = size;
	}

	public void DepthFirstTraversal(Action<TreePathNode> action) {
		DepthFirstTraversal(_root, action);
	}

	void DepthFirstTraversal(TreePathNode root, Action<TreePathNode> action) {
		if (root != null) {
			action(root);
			for (int i = 0; i < root.GetChildrenNumber(); i++) {
				DepthFirstTraversal (root.GetChild(i), action);
			}
		}
	}

	public void BreadthFirstTraversal(Action<TreePathNode> action) {
		const int NODE_STATE_INIT = 0;
		const int NODE_STATE_INQUEUE = 1;
		const int NODE_STATE_DEQUEUED = 2;
		const int MAX_NODE_SIZE = 100;

		// Attention: cannot use GetSize(), id is non-continuous here
		int[] state = new int[MAX_NODE_SIZE];
		
		Queue<TreePathNode> queue = new Queue<TreePathNode>();
		queue.Enqueue(_root);
		state[_root._id] = NODE_STATE_INQUEUE;
		
		while(queue.Count != 0) {
			TreePathNode node = queue.Dequeue();
			state[node._id] = NODE_STATE_DEQUEUED;
			action(node);
			
			for(int i = 0; i < node.GetChildrenNumber(); i++) {
				TreePathNode childNode = node.GetChild(i);
				if (state[childNode._id] == NODE_STATE_INIT) {
					queue.Enqueue(childNode);
					state[childNode._id] = NODE_STATE_INQUEUE;
				} else if (state[childNode._id] == NODE_STATE_INQUEUE) {
					Debug.LogError("Tree has no loop, shouldn't happen");
				}
			}	
		}
	}

	public TreePathNode GetRoot() {
		return _root;
	}

	public void DebugPrintTopology () {
		StringBuilder stringBuilder = new StringBuilder();
		BreadthFirstTraversal((TreePathNode node) => {
			stringBuilder.Append(node._id)
				.Append (": ");
			for (int i = 0; i < node.GetChildrenNumber(); i++) {
				stringBuilder.Append (node.GetChild(i)._id).Append (", ");
			}
			stringBuilder.AppendLine();
		});
		Debug.Log (stringBuilder.ToString());
	}
}

public class GraphPathNode : IEquatable<GraphPathNode> {
	public Vector3 _endPoint;			// The position of this GraphPathNode occupies, used to identify GraphPathNode.
	public int _id;						// Equals to the index in the UniqueList where this GraphPathNode resides.
	public List<int> _adjacentNodes;

	public GraphPathNode() {
		_id = -1;
		_adjacentNodes = new List<int>();
	}

	public bool OccupyEndpoint(Vector3 endPoint) {
		if (MathUtil.Vector3EqualInt(_endPoint, endPoint))
			return true;
		return false;
	}

	public override bool Equals(System.Object other) {
		if (other == null)
			return false;
		
		GraphPathNode graphPathNode = other as GraphPathNode;
		if (graphPathNode == null)
			return false;
		else   
			return Equals(graphPathNode); 
	}

	public override int GetHashCode() {
		return ("#" + _id + ":" + _endPoint).GetHashCode();
	}

	public bool Equals(GraphPathNode other) {
		if (other == null) 
			return false;

		if (MathUtil.Vector3EqualInt(_endPoint, other._endPoint))
			return true;

		return false;
	}
}

/// <summary>
/// No loop, no direction graph, use adjacent list data structure
/// </summary>
public class GraphPath {
	private UniqueList<GraphPathNode> _nodes;

	private GraphPath() {
		_nodes = new UniqueList<GraphPathNode>();
	}

	/// <summary>
	/// Add a GraphPathNode to the GraphPath, no duplicated nodes allowed
	/// </summary>
	/// <param name="graphPathNode">Graph path node.</param>
	public void AddNode (GraphPathNode graphPathNode) {
		_nodes.Add (graphPathNode);
	}

	public static GraphPath Build (Wire[] wires) {
		GraphPath graphPath = new GraphPath();

		int indexCount = 0;
		for(int i = 0; i < wires.Length; i++) {
			Wire wire = wires[i];
			Vector3 endPoint1, endPoint2;
			wire.GetEndPoints(out endPoint1, out endPoint2);

			if (!graphPath.NodeExist(endPoint1)) {
				GraphPathNode graphPathNode1 = new GraphPathNode();
				graphPathNode1._id = indexCount++;
				graphPathNode1._endPoint = endPoint1;

				if (!graphPath.NodeExist(endPoint2)) {
					GraphPathNode graphPathNode2 = new GraphPathNode();
					graphPathNode2._id = indexCount++;
					graphPathNode2._endPoint = endPoint2;
					graphPathNode1._adjacentNodes.Add (graphPathNode2._id);
					graphPathNode2._adjacentNodes.Add (graphPathNode1._id);

					graphPath.AddNode(graphPathNode1);
					graphPath.AddNode(graphPathNode2);
				} else {
					// If the node already exist, don't add the node, but update the 
					// adjacent list of the existing node.
					GraphPathNode existedNode = graphPath.GetNodeByEndpoint(endPoint2);
					existedNode._adjacentNodes.Add (graphPathNode1._id);
					graphPathNode1._adjacentNodes.Add (existedNode._id);
					graphPath.AddNode (graphPathNode1);
				}
			} else {
				if (!graphPath.NodeExist(endPoint2)) {
					// endpoint1 already exist, but endpoint2 doesn't
					GraphPathNode existedNode = graphPath.GetNodeByEndpoint(endPoint1);
					GraphPathNode graphPathNode2 = new GraphPathNode();
					graphPathNode2._id = indexCount++;
					graphPathNode2._endPoint = endPoint2;
					//graphPathNode2._gameObject = wire.gameObject;
					graphPathNode2._adjacentNodes.Add (existedNode._id);
					existedNode._adjacentNodes.Add (graphPathNode2._id);
					graphPath.AddNode(graphPathNode2);
				} else {
					// both endpoint1 and endpoint2 exist, simply connect them
					GraphPathNode existedNode1 = graphPath.GetNodeByEndpoint(endPoint1);
					GraphPathNode existedNode2 = graphPath.GetNodeByEndpoint(endPoint2);
					existedNode1._adjacentNodes.Add (existedNode2._id);
					existedNode1._adjacentNodes.Add (existedNode1._id);
				}
			}
	
		}

		return graphPath;
	}

	public int GetNodeNumber() {
		return _nodes.Count;
	}

	public GraphPathNode GetNodeByEndpoint (Vector3 endPoint) {
		foreach(GraphPathNode node in _nodes) {
			if (node.OccupyEndpoint(endPoint)) {
				return node;
			}
		}
		return null;
	}

	public GraphPathNode GetNodeById(int nodeId) {
		if (nodeId < 0 || nodeId >= _nodes.Count)
			return null;

		foreach(GraphPathNode node in _nodes) {
			if (node._id == nodeId) {
				return node;
			}
		}
		return null;
	}

	public List<int> GetAdjacentNodes(GraphPathNode graphPathNode) {
		return graphPathNode._adjacentNodes;
	}

	public bool NodeExist(GraphPathNode graphPathNode) {
		return _nodes.Contains(graphPathNode);
	}

	public bool NodeExist(Vector3 endPoint) {
		bool exist = false;
		foreach(GraphPathNode node in _nodes) {
			if (node.OccupyEndpoint(endPoint)) {
				exist = true;
				break;
			}
		}
		return exist;
	}

	/// <summary>
	/// Determines whether two GraphPathNode is direct connected, same nodes are considered NOT connected
	/// </summary>
	/// <returns><c>true</c> if this instance is direct connected the specified graphPathNode1 graphPathNode2; otherwise, <c>false</c>.</returns>
	/// <param name="graphPathNode1">Graph path node1.</param>
	/// <param name="graphPathNode2">Graph path node2.</param>
	public bool IsDirectConnected(GraphPathNode graphPathNode1, GraphPathNode graphPathNode2) {
		List<int> adjacentNodes = graphPathNode1._adjacentNodes;
		int node2Id = graphPathNode2._id;

		if (adjacentNodes.Contains(node2Id)) {
			return true;
		}

		return false;
	}

	public List<GraphPathNode> BreadthFirstTraversal(GraphPathNode startNode) {
		List<GraphPathNode> nodeList = new List<GraphPathNode>();

		BreadthFirstTraversal(startNode, (GraphPathNode node) => {
			nodeList.Add(node);
		});

		return nodeList;
	}

	/// <summary>
	/// Builds a tree from breadth first sequence(no loop), and make use of the direct connected function.
	/// </summary>
	/// <returns>The tree from breadth first sequence.</returns>
	/// <param name="startNode">Start node.</param>
	public TreePath BuildTree(GraphPathNode startNode) {
		List<GraphPathNode> nodeList = new List<GraphPathNode>();
		
		BreadthFirstTraversal(startNode, (GraphPathNode node) => {
			nodeList.Add(node);
		});

		if (nodeList.Count == 0)
			return null;

		// Create all the TreePathNode array with the same layout as GraphPathNode List
		TreePathNode[] treePathNodeRepository = new TreePathNode[nodeList.Count];
		for (int i = 0; i < treePathNodeRepository.Length; i++) {
			treePathNodeRepository[i] = new TreePathNode(nodeList[i]);
		}

		// Algorithm: Loop through the TreePathNode List. Record the parent node, if current node is
		// directly connected to the parent node, then current node is the child of the parent node;
		// if not, advance the parent node till it is directly connected to the the current node.
		// Loop till the end of the TreePathNode list. Create Tree with root node at position 0.
		TreePathNode rootNode = treePathNodeRepository[0];
		int parentNodeIndex = 0;
		TreePathNode parentTreePathNode = rootNode;
		for (int i = 1; i < nodeList.Count; i++) {
			TreePathNode currentTreePathNode = treePathNodeRepository[i];
			if (IsDirectConnected(nodeList[parentNodeIndex], nodeList[i])) {
				parentTreePathNode.AddChild(currentTreePathNode);
			} else {
				while (!IsDirectConnected(nodeList[parentNodeIndex], nodeList[i])) {
					parentNodeIndex++;
				}
				parentTreePathNode = treePathNodeRepository[parentNodeIndex];
				parentTreePathNode.AddChild(currentTreePathNode);
			}
		}

		TreePath treePath = new TreePath(rootNode);
		return treePath;
	}

	public void BreadthFirstTraversal(GraphPathNode startNode, Action<GraphPathNode> action) {
		const int NODE_STATE_INIT = 0;
		const int NODE_STATE_INQUEUE = 1;
		const int NODE_STATE_DEQUEUED = 2;
		int[] state = new int[GetNodeNumber()];
		
		Queue<GraphPathNode> queue = new Queue<GraphPathNode>();
		queue.Enqueue(startNode);
		state[startNode._id] = NODE_STATE_INQUEUE;
		
		while(queue.Count != 0) {
			GraphPathNode node = queue.Dequeue();
			state[node._id] = NODE_STATE_DEQUEUED;
			action(node);
			
			for(int i = 0; i < node._adjacentNodes.Count; i++) {
				GraphPathNode adjacentNode = _nodes[node._adjacentNodes[i]];
				if (state[adjacentNode._id] == NODE_STATE_INIT) {
					queue.Enqueue(adjacentNode);
					state[adjacentNode._id] = NODE_STATE_INQUEUE;
				} else if (state[adjacentNode._id] == NODE_STATE_INQUEUE) {
					Debug.LogError("This graph doesn't allow loop, please adjust the level");
				}
			}	
		}
	}

	public bool BreadthFirstSearch(GraphPathNode searchNode, Action<GraphPathNode> action, bool stopIfFound = true) {
		const int NODE_STATE_INIT = 0;
		const int NODE_STATE_INQUEUE = 1;
		const int NODE_STATE_DEQUEUED = 2;
		bool found = false;

		int[] state = new int[GetNodeNumber()];

		if (GetNodeNumber() == 0)
			goto exit;

		GraphPathNode startNode = GetNodeById(0);

		Queue<GraphPathNode> queue = new Queue<GraphPathNode>();
		queue.Enqueue(startNode);
		state[startNode._id] = NODE_STATE_INQUEUE;
		
		while(queue.Count != 0) {
			GraphPathNode node = queue.Dequeue();
			state[node._id] = NODE_STATE_DEQUEUED;

			if (node == searchNode) {
				found = true;
				action(node);
				if (stopIfFound) break;
			}

			for(int i = 0; i < node._adjacentNodes.Count; i++) {
				GraphPathNode adjacentNode = _nodes[node._adjacentNodes[i]];
				if (state[adjacentNode._id] == NODE_STATE_INIT) {
					queue.Enqueue(adjacentNode);
					state[adjacentNode._id] = NODE_STATE_INQUEUE;
				} else if (state[adjacentNode._id] == NODE_STATE_INQUEUE) {
					Debug.LogError("This graph doesn't allow loop, please adjust the level");
				}
			}
		}

	exit:
		return found;
	}

	public UniqueList<GraphPathNode> GetInternalNodes_UnitTest() {
		return _nodes;
	}
}
