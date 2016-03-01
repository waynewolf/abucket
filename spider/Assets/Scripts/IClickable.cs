using UnityEngine;
using System;

public interface IClickable {
	bool IsClicked(Vector3 clickPosition);
	void OnClick();
}


