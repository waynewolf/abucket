using UnityEngine;
using System.Collections;

public class Fan : ItemBase {
	public float _generateWindInterval = 2f;
	public GameObject _windPrefab;
	private SpinWhenEnable _spinWhenEnable;
	private bool _electrified = false;

	private float _lastGenerateWindTime;

	void Awake () {
		_spinWhenEnable = GetComponentInChildren<SpinWhenEnable>();
	}
	
	void Electrified() {
		_electrified = true;
		_spinWhenEnable._enable = true;
		ResetTurnOffTimer();
	}

	void Update() {
		if (_electrified) {
			if (Time.time - _lastGenerateWindTime > _generateWindInterval) {
				GameObject windObject = Instantiate(_windPrefab, transform.position, Quaternion.identity) as GameObject;
				windObject.name = "*Wind*";
				windObject.transform.SetParent(LevelManager.Instance.GetDynamicGameObjectHolder().transform);
				Wind wind = windObject.GetComponent<Wind>();
				wind.Blow(_faceDirection);
				_lastGenerateWindTime = Time.time;
			}
			UpdateTurnOffTimer(Time.deltaTime);
		}
	}

	protected override void OnTurnOffTimerTimeout () {
		_electrified = false;
		_spinWhenEnable._enable = false;
	}
}
