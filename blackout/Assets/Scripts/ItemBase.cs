using UnityEngine;
using System.Collections;

[System.Serializable]
public class ItemIdentity {
	public int _id;
	public int _type;

	public ItemIdentity(int id, int type) {
		_id = id;
		_type = type;
	}
}

public class ItemBase : BOBehaviour {
	[SerializeField] private ItemIdentity _identity;

	public int ID { get { return _identity._id; } }
	public int Type { get { return _identity._type; } }

	public Vector3 _faceDirection;	// (0, 0, 0) disable facing direction adjustment
	public float _turnOffDeviceTimer = 2f;

	private ContextMenu _contextMenu;
	private bool _isRecycleable;	// statically placed in the scene, or recycleable to backpack system
	private float _savedTurnOffDeviceTimer;

	void Start() {
		_isRecycleable = LevelManager.Instance.GameObjectExistInBackpack(gameObject);
		_savedTurnOffDeviceTimer = _turnOffDeviceTimer;
	}

	protected virtual void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") {
			if (_isRecycleable && !LevelManager.Instance.GetPowerSource().IsWorking) {
				if (_contextMenu != null)
					_contextMenu.Close();
				ContextMenu contextMenu = GUIManager.Instance.CreateContextMenu();
				_contextMenu = contextMenu;
				contextMenu._onAdjustButtonClicked = () => {
					if (IsAdjustable()) Adjust();
				};
				contextMenu._onRecycleButtonClicked = () => {
					LevelManager.Instance.RecycleGameObject(gameObject);
					_contextMenu.Close ();
				};
			}
		}
	}
	
	protected virtual void OnTriggerExit(Collider other) {
		if (other.tag == "Player") {
			if (_contextMenu != null) {
				_contextMenu.Close ();
				_contextMenu = null;
			}
		}
	}

	public bool IsAdjustable() {
		return _faceDirection != Vector3.zero;
	}
	
	public void Adjust() {
		if (_faceDirection.x != 0 || _faceDirection.z != 0) {
			// counter clockwise rotation around y axis if facing x or z
			gameObject.transform.Rotate(0, -90, 0, Space.Self);
			if (_faceDirection.x != 0) {
				// the sign of the next facing direction of z doesn't change
				_faceDirection.z = _faceDirection.x;
				_faceDirection.x = 0;
			} else {
				// the sign of the next facing direction of x does change
				_faceDirection.x = -_faceDirection.z;
				_faceDirection.z = 0;
			}
		} else {
			gameObject.transform.Rotate (180, 0, 0, Space.Self);
			// flip facing direction of y
			_faceDirection.y = -_faceDirection.y;
		}
	}

	protected void UpdateTurnOffTimer(float time) {
		_turnOffDeviceTimer -= time;
		if (_turnOffDeviceTimer < float.Epsilon) {
			OnTurnOffTimerTimeout();
		}
	}

	protected void ResetTurnOffTimer() {
		_turnOffDeviceTimer = _savedTurnOffDeviceTimer;
	}

	protected virtual void OnTurnOffTimerTimeout() {
	}
}
