using UnityEngine;
using System.Collections;

public class GenerateElectricTrigger : BOBehaviour {
	public string _interestedTag;

	void OnTriggerEnter(Collider other) {
		if (other.tag == _interestedTag) {
			// Things like wind, electric current are dynamic item
			DynamicItem dynamicItem = other.GetComponent<DynamicItem>();
			ItemBase thisItem = gameObject.GetComponent<ItemBase>();
			if (dynamicItem._direction == -thisItem._faceDirection)
				SendMessage("GenerateElectricMessage");
			Destroy(other.gameObject);
		}
	}
}
