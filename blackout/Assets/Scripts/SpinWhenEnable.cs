using UnityEngine;
using System.Collections;

public class SpinWhenEnable : BOBehaviour {
	public bool _enable = false;
	public float _angleSpeed = 100f;
	public Vector3 _aroundAxis = Vector3.up;

	void Update () {
		if (_enable) {
			transform.Rotate(_aroundAxis, _angleSpeed * Time.deltaTime, Space.Self);
		}
	}
}
