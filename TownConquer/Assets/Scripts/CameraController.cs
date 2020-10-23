using SharedLibrary;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float dragSpeed = 2;
    public float scrollspeed = 200;
    public float minY = 400; // min scroll height
    public float maxY = 1000; // max scroll height
    public LayerMask mask;

    private RaycastHit oldMousePos;
    private bool topdown = false;


    void Update() {

        Vector3 _pos = transform.position;

        _pos = DragWorld(_pos);
        _pos = ZoomWorld(_pos);
        _pos = LimitCameraMovement(_pos);

        transform.position = _pos;

        ToggleTopdownView();
    }

    private void ToggleTopdownView() {
        if (Input.GetKey(KeyCode.Space) && !topdown) {
            transform.rotation = Quaternion.Euler(90, 0, 0);
            topdown = true;
        }
        else if (Input.GetKey(KeyCode.Space) && topdown) {
            transform.rotation = Quaternion.Euler(30, 0, 0);
            topdown = false;
        }
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

    Vector3 LimitCameraMovement(Vector3 _pos) {
        return new Vector3(Mathf.Clamp(_pos.x, 0, Constants.MAP_WIDTH), Mathf.Clamp(_pos.y, minY, maxY), Mathf.Clamp(_pos.z, -500, Constants.MAP_HEIGHT));
    }

}
