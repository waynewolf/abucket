using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackMenu : BOBehaviour, IPointerDownHandler {
	public GameObject _itmePreviewPrefab;			// This is the prefab inserted to the scrollable area
	public GameObject _itemPreviewHolder;           // This is the scroll content that hold the preview items
	private Animator _animator;
	private List<GameObject> _backpackItemsGameObject; // The indexes of these two lists are corelated
	private List<GameObject> _backpackItemsPreview;
	private bool _backpackItemsPopulated;

	public bool IsEntered {
		get { return _animator.GetBool("IsEntered"); }
		set { _animator.SetBool("IsEntered", value); }
	}

	void Awake() {
		_animator = GetComponent<Animator>();
		_backpackItemsGameObject = null;
		_backpackItemsPopulated = false;
	}

	void Start() {
		_backpackItemsGameObject = LevelManager.Instance.GetCurrentItemsInBackpack();
		_backpackItemsPreview = new List<GameObject>();
		PopulateItems(_backpackItemsGameObject);
		LevelManager.Instance._onRecycleItem += (int index) => {
			_backpackItemsPreview[index].SetActive(true);
		};
	}

	void PopulateItems(List<GameObject> _backpackItems) {
		if (_backpackItemsPopulated || _backpackItems == null) return;
		foreach(GameObject go in _backpackItems) {
			SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
			if (sr != null) {
				GameObject itemPreview = Instantiate(_itmePreviewPrefab);
				itemPreview.transform.SetParent(_itemPreviewHolder.transform, false);
				itemPreview.name = "*ItemPreview*";
				Image image = itemPreview.transform.FindChild("Image").gameObject.GetComponent<Image>();
				image.sprite = sr.sprite;
				itemPreview.GetComponent<Toggle>().group = _itemPreviewHolder.GetComponent<ToggleGroup>();
				_backpackItemsPreview.Add (itemPreview);
			}
		}
		_backpackItemsPopulated = true;
	}

	public void ToggleSwitch() {
		IsEntered = !IsEntered;
	}

	public void LoseFocusMessage() {
		if (IsEntered)
			IsEntered = false;
	}

	public void ReturnButtonClicked() {
		IsEntered = false;
	}

	public void ItemSelectedButtonClicked() {
		// Find a preview item that the toggle is on
		int index;
		for (index = 0; index < _backpackItemsPreview.Count; index++) {
			if (_backpackItemsPreview[index].GetComponent<Toggle>().isOn)
				break;
		}

		if (index < _backpackItemsPreview.Count && !LevelManager.Instance.GetPowerSource().IsWorking) {
			Vector3 itemPosition = LevelManager.Instance.Player.transform.position;
			itemPosition.y = Mathf.FloorToInt(itemPosition.y + 0.5f);
			_backpackItemsGameObject[index].transform.position = itemPosition;
			_backpackItemsGameObject[index].SetActive(true);
			_backpackItemsPreview[index].GetComponent<Toggle>().isOn = false;
			_backpackItemsPreview[index].SetActive(false);
		}

		IsEntered = false;
	}

	#region IPointerDwonHandler implementation

	// the empty implementation is used to block click,
	// make IPointerOverGameObject() return true if pointer
	// over this menu (basically an UI panel).
	public void OnPointerDown (PointerEventData eventData) {
	}

	#endregion
}
