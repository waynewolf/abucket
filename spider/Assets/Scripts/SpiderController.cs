using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderController : MonoBehaviour, IClickable {
	public float climbingSpeed = 0.1f;

	public enum State {
		Grounded,
		OnSilk,
		Floating,
	}

	[HideInInspector]
	public State _state;

	private Transform _groundCheck;
	private Transform _savedParent;
	private Coroutine _spiderClimbCoroutine;
	private Collider2D[] _colliders;
	private Rigidbody2D _rb2D;
	private EventManager _eventManager;
	private SilkManager _silkManager;

	void Awake() {
		_colliders = GetComponentsInChildren<Collider2D>();
		_rb2D = GetComponent<Rigidbody2D> ();
		_groundCheck = transform.Find("GroundCheck");

		GameObject eventManagerObj = GameObject.Find ("EventManager");
		if (!eventManagerObj) {
			Debug.LogError ("Scene must contain EventManager and corresponding script");
		}
		_eventManager = eventManagerObj.GetComponent<EventManager> ();
		if (!_eventManager) {
			Debug.LogError("EventManager has no attached script");
		}
		
		GameObject silkManagerObj = GameObject.Find ("SilkManager");
		if (!silkManagerObj) {
			Debug.LogError ("Scene must contain SilkManager and corresponding script");
		}
		_silkManager = silkManagerObj.GetComponent<SilkManager> ();
		if (_silkManager == null) {
			Debug.LogError ("SilkManager has no attached script");
		}

		_savedParent = transform.parent;
	}

	void Start () {
		_state = State.Floating;
		_eventManager.AddClickListener (this);
	}

	public bool IsClicked(Vector3 clickPosition) {
		Vector2 worldPos = Camera.main.ScreenToWorldPoint (clickPosition);
		Collider2D collider2D = Physics2D.OverlapPoint (worldPos);
		if (collider2D && collider2D.gameObject.tag == "Player") {
			return true;
		}
		return false;
	}

	public void OnClick() {
		Debug.Log ("Spider OnClick");
		if (_state == State.OnSilk)
			JumpOff ();
	}

	void JumpOff() {
		if (_spiderClimbCoroutine != null) {
			StopCoroutine (_spiderClimbCoroutine);
		}
		_silkManager.ClimbToTheTop ();

		_rb2D.AddForce (new Vector2 (50, 0));
		_state = State.Floating;
	}

	void FixedUpdate() {
		if (_state == State.OnSilk) {
			foreach (var collider in _colliders)
				collider.isTrigger = true;
			_rb2D.isKinematic = true;
		}

		if (_state == State.Floating) {
			foreach (var collider in _colliders)
				collider.isTrigger = false;
			_rb2D.isKinematic = false;

			transform.SetParent (_savedParent);
			bool grounded = Physics2D.Linecast (transform.position, _groundCheck.position, 1 << LayerMask.NameToLayer ("Ground"));
			if (grounded) {
				_state = State.Grounded;
			}
		}
	}

	public void Silking(Transform destination) {

		if (_silkManager.CanSilk(transform, destination)) {
			// CreateSilk will make old silk go away, so make sure 
			// spider is not at the child of any silk
			transform.SetParent (_savedParent);
			Silk silk = _silkManager.CreateSilk (transform, destination);
			if (silk != null) {
				List<GameObject> chains = silk.GetChains();
				Debug.Log ("chains # " + chains.Count);
				if (_spiderClimbCoroutine != null) {
					StopCoroutine(_spiderClimbCoroutine);
					_state = State.Floating;
					transform.SetParent(_savedParent);
					_spiderClimbCoroutine = null;
				}
				_spiderClimbCoroutine = StartCoroutine(ClimbUp(chains));
			}
		}
	}

	IEnumerator ClimbUp(List<GameObject> chains) {
		ChainController chainController;

		// disable colliders and rigidbody.
		_state = State.OnSilk;

		for (int i = chains.Count - 1; i >= 0; i--) {
			Debug.Log ("climb to " + i + " : " + chains[i].name);
			chainController = chains[i].GetComponent<ChainController>();
			chainController.ClimbOn();
			transform.SetParent(chains[i].transform);
			
			float sqrRemainingDistance = (chains[i].transform.position - transform.position).sqrMagnitude;
			while (sqrRemainingDistance > 0.1f) {
				Vector2 direction = chains[i].transform.position - transform.position;
				float angle = Mathf.Atan2(direction.y, direction.x);
				
				transform.position = new Vector3(transform.position.x + climbingSpeed * Mathf.Cos(angle),
				                                 transform.position.y + climbingSpeed * Mathf.Sin(angle),
				                                 0f);
				sqrRemainingDistance = (chains[i].transform.position - transform.position).sqrMagnitude;
				yield return new WaitForSeconds(0.05f);
			}
			Debug.Log ("on chain[" + i + "]");
			// if it's the last chain, first reparent myself to avoid being deleted
			if (i == chains.Count - 1) {
				transform.SetParent(_savedParent);
			}
			chainController.GoAway();
		}

		_silkManager.ClimbToTheTop ();
		_state = State.Floating;
		transform.SetParent (_savedParent);
		
		yield return null;
	}

	void OnDestroy() {
		Debug.Log ("SpiderController destroyed");
	}

	void OnDisable() {
		Debug.Log ("SpiderController disabled");
		Debug.Break ();
	}
}
