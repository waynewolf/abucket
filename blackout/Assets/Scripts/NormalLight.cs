using UnityEngine;
using System.Collections;

public class NormalLight : DynamicItem {
	public float _speed = 2f;
	private bool _moveStart = false;
	
	void Update () {
		if (_moveStart) {
			transform.position += _speed * Time.deltaTime * _direction;
		}
	}
	
	public void Shine (Vector3 direction) {
		_direction = direction;
		_moveStart = true;
	}
}
