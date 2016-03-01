using UnityEngine;
using System.Collections;

public class OptionMenu : BOBehaviour {
	private Animator _animator;
	//private CanvasGroup _canvasGroup;
	
	public bool IsUnfold {
		get { return _animator.GetBool("IsUnfold"); }
		set { _animator.SetBool("IsUnfold", value); }
	}

	void Awake() {
		_animator = GetComponent<Animator>();
		//_canvasGroup = GetComponent<CanvasGroup>();

		//var rect = GetComponent<RectTransform>();
		// Move the UI to the center when playing, this allows UI
		// put in different position, easy for designing GUI.
		//rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
	}

	void Update() {
//		if (!_animator.GetCurrentAnimatorStateInfo(0).IsName ("Open")) {
//			_canvasGroup.blocksRaycasts = _canvasGroup.interactable = false;
//		} else {
//			_canvasGroup.blocksRaycasts = _canvasGroup.interactable = true;
//		}
	}

	public void ToggleSwitch() {
		IsUnfold = !IsUnfold;
	}

	public void ExitLevelClicked() {
		IsUnfold = false;
	}

	public void RestartLevelClicked() {
		IsUnfold = false;
	}

	public void LoseFocusMessage() {
		if (IsUnfold)
			IsUnfold = false;
	}
}
