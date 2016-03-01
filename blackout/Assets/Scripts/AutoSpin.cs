using UnityEngine;
using System.Collections;

public class AutoSpin : BOBehaviour {

	public float _angleSpeed = 100f;
	public Vector3 _aroundAxis = Vector3.up;

	void Update () {
		transform.rotation = Quaternion.AngleAxis(_angleSpeed * Time.time, _aroundAxis);
	}

}
