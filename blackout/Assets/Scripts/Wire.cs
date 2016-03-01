using UnityEngine;
using System.Collections;

public class Wire : ItemBase {

	[SerializeField]
	public enum Direction {
		XZ_X,
		XZ_Z,
		Y,
	};

	public Direction _direction;

	void Start() {
		// Give a hint if position restriction is not satisfied
		CheckPosition();
	}

	public bool CheckPosition() {
		if (_direction == Direction.XZ_X) {
			float deltaX = Mathf.Abs(transform.position.x - Mathf.RoundToInt(transform.position.x));
			float deltaZ = Mathf.Abs(transform.position.z - Mathf.RoundToInt(transform.position.z));
			if (deltaX != 0.5f || deltaZ != 0f) {
				Debug.LogError("x coordinate of the X direction wire must in half position, z coordinate must in integer position");
				return false;
			}
			return true;
		}
		else if (_direction == Direction.XZ_Z) {
			float deltaZ = Mathf.Abs(transform.position.z - Mathf.RoundToInt(transform.position.z));
			float deltaX = Mathf.Abs(transform.position.x - Mathf.RoundToInt(transform.position.x));
			if (deltaZ != 0.5f || deltaX != 0f) {
				Debug.LogError("z coordinate of the Z direction wire must in half position, x coordinate must in integer position");
				return false;
			}
			return true;
		} else if (_direction == Direction.Y) {
			float deltaY = Mathf.Abs(transform.position.y - Mathf.RoundToInt(transform.position.y));
			// TODO: what about X and Z restriction, firgure this out later !
			if (deltaY != 0.5f) {
				Debug.LogError("y coordinate of the Y direction wire must in half position, like 0.5, -3.5");
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Gets the end points. Make sure the endpoints are at integer value coordinates!
	/// </summary>
	/// <param name="endPoint1">End point1.</param>
	/// <param name="endPoint2">End point2.</param>
	public void GetEndPoints(out Vector3 endPoint1, out Vector3 endPoint2) {
		Vector3 endPoint1Pos = transform.position;
		Vector3 endPoint2Pos = transform.position;
		// Adjust the position of two endpoints, end1 is less than end2
		if (_direction == Direction.XZ_X) {
			endPoint1Pos.x -= 0.5f;
			endPoint1Pos.x = Mathf.RoundToInt(endPoint1Pos.x);
			endPoint2Pos.x += 0.5f;
			endPoint2Pos.x = Mathf.RoundToInt(endPoint2Pos.x);
		} else if (_direction == Direction.XZ_Z) {
			endPoint1Pos.z -= 0.5f;
			endPoint1Pos.z = Mathf.RoundToInt(endPoint1Pos.z);
			endPoint2Pos.z += 0.5f;
			endPoint2Pos.z = Mathf.RoundToInt(endPoint2Pos.z);
		} else {
			endPoint1Pos.y -= 0.5f;
			endPoint1Pos.y = Mathf.RoundToInt(endPoint1Pos.y);
			endPoint2Pos.y += 0.5f;
			endPoint2Pos.y = Mathf.RoundToInt(endPoint2Pos.y);
		}
		endPoint1 = endPoint1Pos;
		endPoint2 = endPoint2Pos;
	}

	public bool GetJoint(Wire other, out Vector3 joint) {
		return Wire.GetJoint(this, other, out joint);
	}

	/// <summary>
	/// Gets the joint of two Wires if exists. Notice: two wires with two same endpoints are not supported
	/// </summary>
	/// <returns><c>true</c>, if joint was gotten, <c>false</c> otherwise.</returns>
	/// <param name="wire1">Wire1.</param>
	/// <param name="wire2">Wire2.</param>
	/// <param name="joint">Joint.</param>
	public static bool GetJoint(Wire wire1, Wire wire2, out Vector3 joint) {
		Vector3 wire1End1Pos, wire1End2Pos;
		Vector3 wire2End1Pos, wire2End2Pos;

		// same wire has no joint
		if (wire1 == wire2) {
			joint = Vector3.zero;
			return false;
		}

		wire1.GetEndPoints(out wire1End1Pos, out wire1End2Pos);
		wire2.GetEndPoints(out wire2End1Pos, out wire2End2Pos);

		if (MathUtil.Vector3EqualInt(wire1End1Pos, wire2End1Pos) || MathUtil.Vector3EqualInt(wire1End1Pos, wire2End2Pos)) {
			joint = wire1End1Pos;
			return true;
		}
		if (MathUtil.Vector3EqualInt(wire1End2Pos, wire2End1Pos) || MathUtil.Vector3EqualInt(wire1End2Pos, wire2End2Pos)) {
			joint = wire1End2Pos;
			return true;
		}

		joint = Vector3.zero;
		return false;
	}
}
