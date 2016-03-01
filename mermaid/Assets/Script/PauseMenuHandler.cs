using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseMenuHandler : MonoBehaviour {

	public Button toMainMenuButton;
	public Button resumeButton;
	
	void Awake () {
		toMainMenuButton.onClick.RemoveAllListeners ();
		toMainMenuButton.onClick.AddListener (ToMainMenu);
		toMainMenuButton.onClick.AddListener (ClosePauseMenu);
		resumeButton.onClick.RemoveAllListeners ();
		resumeButton.onClick.AddListener (Resume);
		resumeButton.onClick.AddListener (ClosePauseMenu);
		// deactivate this pause menu by default
		gameObject.SetActive (false);
	}

	void OnEnable() {
		transform.SetAsLastSibling ();
	}

	void Update () {

	}

	void ToMainMenu() {
		GameManager.Instance.ToMain ();
	}

	void Resume() {
		GameManager.Instance.Resume ();
	}

	void ClosePauseMenu() {
		gameObject.SetActive (false);
	}
}
