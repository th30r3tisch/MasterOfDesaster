using SharedLibrary;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float dragSpeed = 2;
    public float scrollspeed = 200;
    public float minY = 400; // min scroll height
    public float maxY = 1000; // max scroll height
    public LayerMask mask;

    private RaycastHit _oldMousePos;
    private bool _topdown = false;


    void Update() {

        Vector3 pos = transform.position;

        pos = DragWorld(pos);
        pos = ZoomWorld(pos);
        pos = LimitCameraMovement(pos);

        transform.position = pos;

        ToggleTopdownView();
    }

    private void ToggleTopdownView() {
        if (Input.GetKeyDown(KeyCode.Space) && !_topdown) {
            transform.rotation = Quaternion.Euler(90, 0, 0);
            _topdown = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && _topdown) {
            transform.rotation = Quaternion.Euler(30, 0, 0);
            _topdown = false;
        }
    }


    /// <summary>
    /// drags the camera through the world
    /// </summary>
    /// <param name="pos">current position of the camera</param>
    /// <returns>new position of the camera</returns>
    Vector3 DragWorld(Vector3 pos) {

        if (Input.GetKey(KeyCode.LeftAlt)) {
            // on mouse btn click
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out _oldMousePos, Mathf.Infinity, mask);
            }


            // during mouse button hold down
            if (Input.GetMouseButton(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out RaycastHit currentMousePos, Mathf.Infinity, mask);

                pos.z += _oldMousePos.point.z - currentMousePos.point.z;
                pos.x += _oldMousePos.point.x - currentMousePos.point.x;
                pos.y = transform.position.y;
            }
        }

        return pos;
    }


    /// <summary>
    /// Zoomes the world in or out in scrolling with the mouse wheel
    /// </summary>
    /// <param name="pos">current position of the camera</param>
    /// <returns>new position of the camera</returns>
    Vector3 ZoomWorld(Vector3 pos) {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollspeed * 50f * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        return pos;
    }

    Vector3 LimitCameraMovement(Vector3 pos) {
        return new Vector3(Mathf.Clamp(pos.x, 0, Constants.MAP_WIDTH), Mathf.Clamp(pos.y, minY, maxY), Mathf.Clamp(pos.z, -600, Constants.MAP_HEIGHT));
    }

}
