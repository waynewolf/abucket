using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameOverMenuHandler : MonoBehaviour {

	public Button toMainMenuButton;
	public Button replayButton;
	
	private float savedTimeScale;
	
	void Awake () {
		savedTimeScale = Time.timeScale;
		toMainMenuButton.onClick.RemoveAllListeners ();
		toMainMenuButton.onClick.AddListener (ToMainMenu);
		toMainMenuButton.onClick.AddListener (CloseGameOverMenu);
		replayButton.onClick.RemoveAllListeners ();
		replayButton.onClick.AddListener (Replay);
		replayButton.onClick.AddListener (CloseGameOverMenu);
		// deactivate this pause menu by default
		gameObject.SetActive (false);
	}
	
	void Update () {
		
	}
	
	void OnEnable() {
		savedTimeScale = Time.timeScale;
		Time.timeScale = 0;
		transform.SetAsLastSibling ();
	}
	
	void OnDisable() {
		Time.timeScale = savedTimeScale;
	}
	
	void ToMainMenu() {
		GameManager.Instance.LoadScene ("Main");
	}
	
	void Replay() {
		GameManager.Instance.Replay ();
	}
	
	void CloseGameOverMenu() {
		gameObject.SetActive (false);
	}
}
