using UnityEngine;
using System.Collections;

public class CameraNavigator : MonoBehaviour {

    private Vector2 _oldMousePosition;
	public void Update () {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetButtonDown(1))
        {

        }

        _oldMousePosition = mousePosition;
	}
}
