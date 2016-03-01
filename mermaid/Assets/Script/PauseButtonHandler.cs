using UnityEngine;
using System.Collections;

public class PauseButtonHandler : MonoBehaviour {

	public void ButtonClicked() {
		GameManager.Instance.ShowPauseMenu ();
	}
}
