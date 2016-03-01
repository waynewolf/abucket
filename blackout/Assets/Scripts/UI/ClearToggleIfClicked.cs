using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClearToggleIfClicked : BOBehaviour, IPointerDownHandler {

	public void OnPointerDown (PointerEventData eventData) {
		Toggle[] toggles = GetComponentsInChildren<Toggle>();
		foreach(Toggle toggle in toggles)
			toggle.isOn = false;
	}

}
