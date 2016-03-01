using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Use singleton to avoid DontDestroyOnLoad drawback that you have to go back
// to the scene where GameManager is resided. Do not attach this script to any
// game object, simply use GameManager.Instance.XXX wherever you want.
public sealed class GameManager {
	private static GameManager instance = null;

	private GameObject uiCanvas;
	private GameObject levelSelectMenu;	
	private GameObject pauseMenu;
	private GameObject gameOverMenu;
	private Text scoreText;
	private GameObject pauseButton;

	private MermaidController mermaid;

	private const int MAX_LEVEL = 2;

	public static GameManager Instance {
		get {
			if (instance == null) {
				instance = new GameManager();
			}
			return instance;
		}
	}

	public MermaidController Mermaid {
		get {
			return mermaid;
		}
		set {
			mermaid = value;
		}
	}

	#region scene management

	public void LoadScene(int level) {
		Application.LoadLevel (level);
	}

	public void LoadScene(string levelName) {
		Application.LoadLevel (levelName);
	}

	// need MonoBehaviour to use StartCoroutine
	public void LoadSceneInBackground (MonoBehaviour behaviour, string levelName, bool immediate, float minWaitTime) {
		behaviour.StartCoroutine (LoadOrWait (levelName, immediate, minWaitTime));
	}

	public void LoadSceneInBackground (MonoBehaviour behaviour,int level, bool immediate, float minWaitTime) {
		behaviour.StartCoroutine (LoadOrWait (level, immediate, minWaitTime));
	}

	#endregion

	#region UI methods

	public void ShowUI(bool show) {
		if (uiCanvas) {
			uiCanvas.SetActive (show);
		}
	}

	public void ShowLevelSelectMenu() {
		Pause ();
		levelSelectMenu.SetActive (true);
	}

	public void ShowPauseMenu() {
		Pause ();
		pauseMenu.SetActive (true);
	}

	public void ShowGameOverMenu() {
		gameOverMenu.SetActive (true);
	}

	public void ShowScore (int score) {
		scoreText.text = "Score: " + score;
	}
	
	#endregion
	
	public void ToMain ()
	{
		Application.LoadLevel ("Main");
		Resume ();
	}

	public void Pause() {
		Time.timeScale = 0;
	}

	public void Resume() {
		Time.timeScale = 1f;
	}

	public void Replay ()
	{
		Time.timeScale = 1f;
		Application.LoadLevel (Application.loadedLevel);
	}

	public void NextLevel ()
	{
		Time.timeScale = 1f;
		if (Application.loadedLevel < MAX_LEVEL)
			Application.LoadLevel (Application.loadedLevel + 1);
		else
			Application.LoadLevel ("Main");
	}

	#region private methods

	private GameManager() {
		Debug.Log ("GameMnager() ctor");
		GameObject uiCanvasPrefab = Resources.Load ("UI") as GameObject;
		if (!uiCanvasPrefab) {
			Debug.LogError("UI resource not found");
		}

		uiCanvas = Object.Instantiate (uiCanvasPrefab);
		if (!uiCanvas) {
			Debug.LogError ("Failed to instantiate UI");
		}

		Object.DontDestroyOnLoad (uiCanvas);

	    levelSelectMenu = uiCanvas.transform.Find ("LevelSelectMenu").gameObject;
		if (!levelSelectMenu) {
			Debug.LogError("LevelSelectMenu not found");
		}
		
		pauseMenu = uiCanvas.transform.Find ("PauseMenu").gameObject;
		if (!pauseMenu) {
			Debug.LogError("PauseMenu not found");
		}

		gameOverMenu = uiCanvas.transform.Find ("GameOverMenu").gameObject;
		if (!gameOverMenu) {
			Debug.LogError("GameOverMenu not found");
		}

		scoreText = uiCanvas.transform.Find ("ScoreText").gameObject.GetComponent<Text>();
		if (!scoreText) {
			Debug.LogError("ScoreText not found");
		}

		pauseButton = uiCanvas.transform.Find ("PauseButton").gameObject;
		if (!pauseButton) {
			Debug.LogError("PauseButton not found");
		}
	}

	~GameManager() {
		Debug.Log ("~GameManager() dtor, shouldn't happen");
	}

	private IEnumerator LoadOrWait(string levelName, bool immediate, float minWaitTime) {
		float elapsedTime = 0f;
		AsyncOperation async = Application.LoadLevelAsync (levelName);
		if (!immediate)
			async.allowSceneActivation = false;
		while ( elapsedTime < minWaitTime) {
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		if (!immediate)
			async.allowSceneActivation = true;
	}
	
	private IEnumerator LoadOrWait(int level, bool immediate, float minWaitTime) {
		float elapsedTime = 0f;
		AsyncOperation async = Application.LoadLevelAsync (level);
		if (!immediate)
			async.allowSceneActivation = false;
		while ( elapsedTime < minWaitTime) {
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		if (!immediate)
			async.allowSceneActivation = true;
	}

	#endregion private methods
}
