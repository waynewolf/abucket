using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {
	public GameObject pooledObject;
	public int pooledAmount = 20;
	public bool willGrow = true;
	public float reclaimAfterSeconds = 10f;

	private List<GameObject> pooledObjects;

	void Awake() {
		pooledObjects = new List<GameObject> ();
	}

	void Start () {
		for (int i = 0; i < pooledAmount; i++) {
			GameObject obj = (GameObject)Instantiate(pooledObject);
			InitObject(obj);
			pooledObjects.Add (obj);
		}
	}

	void InitObject(GameObject obj) {
		obj.transform.SetParent(gameObject.transform);
		obj.SetActive(false);
	}

	IEnumerator Reclaime(GameObject obj, float delay) {
		yield return new WaitForSeconds (delay);
		obj.SetActive (false);
		yield return null;
	}

	// Before we send this object out of pool, do some preparation work
	GameObject PrepareObject (GameObject selectedObj)
	{
		if (reclaimAfterSeconds > 0f) {
			StartCoroutine(Reclaime(selectedObj, reclaimAfterSeconds));
		}

		return selectedObj;
	}

	public GameObject GetPooledObject() {
		for (int i = 0; i < pooledObjects.Count; i++) {
			if (!pooledObjects[i].activeInHierarchy) {
				GameObject selectedObj = pooledObjects[i];
				return PrepareObject(selectedObj);
			}
		}

		if (willGrow) {
			GameObject obj = (GameObject)Instantiate(pooledObject);
			InitObject(obj);
			pooledObjects.Add (obj);
			return PrepareObject(obj);
		}

		return null;
	}
}
