using UnityEngine;
using System.Collections;

public class PixelPerfectCamera : MonoBehaviour {
	
	public static float pixelsToUnits = 100f;
	public static float scale = 1f;

	// development around iphone 6 resolution, @2x
	public Vector2 devResolution = new Vector2 (1334, 750);
	
	void Awake () {
		var camera = GetComponent<Camera> ();
		if (camera.orthographic) {
			scale = Screen.height / devResolution.y;
			pixelsToUnits *= scale;
			camera.orthographicSize = (Screen.height / 2.0f) / pixelsToUnits;
		}
	}
}
