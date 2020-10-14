using SharedLibrary;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public LayerMask mask;
    public int lineWidth;
    public Material material;

    private Vector3 lineStart;
    private Vector3 lineEnd;
    private bool startCondition = false;
    int? startTownId = null;


    void Update() {
        CheckIfAttackIsHappening();
    }


    /// <summary>
    /// drags the camera through the world
    /// </summary>
    /// <param name="_pos">current position of the camera</param>
    /// <returns>new position of the camera</returns>
    private void CheckIfAttackIsHappening() {
        if (!Input.GetKey(KeyCode.LeftAlt)) {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit _hitInfo = GetRayCastHitInfo();
                int test = _hitInfo.collider.gameObject.GetComponentInParent<TownManager>().ownerid;
                int test2 = Client.instance.myId;
                if (_hitInfo.collider.gameObject.name.StartsWith("Town") &&
                    _hitInfo.collider.gameObject.GetComponentInParent<TownManager>().ownerid == Client.instance.myId) {
                    lineStart = _hitInfo.collider.gameObject.transform.position;
                    startTownId = _hitInfo.collider.gameObject.GetInstanceID();
                    startCondition = true;
                }
            }
            if (Input.GetMouseButtonUp(0) && startCondition) {
                RaycastHit _hitInfo = GetRayCastHitInfo();
                if (_hitInfo.collider.gameObject.name.StartsWith("Town") &&
                    _hitInfo.collider.gameObject.GetInstanceID() != startTownId) {
                    lineEnd = _hitInfo.collider.gameObject.transform.position;
                    ClientSend.AttackRequest(lineStart, lineEnd);
                    startCondition = false;
                    startTownId = null;
                }
            }
        }
    }

    private RaycastHit GetRayCastHitInfo() {
        RaycastHit _hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out _hitInfo, Mathf.Infinity, mask);
        return _hitInfo;
    }
}
