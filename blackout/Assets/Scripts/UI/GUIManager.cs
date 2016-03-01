using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GUIManager : Singleton<GUIManager>, IPointerDownHandler {
	public GameObject _contextMenuPrefab;
	public GameObject _powerSourceContextMenuPrefab;

	// This hack that using a whole screen Image component in Canvas to receive "menu lose focus event" has
	// conflicts with EventSystem IsPointerOverGameObject function, game scene will never receive Raycast,
	// so we don't use this hack. Since we now remove the Image component in Canvas, this function is now
	// NOT an IPointerDownHandler interface, but a normal function that is called by our code.
	public void OnPointerDown(PointerEventData eventData) {
		BroadcastMessage("LoseFocusMessage");
	}

	public ContextMenu CreateContextMenu() {
		GameObject contextMenuObject = Instantiate(_contextMenuPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		contextMenuObject.name = "*ContextMenu*";
		contextMenuObject.transform.SetParent(gameObject.transform, false);
		return contextMenuObject.GetComponent<ContextMenu>();
	}

	public ContextMenu CreatePowerSourceContextMenu() {
		GameObject contextMenuObject = Instantiate(_powerSourceContextMenuPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		contextMenuObject.name = "*PowerSourceContextMenu*";
		contextMenuObject.transform.SetParent(gameObject.transform, false);
		return contextMenuObject.GetComponent<ContextMenu>();
	}
}
