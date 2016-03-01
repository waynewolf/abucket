using UnityEngine;
using System.Collections;

public class ElectricCurrent : DynamicItem {
	public float _speed = 2f;
	public GameObject _electricCurrentPrefab;

	private bool _enable;
	private Vector3 _initPosition;
	private TreePathNode _targetNode;
	private float _startTime;
	private float _journeyLength;

	void Awake() {
		_enable = false;
		_initPosition = transform.position;
	}

	void Update () {
		if (_enable) {
			float distCovered = (Time.time - _startTime) * _speed;
			float fracJourney = distCovered / _journeyLength;
			fracJourney = Mathf.Clamp(fracJourney, 0, 1);
			transform.position = Vector3.Lerp (_initPosition, _targetNode._endPoint, fracJourney);

			if ( Mathf.Abs(fracJourney - 1) < float.Epsilon) {
				// Reach destination, it's the end of the lifetime
				_enable = false;
				FloodToNextHop();
				Destroy(gameObject);
			}
		}
	}

	public void MoveToNode (TreePathNode targetNode) {
		_targetNode = targetNode;
		_journeyLength = Vector3.Distance(_initPosition, _targetNode._endPoint);
		_enable = true;
		_startTime = Time.time;
	}

	public void FloodToNextHop() {
		TreePathNode[] childrenNode = _targetNode.GetChildren();
		for (int i = 0; i < childrenNode.Length; i++) {
			GameObject electricCurrentObj = Instantiate (_electricCurrentPrefab, transform.position, Quaternion.identity) as GameObject;
			electricCurrentObj.name = "*ElectricCurrent-" + _targetNode._id + "-" + childrenNode[i]._id + "*";
			electricCurrentObj.transform.SetParent(LevelManager.Instance.GetElectricCurrentGameObjectHolder().transform);
			ElectricCurrent electricCurrent = electricCurrentObj.GetComponent<ElectricCurrent>();
			electricCurrent.MoveToNode(childrenNode[i]);
		}
	}

}
