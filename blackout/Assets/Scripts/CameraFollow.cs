using UnityEngine;
using System.Collections;

public class CameraFollow : BOBehaviour {
	Transform _target;
	bool _follow;

	void Awake () {
		_follow = true;
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if (player == null) {
			Debug.LogError("No player in the scene!");
			return;
		}
		_target = player.transform;
	}
	
	void LateUpdate() {
		if (_follow) {
			Vector3 velocity = Vector3.zero;
			float smoothTime = 0.3f;
			transform.position = Vector3.SmoothDamp(transform.position, _target.position, ref velocity, smoothTime);
		}
	}

	public void Stop() {
		_follow = false;
	}
}
