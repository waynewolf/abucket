using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelSelectMenuHandler : MonoBehaviour {

	public Button toMainMenuButton;
	public Button replayButton;
	public Button nextLevelButton;
	
	void Awake () {
		toMainMenuButton.onClick.RemoveAllListeners ();
		toMainMenuButton.onClick.AddListener (ToMainMenu);
		toMainMenuButton.onClick.AddListener (CloseLevelSelectMenu);
		replayButton.onClick.RemoveAllListeners ();
		replayButton.onClick.AddListener (Replay);
		replayButton.onClick.AddListener (CloseLevelSelectMenu);
		nextLevelButton.onClick.RemoveAllListeners ();
		nextLevelButton.onClick.AddListener (NextLevel);
		nextLevelButton.onClick.AddListener (CloseLevelSelectMenu);
		gameObject.SetActive (false);
	}
	
	void OnEnable() {
		transform.SetAsLastSibling ();
	}
	
	void ToMainMenu() {
		GameManager.Instance.ToMain ();
	}
	
	void Replay() {
		GameManager.Instance.Replay ();
	}

	void NextLevel() {
		GameManager.Instance.NextLevel ();
	}
	
	void CloseLevelSelectMenu() {
		gameObject.SetActive (false);
	}
}
