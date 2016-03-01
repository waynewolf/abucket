using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MathUtil {

	public static bool Vector3EqualInt(Vector3 vec1, Vector3 vec2) {
		int x1 = Mathf.RoundToInt(vec1.x);
		int y1 = Mathf.RoundToInt(vec1.y);
		int z1 = Mathf.RoundToInt(vec1.z);
		int x2 = Mathf.RoundToInt(vec2.x);
		int y2 = Mathf.RoundToInt(vec2.y);
		int z2 = Mathf.RoundToInt(vec2.z);

		if (x1 == x2 && y1 == y2 && z1 == z2)
			return true;

		return false;
	}

	public static Vector3 Vector3RoundToInt(Vector3 vector) {
		Vector3 intVector = Vector3.zero;
		intVector.x = Mathf.RoundToInt(vector.x);
		intVector.y = Mathf.RoundToInt(vector.y);
		intVector.z = Mathf.RoundToInt(vector.z);
		return intVector;
	}

}

public class UniqueList<T> : List<T> {
	
	public new bool Add(T item) {
		if (this.Contains(item)) return false;
		base.Add (item);
		return true;
	}
	
}
