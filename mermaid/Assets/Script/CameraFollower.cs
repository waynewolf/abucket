using UnityEngine;
using System.Collections;

public class CameraFollower : MonoBehaviour {
	public Transform target;

	private float xOffset;
	private Camera childCamera;

	void Awake() {
		childCamera = GetComponentInChildren<Camera> ();
		if (!childCamera) {
			Debug.LogError("no child camera found");
		}
	}

	void Start() {
		xOffset = childCamera.transform.position.x - target.position.x;
	}

	void Update () {
		//follow the target on the x-axis only
		transform.position = new Vector3 (target.position.x + xOffset, transform.position.y, transform.position.z);
	}
}
