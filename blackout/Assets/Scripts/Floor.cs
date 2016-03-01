using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Floor : BOBehaviour {
	private float _y;

	void Awake() {
		_y = transform.position.y;
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (!Application.isEditor)
			return;
		Handles.color = Color.red;
		Gizmos.color = Color.yellow;
		for (int x = -16; x <= 16; x++) {
			Vector3 from = new Vector3(x, _y, -16);
			Vector3 to = new Vector3(x, _y, 16);
			Gizmos.DrawLine(from, to);
			Handles.Label (new Vector3(x, _y, 0), x.ToString());
		}
		for (int z = -16; z <= 16; z++) {
			Vector3 from = new Vector3(-16,_y, z);
			Vector3 to = new Vector3(16, _y, z);
			Gizmos.DrawLine(from, to);
			Handles.Label (new Vector3(0, _y, z), z.ToString());
		}
		if (GUI.changed)
			EditorUtility.SetDirty (gameObject);
	}
#endif

}
