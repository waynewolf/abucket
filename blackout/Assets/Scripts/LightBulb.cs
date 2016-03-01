using UnityEngine;
using System.Collections;

public class LightBulb : ItemBase {
	public float _generateLightInterval = 2f;
	public GameObject _lightPrefab;
	private bool _electrified;
	private float _lastGenerateLightTime;

	void Update () {
		if (_electrified) {
			if (Time.time - _lastGenerateLightTime > _generateLightInterval) {
				// LightBulb can shine light in 6 directions, it ignors the _faceDirection property
				GameObject[] lightObject = new GameObject[6];
				for (int i = 0; i < 6; i++) {
					lightObject[i] = Instantiate(_lightPrefab, transform.position, Quaternion.identity) as GameObject;
					lightObject[i].name = "*NormalLight*";
					lightObject[i].transform.SetParent(LevelManager.Instance.GetDynamicGameObjectHolder().transform);
				}

				lightObject[0].GetComponent<NormalLight>().Shine(Vector3.right);
				lightObject[1].GetComponent<NormalLight>().Shine(Vector3.left);
				lightObject[2].GetComponent<NormalLight>().Shine(Vector3.up);
				lightObject[3].GetComponent<NormalLight>().Shine(Vector3.down);
				lightObject[4].GetComponent<NormalLight>().Shine(Vector3.forward);
				lightObject[5].GetComponent<NormalLight>().Shine(Vector3.back);

				_lastGenerateLightTime = Time.time;
			}
			UpdateTurnOffTimer(Time.deltaTime);
		}
	}

	void Electrified() {
		_electrified = true;
		ResetTurnOffTimer();
	}

	protected override void OnTurnOffTimerTimeout () {
		_electrified = false;
	}
}
