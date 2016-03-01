using UnityEngine;
using System.Collections;

public class Utils {
	
	public static bool AngleLargeThanPi(Vector2 from, Vector2 to) {
		return from.x * to.y - to.x * from.y < 0 ? true : false;
	}

	/// <summary>
	/// Get 360 degree angle between from and to.
	/// </summary>
	public static float AngleBetween(Vector2 from, Vector2 to) {
		float angle = Vector2.Angle (from, to);

		if (AngleLargeThanPi(from, to))
			angle = 360 - angle;
		
		return angle;
	}
	
	// 360 angle from x+ axis to vector
	public static float AngleFromX(Vector2 vector) {
		float angleRadians = Mathf.Atan2(vector.y, vector.x);
		float angleDegrees = angleRadians * Mathf.Rad2Deg;

		//angleDegrees will be in the range (-180,180].
		if (angleDegrees < 0) 
			angleDegrees += 360;
		
		return angleDegrees;
	}

}
