using UnityEngine;
using System.Collections;

public class AutoSpin : MonoBehaviour {
	public float angleSpeed = 100f;
	public bool counterClockwise = true;

	void Update() {
		transform.transform.rotation = 
			Quaternion.AngleAxis (angleSpeed * Time.time, counterClockwise ? Vector3.back : Vector3.forward);
	}

}
