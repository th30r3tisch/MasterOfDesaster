using UnityEngine;

public class CameraController : MonoBehaviour {
    public float dragSpeed = 2;
    public float scrollspeed = 200;
    public float minY = 200; // min scroll height
    public float maxY = 900; // max scroll height
    public LayerMask mask;

    private RaycastHit oldMousePos;


    void Update() {

        Vector3 _pos = transform.position;

        _pos = DragWorld(_pos);
        _pos = ZoomWorld(_pos);

        transform.position = _pos;
    }


    /// <summary>
    /// drags the camera through the world
    /// </summary>
    /// <param name="_pos">current position of the camera</param>
    /// <returns>new position of the camera</returns>
    Vector3 DragWorld(Vector3 _pos) {

        if (Input.GetKey(KeyCode.LeftAlt)) {
            // on mouse btn click
            if (Input.GetMouseButtonDown(0)) {
                Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(_ray, out oldMousePos, Mathf.Infinity, mask);
            }


            // during mouse button hold down
            if (Input.GetMouseButton(0)) {
                Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit currentMousePos;
                Physics.Raycast(_ray, out currentMousePos, Mathf.Infinity, mask);

                _pos.z += oldMousePos.point.z - currentMousePos.point.z;
                _pos.x += oldMousePos.point.x - currentMousePos.point.x;
                _pos.y = transform.position.y;
            }
        }

        return _pos;
    }


    /// <summary>
    /// Zoomes the world in or out in scrolling with the mouse wheel
    /// </summary>
    /// <param name="_pos">current position of the camera</param>
    /// <returns>new position of the camera</returns>
    Vector3 ZoomWorld(Vector3 _pos) {
        float _scroll = Input.GetAxis("Mouse ScrollWheel");
        _pos.y -= _scroll * scrollspeed * 50f * Time.deltaTime;
        _pos.y = Mathf.Clamp(_pos.y, minY, maxY);

        return _pos;
    }

}
