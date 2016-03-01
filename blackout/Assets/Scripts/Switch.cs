using UnityEngine;
using System.Collections.Generic;

public class Switch : ItemBase {
	public List<GameObject> _switchables;

	Animator _animator;
	bool _electrified;

	public bool IsOn {
		get { return _animator.GetBool("isOn"); }
		set { _animator.SetBool("isOn", value); }
	}

	void Awake() {
		_electrified = false;
		_animator = GetComponent<Animator>();
	}

	void Update() {
		UpdateTurnOffTimer(Time.deltaTime);
	}
	
	void Electrified() {
		if (!_electrified) {
			foreach(GameObject go in _switchables) {
				go.GetComponent<ISwitchable>().Switch();
			}
			_electrified = true;
			IsOn = true;
		}

		ResetTurnOffTimer();

	}
	
	protected override void OnTurnOffTimerTimeout () {
		if (_electrified) {
			foreach(GameObject go in _switchables) {
				go.GetComponent<ISwitchable>().Switch();
			}
			_electrified = false;
			IsOn = false;
		}
	}
}
