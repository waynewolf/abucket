using UnityEngine;
using System.Collections;

public class Checkpoint : ItemBase {
	public GameObject _cameraTarget;
	public Transform _exitFlag;

	protected override void OnTriggerEnter (Collider other) {
		if (other.tag == "Player") {
			//1. Notify camera to stop following player
			_cameraTarget.GetComponent<CameraFollow>().Stop();

			//2. Move player to the exit flag without camera following
			LevelManager.Instance.Player.MoveToTarget(_exitFlag);
		}
	}
}
