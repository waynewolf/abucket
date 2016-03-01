using UnityEngine;
using System.Collections;

public class SceneryScroller : MonoBehaviour {
	public float amount = 54.6f;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Scenery") 
		{
			Vector3 pos = other.transform.position;
			pos.x += amount;
			other.transform.position = pos;
		}
	}
}
