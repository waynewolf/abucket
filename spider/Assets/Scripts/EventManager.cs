using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {
	// a game object can be interested in multiple events, event names are unique,
	// an event can be listening on by multiple game objects
	private Dictionary <GameObject, HashSet<string> > registeredObjs;

	private Dictionary <string, UnityEvent > eventDict;


	private Dictionary <string, List<IClickable> > inputObjMap;

	void Awake () {
		_Init ();
	}

	// easy for unit testing
	public void _Init() {
		registeredObjs = new Dictionary <GameObject, HashSet<string> > ();
		eventDict = new Dictionary<string, UnityEvent> ();
		eventDict.Add ("click", new UnityEvent ());
		eventDict.Add ("swipe", new UnityEvent ());
		inputObjMap = new Dictionary <string, List<IClickable> > ();
	}

	void Update () {
		// Input is checked ONLY here
		bool clicked = false;
		Vector3 clickPosition = new Vector3 (0, 0);
		
		if (Input.touchCount > 0) {
			Touch touch = Input.GetTouch (0);
			if (touch.phase == TouchPhase.Began) {
				clicked = true;
				clickPosition = touch.position;
			}
		}
		#if UNITY_EDITOR
		else {
			if (Input.GetMouseButtonDown(0)) {
				clicked = true;
				clickPosition = Input.mousePosition;
			}
		}
		#endif

		if (clicked) {
			List<IClickable> clickables;
			if (inputObjMap.TryGetValue("click", out clickables)) {
				foreach(IClickable clickable in clickables) {
					if (clickable.IsClicked(clickPosition)) {
						clickable.OnClick();
					}
				}
			}
		}

//		if (clicked && HitSomething(clickPosition, out target)) {
//			if (EventInterested(target, "click"))
//			    TriggerEvent ("click");
//		}
	}

	/// <summary>
	/// 	Check if game object is interested in a specific event.
	/// </summary>
	/// <returns><c>true</c>, if gameobject is interested in the event, <c>false</c> otherwise.</returns>
	/// <param name="obj">Object.</param>
	/// <param name="eventName">Event name.</param>
	bool EventInterested(GameObject obj, string eventName) {
		HashSet<string> eventNameSet;
		if (registeredObjs.TryGetValue (obj, out eventNameSet)) {
			if (eventNameSet.Contains(eventName))
				return true;
		}
		return false;
	}

	// if not hit, target is untouched
	bool HitSomething (Vector3 touchScreenPos, out GameObject target) {
		bool hit = false;
		Vector2 worldPos = Camera.main.ScreenToWorldPoint(touchScreenPos);
		Collider2D collider2D = Physics2D.OverlapPoint (worldPos);
		if (collider2D) {
			target = collider2D.gameObject;
			hit = true;
		} else {
			target = null;
		}
		return hit;
	}

	// same object can have only one action for same event
	public void RegisterEvent(string eventName, GameObject obj, UnityAction action) {
		UnityEvent thisEvent;
		if (eventDict.TryGetValue (eventName, out thisEvent)) {
			// UnityEvent behaviour: same action can be added multiple times, but
			// only one RemoveListener will remove all the same action. here we 
			// Remove then Add to make sure the same action can be added once once.
			thisEvent.RemoveListener(action);
			thisEvent.AddListener (action);
			HashSet<string> eventNameSet;
			if (registeredObjs.TryGetValue(obj, out eventNameSet)) {
				// If this gameobject has been registered, simply put the event name to event name set,
				// doesn't matter if the event name already exists or not, HashSet will handle that
				eventNameSet.Add (eventName);
			} else {
				// The game object is first time registered
				eventNameSet = new HashSet<string> ();
				eventNameSet.Add (eventName);
				registeredObjs.Add (obj, eventNameSet);
			}
		} else {
			Debug.LogError("event name " + eventName + " not exsisted");
		}
	}

	public void UnRegisterEvent(string eventName, GameObject obj, UnityAction action) {
		HashSet<string> eventNameSet;
		if (!registeredObjs.TryGetValue (obj, out eventNameSet)) {
			return;
		}

		if (eventNameSet.Contains (eventName)) {
			UnityEvent thisEvent;
			if (eventDict.TryGetValue(eventName, out thisEvent)){
				// doesn't matter if action not one of the listeners
				thisEvent.RemoveListener (action);
			} else {
				Debug.LogError("event name " + eventName + " not exsisted");
			}
			eventNameSet.Remove (eventName);
		}

		if (eventNameSet.Count == 0) {
			registeredObjs.Remove(obj);
		}
	}

	// all events that the game object is intereted
	//
	// return non-null HashSet<string>, 0 item if obj not registered,
	// at least one element if the obj currently registered.
	public HashSet<string> GetRegisteredEvent(GameObject obj) {
		HashSet<string> eventNameSet;
		if (!registeredObjs.TryGetValue (obj, out eventNameSet)) {
			eventNameSet = new HashSet<string>();
		}

		return eventNameSet;
	}

	// all game objects that are listening on the event
	public List<GameObject> GetListeners(string eventName) {
		List<GameObject> objs = new List<GameObject> ();
		foreach (KeyValuePair<GameObject, HashSet<string> > entry in registeredObjs) {
			if (entry.Value.Contains(eventName))
				objs.Add (entry.Key);
		}
		return objs;
	}

	public void TriggerEvent(string eventName) {
		UnityEvent thisEvent;
		if (eventDict.TryGetValue (eventName, out thisEvent)) {
			thisEvent.Invoke();
		}
	}

	public void AddClickListener(IClickable clickable) {
		List<IClickable> clickables;
		if (inputObjMap.TryGetValue ("click", out clickables)) {
			clickables.Add (clickable);
		} else {
			clickables = new List<IClickable>();
			clickables.Add (clickable);
			inputObjMap.Add ("click", clickables);
		}
	}

	public void RemoveClickListener(IClickable clickable) {
		List<IClickable> clickables;
		if (inputObjMap.TryGetValue ("click", out clickables)) {
			clickables.Remove(clickable);
		}
	}

}
