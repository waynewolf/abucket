using UnityEngine;
using System.Collections;

public class Destroyer : BOBehaviour {
	public bool _destroyOnAwake;
	public float _awakeDestroyDelay;
	public bool _findChild = false;
	public string _namedChild;

	void Awake () {
		if(_destroyOnAwake) {
			if(_findChild){
				Destroy (transform.Find(_namedChild).gameObject);
			} else {
				Destroy(gameObject, _awakeDestroyDelay);
			}
		}
	}
	
	void DestroyChildGameObject () {
		if(transform.Find(_namedChild).gameObject != null)
			Destroy (transform.Find(_namedChild).gameObject);
	}
	
	void DisableChildGameObject () {
		if(transform.Find(_namedChild).gameObject.activeSelf == true)
			transform.Find(_namedChild).gameObject.SetActive(false);
	}
	
}
