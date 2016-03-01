using UnityEngine;
using System.Collections;

public delegate void ButtonClicked();

public class ContextMenu : BOBehaviour {

	[HideInInspector]
	public ButtonClicked _onRecycleButtonClicked;
	[HideInInspector]
	public ButtonClicked _onAdjustButtonClicked;
	[HideInInspector]
	public ButtonClicked _onEnableButtonClicked;

	public void RecycleButtonClickec() {
		if (_onRecycleButtonClicked != null)
			_onRecycleButtonClicked();
	}

	public void AdjustButtonClicked() {
		if (_onAdjustButtonClicked != null)
			_onAdjustButtonClicked();
	}

	public void EnableButtonClicked() {
		if (_onEnableButtonClicked != null)
			_onEnableButtonClicked();
	}
	
	public void Close() {
		Destroy(gameObject);
	}

}
