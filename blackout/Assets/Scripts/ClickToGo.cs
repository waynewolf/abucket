using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.EventSystems;

public class ClickToGo : BOBehaviour {

	void Start () {
	}
	
	void Update () {
		bool clicked = false;
		Vector3 clickPosition = Vector3.zero;
		if (Input.touchCount > 0) {
			Touch touch = Input.GetTouch(0);
			if (touch.phase == TouchPhase.Began) {
				clicked = true;
				clickPosition = touch.position;
			}
		}
#if UNITY_EDITOR
		else {
			if (Input.GetMouseButtonUp(0)) {
				clicked = true;
				clickPosition = Input.mousePosition;
			}
		}
#endif
		if (clicked) {
			RaycastHit hitInfo;
			Ray ray = Camera.main.ScreenPointToRay (clickPosition);
			if (!EventSystem.current.IsPointerOverGameObject()) {
				if (Physics.Raycast(ray, out hitInfo, 100)) {
					// We are here means we don't hit the UI, so we have to tell
					// GUI manaer that UI menu(if any) to lose focus, thus need
					// to be closed.
					GUIManager.Instance.OnPointerDown(null);
					Vector3 targetPosition = transform.position;
					targetPosition.x = Mathf.RoundToInt(hitInfo.point.x);
					targetPosition.z = Mathf.RoundToInt(hitInfo.point.z);
					// FIXME: delete this when hero character is ready
					StartCoroutine(SmoothMovement(transform, targetPosition));
				}
			}
		}
	}

	IEnumerator SmoothMovement(Transform transform, Vector3 targetPosition) {
		Vector3 startPosition = transform.position;
		float t = 0;
		while (t <= 1f) {
			transform.position = Vector3.Lerp(startPosition, targetPosition, t);
			t += 0.1f;
			yield return new WaitForFixedUpdate();
		}
		transform.position = targetPosition;
		yield return null;
	}
}
