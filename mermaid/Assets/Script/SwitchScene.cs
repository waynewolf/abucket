using UnityEngine;
using System.Collections;

public class SwitchScene : MonoBehaviour {
	public string levelName = "Main";
	public float minWaitTime = 2f;

	void Start() {
		GameManager.Instance.LoadSceneInBackground (this, levelName, false, minWaitTime);
	}

}
