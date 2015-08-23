using UnityEngine;

public class CameraNavigator : MonoBehaviour
{
    public float minOrthographicSize = 4;
    public float maxOrthographicSize = 25;
    public float mouseSensitivity = 1;
    private Vector3 _lastPosition;

    public void Update()
    {
        HandleScrollWheel();
        HandleDragging();
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _lastPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - _lastPosition;
            delta = Camera.main.ScreenToWorldPoint(delta) - Camera.main.ScreenToWorldPoint(Vector3.zero);
            Camera.main.transform.Translate(-delta.x * mouseSensitivity, -delta.y * mouseSensitivity, 0);
            _lastPosition = Input.mousePosition;
        }

        Camera.main.transform.Translate(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
    }

    private void HandleScrollWheel()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Camera.main.orthographicSize--;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Camera.main.orthographicSize++;
        }
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minOrthographicSize, maxOrthographicSize);
    }
}