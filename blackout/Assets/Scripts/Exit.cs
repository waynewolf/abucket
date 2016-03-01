using UnityEngine;
using System.Collections;

public class Exit : ItemBase {
	private bool _electrified = false;
	public float _angleSpeed = 100f;
	public Vector3 _aroundAxis = Vector3.up;

	void Update() {
		if (_electrified) {
			transform.rotation = Quaternion.AngleAxis(_angleSpeed * Time.time, _aroundAxis);
			UpdateTurnOffTimer(Time.deltaTime);
		}
	}

	void Electrified() {
		_electrified = true;
		ResetTurnOffTimer();
	}

	protected override void OnTurnOffTimerTimeout () {
		_electrified = false;
	}
}
