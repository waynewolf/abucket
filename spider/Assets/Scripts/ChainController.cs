using UnityEngine;
using System.Collections;

public class ChainController : MonoBehaviour {
	private Silk _silk;

	private enum State {
		Created,
		SpiderIsOn,
		SpiderAway,
	}

	private State _state;

	void Awake() {
		_state = State.Created;
	}

	public void SetSilk (Silk silk) {
		_silk = silk;
	}

	void Update () {
		if (_state == State.SpiderAway)
			Destroy (gameObject);
	}

	public void ClimbOn() {
		if (_state == State.Created)
			_state = State.SpiderIsOn;
	}

	public void GoAway() {
		if (_state == State.SpiderIsOn) {
			_silk.RemoveTailChain(gameObject);
			_state = State.SpiderAway;
		}
	}

	public void JumpOff() {
		if (_state == State.SpiderIsOn) {
			_state = State.Created;
		}
	}
}
