using UnityEngine;
using System.Collections;

public class KillSelfAndExplode : MonoBehaviour {

	// spawned from object pool, do not destory myself
//	void Start() {
//		Invoke ("DeactivateMe", 20);
//	}
//
//	void DeactivateMe() {
//		gameObject.SetActive (false);
//	}

	void OnTriggerEnter2D(Collider2D col) {
		string colliderTag = col.gameObject.tag;
		if (colliderTag == "Player") {
			// Do not destory because the object is in pool
			gameObject.SetActive(false);
			CreateExplosion();
		}
	}

	void CreateExplosion() {
	}
}
