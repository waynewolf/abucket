using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SilkManager : MonoBehaviour {
	public Material silkMaterial;

	// silk width if we use linerenderer
	public float silkWidth = 0.1f;

	// The game allows only one silk at a particular time
	private Silk _silk;
	private GameObject _currentHangOn;
	private bool _climbedOnTop;

	void Start () {
		_climbedOnTop = false;
	}

	public bool CanSilk (Transform pointA, Transform pointB)
	{
		if (pointA.GetInstanceID() == pointB.GetInstanceID()) {
			Debug.LogWarning ("two endpoints are the same");
			return false;
		}

		if (!_climbedOnTop && _currentHangOn && _currentHangOn.GetInstanceID() == pointB.gameObject.GetInstanceID()) {
			Debug.Log ("No need to hang the silk twice on " + _currentHangOn.name);
			return false;
		}

		// TODO: other valid check

		return true;
	}

	public void ClimbToTheTop () {
		_climbedOnTop = true;
	}

	public Silk CreateSilk (Transform pointA, Transform pointB) {
		if (!CanSilk (pointA, pointB))
			return null;

		_climbedOnTop = false;
		RemoveSilkIfAny ();
		_silk = new Silk (silkWidth);

		DoCreateSilk (pointA, pointB);

		_currentHangOn = pointB.gameObject;

		return _silk;
	}
	
	public void RemoveSilkIfAny() {
		if (_silk != null) {
			_silk.Remove();
			_silk = null;
			_currentHangOn = null;
		}
	}

	private void DoCreateSilk(Transform pointA, Transform pointB)
	{
		//create "Chains Holder" object, used to make chains children of that object
		GameObject chainsHolder = new GameObject("Chains Holder");
		
		_silk.Shoot (chainsHolder, pointA, pointB, 20, silkMaterial);
	}

	void Update() {
		if (_silk != null) {
			_silk.Refresh ();
		}
	}
}
