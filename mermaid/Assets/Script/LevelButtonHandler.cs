using UnityEngine;
using System.Collections;

public class LevelButtonHandler : MonoBehaviour {
	
	public void ButtonClicked(int level) {
		GameManager.Instance.LoadScene (level);
	}
	
}
