using UnityEngine;

/// <summary>
/// Derived singleton from this template doesn't have to be attached
/// to a node in the scene, code will automally do that.
/// </summary>
public class Singleton<T> : BOBehaviour where T : Component {
	private static T _instance;
	
	public static T Instance {
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<T> ();
				if (_instance == null) {
					GameObject obj = new GameObject ();
					_instance = obj.AddComponent<T> ();
				}
			}
			return _instance;
		}
	}

	protected virtual void Awake () {
		if (_instance == null) {
			_instance = this as T;
			DontDestroyOnLoad (gameObject);
		}
		else if(_instance != this) {
			Destroy(gameObject);
		}
	}
}

