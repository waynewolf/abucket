using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MermaidController : MonoBehaviour {
	public float forwardSpeed = 5f;
	public float downCompensationRatio = 2f;
	public float expectedForce = 30f;

	private Rigidbody2D rb2D;
	private Vector2 newPos = new Vector2(0f, 0f);

	private int score;

	void Awake () {
		rb2D = GetComponent<Rigidbody2D> ();
		score = 0;
		// register the MermaidController to GameManager
		GameManager.Instance.Mermaid = this;
	}

	void Update () {
		bool keepJumping = false;

		newPos.x = transform.position.x + forwardSpeed * Time.deltaTime;
		newPos.y = transform.position.y;
		transform.position = newPos;

		if (Input.touchCount == 0 && !Input.GetMouseButtonDown(0))
			return;

		if (Input.touchCount > 0) {
			Touch touch = Input.GetTouch (0);

			// keep jumping when tap and hold
			if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary) {
				keepJumping = true;
			}
		} else if(Input.GetMouseButtonDown(0)) {
			keepJumping = true;
		}

		if (keepJumping) {
			// adjust force based on current velocity
			float upForce = expectedForce - rb2D.velocity.y * downCompensationRatio;
			upForce = Mathf.Max (upForce, 0f);
			rb2D.AddForce(new Vector2(0f, upForce));
		}
	}

	void OnEnable () {
		GameManager.Instance.ShowUI (true);
		GameManager.Instance.ShowScore (score);
	}
	
	void OnDisable() {
		GameManager.Instance.ShowUI (false);
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		string colliderTag = col.gameObject.tag;
		if (colliderTag == "Food") {
			++score;
			GameManager.Instance.ShowScore(score);
			if(score == 10) {
				GameManager.Instance.ShowLevelSelectMenu();
			}
		} else if (colliderTag == "Bait") {
			GameManager.Instance.ShowGameOverMenu();
		}
	}

	public int Score {
		get {
			return score;
		}
	}
}
