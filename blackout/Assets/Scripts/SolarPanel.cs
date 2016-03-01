using UnityEngine;
using System.Collections;

public class SolarPanel : GenerateElectricItem {
	public float _electricGenerationDuration = 1f;
	private bool _working = false;
	private float _deltaTime;
	
	void Awake () {
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
		ResetTurnOffTimer();
		_working = true;
	}
	
	protected override void OnTurnOffTimerTimeout() {
		_working = false;
	}
}
