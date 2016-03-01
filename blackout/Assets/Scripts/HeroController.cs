using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeroController : BOBehaviour {
	public float _speed = 0.1f;
	
	bool _swipeTouchBegan = false;
	Vector3 _initialTouchPosition;
	Vector3 _currentTouchPosition;
	readonly float moveThreashold = 10f;
	Rigidbody _rigidbody;
	Animator _animator;
	Vector3 _faceDirection;
	Transform _groundCheck;
	bool _touchLadder = false;
	Vector3 _ladderFacingDirection;
	Vector3 _ladderPosition;
	bool _isGrounded;
	bool _inputDisabled;

	bool OnGround {
		get { return _isGrounded; }
		set {
			if (value && !_isGrounded) {
				_touchLadder = false;
			}
			_isGrounded = value;
		}
	}

	bool OnLadder {
		get { return _touchLadder; }
		set {
			_touchLadder = value;
			if (_touchLadder) _rigidbody.isKinematic = true;
			else _rigidbody.isKinematic = false;
		}
	}

	void Awake() {
		_rigidbody = GetComponent<Rigidbody>();
		_animator = GetComponent<Animator>();
		_faceDirection = Vector3.right;
		_groundCheck = transform.FindChild("GroundCheck").transform;
		_inputDisabled = false;
	}
	
	void FixedUpdate() {
		if (_inputDisabled) return;

		Ray ray = new Ray(_groundCheck.position, Vector3.down);
		bool isGrounded = Physics.Raycast(ray, 0.1f);
		OnGround = isGrounded;
		_animator.SetBool("isGrounded", OnGround);

		if (Input.touchCount > 0 ) {
			Touch touch = Input.GetTouch(0);
			if (!IsPointerOverUIObject()) {
				// We are here means we don't hit the UI, so we have to tell
				// GUI manaer that UI menu(if any) to lose focus, thus need
				// to be closed.
				GUIManager.Instance.OnPointerDown(null);

				if (touch.phase == TouchPhase.Began) {
					_swipeTouchBegan = true;
					_initialTouchPosition = touch.position;
					_currentTouchPosition = _initialTouchPosition;
				}
			}
			if (touch.phase == TouchPhase.Moved && _swipeTouchBegan) {
				_currentTouchPosition = touch.position;
			} else if (touch.phase == TouchPhase.Ended && _swipeTouchBegan) {
				_currentTouchPosition = MathUtil.Vector3RoundToInt(touch.position);
				_swipeTouchBegan = false;
			}
		} else {
			_swipeTouchBegan = false;
		}

		if (_swipeTouchBegan) {
			Vector3 delta = _currentTouchPosition - _initialTouchPosition;

			float deltaX = delta.x;
			float deltaY = delta.y;

			Vector3 moveDirection = Vector3.zero;

			if (deltaX > moveThreashold) {
				if (deltaY > 0)
					moveDirection = Vector3.forward;
				else if (deltaY < 0)
					moveDirection = Vector3.right;
			} else if (deltaX < -moveThreashold) {
				if (deltaY > 0)
					moveDirection = Vector3.left;
				else if (deltaY < 0)
					moveDirection = Vector3.back;
			}

			if (moveDirection == Vector3.right) {
				if (_faceDirection != moveDirection) {
					transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
					_faceDirection = moveDirection;
				}
			}
			else if (moveDirection == Vector3.left) {
				if (_faceDirection != moveDirection) {
					transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
					_faceDirection = moveDirection;
				}
			}
			else if (moveDirection == Vector3.forward) {
				if (_faceDirection != moveDirection) {
					transform.rotation = Quaternion.AngleAxis(-90, Vector3.up);
					_faceDirection = moveDirection;
				}
			}
			else if (moveDirection == Vector3.back) {
				if (_faceDirection != moveDirection) {
					transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
					_faceDirection = moveDirection;
				}
			}

			if (OnLadder) {
				// make sure the hero is inside the trigger,
				// avoid in and out again and again.
				Vector3 adjustedPosition = transform.position;
				if (Mathf.Abs (moveDirection.x) == 1)
					adjustedPosition.x = _ladderPosition.x + 0.1f * _ladderFacingDirection.x;
				else if (Mathf.Abs (moveDirection.z) == 1)
					adjustedPosition.z = _ladderPosition.z + 0.1f * _ladderFacingDirection.z;

				transform.position = adjustedPosition;

				if (moveDirection == -_ladderFacingDirection) {
					iTween.MoveUpdate(gameObject, iTween.Hash(
						"position", transform.position + _speed * Vector3.up,
						"speed", _speed,
						"easetype", iTween.EaseType.linear
						));
					_animator.SetFloat("speed", 0);
				} else if (moveDirection == _ladderFacingDirection) {
					iTween.MoveUpdate(gameObject, iTween.Hash(
						"position", transform.position + _speed * Vector3.down,
						"speed", _speed,
						"easetype", iTween.EaseType.linear
						));
					_animator.SetFloat("speed", 0);
				}

			}
			else {
				iTween.MoveUpdate(gameObject, iTween.Hash(
					"position", transform.position + _speed * moveDirection,
					"speed", _speed,
					"easetype", iTween.EaseType.linear
					));
				_animator.SetFloat("speed", _speed);
			}
		} else {
			_animator.SetFloat("speed", 0);
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Ladder") {
			OnLadder = true;
			_ladderPosition = other.transform.position;
			_ladderFacingDirection = other.GetComponent<Ladder>()._facingDirection;
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.tag == "Ladder") {
			OnLadder = false;
		}
	}

	// Called when the player reach the checkpoint(level complete)
	public void MoveToTarget (Transform _target) {
		iTween.MoveTo(gameObject, iTween.Hash(
			"position", _target,
			"speed", 1,
			"easetype", iTween.EaseType.linear
			));
		_animator.SetFloat("speed", 1);
		_inputDisabled = true;
	}

	// These solutions come from unity forum, thanks
	/// <summary>
	/// Cast a ray to test if Input.mousePosition is over any UI object in EventSystem.current. This is a replacement
	/// for IsPointerOverGameObject() which does not work on Android in 4.6.0f3
	/// </summary>
	private bool IsPointerOverUIObject() {
		// Referencing this code for GraphicRaycaster https://gist.github.com/stramit/ead7ca1f432f3c0f181f
		// the ray cast appears to require only eventData.position.
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
	
	/// <summary>
	/// Cast a ray to test if screenPosition is over any UI object in canvas. This is a replacement
	/// for IsPointerOverGameObject() which does not work on Android in 4.6.0f3
	/// </summary>
	private bool IsPointerOverUIObject(Canvas canvas, Vector2 screenPosition) {
		// Referencing this code for GraphicRaycaster https://gist.github.com/stramit/ead7ca1f432f3c0f181f
		// the ray cast appears to require only eventData.position.
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = screenPosition;
		
		GraphicRaycaster uiRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
		List<RaycastResult> results = new List<RaycastResult>();
		uiRaycaster.Raycast(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
}