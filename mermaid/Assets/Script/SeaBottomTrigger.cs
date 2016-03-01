using UnityEngine;
using System.Collections;

public class SeaBottomTrigger : MonoBehaviour
{
	void OnTriggerEnter2D(Collider2D col)
	{
		string colliderTag = col.gameObject.tag;
		if (colliderTag == "Player") {
			GameManager.Instance.ShowGameOverMenu();
		}
	}
}