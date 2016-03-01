using UnityEngine;
using System.Collections;

public class Platform : BOBehaviour, ISwitchable {
	// none of them is allowed true simultaneously
	public bool _xMoveable = false;
	public bool _yMoveable = false;
	public bool _zMoveable = false;

	public float _moveTo;

	private Vector3 _savedOriginalPosition;
	private bool _inOriginalPosition;

	void Awake() {
		_savedOriginalPosition = transform.position;
		_inOriginalPosition = true;
	}

	public void Switch() {
		if (!(_xMoveable || _yMoveable || _zMoveable))
			return;

		Vector3 targetPosition = _savedOriginalPosition;
		if (!_inOriginalPosition)
			targetPosition = _savedOriginalPosition;
		else {
			if (_xMoveable)
				targetPosition.x = _moveTo;
			else if (_yMoveable)
				targetPosition.y = _moveTo;
			else if (_zMoveable)
				targetPosition.z = _moveTo;
		}
		iTween.MoveTo(gameObject, targetPosition, 1f);
		_inOriginalPosition = !_inOriginalPosition;
	}
}
