using UnityEngine;
using System.Collections;

// Move object with right mouse for debugging
public class MoveableInEditor: MonoBehaviour {
	private bool _startDrag = false;

	void Update () {
#if UNITY_EDITOR
		if (Input.GetMouseButtonDown(1)) {
			Vector2 mousePos = Input.mousePosition;
			Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
			Collider2D collider2D = Physics2D.OverlapPoint (worldPos);
			// collider with myself
			if (collider2D && collider2D.gameObject.name == gameObject.name) {
				_startDrag = true;
			}
		}

		if (Input.GetMouseButtonUp(1)) {
			_startDrag = false;
		}

		if (_startDrag) {
			Vector2 mousePos = Input.mousePosition;
			Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
			transform.position = worldPos;
		}
#endif
	}

}
