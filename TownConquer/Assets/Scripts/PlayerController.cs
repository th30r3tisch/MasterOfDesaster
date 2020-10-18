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
        CheckIfAttackIsAborted();
    }


    private void CheckIfAttackIsHappening() {
        if (!Input.GetKey(KeyCode.LeftAlt)) {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit _hitInfo = GetRayCastHitInfo();
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

    private void CheckIfAttackIsAborted() {
        if (!Input.GetKey(KeyCode.LeftAlt)) {
            if (Input.GetMouseButtonUp(1)) {
                RaycastHit _hitInfo = GetRayCastHitInfo();
                GameObject go = _hitInfo.collider.gameObject;
                if (go.name.StartsWith("at") && go.GetComponent<AttackManager>().ownerid == Client.instance.myId) {
                    ClientSend.RetreatRequest(go.GetComponent<AttackManager>().start, go.GetComponent<AttackManager>().end);
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
