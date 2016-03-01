using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Silk {
	private float _lineWidth;
	private List<GameObject> _chains;
	private LineRenderer _lineRenderer;
	private GameObject _savedChainsHolder;

	private enum State {
		Init,
		Completed,
		Removed,
	};
	private State _state;

	public Silk(float lineWidth) {
		_chains = new List<GameObject> ();
		_state = State.Init;
		_lineWidth = lineWidth;
	}

	/// <summary>
	/// Shoot the specified start and target.
	/// </summary>
	/// <param name="chainsHolder">chain objects holder</param> 
	/// <param name="start">Start location where the silk is shoot </param>
	/// <param name="target">The target location that the silk finally hangs on</param>
	/// <returns>true if silking succeeeded, false otherwise</returns>
	public bool Shoot(GameObject chainsHolder, Transform start, Transform target, int chainsNumber, Material silkMaterial) {
		_savedChainsHolder = chainsHolder;

		Vector2 startPos = start.position;
		Vector2 targetPos = target.position;
		float distance = Vector2.Distance (startPos, targetPos);

		if (distance < 0.1f) {
			Debug.LogWarning("too close to shoot a silk");
			return false;
		}

		_lineRenderer = chainsHolder.GetComponent<LineRenderer> ();
		if (!_lineRenderer)
			_lineRenderer = chainsHolder.AddComponent<LineRenderer> ();
		
		float chainLength = distance / chainsNumber;
		Debug.Log ("chain length: " + chainLength);

		Vector2 chainBoxColliderSize = new Vector2(chainLength, _lineWidth);

		float angle = Utils.AngleFromX (start.position - target.position);
		float angleRadian = angle * Mathf.Deg2Rad;

		Vector2 chainOffset = new Vector2 (chainLength * Mathf.Cos (angleRadian), chainLength * Mathf.Sin (angleRadian));
		Vector2 currentPos = targetPos + 0.5f * chainOffset;
		Quaternion rotation = Quaternion.AngleAxis (angle, Vector3.forward);

		// chain objects in reverse order
		for (int i = 0; i < chainsNumber; i++) {
			GameObject obj = new GameObject("Chain" + i);
			ChainController chainController = obj.AddComponent<ChainController>();
			chainController.SetSilk(this);

			Rigidbody2D rb2D = obj.AddComponent<Rigidbody2D>();
			//rb2D.mass = 1f;

			BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
			box.size = chainBoxColliderSize;

			HingeJoint2D joint = obj.AddComponent<HingeJoint2D>();
			if (i == 0) {
				// joint.anchor is always in its local space
				joint.anchor = new Vector2(-0.5f * chainLength, 0);
				// joint.connectedAnchor in world space since not connected to other
				// rigidbody for the first hinge
				joint.connectedAnchor = targetPos;
			} else {
				// connect to parent rigid body if not the first chain
				joint.connectedBody = _chains[i-1].GetComponent<Rigidbody2D>();

				// joint.anchor is always in my own local space
				joint.anchor = new Vector2(-0.5f * chainLength, 0);

				// joint.connectedBody now in other object’s local space
				joint.connectedAnchor = new Vector2(0.5f * chainLength, 0);
			}

			obj.transform.SetParent(chainsHolder.transform);
			obj.transform.position = currentPos;
			obj.transform.rotation = rotation;
			obj.transform.localScale = Vector3.one;

			_chains.Add (obj);
			currentPos += chainOffset;
		}

		_lineRenderer.SetVertexCount (chainsNumber);
		_lineRenderer.SetWidth (_lineWidth, _lineWidth);
		_lineRenderer.material = silkMaterial;
		for (int i = 0; i < chainsNumber; i++) {
			_lineRenderer.SetPosition(i, _chains[i].transform.position);
		}

		_state = State.Completed;

		return true;
	}

	public void Refresh() {
		if (_lineRenderer && _state == State.Completed) {
			int newChainsNumber = GetChainsNumber();
			_lineRenderer.SetVertexCount(newChainsNumber);
			for (int i = 0; i < newChainsNumber; i++) {
				_lineRenderer.SetPosition (i, _chains [i].transform.position);
			}
		}

		if (GetChainsNumber () == 0)
			Object.Destroy (_savedChainsHolder);
	}

	public void Remove() {
		foreach (GameObject obj in _chains) {
			Object.Destroy(obj);
		}
		_chains.Clear ();
		if (_savedChainsHolder) {
			Object.Destroy (_savedChainsHolder);
		}

		if (_lineRenderer) {
			_lineRenderer.SetVertexCount(0);
			Object.Destroy(_lineRenderer);
		}

		_state = State.Removed;
	}

	public GameObject ChainAtTail() {
		if (_state == State.Completed) {
			return _chains [_chains.Count - 1];
		}
		return null;
	}

	public void RemoveTailChain (GameObject gameObject) {
		if (ChainAtTail () != gameObject) {
			Debug.LogError("the tail gameobject is not the same, input: " +
			               gameObject.name + ", current tail: " + ChainAtTail().name);
		}
		Debug.Log ("remove tail chain: " + gameObject.name);
		_chains.Remove (gameObject);
		Refresh ();
	}

	public int GetChainsNumber() {
		return _chains.Count;
	}

	public bool IsCompleted() {
		return _state == State.Completed;
	}

	public List<GameObject> GetChains() {
		if (_state == State.Completed) {
			return _chains;
		}
		return null;
	}
}
