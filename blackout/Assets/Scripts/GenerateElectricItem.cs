using UnityEngine;
using System.Collections;

public abstract class GenerateElectricItem : ItemBase {
	public GameObject _electricCurrentPrefab;

	protected virtual void GenerateElectric() {
		GraphPath wireGraph = LevelManager.Instance.GetWireGraph();
		GraphPathNode startNode = wireGraph.GetNodeByEndpoint(transform.position);
		if (startNode != null) {
			TreePath treePath = wireGraph.BuildTree(startNode);
			TreePathNode rootNode = treePath.GetRoot();
			TreePathNode[] childrenNode = rootNode.GetChildren();
			for (int i = 0; i < childrenNode.Length; i++) {
				GameObject electricCurrentObj = Instantiate (_electricCurrentPrefab, transform.position, Quaternion.identity) as GameObject;
				electricCurrentObj.name = "*ElectricCurrent-" + startNode._id + "-" + childrenNode[i]._id + "*";
				electricCurrentObj.transform.SetParent(LevelManager.Instance.GetElectricCurrentGameObjectHolder().transform);
				ElectricCurrent electricCurrent = electricCurrentObj.GetComponent<ElectricCurrent>();
				electricCurrent.MoveToNode(childrenNode[i]);
			}
		}
	}
}
