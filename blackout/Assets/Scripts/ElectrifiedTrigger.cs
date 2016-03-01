using UnityEngine;
using System.Collections;

public class ElectrifiedTrigger : BOBehaviour {
	
	void OnTriggerEnter(Collider other) {
		if (other.tag == "ElectricCurrent") {
			SendMessage("Electrified");
		}
	}

}
