using UnityEngine;
using System.Collections;

public class TackController : MonoBehaviour, IClickable {

	private EventManager _eventManager;
	private SpiderController _spiderController;

	void Awake () {
		GameObject eventManagerObj = GameObject.Find ("EventManager");
		if (!eventManagerObj) {
			Debug.LogError ("Scene must contain EventManager and corresponding script");
		}
		_eventManager = eventManagerObj.GetComponent<EventManager> ();
		if (!_eventManager) {
			Debug.LogError("EventManager has no attached script");
		}
		GameObject spider = GameObject.Find ("Spider");
		if (!spider) {
			Debug.LogError ("Scene must contain Spider and corresponding script");
		}
		_spiderController = spider.GetComponent<SpiderController>();
		if (!_spiderController) {
			Debug.LogError("Spider has no attached script");
		}
	}
	
	void Start () {
		_eventManager.AddClickListener (this);
	}

	public bool IsClicked(Vector3 clickPosition) {
		Vector2 worldPos = Camera.main.ScreenToWorldPoint (clickPosition);
		Collider2D collider2D = Physics2D.OverlapPoint (worldPos);
		if (collider2D && collider2D.gameObject == gameObject) {
			return true;
		}
		return false;
	}

	public void OnClick() {
		_spiderController.Silking (transform);
	}
	
}
