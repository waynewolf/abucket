using UnityEngine;
using System.Collections;

public class Windmill : GenerateElectricItem {
	public float _electricGenerationDuration = 1f;
	private SpinWhenEnable _spinWhenEnable;
	private bool _working = false;
	private float _deltaTime;

	void Awake () {
		_spinWhenEnable = GetComponentInChildren<SpinWhenEnable>();
		_deltaTime = 0;
	}

	void Update () {
		if (_working) {
			_deltaTime += Time.deltaTime;
			if (_deltaTime > _electricGenerationDuration) {
				base.GenerateElectric();
				_deltaTime = 0;
			}
			UpdateTurnOffTimer(Time.deltaTime);
		}
	}

	void GenerateElectricMessage() {
		_spinWhenEnable._enable = true;
		ResetTurnOffTimer();
		_working = true;
	}

	protected override void OnTurnOffTimerTimeout() {
		_spinWhenEnable._enable = false;
		_working = false;
	}
	
}
