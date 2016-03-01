using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class ItemSpawner : MonoBehaviour {
	public ObjectPool heartPool;
	public ObjectPool baitPool;

	private const float SPAWN_RATE = 0.1f;
	private float initPosX;

	void Start() {
		initPosX = Camera.main.transform.position.x;
		StartCoroutine ("SpawnLoop");
	}

	public void StopSpawn() {
		StopCoroutine ("SpawnLoop");
	}

	IEnumerator SpawnLoop() {
		Vector3 spawnPos = new Vector3 (initPosX, 0, 0);
		int count = 0;
		bool[] heartOrBait = {true, true, true, true, false, true, true, true, true, true};
		while (true) {
			spawnPos.y = Random.Range (0, 8) - 3;
			// wait for the camera to forward if we're far ahead
			while (spawnPos.x - Camera.main.transform.position.x > 30f)
				yield return null;

			spawnPos.x += 2;
			bool isHeart = heartOrBait[Random.Range (0, 9)];
			GameObject obj;
			if (isHeart) {
				obj = heartPool.GetPooledObject();
			} else {
				obj = baitPool.GetPooledObject();
			}
			if (!obj) { 
				yield return null;
				continue;
			}
			obj.transform.position = spawnPos;
			obj.transform.rotation = Quaternion.identity;
			obj.SetActive(true);

			// spawn the first screen items immediately
			if (count++ < 20)
				yield return null;
			else
				yield return new WaitForSeconds(SPAWN_RATE);
		}
	}

}
